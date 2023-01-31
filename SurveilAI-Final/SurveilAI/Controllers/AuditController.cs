using Newtonsoft.Json;
using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using static SurveilAI.DataContext.Audit;

namespace SurveilAI.Controllers
{
    public class AuditController : Controller
    {
        #region private_varaibles
        private SurveilAIEntities db = new SurveilAIEntities();
        private SurveilAIEntities dbPro = new SurveilAIEntities();
        private UserCustom userCustom = new UserCustom();
        UserCustom UserCustom = new UserCustom();

        private bool Check(string num)
        {
            if (IsSessionExpired())
            {
                return false;
            }
            var data = Session["UserRole"] as List<object>;
            if (data == null)
            {
                return false;
            }
            else
            {
                if (data.Contains(num))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsSessionExpired()
        {
            if (Session["UserSessionID"] != null)
            {
                string userid = Session["UserID"].ToString();
                string CurrentSessionId = Session["UserSessionID"].ToString();
                CurrentSessionId = CurrentSessionId.Trim();
                string SessionIdFromDB = db.usersessions.Where(x => x.extuserid == userid).Select(x => x.sessionid).FirstOrDefault();

                if (CurrentSessionId == SessionIdFromDB.Trim())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private ILogger userlog = LogManager.GetLogger("user");
        private ILogger activitylog = LogManager.GetLogger("activity");
        private ILogger errorlog = LogManager.GetLogger("error");
        #endregion
        // GET: Audit

        #region public
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("54");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            LoadCommands();
            LoadActions();
            LoadOpertaionType();

            LoadPvJournalAll();
            LoadPvJournalByJob();
            LoadPvJournalByRole();


            if (Session["list"] != null)
            {
                string subcat = Session["subcat"].ToString();
                if (subcat.Equals("3"))
                {
                    ViewBag.pvJob = Session["list"];
                }
                else if (subcat.Equals("4"))
                {
                    ViewBag.pvRole = Session["list"];
                }
                else if (subcat.Equals("1"))
                    ViewBag.pvjournal = Session["list"];
                Session["list"] = null;
                return View();
            }

            return View();
        }

        public ActionResult Users()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("54");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            //LoadCommands();
            //LoadActions();
            //LoadOpertaionType();

            if (Session["list"] != null)
            {
                ViewBag.pvUsers = Session["list"];
                Session["list"] = null;
                return View();
            }
            ViewBag.HtmlUsersStr = "Users Transactions";
            List<object[]> _pvUsersList = UserCustom.GetUserSessionInfo();
            ViewBag.pvUsers = _pvUsersList;

            return View();
        }

        public ActionResult Agents()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("54");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            //LoadCommands();
            //LoadActions();
            //LoadOpertaionType();

            if (Session["list"] != null)
            {
                ViewBag.pvAgents = Session["list"];
                Session["list"] = null;
                return View();
            }
            ViewBag.HtmlUsersStr = "Agents Transactions";

            String usr = Session["UserID"].ToString();

            var getDevice = userCustom.GetAssignDevice(usr);
            //var pvAgents = (from pv in db.pvjournals
            //               join d in db.Devices
            //                   on pv.device equals d.DeviceID
            //                join s in db.states
            //                    on d.DeviceID equals s.deviceid
            //               select new
            //               {
            //                   id= pv.id,
            //                   DeviceID = d.DeviceID,
            //                   IP = d.IP,
            //                   Port = "9992",
            //                   DeviceState = s.devicestate,
            //                   ConnectedGateway = ""
            //               }).ToList().Distinct();

            var pvAgents = (from pv in db.states
                            join d in db.Devices
                                on pv.deviceid equals d.DeviceID
                            select new
                            {
                                //id = pv.id,
                                DeviceID = d.DeviceID,
                                IP = d.IP,
                                Port = "9992",
                                DeviceState = pv.connected,
                                ConnectedGateway = ""
                            }).ToList().Distinct();


            pvAgents = pvAgents.Where(x => getDevice.Any(b => x.DeviceID.Contains(b.DeviceID))).ToList();
            List<object[]> _pvAgentsList = new List<object[]>();
            foreach (var pvAgent in pvAgents)
            {
                Object[] obj = new object[]
                {
                    //pvAgent.id,
                    pvAgent.DeviceID  ?? "" as object,
                    pvAgent.IP  ?? "" as object,
                    pvAgent.Port ?? "" as object,
                    //pvAgent.DeviceState != null && pvAgent.DeviceState != 8192 
                    pvAgent.DeviceState == true
                        ? "Connected": "Disconnected" ,
                    pvAgent.ConnectedGateway ?? "" as object
                };
                _pvAgentsList.Add(obj);
            }

            ViewBag.pvAgents = _pvAgentsList;

            return View();
        }

        public string SendEmail(string to, string mt_subject, string mailtext, int cat, List<string> table = null, bool isAll = true, int subcat = 0)
        {
            EmailUtil AccountDetails = GetMailAccount();
            string mailfrom = AccountDetails.EmailAddress;
            MailMessage mailMessage = new MailMessage(mailfrom, to);
            mailMessage.Subject = mt_subject;
            mailMessage.Body = mailtext;

            try
            {
                GenerateEmailBody(cat, table, isAll, out string emailBody, subcat);
                mailMessage.Body += emailBody;
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = true; //true;
                smtpClient.Host = AccountDetails.Host; //"smtp.gmail.com";
                smtpClient.Port = Convert.ToInt32(AccountDetails.Port); //587;
                mailMessage.IsBodyHtml = true;

                smtpClient.Credentials = new System.Net.NetworkCredential()
                {
                    UserName = mailfrom,
                    Password = AccountDetails.Password //"Te$t123email"
                };

                smtpClient.Send(mailMessage);
                String Response = "Email has been sent to " + to;
                activitylog.Info(Session["UserID"].ToString() + ": Email sent to " + to);
                return Response;
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + "Error: " + ex);
                String Response = "Delivery has failed to " + to;
                return Response;
            }
        }

        public void ApplyFilter(int cat,
          string lastLoginDateFrom,
          string lastLoginDateTo,
          string operationType,
          string deviceId,
          string action,
          string user,
          string result,
          int? commands = 0,
          int subcat = 0)
        {
            List<object[]> list = new List<object[]>();

            #region SwitchCase 
            switch (cat)
            {
                case 1:
                    {
                        list = LoadPvUsers();
                        break;
                    }
                case 2:
                    {
                        list = LoadPvAgents();
                        break;
                    }
                case 3:
                    {
                        string query = "select * from pvjournal where";
                        #region FilterationOnList
                        if (!string.IsNullOrEmpty(lastLoginDateFrom))
                        {
                            DateTime dateTimeFrom = Convert.ToDateTime(lastLoginDateFrom);
                            query = query + " and serverdatetime >= '" + dateTimeFrom + "' ";
                        }
                        if (!string.IsNullOrEmpty(lastLoginDateTo))
                        {
                            DateTime dateTimeTo = Convert.ToDateTime(lastLoginDateTo);
                            TimeSpan ts = new TimeSpan(23, 59, 59);
                            dateTimeTo = dateTimeTo.Date + ts;
                            query = query + " and serverdatetime <= '" + dateTimeTo + "' ";
                        }
                        if (!string.IsNullOrEmpty(operationType))
                        {
                            query = query + " and Operation_Type = '" + operationType + "' ";
                        }
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            query = query + " and device = '" + deviceId + "' ";
                        }
                        if (!string.IsNullOrEmpty(action))
                        {
                            if (action == "All")
                            {
                                query = query + " and adminaction in (select distinct(adminaction) from pvjournal) ";
                            }
                            else
                            {
                                query = query + " and adminaction = '" + action + "' ";
                            }
                        }
                        if (!string.IsNullOrEmpty(user))
                        {
                            query = query + " and desktopuser = '" + user + "' ";
                        }
                        if (!string.IsNullOrEmpty(result))
                        {
                            query = query + " and vardata like '%" + result + "%' ";
                        }
                        if (commands != 0)
                        {
                            query = query + " and command = " + commands + " ";
                        }

                        #endregion


                        query = query.Trim();
                        if (query == "select * from pvjournal where")
                        {
                            query = "select * from pvjournal";
                        }
                        query = query.Replace("where and", "where");

                        list = LoadPvAllN(query);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            #endregion

            #region FilterationOnList
            if (cat != 3)
            {
                if (!string.IsNullOrEmpty(lastLoginDateFrom))
                {
                    DateTime dateTimeFrom = Convert.ToDateTime(lastLoginDateFrom);
                    switch (cat)
                    {
                        case 1:
                            list = list.Where(x => Convert.ToDateTime(x[1].ToString()) >= dateTimeFrom).ToList();
                            break;
                        case 2:
                            list = list.Where(x => Convert.ToDateTime(x[6].ToString()) >= dateTimeFrom).ToList();
                            break;
                        case 3:
                            list = list.Where(x => Convert.ToDateTime(x[6].ToString()) >= dateTimeFrom).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(lastLoginDateTo))
                {
                    DateTime dateTimeTo = Convert.ToDateTime(lastLoginDateTo);
                    TimeSpan ts = new TimeSpan(23, 59, 59);
                    dateTimeTo = dateTimeTo.Date + ts;
                    switch (cat)
                    {
                        case 1:
                            list = list.Where(x => Convert.ToDateTime(x[1].ToString()) <= dateTimeTo).ToList();
                            break;
                        case 2:
                            list = list.Where(x => Convert.ToDateTime(x[6].ToString()) <= dateTimeTo).ToList();
                            break;
                        case 3:
                            list = list.Where(x => Convert.ToDateTime(x[6].ToString()) <= dateTimeTo).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(operationType))
                {
                    switch (cat)
                    {
                        case 1:
                            list = list.Where(x => x[2].ToString().Trim().Equals(operationType)).ToList();
                            break;
                        case 2:
                            list = list.Where(x => x[3].ToString().Trim().Equals(operationType)).ToList();
                            break;
                        case 3:
                            list = list.Where(x => x[7].ToString().Trim().Equals(operationType)).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(deviceId))
                {
                    switch (cat)
                    {
                        case 1:
                            list = list.Where(x => x[0].ToString().Trim().Equals(deviceId)).ToList();
                            break;
                        case 2:
                            list = list.Where(x => x[0].ToString().Trim().Equals(deviceId)).ToList();
                            break;
                        case 3:
                            list = list.Where(x => x[1].ToString().Trim().Equals(deviceId)).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(action))
                {
                    switch (cat)
                    {
                        case 1:
                            if (action == "127.0.0.1")
                                action = "localhost";
                            list = list.Where(x => x[3].ToString().Trim().Equals(action)).ToList();
                            break;
                        case 2:
                            list = list.Where(x => x[3].ToString().Trim().Equals(action)).ToList();
                            break;
                        case 3:
                            if (action == "All")
                            {
                                this.LoadActions();
                                List<string> lst = new List<string>();

                                foreach (var item in ViewBag.pvActions)
                                {
                                    lst.Add(item);
                                }
                                //ViewBag.pvActions
                                //ViewBag.pvActions;

                                list = list.Where(r => ((IEnumerable<object>)lst).Contains<object>(r[3])).ToList();
                                break;
                            }
                            list = list.Where(x => x[3].ToString().Trim().Equals(action)).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(user))
                {
                    switch (cat)
                    {
                        case 1:
                            if (user == "127.0.0.1")
                                user = "localhost";
                            list = list.Where(x => x[4].ToString().Trim().Equals(user)).ToList();
                            break;
                        case 2:
                            list = list.Where(x => x[1].ToString().Trim().Equals(user)).ToList();
                            break;
                        case 3:
                            list = list.Where(x => x[4].ToString().Trim().Equals(user)).ToList();
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(result))
                {
                    list = list.Where(x => x[5].ToString().Trim().Equals(result)).ToList();
                }
                if (commands != 0)
                {
                    list = list.Where(x => Convert.ToInt32(x[2]) == commands).ToList();
                }
            }
            #endregion

            List<object[]> _list = new List<object[]>();

            #region SelectResult

            switch (cat)
            {
                case 1:
                    {
                        //foreach (object[] l in list)
                        //{
                        //    var id = l[0];
                        //    var userid = l[8];
                        //    var lastLogin = l[9];
                        //    var isOnline = l[10];
                        //    var serverTerminalIP = l[11];
                        //    var IP = l[12];
                        //    object[] obj = new object[]
                        //    {
                        //        id,
                        //        userid,
                        //        lastLogin,
                        //        isOnline,
                        //        IP,
                        //        serverTerminalIP
                        //    };
                        //    _list.Add(obj);
                        //}
                        _list = list;
                        break;
                    }
                case 2:
                    {

                        //foreach (object[] l in list)
                        //{
                        //    var id = l[0];
                        //    var deviceid = l[8];
                        //    var IP = l[9];
                        //    var Port = l[10];
                        //    var DeviceState = l[11];
                        //    var ConnectedGateway = l[12];
                        //    object[] obj = new object[]
                        //    {
                        //        id,
                        //        deviceid,
                        //        IP,
                        //        Port,
                        //        DeviceState,
                        //        ConnectedGateway
                        //    };
                        //    _list.Add(obj);
                        //}
                        _list = list;
                        break;
                    }
                case 3:
                    {
                        foreach (object[] l in list)
                        {
                            var id = l[0];
                            var serverdatetime = l[6];
                            var issuertype = l[8];
                            var pvuser = l[9];
                            var deviceid = l[1];
                            var command = l[2];
                            var vardata = l[10];
                            var errorcode = l[11];
                            var desktopuser = l[12];
                            object[] obj = new object[]
                            {
                                id,
                                serverdatetime,
                                issuertype,
                                pvuser,
                                deviceId,
                                command,
                                vardata,
                                errorcode,
                                desktopuser
                            };
                            _list.Add(obj);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            #endregion

            Session["list"] = _list;

            if (subcat > 0)
            {
                Session["subcat"] = subcat;
            }

        }

        public string ViewDetails(string id)
        {
            try
            {
                var pvAll = LoadPvAll(1).Where(x => x[0].ToString().Equals(id.Trim()));//to get selected row
                string serverdatetime = pvAll.ElementAt(0)[6].ToString();
                string deviceId = pvAll.ElementAt(0)[1].ToString();

                pvAll = LoadPvAll(1);//to get all;

                if (!string.IsNullOrEmpty(serverdatetime))
                {
                    DateTime dateTimeFrom = Convert.ToDateTime(serverdatetime);
                    pvAll = pvAll.Where(x => Convert.ToDateTime(x[6].ToString()) == dateTimeFrom).ToList();
                }
                if (!string.IsNullOrEmpty(deviceId))
                {
                    pvAll = pvAll.Where(x => x[1].ToString().Trim().Equals(deviceId.Trim())).ToList();
                }

                pvAll = pvAll.OrderBy(x => x[6]).ToList().Take(2).ToList();

                return JsonConvert.SerializeObject(pvAll);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject("error occured " + ex.Message);
            }
        }


        #endregion

        #region Send Email 
        public EmailUtil GetMailAccount()
        {
            EmailUtil MailAccount = new EmailUtil();
            var path = System.Configuration.ConfigurationManager.AppSettings["ProblemManagerConf"].ToString();
            //var path = @"D:\IMS Server\ProblemManager\ProblemManager.exe.config";
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var configMap = new ExeConfigurationFileMap { ExeConfigFilename = path };
                    var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                    // get the sectionGroup!
                    var userSectionGroup = config.GetSectionGroup("applicationSettings");
                    foreach (var userSection in userSectionGroup.Sections)
                    {
                        // check for a ClientSettingSection
                        if (userSection is ClientSettingsSection)
                        {
                            // cast from ConfigSection to a more specialized type
                            var clientSettingSect = (ClientSettingsSection)userSection;
                            foreach (SettingElement clientSetting in clientSettingSect.Settings)
                            {
                                switch (clientSetting.Name)
                                {
                                    case "SMTPClient":
                                        MailAccount.Host = clientSetting.Value.ValueXml.InnerText;
                                        break;

                                    case "SMTPPort":
                                        MailAccount.Port = clientSetting.Value.ValueXml.InnerText;
                                        break;

                                    case "MailFrom":
                                        MailAccount.EmailAddress = clientSetting.Value.ValueXml.InnerText;
                                        break;

                                    case "MailPassword":
                                        MailAccount.Password = clientSetting.Value.ValueXml.InnerText;
                                        break;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + "Error: " + ex);
            }
            return MailAccount;
        }
        #endregion

        #region private

        private void LoadPvJournalAll()
        {

            var pvAll = db.pvjournals.ToList().Take(500);
            List<object[]> _pvAllList = new List<object[]>();
            foreach (var pv in pvAll)
            {
                Object[] obj = new object[]
                {
                    pv.id,
                    pv.serverdatetime,//6
                    pv.issuertype,
                    pv.pvuser ?? "" as object,
                    pv.device ?? "" as object,//1
                    pv.command,//2
                    pv.vardata ?? "" as object,//5
                    pv.errorcode,
                    pv.desktopuser ?? "" as object
                };
                _pvAllList.Add(obj);
            }

            ViewBag.pvjournal = _pvAllList;
            ViewBag.HtmlStr = "PV Journal All Transactions";
        }

        private void LoadPvJournalByRole()
        {
            var pvAll = db.pvjournals.Where(x => x.issuertype == 4).ToList().Take(500);

            List<object[]> _pvAllList = new List<object[]>();
            foreach (var pv in pvAll)
            {
                Object[] obj = new object[]
                {
                    pv.id,
                    pv.serverdatetime,//6
                    pv.issuertype,
                    pv.pvuser ?? "" as object,
                    pv.device ?? "" as object,//1
                    pv.command,//2
                    pv.vardata ?? "" as object,//5
                    pv.errorcode,
                    pv.desktopuser ?? "" as object
                };
                _pvAllList.Add(obj);
            }

            ViewBag.pvRole = _pvAllList;
            ViewBag.HtmlRoleStr = "PV Journal Role Transactions";
        }

        private void LoadPvJournalByJob()
        {
            var pvAll = db.pvjournals.Where(x => x.issuertype == 3).ToList().Take(500);
            List<object[]> _pvAllList = new List<object[]>();
            foreach (var pv in pvAll)
            {
                Object[] obj = new object[]
                {
                    pv.id,
                    pv.serverdatetime,//6
                    pv.issuertype,
                    pv.pvuser ?? "" as object,
                    pv.device ?? "" as object,//1
                    pv.command,//2
                    pv.vardata ?? "" as object,//5
                    pv.errorcode,
                    pv.desktopuser ?? "" as object
                };
                _pvAllList.Add(obj);
            }

            ViewBag.pvJob = _pvAllList;
            ViewBag.HtmlJobStr = "PV Journal All Transactions";
        }

        private void LoadCommands()
        {

            //var sss = db.Sheet1_.Where(x => x.Active == true).Distinct();

            List<Sheet1_> sheet1List = new List<Sheet1_>();
            DataSet dataSet = new DataSet();
            //using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["SurveilAIEntities"].ConnectionString))
            //{
            //    using (SqlCommand selectCommand = new SqlCommand("select distinct F1,F2,F3 from Sheet1$ where Active = 1"))
            //    {
            //        selectCommand.Connection = sqlConnection;
            //        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand))
            //            sqlDataAdapter.Fill(dataSet);
            //    }
            //}
            foreach (DataTable table in (InternalDataCollectionBase)dataSet.Tables)
            {
                foreach (DataRow row in (InternalDataCollectionBase)table.Rows)
                {
                    Sheet1_ sheet1 = new Sheet1_()
                    {
                        F1 = new double?(Convert.ToDouble(row["F1"])),
                        F2 = new double?(Convert.ToDouble(row["F2"])),
                        F3 = row["F3"].ToString()
                    };
                    sheet1List.Add(sheet1);
                }
            }



            //ViewBag.pvCommands = db.Sheet1_.Where(x => x.Active == true).Distinct();
            ViewBag.pvCommands = sheet1List;
        }

        private void LoadActions()
        {
            //var aaaa = db.pvjournals.Select(x => x.adminaction).Distinct().ToList();
            ViewBag.pvActions = (from pv in db.pvjournals select pv.adminaction).Distinct().ToList().Take(500); ;


        }

        private void LoadOpertaionType()
        {
            ViewBag.pvOperationType = (from pv in db.pvjournals select pv.Operation_Type).Distinct().ToList().Take(500); ;
        }

        private void GenerateEmailBody(int cat, List<string> table, bool isAll, out string emailBody, int subcat)
        {

            switch (cat)
            {
                case 1:
                    {
                        GenerateUserEmailBody(table, isAll, out emailBody);
                        break;
                    }
                case 2:
                    {
                        GenerateAgentEmailBody(table, isAll, out emailBody);
                        break;
                    }
                case 3:
                    {
                        GenerateAllEmailBody(table, isAll, out emailBody, subcat);
                        break;
                    }
                default:
                    {
                        emailBody = "";
                        break;
                    }
            }
        }

        private void GenerateUserEmailBody(List<string> table, bool isAll, out string emailBody)
        {
            List<usersession> pvUsers = new List<usersession>();
            if (!isAll)
            {
                string id = table[0].ToString();
                pvUsers = db.usersessions.Where(x => x.extuserid == id).ToList();
            }
            else
            {
                pvUsers = db.usersessions.ToList();
            }
            List<object[]> _pvUsersList = new List<object[]>();
            foreach (var usersessions in pvUsers)
            {
                string isOnline = usersessions.sessionflag == 0 ? "Offline" : "Online";

                Object[] obj = new object[]
                {
                    usersessions.extuserid  ?? "" as object,
                    usersessions.logintime  ?? "" as object,
                    isOnline ?? "" as object,
                    usersessions.ipaddress ?? "" as object,
                    usersessions.terminalserver ?? "" as object,
                   };
                _pvUsersList.Add(obj);
            }


            emailBody = "<table><thead><th>UserId</th><th>LastLogin</th><th>IsOnline</th><th>IP</th><th>ServerTerminalIP</th></thead>";
            emailBody += "<tbody>";

            foreach (var item in _pvUsersList)
            {
                emailBody += "<tr>";
                emailBody += "<td>" + @item[0].ToString() + "</td>";
                emailBody += "<td>" + @item[1].ToString() + "</td>";
                emailBody += "<td>" + @item[2].ToString() + "</td>";
                emailBody += "<td>" + @item[3].ToString() + "</td>";
                emailBody += "<td>" + @item[4].ToString() + "</td>";
                emailBody += "</tr>";
            }
            emailBody += "</tbody>";
        }

        private void GenerateAgentEmailBody(List<string> table, bool isAll, out string emailBody)
        {
            var pvAgents = (from pv in db.states
                            join d in db.Devices
                                on pv.deviceid equals d.DeviceID
                            select new
                            {
                                //id = pv.id,
                                DeviceID = d.DeviceID,
                                IP = d.IP,
                                Port = "9992",
                                DeviceState = pv.connected,
                                ConnectedGateway = ""
                            }).ToList().Distinct();


            //pvAgents = (from pv in db.pvjournals
            //    join d in db.Devices
            //        on pv.device equals d.DeviceID
            //    join s in db.states
            //        on d.DeviceID equals s.deviceid
            //    select new
            //    {
            //        id = pv.id,
            //        DeviceID = d.DeviceID,
            //        IP = d.IP,
            //        Port = "9992",
            //        DeviceState = s.devicestate,
            //        ConnectedGateway = ""
            //    }).ToList();

            if (!isAll)
            {
                pvAgents = pvAgents.Where(u => table.Any(x => x == u.DeviceID)).ToList();
            }

            emailBody =
                "<table><thead><th>DeviceID</th><th>IP</th><th>Port</th><th>DeviceState</th><th>ConnectedGateway</th></thead>";
            emailBody += "<tbody>";
            try
            {
                foreach (var agent in pvAgents)
                {
                    emailBody += "<tr>";
                    emailBody += "<td>" + agent.DeviceID + "</td>";
                    emailBody += "<td>" + agent.IP + "</td>";
                    emailBody += "<td>" + agent.Port + "</td>";
                    emailBody += "<td>" + (agent.DeviceState == true ? "Connected" : "Disconnected") + "</td>";
                    emailBody += "<td>" + agent.ConnectedGateway + "</td>";
                    emailBody += "</tr>";
                }
            }
            catch (Exception ex)
            {


            }

            emailBody += "</tbody>";

        }

        private void GenerateAllEmailBody(List<string> table, bool isAll, out string emailBody, int subcat)
        {
            var pvAll = db.pvjournals.ToList().Take(500);

            if (!isAll)
            {
                string ids = "";
                foreach (var item in table)
                {
                    ids = ids + "," + item;
                }
                ids = ids.Remove(0, 1);
                pvAll = db.pvjournals.SqlQuery("select * from pvjournal where id in (" + ids + ")");
            }
            else
            {
                if (subcat == 4)//role
                {
                    pvAll = pvAll.Where(x => x.issuertype == 4).ToList();
                }
                else if (subcat == 3)//job
                {
                    pvAll = pvAll.Where(x => x.issuertype == 3).ToList();
                }
                else//all
                {

                }
            }

            emailBody =
                "<table><thead><th>ServerDateTime</th><th>IssuerType</th><th>PVUser</th><th>Device</th><th>Command</th><th>Var Data</th><th>Error Code</th><th>Desktop User</th></thead>";
            emailBody += "<tbody>";

            foreach (var all in pvAll)
            {
                emailBody += "<tr>";
                emailBody += "<td>" + all.serverdatetime + "</td>";
                emailBody += "<td>" + all.issuertype + "</td>";
                emailBody += "<td>" + all.pvuser + "</td>";
                emailBody += "<td>" + all.device + "</td>";
                emailBody += "<td>" + all.command + "</td>";
                emailBody += "<td>" + all.vardata + "</td>";
                emailBody += "<td>" + all.errorcode + "</td>";
                emailBody += "<td>" + all.desktopuser + "</td>";
                emailBody += "</tr>";
            }
            emailBody += "</tbody>";

        }

        private List<object[]> LoadPvUsers()
        {
            //var pvUsers = (from pv in db.pvjournals
            //    join u in db.Users
            //        on pv.desktopuser equals u.UserID
            //    join d in db.Devices
            //        on pv.device equals d.DeviceID
            //    select new
            //    {
            //        id = pv.id,
            //        UserID = u.UserID,
            //        LastLogin = u.LastLogin,
            //        IsOnline = u.IsOnline,
            //        IP = d.IP,
            //        Device = pv.device,
            //        Command = pv.command,
            //        AdminAction = pv.adminaction,
            //        DesktopUser = pv.desktopuser,
            //        Vardata = pv.vardata,
            //        serverDateTime = pv.serverdatetime,
            //        OperationType = pv.Operation_Type,
            //        ServerTerminalIP = ""
            //    }).ToList();



            //foreach (var pvUser in pvUsers)
            //{
            //    Object[] obj = new object[]
            //    {
            //        pvUser.id,
            //        pvUser.Device ?? "" as object,//1
            //        pvUser.Command,//2
            //        pvUser.AdminAction ?? "" as object,//3
            //        pvUser.DesktopUser ?? "" as object,//4
            //        pvUser.Vardata ?? "" as object,//5
            //        pvUser.serverDateTime,//6
            //        pvUser.OperationType ?? "" as object,//7
            //        pvUser.UserID  ?? "" as object,//8
            //        pvUser.LastLogin  ?? "" as object,//9
            //        pvUser.IsOnline!= null && pvUser.IsOnline != true ? "Online" : "Offline",//10
            //        pvUser.ServerTerminalIP ?? "" as object,//11
            //        pvUser.IP ?? "" as object//12
            //    };
            //    _pvUsersList.Add(obj);
            //}
            List<object[]> _pvUsersList = new List<object[]>();
            foreach (var usersessions in db.usersessions)
            {
                string isOnline = usersessions.sessionflag == 0 ? "Offline" : "Online";

                Object[] obj = new object[]
                {
                    usersessions.extuserid  ?? "" as object,
                    usersessions.logintime  ?? "" as object,
                    isOnline ?? "" as object,
                    usersessions.ipaddress ?? "" as object,
                    usersessions.terminalserver ?? "" as object,
                   };
                _pvUsersList.Add(obj);
            }
            return _pvUsersList;
        }

        private List<object[]> LoadPvAgents()
        {
            string usr = Session["UserID"].ToString();
            var getDevice = userCustom.GetAssignDevice(usr);
            var pvAgents = (from pv in db.states
                            join d in db.Devices
                                on pv.deviceid equals d.DeviceID
                            select new
                            {
                                //id = pv.id,
                                DeviceID = d.DeviceID,
                                IP = d.IP,
                                Port = "9992",
                                DeviceState = pv.connected,
                                ConnectedGateway = ""
                            }).ToList().Distinct();


            pvAgents = pvAgents.Where(x => getDevice.Any(b => x.DeviceID.Contains(b.DeviceID))).ToList();
            List<object[]> _pvAgentsList = new List<object[]>();
            foreach (var pvAgent in pvAgents)
            {
                Object[] obj = new object[]
                {
                    //pvAgent.id,
                    pvAgent.DeviceID  ?? "" as object,
                    pvAgent.IP  ?? "" as object,
                    pvAgent.Port ?? "" as object,
                    //pvAgent.DeviceState != null && pvAgent.DeviceState != 8192 
                    pvAgent.DeviceState == true
                        ? "Connected": "Disconnected" ,
                    pvAgent.ConnectedGateway ?? "" as object
                };
                _pvAgentsList.Add(obj);
            }
            //var pvAgents = (from pv in db.pvjournals
            //                join d in db.Devices
            //                    on pv.device equals d.DeviceID
            //                join s in db.states
            //                    on d.DeviceID equals s.deviceid
            //                select new
            //                {
            //                    id = pv.id,
            //                    DeviceID = d.DeviceID,
            //                    IP = d.IP,
            //                    Port = "9992",
            //                    DeviceState = s.devicestate,
            //                    ConnectedGateway = "",
            //                    Device = pv.device,
            //                    Command = pv.command,
            //                    AdminAction = pv.adminaction,
            //                    DesktopUser = pv.desktopuser,
            //                    Vardata = pv.vardata,
            //                    serverDateTime = pv.serverdatetime,
            //                    OperationType = pv.Operation_Type
            //                }).ToList();


            //List<object[]> _pvAgentsList = new List<object[]>();
            //foreach (var pvAgent in pvAgents)
            //{
            //    Object[] obj = new object[]
            //    {
            //        pvAgent.id,
            //        pvAgent.Device ?? "" as object,//1
            //        pvAgent.Command,//2
            //        pvAgent.AdminAction ?? "" as object,//3
            //        pvAgent.DesktopUser ?? "" as object,//4
            //        pvAgent.Vardata ?? "" as object,//5
            //        pvAgent.serverDateTime,//6
            //        pvAgent.OperationType ?? "" as object,//7
            //        pvAgent.DeviceID  ?? "" as object,//8
            //        pvAgent.IP  ?? "" as object,//9
            //        pvAgent.Port ?? "" as object,//10
            //        pvAgent.DeviceState != null && pvAgent.DeviceState != 8192
            //            ? "Connected": "Disconnect" ,//11
            //        pvAgent.ConnectedGateway ?? "" as object//12
            //    };
            //    _pvAgentsList.Add(obj);
            //}

            return _pvAgentsList;
        }

        private List<object[]> LoadPvAll(int subcat)
        {

            //List<pvjournal>  pvj = db.pvjournals.ToList();

            var pvAll = db.pvjournals.ToList();

            if (subcat == 3)//job
            {
                pvAll = pvAll.Where(x => x.issuertype == 3).ToList();
            }
            else if (subcat == 4)//role
            {
                pvAll = pvAll.Where(x => x.issuertype == 4).ToList();
            }

            List<object[]> _pvAgentsList = new List<object[]>();
            foreach (var pv in pvAll)
            {
                Object[] obj = new object[]
                {
                    pv.id,
                    pv.device ?? "" as object,//1
                    pv.command,//2
                    pv.adminaction ?? "" as object,//3
                    pv.desktopuser ?? "" as object,//4
                    pv.vardata ?? "" as object,//5
                    pv.serverdatetime,//6
                    pv.Operation_Type ?? "" as object,//7
                    pv.issuertype,
                    pv.pvuser ?? "" as object,
                    pv.vardata ?? "" as object,
                    pv.errorcode,
                    pv.desktopuser ?? "" as object
                };
                _pvAgentsList.Add(obj);
            }

            return _pvAgentsList;
        }


        private List<object[]> LoadPvAllN(string query)
        {
            var pvAll = db.pvjournals.SqlQuery(query);

            List<object[]> _pvAgentsList = new List<object[]>();
            foreach (var pv in pvAll)
            {
                Object[] obj = new object[]
                {
                    pv.id,
                    pv.device ?? "" as object,//1
                    pv.command,//2
                    pv.adminaction ?? "" as object,//3
                    pv.desktopuser ?? "" as object,//4
                    pv.vardata ?? "" as object,//5
                    pv.serverdatetime,//6
                    pv.Operation_Type ?? "" as object,//7
                    pv.issuertype,
                    pv.pvuser ?? "" as object,
                    pv.vardata ?? "" as object,
                    pv.errorcode,
                    pv.desktopuser ?? "" as object
                };
                _pvAgentsList.Add(obj);
            }

            return _pvAgentsList;
        }

        #endregion
    }
}