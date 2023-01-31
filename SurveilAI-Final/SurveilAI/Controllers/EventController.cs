using NLog;
using Rotativa;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SurveilAI.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class EventController : Controller
    {

        SurveilAIEntities db = new SurveilAIEntities();
        SurveilAIEntities dbPro = new SurveilAIEntities();
        Device obj = new Device();
        @event evt = new @event();
        UserCustom UserCustom = new UserCustom();
        DateTime DF = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " 00:00:00.000");
        DateTime DT = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " 23:59:59.999");
        DateTime? DTime = null;
        String radioCheck = null;


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

        ILogger userlog = LogManager.GetLogger("user");
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger errorlog = LogManager.GetLogger("error");

        public ActionResult Event()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("41");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to monitoring");
                List<Tuple<String, String>> data = new List<Tuple<string, string>>();
                String usr = Session["UserID"].ToString();

                var getDevice = UserCustom.GetAssignDevice(usr);

                var ATMID = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).Single();
                var PP = db.Devices.Select(a => new { a.DeviceID, a.BranchName });
                if (ATMID != "" && ATMID != null && ATMID != "|")
                {
                    //var atmsFull = ATMID.Split('%');
                    //foreach (var a in atmsFull)
                    //{
                    //    var id = a.Split('|')[0];
                    //    var name = a.Split('|')[1];
                    //    data.Add(new Tuple<string, string>(id, name));
                    //}

                    //var AllDev = db.Devices.ToList();
                    //AllDev.RemoveAll(a => !data.Any(d => d.Item1 == a.DeviceID && d.Item2 == a.BranchName));
                    evt.dev = getDevice;
                    evt.hrar = db.Hierarchies.ToList();

                    string User4View = Session["UserID"].ToString();

                    var Piview = db.consoleviews.Where(a => a.extuserid.Contains(User4View) && a.viewtype.Equals(5)).Select(a => a.viewname).ToList();
                    var Alview = db.consoleviews.Where(a => a.extuserid.Contains(User4View) && a.viewtype.Equals(1)).Select(a => a.viewname).ToList();
                    var Chessview = db.consoleviews.Where(a => a.extuserid.Contains(User4View) && a.viewtype.Equals(6)).Select(a => a.viewname).ToList(); //Chessboard view
                    evt.Alview = Alview;
                    evt.Piview = Piview;
                    evt.Chessview = Chessview;
                }
                else
                {
                    return RedirectToAction("Monitoring", "Event");
                    //                    return Content(@"<body>
                    //                       <script type='text/javascript'>
                    //                document.documentElement.innerHTML ='No Devices Assigned to this user';        
                    //setTimeout(function(){  history.go(-1); }, 3000);




                    //</script></body> ");
                }

                evt.Hiertree = HierarchyList.GenrateTree(evt.hrar);

                return View(evt);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return View();
            }

        }

        public ActionResult Monitoring()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("41");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return View();
        }

        public ActionResult EventFromAlert(string id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("41");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            try
            {

                List<Tuple<String, String>> data = new List<Tuple<string, string>>();
                String usr = Session["UserID"].ToString();

                var getDevice = UserCustom.GetAssignDevice(usr);

                var ATMID = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).Single();
                var PP = db.Devices.Select(a => new { a.DeviceID, a.BranchName });
                if (ATMID != "" && ATMID != null && ATMID != "|")
                {
                    //var atmsFull = ATMID.Split('%');
                    //foreach (var a in atmsFull)
                    //{
                    //    var id = a.Split('|')[0];
                    //    var name = a.Split('|')[1];
                    //    data.Add(new Tuple<string, string>(id, name));
                    //}

                    //var AllDev = db.Devices.ToList();
                    //AllDev.RemoveAll(a => !data.Any(d => d.Item1 == a.DeviceID && d.Item2 == a.BranchName));
                    evt.dev = getDevice;
                    evt.hrar = db.Hierarchies.ToList();

                    string User4View = Session["UserID"].ToString();

                    var Piview = db.consoleviews.Where(a => a.extuserid.Contains(User4View) && a.viewtype.Equals(5)).Select(a => a.viewname).ToList();
                    var Alview = db.consoleviews.Where(a => a.extuserid.Contains(User4View) && a.viewtype.Equals(1)).Select(a => a.viewname).ToList();

                    evt.Alview = Alview;
                    evt.Piview = Piview;
                    ViewData["SelectedDevice"] = id;
                }
                else
                {
                    return Content(@"<body>
					   <script type='text/javascript'>
						 alert('No Device Assigned To This User');
					   </script>
					 </body> ");
                }
                return View("Event", evt);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeviceData(string events, string jobresult, string basedata, string image, string counters,
            string screenshot, string Inventory, string GetJournal, string PcState, string Download, string Upload,
            string JobRun, string DeviceFail, string ViewFile, @event collection, string dntUpdate, string StartProgram,
            string Services, string KillProcess, string RestartMachine, string ServiceControl, string AgentUpdater, string AgentFile)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("42");
                var ret1 = Check("43");
                var ret2 = Check("44");
                if (ret == false && ret1 == false && ret2 == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            String DeviceID = collection.deviceid;
            ViewData["DeviD"] = DeviceID;
            TempData["DeviD"] = DeviceID;
            try
            {

                if (!string.IsNullOrEmpty(events))
                {
                    activitylog.Info(Session["UserID"] + " navigate to events");
                    Int64 leave = 0;
                    Int64 next = 100;
                    ViewBag.Leave = leave.ToString();
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    ViewBag.CRadio = true;
                    ViewBag.URadio = false;
                    ViewBag.SRadio = false;
                    ViewBag.Date = DateTime.Now.ToShortDateString();
                    ViewBag.Date = DateTime.Now.ToShortDateString();
                    //GetSql(DeviceID, leave, next, DTime, radioCheck);
                    GetSql(DeviceID, leave, next, DTime, radioCheck, "", "");
                    ViewBag.AllRadio = true;
                    ViewBag.SuRadio = false;
                    ViewBag.FRadio = false;

                    return View("ShowEvent", evt);
                }
                else if (!string.IsNullOrEmpty(jobresult))
                {
                    activitylog.Info(Session["UserID"] + " navigate to job result of device : " + DeviceID);

                    Int64 leave = 0;
                    Int64 next = 100;
                    ViewBag.Leave = leave.ToString();
                    ViewBag.CRadio = true;
                    ViewBag.URadio = false;
                    ViewBag.SRadio = false;
                    ViewBag.Date = DateTime.Now.ToShortDateString();
                    ViewBag.Date = DateTime.Now.ToShortDateString();
                    GetSqlJr(DeviceID, leave, next, DTime, radioCheck, "All");
                    var job1 = db.jobs.Select(x => x.jobid).Distinct();
                    ViewBag.job = job1;
                    ViewBag.AllRadio = true;
                    ViewBag.SuRadio = false;
                    ViewBag.FRadio = false;
                    ViewBag.RRadio = false;

                    return View("JobResult", evt);
                }
                else if (!string.IsNullOrEmpty(image))
                {
                    activitylog.Info(Session["UserID"] + " navigate to Images Tab");
                    return RedirectToAction("ImageFromMonitoring", "Image", new { id = DeviceID });
                }
                else if (!string.IsNullOrEmpty(basedata))
                {
                    activitylog.Info(Session["UserID"] + " navigate to basedata tab");
                    DataSet ds = new DataSet();
                    string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select distinct id, (select value from basedata where reference = 999904 and id = '" + DeviceID + "') as Profile, " +
                       "(select value from basedata where reference = 999988 and id = '" + DeviceID + "') as Timezone, " +
                       "(select value from basedata where reference = 999905 and id = '" + DeviceID + "') as URL, " +
                       "(select value from basedata where reference = 999998 and id = '" + DeviceID + "') as Location, " +
                       "(select value from basedata where reference = 999992 and id = '" + DeviceID + "') as Description, " +
                       "(select value from basedata where reference = 999997 and id = '" + DeviceID + "') as Street, " +
                       "(select value from basedata where reference = 999995 and id = '" + DeviceID + "') as City, " +
                       "(select value from basedata where reference = 999996 and id = '" + DeviceID + "') as Zip, " +
                       "(select value from basedata where reference = 999978 and id = '" + DeviceID + "') as State, " +
                       "(select value from basedata where reference = 999979 and id = '" + DeviceID + "') as Country, " +
                 "(select value from basedata where reference = 999915 and id = '" + DeviceID + "') as Longitude, " +
                 "(select value from basedata where reference = 999916 and id = '" + DeviceID + "') as Latitude, " +
                 "(select value from basedata where reference = 999912 and id = '" + DeviceID + "') as Vendor, " +
                 "(select value from basedata where reference = 999913 and id = '" + DeviceID + "') as DeviceModel, " +
                 "(select value from basedata where reference = 999914 and id = '" + DeviceID + "') as DeviceType, " +
                 "(select value from basedata where reference = 999977 and id = '" + DeviceID + "') as Branch, " +
                 "(select value from basedata where reference = 1 and id = '" + DeviceID + "') as BranchCode, " +
                 "(select value from basedata where reference = 999999 and id = '" + DeviceID + "') as Orginization, " +
                 "(select value from basedata where reference = 999994 and id = '" + DeviceID + "') as SoftwareVer, " +
                 "(select value from basedata where reference = 999993 and id = '" + DeviceID + "') as InstDate, " +
                 "(select value from basedata where reference = 999991 and id = '" + DeviceID + "') as SerialNo, " +
                 "(select value from basedata where reference = 999990 and id = '" + DeviceID + "') as System, " +
                 "(select value from basedata where reference = 999986 and id = '" + DeviceID + "') as AgentVer, " +
                 "(select value from basedata where reference = 999901 and id = '" + DeviceID + "') as Contact1, " +
                 "(select value from basedata where reference = 999902 and id = '" + DeviceID + "') as Contact2, " +
                 "(select value from basedata where reference = 999903 and id = '" + DeviceID + "') as Contact3 " +
                 "from basedata  where id = '" + DeviceID + "'";

                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds);
                            }
                        }
                    }
                    return View("BasedataView", ds);
                }
                else if (!string.IsNullOrEmpty(counters))
                {
                    activitylog.Info(Session["UserID"] + " navigate to counters");
                    ViewData["DeviD"] = collection.deviceid;
                    DataSet ds = new DataSet();
                    //DataSet ds1 = new DataSet();
                    string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select cs.postingdatetime,cs.deviceid,cs.containerid,cs.quantity,cs.cashin,cs.cashout,cs.reject,cc.currency,cc.denomination,cs.containerid from cashstock cs  join cashcontitem cc on cs.deviceid = cc.deviceid and cs.containerid = cc.containerid and cs.postingdatetime = cc.postingdatetime and cs.deviceid = '" + collection.deviceid + "'and cs.postingdatetime = (select MAX(postingdatetime) from cashstock where deviceid = '" + collection.deviceid + "')";
                        string query1 = "select Count(*)from v_card_cpture where deviceid='" + collection.deviceid + "'";
                        //string query = "select cs.postingdatetime,cs.deviceid,cs.containerid,cs.quantity,cs.cashin,cs.cashout,cs.reject,cc.currency,cc.denomination from cashstock cs  join cashcontitem cc on cs.deviceid = cc.deviceid and cs.containerid = cc.containerid and cs.postingdatetime = cc.postingdatetime and cs.deviceid = '0001'and cs.postingdatetime = (select max(postingdatetime) from cashstock where deviceid = '0001')";

                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds, "Table 1");
                            }
                        }
                        using (SqlCommand cmd = new SqlCommand(query1))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds, "Table 2");
                            }
                        }
                        if (ds.Tables[1].Rows.Count != 0)
                        {
                            TempData["RetainedCards"] = ds.Tables[1].Rows[0][0];
                        }
                        else
                        {
                            TempData["RetainedCards"] = "NULL";
                        }

                        if (ds.Tables[0].Rows.Count != 0)
                        {
                            TempData["Date"] = ds.Tables[0].Rows[0][0];
                        }
                        else
                        {
                            TempData["Date"] = "NULL";
                        }

                    }
                    return View("CashCounters", ds);
                }
                else if (!string.IsNullOrEmpty(screenshot))
                {
                    activitylog.Info(Session["UserID"] + " navigate to job screenshot");
                    TempData["Image"] = "NULL";
                    ViewData["DeviD"] = DeviceID;
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    int commandno = 353;
                    string screen = obj.DownloadScreenfile(DeviceID, commandno);
                    string[] seperator = screen.Split(',');
                    if (seperator[0] == "Success")
                    {
                        if (seperator.Length > 2)
                        {
                            TempData["Image"] = seperator[2];
                        }
                        else
                        {
                            TempData["ErrMsg"] = "Image not found";
                        }
                    }
                    else
                    {
                        errorlog.Error("Error Getting Screenshot " + screen);
                        TempData["ErrMsg"] = "Connection failed";

                    }
                    return View("ScreenShot");
                }
                else if (!string.IsNullOrEmpty(PcState))
                {
                    activitylog.Info(Session["UserID"] + " navigate to PC-state");
                    ViewData["DeviD"] = DeviceID;
                    return View("PcState");
                }
                else if (!string.IsNullOrEmpty(GetJournal))
                {
                    activitylog.Info(Session["UserID"] + " navigate to journal");
                    string path = "C:\\Journal";
                    TempData["check"] = "null";
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    var subDir = obj.GetDirFiles(DeviceID, path, 906);
                    if (subDir.Contains("Success,Please find attached all Directories & Files from path:"))
                    {
                        subDir = subDir.Substring(subDir.IndexOf("1%%") + 3);
                        string[] listDir = subDir.Split(new string[] { "\n", "," }, StringSplitOptions.None);
                        Array.Reverse(listDir);
                        ViewBag.RootDir = listDir;
                        TempData["check"] = "1";
                    }
                    else
                    {
                        errorlog.Error("Error Getting Directoris " + subDir);
                        ViewBag.RootDir = "Check connectivity"; ;

                    }

                    return View("GetJournal");
                }
                else if (!string.IsNullOrEmpty(Inventory))
                {
                    activitylog.Info(Session["UserID"] + " navigate to inventory");
                    return View("Inventory");
                }
                else if (!string.IsNullOrEmpty(Upload))
                {
                    activitylog.Info(Session["UserID"] + " navigate to download");
                    // string path = System.Configuration.ConfigurationManager.AppSettings["PFUserName"];
                    string path = WebConfigurationManager.AppSettings["Download"];
                    //string path = @"D:\IMS Server\Download\";
                    string[] folder = Directory.GetDirectories(path);
                    string[] files = Directory.GetFiles(path);
                    if (folder.Length != 0)
                    {
                        ViewBag.folder = folder;
                    }
                    else
                    {
                        ViewBag.folder = "";
                    }
                    if (files.Length != 0)
                    {
                        string[] authors = new string[files.Length];

                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo info = new FileInfo(files[i]);
                            authors[i] = files[i] + "*" + info.Length.ToString() + " KB";

                        }

                        ViewBag.files = authors;
                    }
                    else
                    {
                        ViewBag.files = "";
                    }
                    return View("Upload");
                }
                else if (!string.IsNullOrEmpty(Download))
                {
                    activitylog.Info(Session["UserID"] + " navigate to upload");
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    var dir = obj.GetDirectories(DeviceID, 904);
                    string[] seperator = dir.Split(',');
                    TempData["Check"] = "null";

                    //If API connection successfull
                    if (seperator[0] == "Success")
                    {
                        if (dir.Contains("Success,Please find attached all Directories: 1%%"))
                        {
                            dir = dir.Replace("Success,Please find attached all Directories: 1%%", "");
                            string[] seperator1 = dir.Split('|');
                            ViewBag.Drives = seperator1;
                            TempData["OkMsg"] = "Drive fetched";
                            TempData["Check"] = "1";

                        }
                        else
                        {
                            //Unable to fetch data
                            errorlog.Error("Error Getting Directories " + dir);
                            TempData["ErrMsg"] = "Failed retriving drive";
                        }
                    }
                    else
                    {
                        //Unable to connect API
                        errorlog.Error("Error Getting Directories " + dir);
                        TempData["ErrMsg"] = "API connection failed";
                    }
                    GetServerUploadDir();
                    return View("Download");
                }
                else if (!string.IsNullOrEmpty(JobRun))
                {
                    activitylog.Info(Session["UserID"] + " navigate to job execution on " + DeviceID);
                    job model = new job();
                    model.Jobs = db.jobs.ToList();
                    model.JRData = db.jobresults.Where(x => x.resourceid.Equals(DeviceID)).OrderByDescending(x => x.timestamp).ToList();

                    return View("DeviceJobExecution", model);
                }
                else if (!string.IsNullOrEmpty(ViewFile))
                {
                    activitylog.Info(Session["UserID"] + " navigate to view file");
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    var dir = obj.GetDirectories(DeviceID, 904);
                    string[] seperator = dir.Split(',');
                    TempData["Check"] = "null";

                    //If API connection successfull
                    if (seperator[0] == "Success")
                    {
                        if (dir.Contains("Success,Please find attached all Directories: 1%%"))
                        {
                            dir = dir.Substring(dir.IndexOf("1%%") + 3);
                            //dir = dir.Replace("Success,Please find attached all Directories: 1%%", "");
                            string[] seperator1 = dir.Split('|');
                            ViewBag.Drives = seperator1;
                            TempData["OkMsg"] = "Drive fetched";
                            TempData["Check"] = "1";

                        }
                        else
                        {
                            //Unable to fetch data
                            errorlog.Error("Error Fetching Drives" + dir);
                            TempData["ErrMsg"] = "Failed retriving drive";
                        }
                    }
                    else
                    {
                        //Unable to connect API
                        errorlog.Error("Error Fetching Drives" + dir);
                        TempData["ErrMsg"] = "API connection failed";
                    }

                    return View("ViewFile");
                }
                else if (!string.IsNullOrEmpty(StartProgram))
                {
                    activitylog.Info(Session["UserID"] + " navigate to view file");
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    var dir = obj.GetDirectories(DeviceID, 904);
                    string[] seperator = dir.Split(',');
                    TempData["Check"] = "null";

                    //If API connection successfull
                    if (seperator[0] == "Success")
                    {
                        if (dir.Contains("Success,Please find attached all Directories: 1%%"))
                        {
                            dir = dir.Substring(dir.IndexOf("1%%") + 3);
                            //dir = dir.Replace("Success,Please find attached all Directories: 1%%", "");
                            string[] seperator1 = dir.Split('|');
                            ViewBag.Drives = seperator1;
                            TempData["OkMsg"] = "Drive fetched";
                            TempData["Check"] = "1";

                        }
                        else
                        {
                            //Unable to fetch data
                            errorlog.Error("Error Fetching Drives" + dir);
                            TempData["ErrMsg"] = "Failed retriving drive";
                        }
                    }
                    else
                    {
                        //Unable to connect API
                        errorlog.Error("Error Fetching Drives" + dir);
                        TempData["ErrMsg"] = "API connection failed";
                    }

                    return View("ViewFileStartProg");
                }
                else if (!string.IsNullOrEmpty(DeviceFail))
                {
                    activitylog.Info(Session["UserID"] + " navigate to view device fail");

                    ViewData["DeviD"] = DeviceID;
                    string leave = "0";
                    string next = "100";
                    ViewData["leave"] = "0";
                    ViewData["next"] = "100";

                    DataSet ds = new DataSet();
                    string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "SELECT D.deviceid,D.componentid,(select m.messagetext from message0001 m where m.textno=C.textno) As component," +
                           "D.started,D.ended,D.eventno,(select m.messagetext from message0001 m where m.textno=D.eventno) As StartEvent," +
                           "D.endeventno,(select m.messagetext from message0001 m where m.textno=D.endeventno and m.texttype = '1') As Endmessage FROM devicefail D " +
                           "INNER JOIN component C ON (C.componentid= D.componentid) where deviceid='" + DeviceID + "' order by d.started desc OFFSET " + leave + " ROWS FETCH NEXT " + next + " ROWS ONLY";


                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds, "Table 1");
                            }
                        }
                        if (ds.Tables[0].Rows.Count != 0)
                        {
                            //TempData["RetainedCards"] = ds.Tables[0].Rows[0][0];
                        }
                        else
                        {
                            //TempData["RetainedCards"] = "NULL";
                        }

                    }
                    return View("DeviceFail", ds);

                }
                else if (!string.IsNullOrEmpty(dntUpdate))
                {
                    activitylog.Info(Session["UserID"] + " navigate to date & time update");

                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    Device obj1 = new Device
                    {
                        DeviceID = DeviceID
                    };
                    try
                    {
                        string response = obj.GetDateTime(DeviceID, 925);


                        //string response = "Success,Please find attached Date Time: 1%%Current Date: Thursday, August 13, 2020 Current Time: 3:11:48 PM Timezone: Pakistan Standard Time Universal Time 8/13/2020 10:11:48 AM";

                        if (response.Contains("Success,Please find attached Date Time: 1%%"))
                        {
                            response = response.Substring(response.IndexOf("1%%") + 3);
                            int FirstIndex = response.IndexOf("Current Date:");
                            int SecondIndex = response.IndexOf("Current Time:");
                            int ThirdIndex = response.IndexOf("Timezone:");
                            string date = response.Substring(FirstIndex, SecondIndex - 1); ;
                            string time = response.Substring(SecondIndex, ThirdIndex - SecondIndex);
                            date = date.Substring(response.IndexOf(":") + 1);
                            time = time.Substring(time.IndexOf(":") + 1);
                            DateTime MachineDate = Convert.ToDateTime(date + " " + time); ;
                            obj1.date = string.Concat(date, time);


                        }
                        else
                        {
                            errorlog.Error("Error Updating date and time of ATM:" + DeviceID + " Error " + response);
                            obj1.date = "Unable to fetch Date and Time";
                        }
                        //string dnt = dir;
                        return View("dntUpdate", obj1);
                    }
                    catch (Exception ex)
                    {
                        errorlog.Error("Error: " + ex);
                        return RedirectToAction("Error", "Error");
                    }
                }
                else if (!string.IsNullOrEmpty(Services))
                {
                    activitylog.Info(Session["UserID"] + " navigate to PC-state");
                    ViewData["DeviD"] = DeviceID;
                    return View("AdminActions");
                }

                return View();
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public class Eventdata
        {
            public DateTime Timestamp { get; set; }
            public int devicestate { get; set; }
            public string Messagetext { get; set; }
            public int Eventno { get; set; }
            public DateTime Servertimestamp { get; set; }
            public string Orgmessage { get; set; }
            public int eventid { get; set; }
        }

        public class JobRData
        {
            public string JobID { get; set; }
            public string ResourceID { get; set; }
            public string Command { get; set; }
            public int? CommandNo { get; set; }
            public int Attempt { get; set; }
            public DateTime jobtsp { get; set; }
            public string Result { get; set; }
            public string ResultDetails { get; set; }
        }

        public ActionResult EventRefresh(String L, String Dev, String R, String D, String Evt, String Filter, String OrgMsg)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            String DeviceID = Dev;
            Int64 leave = 0;
            Int64 next = 100;
            ViewBag.Leave = leave.ToString();
            String EvtNoIn = "";
            String orgMsgIn = "";
            TempData["EvNo"] = Evt;
            TempData["MsgText"] = OrgMsg;
            if (!string.IsNullOrEmpty(Evt) && !string.IsNullOrEmpty(OrgMsg))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
            }
            else if (!string.IsNullOrEmpty(Evt))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
            }
            else if (!string.IsNullOrEmpty(OrgMsg))
            {
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }
            else
            {
                EvtNoIn = "";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }

            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
            }

            if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
            }
            else
            {
                GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
            }

            return View("ShowEvent", evt);
        }


        public ActionResult JRRefresh(String L, String Dev, String R, String D, String JobName, String type, String nextRows)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var job1 = db.jobs.Select(x => x.jobid).Distinct();
            ViewBag.job = job1;
            JobName = JobName ?? "ALL Job";
            ViewBag.jobid = JobName;
            String DeviceID = "ALL";
            Int64 leave = 0;
            Int64 next = Convert.ToInt64(nextRows);
            ViewBag.NextRows = nextRows;
            ViewBag.Leave = leave.ToString();
            R = R ?? "Current";
            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Range"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = true;
            }
            if (type.Equals("All"))
            {
                ViewBag.AllRadio = true;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Success"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = true;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Fail"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = true;
            }
            if (D != null && R.Equals("Range"))
            {
                String SDTF = D.Split('|')[0];
                String SDTT = D.Split('|')[1];

                ViewBag.DTF = SDTF;
                ViewBag.DTT = SDTT;

                DateTime DTF = DateTime.ParseExact(SDTF, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                DateTime DTT = DateTime.ParseExact(SDTT, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                radioCheck = R;
                if (DeviceID == "ALL")
                {
                    GetSqlJrALL(leave, next, DTF, DTT, radioCheck, JobName, 0, type);
                }
                else
                {
                    GetSqlJr(DeviceID, leave, next, DTF, DTT, radioCheck, type);
                }
            }
            else if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                ViewBag.Time = time;
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (DeviceID == "ALL")
                {
                    GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                }
                else
                {
                    GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                }
            }
            else
            {
                if (DeviceID == "ALL")
                {
                    //                    GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                    GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                }
                else
                {
                    GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                }
                ViewBag.Time = "00:00:00";
            }

            return View("JobResult", evt);
        }

        public ActionResult JrByJOBID(FormCollection formcollection)
        {
            activitylog.Info(Session["UserID"] + " searched job result by job id : " + formcollection["jobid"]);

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.jobid = formcollection["jobid"] ?? "ALL Job";
            var job1 = db.jobs.Select(x => x.jobid).Distinct();
            ViewBag.job = job1;

            formcollection["jobid"] = formcollection["jobid"] ?? "ALL Job";
            string deviceid = formcollection["deviceid"] ?? "ALL";
            var R = formcollection["Radio"];
            var D = formcollection["DateFrom"];
            var type = formcollection["Res"];

            var NoOfRows = formcollection["NoOfRows"] ?? "100";

            ViewBag.NextRows = NoOfRows.ToString();

            string SDTF = formcollection["DateTimeFrom"];
            string SDTT = formcollection["DateTimeTo"];



            ViewBag.Date = D;
            ViewBag.time = formcollection["TimeFrom"];
            //String DeviceID = "ALL";
            Int64 leave = 0;
            Int64 next = Convert.ToInt64(NoOfRows);

            ViewBag.Leave = leave.ToString();
            string ComparisonOperator = "<=";
            if (R.Equals("Current"))
            {

                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Until"))
            {

                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ComparisonOperator = ">=";
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Range"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = true;
            }
            if (type.Equals("All"))
            {
                ViewBag.AllRadio = true;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Success"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = true;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Fail"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = true;
            }
            if (D != null && R.Equals("Range"))
            {
                ViewBag.DTF = SDTF;
                ViewBag.DTT = SDTT;
                DateTime DTF = DateTime.ParseExact(SDTF, "yyyy/MM/dd HH:mm",
                                      System.Globalization.CultureInfo.InvariantCulture);
                DateTime DTT = DateTime.ParseExact(SDTT, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);

                radioCheck = R;
                var sql = "";
                string test = formcollection["jobid"];
                if (formcollection["jobid"] == "")
                {
                    sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                    + " from jobresult jr where jr.jobtsp between '" + DTF + "' and '" + DTT + "'"
                                     + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                    + "  ORDER BY jr.timestamp desc, jr.resourceid ";
                }
                else
                {

                    sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                 + " from jobresult jr where jr.jobtsp between '" + DTF + "' and '" + DTT + "' AND jr.jobid='" + formcollection["jobid"] + "'"
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                 + "  ORDER BY jr.timestamp desc, jr.resourceid ";
                }

                var result = dbPro.Database.SqlQuery<JobRData>(sql);

                foreach (var x in result)
                {
                    String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string b = x.JobID.Trim();
                    string c = x.ResourceID.Trim();

                    evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                }

                int z = 100 - evt.JrData.Count();
                TempData["JrCount"] = (leave + 100) - z;
                evt.deviceid = "ALL";
            }
            else if (D != null)
            {
                String date = D.Split('|')[0];
                //String time = D.Split('|')[1];
                String time = formcollection["TimeFrom"];
                ViewBag.Time = time;
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                var sql = "";
                string test = formcollection["jobid"];
                if (formcollection["jobid"] == "")
                {
                    sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                    + " from jobresult jr where jr.jobtsp '" + ComparisonOperator + "' '" + DTime + "'"
                                     + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                    + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                    + "OFFSET " + leave + " ROWS "
                                    + "FETCH NEXT " + next + " ROWS ONLY";
                }
                else
                {

                    sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                 + " from jobresult jr where jr.jobtsp " + ComparisonOperator + " '" + DTime + "'AND jr.jobid='" + formcollection["jobid"] + "'"
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                 + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                 + "OFFSET " + leave + " ROWS "
                                 + "FETCH NEXT " + next + " ROWS ONLY";
                }

                var result = dbPro.Database.SqlQuery<JobRData>(sql);

                foreach (var x in result)
                {
                    String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string b = x.JobID.Trim();
                    string c = x.ResourceID.Trim();

                    evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                }

                int z = 100 - evt.JrData.Count();
                TempData["JrCount"] = (leave + 100) - z;
                evt.deviceid = "ALL";
            }

            return View("JobResult", evt);
        }

        public ActionResult Event_More(String L, String Dev, String R, String D, String Evt, String Filter, String OrgMsg)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            String DeviceID = Dev;
            Int64 leave = Convert.ToInt64(L) + 100;
            Int64 next = 100;
            ViewBag.Leave = leave.ToString();
            String EvtNoIn = "";
            String orgMsgIn = "";
            TempData["EvNo"] = Evt;
            TempData["MsgText"] = OrgMsg;
            if (!string.IsNullOrEmpty(Evt) && !string.IsNullOrEmpty(OrgMsg))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
            }
            else if (!string.IsNullOrEmpty(Evt))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
            }
            else if (!string.IsNullOrEmpty(OrgMsg))
            {
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }
            else
            {
                EvtNoIn = "";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }
            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
            }

            var cnt = dbPro.events.Where(a => a.deviceid.Equals(DeviceID)).Count();
            if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (leave < cnt)
                {
                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
                else
                {
                    leave -= 100;
                    ViewBag.Leave = leave.ToString();

                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
            }
            else
            {
                if (leave < cnt)
                {
                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
                else
                {
                    leave -= 100;
                    ViewBag.Leave = leave.ToString();

                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
            }

            return View("ShowEvent", evt);

        }

        public ActionResult JR_More(String L, String Dev, String R, String D, String JobName, String type, String nextRows)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            JobName = JobName ?? "ALL Job";
            ViewBag.jobid = JobName;

            var job1 = db.jobs.Select(x => x.jobid).Distinct();
            ViewBag.job = job1;
            String DeviceID = Dev;

            Int64 NoOfRows = Convert.ToInt64(nextRows);
            ViewBag.NextRows = nextRows;

            Int64 leave = Convert.ToInt64(L) - NoOfRows;
            if (leave < 0) { leave = 0; };
            Int64 next = NoOfRows;
            ViewBag.Leave = leave.ToString();
            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Range"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = true;
            }

            if (type.Equals("All"))
            {
                ViewBag.AllRadio = true;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Success"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = true;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Fail"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = true;
            }
            var cnt = 0;

            if (DeviceID == "ALL")
            {
                cnt = dbPro.jobresults.Count();
            }
            else
            {
                cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID)).Count();
            }



            if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                ViewBag.Time = time;
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (leave < cnt)
                {
                    if (DeviceID == "ALL")
                    {
                        ///GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                else
                {
                    leave -= 100;
                    ViewBag.Leave = leave.ToString();

                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
            }
            else
            {
                if (leave < cnt)
                {
                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                else
                {
                    leave -= 100;
                    ViewBag.Leave = leave.ToString();

                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                ViewBag.Time = "00:00:00";
            }

            return View("JobResult", evt);

        }

        public ActionResult Event_Less(String L, String Dev, String R, String D, String Evt, String Filter, String OrgMsg)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            String DeviceID = Dev;
            Int64 leave = Convert.ToInt64(L) - 100;
            Int64 next = 100;
            ViewBag.Leave = leave.ToString();
            String EvtNoIn = "";
            String orgMsgIn = "";
            TempData["EvNo"] = Evt;
            TempData["MsgText"] = OrgMsg;
            if (!string.IsNullOrEmpty(Evt) && !string.IsNullOrEmpty(OrgMsg))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
            }
            else if (!string.IsNullOrEmpty(Evt))
            {
                if (Filter == "Exclude")
                {
                    ViewBag.IncRadio = false;
                    ViewBag.ExcRadio = true;
                    EvtNoIn = "and eventno !='" + Evt + "'";
                }
                else
                {
                    ViewBag.IncRadio = true;
                    ViewBag.ExcRadio = false;
                    EvtNoIn = "and eventno ='" + Evt + "'";
                }
            }
            else if (!string.IsNullOrEmpty(OrgMsg))
            {
                orgMsgIn = "and orgmessage like '%" + OrgMsg + "%'";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }
            else
            {
                EvtNoIn = "";
                ViewBag.IncRadio = true;
                ViewBag.ExcRadio = false;
            }
            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
            }

            if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (leave > -1)
                {
                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
                else
                {
                    leave += 100;
                    ViewBag.Leave = leave.ToString();

                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
            }
            else
            {
                if (leave > -1)
                {
                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
                else
                {
                    leave += 100;
                    ViewBag.Leave = leave.ToString();

                    GetSql(DeviceID, leave, next, DTime, radioCheck, EvtNoIn, orgMsgIn);
                }
            }

            return View("ShowEvent", evt);

        }

        public ActionResult JR_Less(String L, String Dev, String R, String D, String JobName, String type, String nextRows)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            JobName = JobName ?? "ALL Job";
            ViewBag.jobid = JobName;
            var job1 = db.jobs.Select(x => x.jobid).Distinct();
            ViewBag.job = job1;
            String DeviceID = Dev;

            Int64 NoOfRows = Convert.ToInt64(nextRows);
            ViewBag.NextRows = nextRows;

            Int64 leave = Convert.ToInt64(L) + NoOfRows;
            Int64 next = NoOfRows;
            ViewBag.Leave = leave.ToString();
            if (R.Equals("Current"))
            {
                ViewBag.CRadio = true;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Until"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = true;
                ViewBag.SRadio = false;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Since"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = true;
                ViewBag.RRadio = false;
            }
            else if (R.Equals("Range"))
            {
                ViewBag.CRadio = false;
                ViewBag.URadio = false;
                ViewBag.SRadio = false;
                ViewBag.RRadio = true;
            }

            if (type.Equals("All"))
            {
                ViewBag.AllRadio = true;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Success"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = true;
                ViewBag.FRadio = false;
            }
            else if (type.Equals("Fail"))
            {
                ViewBag.AllRadio = false;
                ViewBag.SuRadio = false;
                ViewBag.FRadio = true;
            }
            if (D != null && R.Equals("Range"))
            {
                if (leave < 0)
                {
                    leave += NoOfRows;
                    ViewBag.Leave = leave.ToString();
                }
                String SDTF = D.Split('|')[0];
                String SDTT = D.Split('|')[1];
                ViewBag.DTF = SDTF;
                ViewBag.DTT = SDTT;
                DateTime DTF = DateTime.ParseExact(SDTF, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                DateTime DTT = DateTime.ParseExact(SDTT, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                radioCheck = R;
                if (DeviceID == "ALL")
                {
                    GetSqlJrALL(leave, next, DTF, DTT, radioCheck, JobName, 0, type);
                }
                else
                {
                    GetSqlJr(DeviceID, leave, next, DTF, DTT, radioCheck, type);
                }
            }
            else if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                ViewBag.Time = time;
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (leave > -1)
                {
                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                else
                {
                    leave += NoOfRows;
                    ViewBag.Leave = leave.ToString();

                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
            }
            else
            {
                if (leave > -1)
                {
                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                else
                {
                    leave += NoOfRows;
                    ViewBag.Leave = leave.ToString();

                    if (DeviceID == "ALL")
                    {
                        //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
                        GetSqlJrALL(leave, next, DTime, radioCheck, JobName, 0, type);
                    }
                    else
                    {
                        GetSqlJr(DeviceID, leave, next, DTime, radioCheck, type);
                    }
                }
                ViewBag.Time = "00:00:00";
            }

            return View("JobResult", evt);

        }

        public ViewResult GetSql(String DeviceID, Int64 leave, Int64 next, DateTime? DTime, String Rad, String EvtNoIn, String OrgMsg)
        {
            IEnumerable<Eventdata> result;
            if (DTime != null && Rad == "Until")//Until
            {
                var cnt = dbPro.events.Where(a => a.deviceid.Equals(DeviceID) && a.timestamp <= DTime).Count();
                TempData["TCount"] = cnt;

                var sql = "SELECT event.timestamp,event.devicestate,message0001.messagetext,event.eventno,event.servertimestamp,event.orgmessage,event.eventid " +
                    "FROM event JOIN message0001 ON event.eventno = message0001.textno " +
                    "AND deviceid = '" + DeviceID + "' AND texttype = '1'" + EvtNoIn + OrgMsg + " AND event.timestamp <= '" + DTime + "' " +
                    "ORDER BY timestamp desc , eventid desc " +
                    "OFFSET " + leave + " ROWS " +
                    "FETCH NEXT " + next + " ROWS ONLY";



                result = dbPro.Database.SqlQuery<Eventdata>(sql);
                ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
            }
            else if (DTime != null && Rad == "Since")//Since
            {
                var cnt = db.events.Where(a => a.deviceid.Equals(DeviceID) && a.timestamp >= DTime).Count();
                TempData["TCount"] = cnt;

                var sql = "SELECT event.timestamp,event.devicestate,message0001.messagetext,event.eventno,event.servertimestamp,event.orgmessage,event.eventid " +
                    "FROM event JOIN message0001 ON event.eventno = message0001.textno " +
                    "AND deviceid = '" + DeviceID + "' AND texttype = '1' " + EvtNoIn + OrgMsg + " AND event.timestamp >= '" + DTime + "' " +
                    "ORDER BY timestamp desc , eventid desc " +
                    "OFFSET " + leave + " ROWS " +
                    "FETCH NEXT " + next + " ROWS ONLY";

                result = dbPro.Database.SqlQuery<Eventdata>(sql);
                ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
            }
            else
            {
                var cnt = dbPro.events.Where(a => a.deviceid.Equals(DeviceID)).Count();
                TempData["TCount"] = cnt;

                var sql = "SELECT event.timestamp,event.devicestate,message0001.messagetext,event.eventno,event.servertimestamp,event.orgmessage,event.eventid " +
                    "FROM event JOIN message0001 ON event.eventno = message0001.textno " +
                    "where deviceid = '" + DeviceID + "' AND texttype = '1' " + EvtNoIn + OrgMsg +
                    "ORDER BY  timestamp desc , eventid desc " +
                    "OFFSET " + leave + " ROWS " +
                    "FETCH NEXT " + next + " ROWS ONLY";

                result = dbPro.Database.SqlQuery<Eventdata>(sql);
                ViewBag.Date = DateTime.Now.ToShortDateString();

            }

            var stbs = (from st in db.stbs
                        join msg in db.message0001
                        on st.textno equals msg.textno
                        where st.type.Equals(0)
                        orderby st.prio ascending
                        select new
                        {
                            st,
                            messagetext = msg.messagetext
                        }).ToList();

            foreach (var x in result)
            {
                String a = x.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String b = x.Servertimestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String col = "rgb(0,255,0)";

                //var getState = UserCustom.GetStateByBit(x.devicestate);

                var statebit = x.devicestate;
                var f = 0;
                int[] bit = new int[25];
                while (statebit > 0)
                {
                    bit[f] = statebit % 2;
                    statebit = Convert.ToInt32(Math.Truncate(Convert.ToDecimal(statebit / 2)));
                    f++;
                }
                List<int> s = new List<int>();
                for (int i = 0; i < bit.Length - 1; i++)
                {
                    if (bit[i] == 1)
                    {
                        s.Add(i + 1);
                    }
                }

                var getState = stbs.Where(m => s.Contains(m.st.bit)).OrderBy(m => m.st.prio).Select(m => m.st.color).FirstOrDefault();

                //String sql = "select distinct st.color from state s join DEVICE_STATE_TEMP dst on dst.deviceid=s.deviceid join stb st on st.bit=dst.bit"
                //             + " where st.type = '0' and dst.eventid = '" + x.eventid + "' AND dst.deviceid = '" + DeviceID + "'  and dst.timestamp >= '" + a + "' and dst.timestamp < DATEADD(SECOND, 1, '" + a + "')"
                //             + " and dst.prio = (select min(dstt.prio) from DEVICE_STATE_TEMP dstt where dstt.deviceid = dst.deviceid and dstt.timestamp >= '" + a + "' and dstt.timestamp < DATEADD(SECOND, 1, '" + a + "') )";
                ////String sql = "Select s.color FROM stb s JOIN DEVICE_STATE_TEMP d ON s.bit = d.bit where s.type = '0' AND d.eventid = '" + x.eventid + "' AND d.deviceid = '" + DeviceID + "' AND d.timestamp = '" + a + "'";


                //var res = db.Database.SqlQuery<string>(sql);

                //if (res != null)
                //{
                //    foreach (var ii in res)
                //    {
                //        string[] words = ii.ToString().Split(' ');
                //        col = "rgb(" + words[0] + "," + words[1] + "," + words[2] + ")";
                //    }
                //}
                if (getState != null)
                {
                    string[] words = getState.ToString().Split(' ');
                    col = "rgb(" + words[0] + "," + words[1] + "," + words[2] + ")";
                }


                evt.data.Add(new Tuple<string, string, string, int, string, string>(col, a, x.Messagetext, x.Eventno, b, x.Orgmessage));
            }

            int z = 100 - evt.data.Count();
            TempData["Count"] = (leave + 100) - z;
            evt.deviceid = DeviceID;

            return View("ShowEvent", evt);
        }

        public ViewResult GetSqlJr(String DeviceID, Int64 leave, Int64 next, DateTime? DTime, String Rad, String tp)
        {
            string type = "";
            if (tp == "All")
            {
                type = " ";
            }
            else if (tp == "Success")
            {
                type = " and jr.result = '0'";
            }
            if (tp == "Fail")
            {
                type = " and jr.result = '1'";
            }
            IEnumerable<JobRData> result;
            if (DTime != null && Rad == "Until")//Until
            {
                var cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID) && a.jobtsp <= DTime).Count();
                TempData["JrTCount"] = cnt;

                var sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                            + " from jobresult jr where jr.resourceid='" + DeviceID + "' and  jr.jobtsp <= '" + DTime + "' " + type
                             + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                            + "  ORDER BY jr.timestamp desc, jr.resourceid "
                            + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";

                result = dbPro.Database.SqlQuery<JobRData>(sql);
                ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
            }
            else if (DTime != null && Rad == "Since")//Since
            {
                var cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID) && a.jobtsp >= DTime).Count();
                TempData["JrTCount"] = cnt;

                //var sql = "SELECT event.timestamp,message0001.messagetext,event.eventno,event.servertimestamp,event.orgmessage,event.eventid " +
                //    "FROM event JOIN message0001 ON event.eventno = message0001.textno " +
                //    "AND deviceid = '" + DeviceID + "' AND texttype = '1' AND event.timestamp >= '" + DTime + "' " +
                //    "ORDER BY timestamp desc , eventid desc " +
                //    "OFFSET " + leave + " ROWS " +
                //    "FETCH NEXT " + next + " ROWS ONLY";

                var sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                            + " from jobresult jr where jr.resourceid='" + DeviceID + "' and  jr.jobtsp <= '" + DTime + "'"
                             + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                            + "  ORDER BY jr.timestamp desc, jr.resourceid "
                            + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";


                result = dbPro.Database.SqlQuery<JobRData>(sql);
                ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
            }
            else
            {
                var cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID)).Count();
                TempData["JrTCount"] = cnt;

                var sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                            + " from jobresult jr where jr.resourceid='" + DeviceID + "' "
                             + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                            + "  ORDER BY jr.timestamp desc, jr.resourceid "
                            + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";

                result = dbPro.Database.SqlQuery<JobRData>(sql);
                ViewBag.Date = DateTime.Now.ToShortDateString();

            }

            foreach (var x in result)
            {
                String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string b = x.JobID.Trim();
                string c = x.ResourceID.Trim();

                evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
            }

            int z = 100 - evt.JrData.Count();
            TempData["JrCount"] = (leave + 100) - z;
            evt.deviceid = DeviceID;


            return View("JobResult", evt);
        }

        //        public ViewResult GetSqlJrALL(Int64 leave, Int64 next, DateTime? DTime, String Rad, String JobID, int? Report, String tp)
        public ViewResult GetSqlJrALL(Int64 leave, Int64 next, DateTime? DTime, String Rad, String JobID, int? Report, String tp)
        {
            using (SurveilAIEntities dbPro = new SurveilAIEntities())
            {
                dbPro.Database.CommandTimeout = 300;
                string type = "";
                if (tp == "All")
                {
                    //type = " ";
                    type = " and jr.result in ('0','1')";
                }
                else if (tp == "Success")
                {
                    type = " and jr.result = '0'";
                }
                if (tp == "Fail")
                {
                    type = " and jr.result = '1'";
                }
                IEnumerable<JobRData> result;
                if (DTime != null && Rad == "Until")//Until
                {
                    var cnt = dbPro.jobresults.Where(a => a.jobtsp <= DTime).Count();
                    TempData["JrTCount"] = cnt;
                    var sql = "";
                    if (JobID == "ALL Job")
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                  + " from jobresult jr where jr.jobtsp <= '" + DTime + "' " + type
                                   + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                  + "OFFSET " + leave + " ROWS "
                                  + "FETCH NEXT " + next + " ROWS ONLY";
                    }
                    else
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                  + " from jobresult jr where jr.jobtsp <= '" + DTime + "'AND jr.jobid='" + JobID + "'"
                                   + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                  + "OFFSET " + leave + " ROWS "
                                  + "FETCH NEXT " + next + " ROWS ONLY";
                    }


                    result = dbPro.Database.SqlQuery<JobRData>(sql);
                    ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                }
                else if (DTime != null && Rad == "Since")//Since
                {
                    var cnt = dbPro.jobresults.Where(a => a.jobtsp >= DTime).Count();
                    TempData["JrTCount"] = cnt;

                    var sql = "";
                    if (JobID == "ALL Job")
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr where jr.jobtsp >= '" + DTime + "'"
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";
                    }
                    else
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr where jr.jobtsp >= '" + DTime + "'AND jr.jobid='" + JobID + "'"
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";

                    }



                    result = dbPro.Database.SqlQuery<JobRData>(sql);
                    ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                }
                else
                {
                    string type1 = "";
                    if (tp == "All")
                    {

                        type1 = "";
                    }
                    else if (tp == "Success")
                    {
                        type1 = " where jr.result = '1'";
                    }
                    if (tp == "Fail")
                    {
                        type1 = " where jr.result = '0'";
                    }
                    if (JobID != "ALL Job")
                    {
                        if (type1 != "")
                        {
                            type1 = type1 + " and jr.jobid='" + JobID + "'";
                        }
                        else
                        {
                            type1 = "where jr.jobid='" + JobID + "'";
                        }
                    }
                    var cnt = dbPro.jobresults.Count();
                    TempData["JrTCount"] = cnt;
                    var sql = "";
                    if (JobID == "ALL Job")
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr " + type1
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";
                    }
                    else
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr " + type1
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";
                    }


                    result = dbPro.Database.SqlQuery<JobRData>(sql);
                    ViewBag.Date = DateTime.Now.ToShortDateString();

                }

                foreach (var x in result)
                {
                    String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string b = x.JobID.Trim();
                    string c = x.ResourceID.Trim();

                    evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                }

                int z = 100 - evt.JrData.Count();
                TempData["JrCount"] = (leave + 100) - z;
                evt.deviceid = "ALL";


                return View("JobResult", evt);
            }


        }

        public ActionResult ShowDataForm(String ET, String EN, String ST, String id)
        {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            id = id.Trim();
            //event.eventcount,
            var sql = "SELECT event.timestamp,event.servertimestamp,event.eventno,event.messageno,message0001.messagetext,event.orgmessage,event.eventid " +
                    "FROM event JOIN message0001 ON event.eventno = message0001.textno " +
                    "AND deviceid = '" + id + "' AND timestamp = '" + ET + "' AND servertimestamp = '" + ST + "' AND eventno = '" + EN + "' AND texttype = '1' ";

            var result = dbPro.Database.SqlQuery<Formdata>(sql);

            foreach (var x in result)
            {
                //evt.eventcount = x.Eventcount;
                evt.timestamp = x.Timestamp;
                evt.servertimestamp = x.Servertimestamp;
                evt.eventno = x.Eventno;
                evt.messageno = x.messageno;
                evt.messagetext = x.Messagetext;
                evt.orgmessage = x.Orgmessage;
                evt.eventid = x.eventid;
            }

            var res = db.DEVICE_COMP_TEMP.Where(a => a.deviceid.Contains(id) && a.eventid.Equals(evt.eventid)).Select(a => a.CATEGORY).FirstOrDefault();
            var res1 = db.DEVICE_STATE_TEMP.Where(a => a.deviceid.Contains(id) && a.eventid.Equals(evt.eventid)).Select(a => a.STATE).FirstOrDefault();

            evt.deviceid = id;
            evt.Component = res;
            evt.State = res1;

            return PartialView("_ShowDataForm", evt);


        }

        public class Formdata
        {
            //public int Eventcount { get; set; }
            public DateTime Timestamp { get; set; }
            public string Messagetext { get; set; }
            public int Eventno { get; set; }
            public int messageno { get; set; }
            public DateTime Servertimestamp { get; set; }
            public string Orgmessage { get; set; }
            public int eventid { get; set; }
        }

        public ActionResult piechart(string id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("45");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                //------------ old method -------------//

                //activitylog.Info(Session["UserID"].ToString() + " navigate to Pie Chart");
                //var count = (from row in db.Devices select row).Count();
                ////int c = db.Devices.Select(a => a.DeviceID).Count();

                //ViewData["count"] = count;

                //String usr = Session["UserID"].ToString();
                //var ATMID = UserCustom.GetAssignDevice(usr);//db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).First();
                //var atms = ATMID.Select(a => a.DeviceID).ToList();//new SelectList(ATMID.Split('%').ToList());

                //ViewData["assigned"] = atms.Count();

                //userlog.Info(Session["UserID"].ToString() + "no. of assigned ATMs: " + atms.Count());

                //string User = Session["UserID"].ToString();

                //try
                //{
                //    var statebitmask = db.consoleviews.Where(a => a.viewname.Equals(id) && a.viewtype.Equals(5) && a.extuserid.Equals(User)).Select(a => a.states).FirstOrDefault();
                //    int statebit = Convert.ToInt32(statebitmask);

                //    var f = 0;
                //    int[] bit = new int[25];
                //    while (statebit > 0)
                //    {
                //        bit[f] = statebit % 2;
                //        statebit = Convert.ToInt32(Math.Truncate(Convert.ToDecimal(statebit / 2)));
                //        f++;
                //    }

                //    var message = "select m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype in (3,1) order by prio";
                //    var states = db.Database.SqlQuery<IISWebApp.Models.Main.Data2>(message);

                //    List<string> statesname = new List<string>();

                //    for (var g = 0; g < bit.Length; g++)
                //    {
                //        if (bit[g] == 1)
                //        {
                //            statesname.Add(states.Skip(g).First().Messagetext.ToString().Trim());
                //        }
                //    }
                //    statesname.Add("Operational");

                //    var condition = db.consoleviews.Where(a => a.viewname.Equals(id) && a.viewtype.Equals(5) && a.extuserid.Equals(User)).Select(a => a.devices).FirstOrDefault();

                //    DataSet ds = new DataSet();
                //    string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                //    using (SqlConnection con = new SqlConnection(constr))
                //    {
                //        string query = "select d.deviceid from Device d where " + condition + "";

                //        using (SqlCommand cmd = new SqlCommand(query))
                //        {
                //            cmd.Connection = con;
                //            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                //            {
                //                sda.Fill(ds);
                //            }
                //        }
                //    }

                //    var devices = ds.Tables[0].AsEnumerable()
                //            .Select(dataRow => new Device
                //            {
                //                DeviceID = dataRow.Field<string>("DeviceID")
                //            }).ToList();

                //    var mymodel = new User();
                //    String usr1 = Session["UserID"].ToString();

                //    foreach (var a in devices)
                //    {
                //        mymodel.atmss.Add(a.DeviceID.ToString());
                //    }

                //    int counting = mymodel.atmss.Count();

                //    var result = from V_Downtime in db.V_Downtime
                //                 where
                //                   (mymodel.atmss.ToList()).Contains(V_Downtime.deviceid) && 
                //                   statesname.Contains(V_Downtime.state)
                //                 group V_Downtime by new
                //                 {
                //                     V_Downtime.state
                //                 } into g
                //                 select new
                //                 {
                //                     g.Key.state,
                //                     Column1 = g.Count(p => p.deviceid != null)
                //                 };

                //    var chartData1 = new object[result.Count() + 1];
                //    var chartData2 = new object[result.Count() + 1];
                //    string ac = null;
                //    string bd = null;
                //    string ef = null;
                //    int j = 0;
                //    foreach (var i in result)
                //    {
                //        double cc = ((double)i.Column1 / (double)counting) * 100;
                //        ef += "'" + i.state.ToString().Trim() + "',";
                //        ac += "'" + i.state.ToString().Trim() + " (" + String.Format("{0:0.0}", cc) + "%" + ")',";
                //        bd += i.Column1 + ",";
                //        j++;
                //    }
                //    ef += "' '";
                //    //ac += "'total'";
                //    //bd += "0";

                //    String sql = "select s.color,m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.messagetext in (" + ef + ") order by prio";
                //    String col = "'RGB(0,255,0)'";

                //    var result1 = dbPro.Database.SqlQuery<Data>(sql);

                //    if (result1 != null)
                //    {
                //        foreach (var ii in result1)
                //        {
                //            string[] words = ii.Color.ToString().Split(' ');
                //            col += ",'rgb(" + words[0] + "," + words[1] + "," + words[2] + ")'";
                //        }
                //    }
                //    ViewBag.color = col;
                //    ViewBag.ObjectName = bd;//list of strings that you need to show on the chart. as mentioned in the example from c-sharpcorner
                //    ViewBag.Data = ac;

                //}
                //catch (Exception ex)
                //{
                //    errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                //}


                //return View();

                //------------ old method -------------//

                int DeviceCount = db.Devices.Select(a => a.DeviceID).Count();
                ViewData["count"] = DeviceCount;

                activitylog.Info(Session["UserID"].ToString() + " navigate to Pie Chart");
                string User = Session["UserID"].ToString();
                var statebitmask = db.consoleviews.Where(a => a.viewname.Equals(id) && a.viewtype.Equals(5) && a.extuserid.Equals(User)).Select(a => a.states).FirstOrDefault();

                int statebit = Convert.ToInt32(statebitmask);

                var f = 0;
                int[] bit = new int[25];
                while (statebit > 0)
                {
                    bit[f] = statebit % 2;
                    statebit = Convert.ToInt32(Math.Truncate(Convert.ToDecimal(statebit / 2)));
                    f++;
                }
                List<int> t = new List<int>();
                for (int i = 0; i < bit.Length - 1; i++)
                {
                    if (bit[i] == 1)
                    {
                        t.Add(i + 1);
                    }
                }

                string bitcond = " and s.bit in ('";
                foreach (int x in t)
                {
                    bitcond += x + "','";
                }
                bitcond = bitcond.Substring(0, bitcond.Length - 2);
                bitcond += ")";

                var message = "select m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype in (3,1) " + bitcond + " order by prio";
                var states = db.Database.SqlQuery<SurveilAI.Models.Data2>(message);

                List<string> statesname = new List<string>();
                statesname.Add("Operational");
                foreach (var y in states)
                {
                    statesname.Add(y.Messagetext.ToString().Trim());
                }

                UserCustom objuc = new UserCustom();
                var condition = db.consoleviews.Where(a => a.viewname.Equals(id) && a.viewtype.Equals(5) && a.extuserid.Equals(User)).Select(a => a.devices).FirstOrDefault();
                condition = condition.Replace("(", "('");
                condition = condition.Replace(",", "','");
                condition = condition.Replace(")", "')");
                if (condition != "")
                {
                    DataSet ds = new DataSet();
                    string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        //string query = "select DISTINCT s.deviceid, s.devicestate from state s join device d on d.DeviceID = s.deviceid and " + condition + "";
                        string query = @"select top 1 with ties t.deviceid, t.devicestate, t.timestamp
										From 
										(
										select st.deviceid, st.devicestate, st.timestamp from state st
											inner join (select deviceid, max(timestamp) as mts from state s
											where s.DeviceID IN (select d.deviceid from Device d where " + condition + ") group by deviceid" +
                                         ") s2 on s2.deviceid = st.deviceid and s2.mts = st.timestamp) t " +
                                         "order by ROW_NUMBER() OVER(PARTITION BY t.deviceid ORDER BY timestamp)";
                        condition = condition.Replace("dev.HierLevel", "d.HierLevel");

                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds);
                            }
                        }
                    }

                    var assigneddevices = objuc.GetAssignDevice(Session["UserID"].ToString()).Select(a => a.DeviceID).ToList();
                    foreach (var dev in assigneddevices)
                    {
                        for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow dr = ds.Tables[0].Rows[i];
                        }
                        ds.AcceptChanges();
                    }

                    ViewData["AssignedCount"] = ds.Tables[0].Rows.Count;

                    List<Tuple<string, string, string>> data = new List<Tuple<string, string, string>>();

                    foreach (DataTable dt in ds.Tables)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string a = dr["deviceid"].ToString().Trim();
                            int bit1 = Convert.ToInt32(dr["devicestate"]);
                            stb objstb1 = objuc.GetStateByBit(bit1);
                            string b = objstb1.color.ToString();
                            string c = objstb1.messagetext.ToString().Trim();
                            data.Add(new Tuple<string, string, string>(a, b, c));
                        }
                    }

                    foreach (Tuple<string, string, string> tuple in data.ToList())
                    {
                        if (statesname.Any(s => tuple.Item3.Contains(s)))
                        { }
                        else
                        {
                            data.Remove(tuple);
                        }
                    }

                    string cc = null;
                    string ac = null;
                    string bd = null;
                    String col = null;
                    List<string> list = new List<string>();

                    foreach (Tuple<string, string, string> tuple in data)
                    {
                        list.Add(tuple.Item2.ToString());
                    }

                    List<string> distinct = list.Distinct().ToList();

                    foreach (string tuple in distinct)
                    {
                        string[] words = tuple.Split(' ');
                        col += "'rgb(" + words[0] + "," + words[1] + "," + words[2] + ")',";
                    }


                    DataSet ds1 = new DataSet();
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select d.deviceid from Device d where " + condition;

                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                sda.Fill(ds1);
                            }
                        }
                    }

                    string devicequery = "deviceid in ('";

                    foreach (DataTable dt in ds1.Tables)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string a = dr["deviceid"].ToString().Trim();
                            devicequery += a + "','";
                        }
                    }

                    devicequery = devicequery.Substring(0, devicequery.Length - 2);
                    devicequery += ")";

                    var wordCount = (from word in data
                                     group word by word.Item3 into g
                                     select new { Count = g.Count() }).ToList();

                    foreach (var a in wordCount)
                    {
                        string b = a.ToString();
                        b = Regex.Match(b, @"\d+").Value;
                        bd += b + ",";
                        cc += ((Convert.ToDouble(b) / (double)data.Count) * 100).ToString() + ",";
                    }

                    cc = cc.TrimEnd(',');
                    string[] cc1 = cc.Split(',');

                    var wording = (from word in data
                                   group word by word.Item3 into g
                                   select new { a = g.Key.ToString() }).ToList();

                    int h = 0;
                    foreach (var a in wording)
                    {
                        string b = a.ToString();
                        b = b.Substring(6, b.Length - 8);
                        if (cc1[h].ToString().Length > 5)
                        {
                            ac += "'" + b + " (" + cc1[h].Substring(0, 5) + "%" + ")',";
                        }
                        else
                        {
                            ac += "'" + b + " (" + cc1[h] + "%" + ")',";
                        }
                        h++;
                    }

                    ViewBag.color = col;
                    ViewBag.ObjectName = bd;
                    ViewBag.Data = ac;

                    return View();
                }
                else
                {
                    @TempData["AlertMsg"] = "No Devices Selected In Pie Chart!";
                    errorlog.Error(Session["UserID"].ToString() + " Pie Chart Opening Unsuccessful");
                    return RedirectToAction("Event", "Event");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                // return RedirectToAction("Index", "Login");
                return RedirectToAction("Error", "Error");
            }

        }

        [HttpGet]
        public ActionResult getstate(String id)
        {
            if (Session["UserID"] == null)
            {
                //return RedirectToAction("Index", "Login");
                return Content("Logout");
            }


            string query = "select top 1 devicestate from state where deviceid = '" + id + "' and timestamp = (select max(timestamp) from state where deviceid = '" + id + "')";

            //query = query.Substring(0, query.Length - 6);
            DataSet dt = new DataSet();
            string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
            //using (SqlConnection con = new SqlConnection(constr))
            //{
            //    using (SqlCommand cmd = new SqlCommand(query))
            //    {
            //        cmd.Connection = con;
            //        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            //        {
            //            sda.Fill(dt);
            //        }
            //    }
            //}


            int? state_Bit = null;
            foreach (DataRow row in dt.Tables[0].Rows)
            {
                state_Bit = Convert.ToInt32(row["devicestate"]);
            }
            var getState = UserCustom.GetStateByBit(state_Bit);

            //var stateBit = db.states.Where(a => a.deviceid.Equals(id)).Select(a => a.devicestate).FirstOrDefault();
            //var getState = UserCustom.GetStateByBit(stateBit);

            var mymodel = new V_Current_State();
            string col = null;

            //foreach (var i in state)
            //{
            //    string[] words = i.color.ToString().Split(' ');
            //    col = "rgb(" + words[0] + "," + words[1] + "," + words[2] + ")";

            //    mymodel.data.Add(new Tuple<string, string, string>(i.deviceid, col, i.state));

            //}
            if (getState != null)
            {
                string[] words = getState.color.ToString().Split(' ');
                col = "rgb(" + words[0] + "," + words[1] + "," + words[2] + ")";
                mymodel.data.Add(new Tuple<string, string, string>(id, col, getState.messagetext));
            }
            if (col == null)
            {
                mymodel.data.Add(new Tuple<string, string, string>("0000", "Black", "No State Available"));
            }

            return PartialView("_ShowState", mymodel);
        }

        [HttpGet]
        public ActionResult getcomp(String id)
        {
            if (Session["UserID"] == null)
            {
                //return RedirectToAction("Index", "Login");
                return Content("Logout");
            }


            //string query = "SELECT a.description, d.messagetext, b.componentid, b.timestamp, b.orgmessage, b.servertimestamp, b.messageno, b.eventno " +
            //               "FROM lastcomponentevent b " +
            //               "join statsid a on a.id = b.compstate " +
            //               "join component c on c.componentid = b.componentid and c.ForMonitoring = 1 " +
            //               "join message0001 d on d.textno = c.textno " +
            //               "where b.deviceid = '" + id + "' " +
            //               "order by b.timestamp desc";


            string query = "select top 1 with ties t.componentid, t.description, t.eventno, t.messageno, t.messagetext, t.orgmessage, t.servertimestamp, t.timestamp from ( " +
                            "SELECT a.description, d.messagetext, b.componentid, b.timestamp, b.orgmessage, b.servertimestamp, b.messageno, b.eventno FROM " +
                            "lastcomponentevent b join statsid a on a.id = b.compstate join component c on c.componentid = b.componentid and c.ForMonitoring = 1 " +
                            "join message0001 d on d.textno = c.textno where b.deviceid = 'DN100'  ) t order by ROW_NUMBER() OVER(PARTITION BY t.messagetext ORDER BY timestamp desc)";

            //query = query.Substring(0, query.Length - 6);
            DataSet dt = new DataSet();
            string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }

            if (!(dt.Tables[0].Rows.Count > 0))
            {
                TempData["Alert"] = "No Data Exists";
            }


            List<lastcomponentevent> levents = new List<lastcomponentevent>();

            foreach (DataRow row in dt.Tables[0].Rows)
            {

                DateTime timestamp = DateTime.Now;
                if (row["timestamp"] != DBNull.Value)
                {
                    string td = Convert.ToDateTime(row["timestamp"]).ToString("yyyy-MM-dd HH:mm:ss tt");
                    timestamp = DateTime.ParseExact(td, "yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture);
                }
                DateTime servertimestamp = DateTime.Now;
                if (row["servertimestamp"] != DBNull.Value)
                {
                    string td = Convert.ToDateTime(row["servertimestamp"]).ToString("yyyy-MM-dd HH:mm:ss tt");
                    servertimestamp = DateTime.ParseExact(td, "yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture);
                }

                lastcomponentevent levent = new lastcomponentevent
                {
                    description = row["description"].ToString(),
                    componentid = Convert.ToInt32(row["Componentid"]),
                    messagetext = row["messagetext"].ToString(),
                    timestamp = timestamp,
                    orgmessage = row["orgmessage"].ToString(),
                    servertimestamp = servertimestamp,
                    messageno = Convert.ToInt32(row["messageno"]),
                    eventno = Convert.ToInt32(row["eventno"])

                };
                levents.Add(levent);
            }

            levents = levents
                .GroupBy(m => new { m.messagetext, m.componentid, m.timestamp })
                .Select(g => g.First())
                .ToList();


            return PartialView("_ShowComp", levents);
        }

        public ActionResult getLastEvent(String id)
        {
            if (Session["UserID"] == null)
            {
                //return RedirectToAction("Index", "Login");
                return Content("Logout");
            }

            //string query = "SELECT Top 1 timestamp, messageno, orgmessage, servertimestamp, devicestate, eventno FROM lastevent where deviceid = '" + id + "' order by timestamp desc";
            string query = @" SELECT top 1 with ties t.timestamp, t.messageno, t.orgmessage, t.servertimestamp, t.devicestate, t.eventno 
							 from
							 (
							 SELECT timestamp, messageno, orgmessage, servertimestamp, devicestate, eventno FROM lastevent where deviceid = '" + id + "' and timestamp = (select max(timestamp) from lastevent where deviceid = '" + id + "') " +
                             ") t order by ROW_NUMBER() " +
                            "OVER(PARTITION BY t.eventno ORDER BY timestamp)";

            DataSet dt = new DataSet();
            string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }
            List<lastevent> levents = new List<lastevent>();
            if (dt.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dt.Tables[0].Rows)
                {
                    DateTime timestamp = DateTime.Now;
                    if (row["timestamp"] != DBNull.Value)
                    {
                        string td = Convert.ToDateTime(row["timestamp"]).ToString("yyyy-MM-dd HH:mm:ss tt");
                        timestamp = DateTime.ParseExact(td, "yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture);
                    }
                    DateTime servertimestamp = DateTime.Now;
                    if (row["servertimestamp"] != DBNull.Value)
                    {
                        string td = Convert.ToDateTime(row["servertimestamp"]).ToString("yyyy-MM-dd HH:mm:ss tt");
                        servertimestamp = DateTime.ParseExact(td, "yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture);
                    }
                    int eventno = 0;
                    if (row["eventno"] != DBNull.Value)
                    {
                        eventno = Convert.ToInt32(row["eventno"]);
                    }

                    lastevent ls = new lastevent
                    {
                        timestamp = timestamp,
                        messageno = Convert.ToInt32(row["messageno"]),
                        orgmessage = row["orgmessage"].ToString(),
                        servertimestamp = servertimestamp,
                        devicestate = Convert.ToInt32(row["devicestate"]),
                        eventno = eventno
                    };
                    levents.Add(ls);

                }

                lastevent le = levents.FirstOrDefault();
                levents.Clear();
                levents.Add(le);
            }



            if (!(dt.Tables[0].Rows.Count > 0))
            {
                TempData["Alert"] = "No Data Exists";
            }



            return PartialView("_ParviewLastEvent", levents);
        }

        public ActionResult JobResult()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("43");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " navigate to Job Result");

            Int64 leave = 0;
            Int64 next = 100;
            String Jobid = "ALL Job";
            ViewBag.Leave = leave.ToString();
            ViewBag.jobid2 = "None";
            ViewBag.AllRadio = true;
            ViewBag.SuRadio = false;
            ViewBag.FRadio = false;
            ViewBag.CRadio = true;
            ViewBag.URadio = false;
            ViewBag.SRadio = false;
            ViewBag.RRadio = false;
            ViewBag.Time = "00:00:00";
            ViewBag.Date = DateTime.Now.ToShortDateString();
            ViewBag.Date = DateTime.Now.ToShortDateString();
            //GetSqlJrALL(leave, next, DTime, radioCheck, 0);
            GetSqlJrALL(leave, next, DTime, radioCheck, Jobid, 0, "All");
            var job1 = db.jobs.Select(x => x.jobid).Distinct();
            ViewBag.job = job1;
            return View("JobResult", evt);

        }

        public ActionResult JobResultReport(String L, String Dev, String R, String D)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("43");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            String DeviceID = Dev;
            Int64 leave = Convert.ToInt64(L);
            Int64 next = 100;
            ViewBag.Leave = leave.ToString();

            if (D != null)
            {
                String date = D.Split('|')[0];
                String time = D.Split('|')[1];
                date = date.Replace('/', '-');
                DateTime DTime = Convert.ToDateTime(date + " " + time);
                radioCheck = R;
                if (DeviceID == "ALL")
                {
                    IEnumerable<JobRData> result;
                    if (DTime != null && R == "Until")//Until
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp ,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                    + " from jobresult jr where jr.jobtsp <= '" + DTime + "'"
                                     + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                    + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                    + "OFFSET " + leave + " ROWS "
                                    + "FETCH NEXT " + next + " ROWS ONLY";

                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                    }
                    else if (DTime != null && R == "Since")//Since
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                   + " from jobresult jr where jr.jobtsp >= '" + DTime + "'"
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                   + "OFFSET " + leave + " ROWS "
                                   + "FETCH NEXT " + next + " ROWS ONLY";


                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                    }
                    else if (DTime != null && R == "Range")//Range
                    {
                        String SDTF = D.Split('|')[0];
                        String SDTT = D.Split('|')[1];

                        ViewBag.DTF = SDTF;
                        ViewBag.DTT = SDTT;

                        DateTime DTF = DateTime.ParseExact(SDTF, "yyyy/MM/dd HH:mm",
                                               System.Globalization.CultureInfo.InvariantCulture);
                        DateTime DTT = DateTime.ParseExact(SDTT, "yyyy/MM/dd HH:mm",
                                               System.Globalization.CultureInfo.InvariantCulture);


                        var cnt = dbPro.jobresults.Where(a => a.jobtsp >= DTF && a.jobtsp <= DTT).Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                   + " from jobresult jr where jr.jobtsp between '" + DTF + "' and '" + DTT + "'"
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid ";


                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                    }
                    else
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails'"
                                   + " from jobresult jr "
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                           + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";

                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = DateTime.Now.ToShortDateString();

                    }

                    foreach (var x in result)
                    {
                        String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        string b = x.JobID.Trim();
                        string c = x.ResourceID.Trim();

                        evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                    }

                    int z = 100 - evt.JrData.Count();
                    TempData["JrCount"] = (leave + 100) - z;
                    evt.deviceid = "ALL";
                }
            }
            else
            {
                if (DeviceID == "ALL")
                {
                    IEnumerable<JobRData> result;
                    if (DTime != null && R == "Until")//Until
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp ,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                    + " from jobresult jr where jr.jobtsp <= '" + DTime + "'"
                                     + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                    + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                    + "OFFSET " + leave + " ROWS "
                                    + "FETCH NEXT " + next + " ROWS ONLY";

                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                    }
                    else if (DTime != null && R == "Since")//Since
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                   + " from jobresult jr where jr.jobtsp >= '" + DTime + "'"
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                   + "OFFSET " + leave + " ROWS "
                                   + "FETCH NEXT " + next + " ROWS ONLY";


                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
                    }
                    else
                    {
                        var cnt = dbPro.jobresults.Count();
                        TempData["JrTCount"] = cnt;

                        var sql = " select jr.jobid,jr.resourceid,case when jr.result=0 then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS varchar) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,case when jr.result=1 then null  else jr.commandno end as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails'"
                                   + " from jobresult jr "
                                  + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                  + "  ORDER BY jr.timestamp desc, jr.resourceid "
                           + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";

                        result = dbPro.Database.SqlQuery<JobRData>(sql);
                        ViewBag.Date = DateTime.Now.ToShortDateString();

                    }

                    foreach (var x in result)
                    {
                        String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        string b = x.JobID.Trim();
                        string c = x.ResourceID.Trim();

                        evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                    }

                    int z = 100 - evt.JrData.Count();
                    TempData["JrCount"] = (leave + 100) - z;
                    evt.deviceid = "ALL";
                }
            }

            var report = new PartialViewAsPdf("~/Views/Event/_JobResultRep.cshtml", evt);
            return report;
        }

        //Event configuration Page View
        public ActionResult EventConfiguration()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("60");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " navigate to Event Configuration");


            message0001 Eventconf = new message0001();

            var d = from t1 in db.eventbases
                    join t2 in db.message0001 on new { t1.textno, t1.texttype } equals new { t2.textno, t2.texttype }
                    orderby t2.textno
                    select new { t2.textno, t2.messagetext };

            foreach (var x in d.Skip(1))
            {

                Eventconf.Data.Add(new Tuple<int, string>(x.textno, x.messagetext));
            }

            Eventconf.eventComponents = db.message0001.Where(x => x.texttype.Equals(7)).OrderBy(x => x.messagetext).ToList();


            var sql = "select s.bit, s.color, m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype in (3,1) order by prio";

            var result = db.Database.SqlQuery<SurveilAI.Models.Data2>(sql);

            foreach (var x in result)
            {

                Eventconf.state.Add(x);
            }

            return View(Eventconf);
        }

        //Details of particular event
        public JsonResult EventInformation(string id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            int eventno = Int32.Parse(id);

            //fetching eventdetails on behalf of eventno

            var EventDetails = from e in db.eventbases
                               where e.eventno == eventno
                               select e;

            //fetching component id

            int componentid = EventDetails.Select(x => x.componentid).FirstOrDefault();

            //fetching component textno

            var compdetails = from c in db.components
                              where c.componentid == componentid
                              select c;

            var comptextno = compdetails.Select(x => x.textno).FirstOrDefault();


            //fetching component name;

            var componenttext = from m in db.message0001
                                where m.textno == comptextno
                                select m;
            var obj = new { Details = EventDetails.ToList(), Results = componenttext.ToList() };

            //return Json(EventDetails.ToList(), JsonRequestBehavior.AllowGet);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        //Update event Or New Event Add
        [HttpPost]
        public ActionResult EventSubmitForm(FormCollection formCollection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                if (formCollection["confidential"] == "false")
                { formCollection["masktype"] = "-1"; formCollection["confidentialmask"] = "NULL"; formCollection["confidential"] = "0"; }
                else { formCollection["confidential"] = "1"; }
                if (formCollection["submitButton"] == "UpdateEvent")
                {
                    activitylog.Info(Session["UserID"].ToString() + " is Updating event no" + formCollection["eventno"]);

                    string compsetbit = formCollection["CompSetCheck"] ?? "0";
                    string compunsetbit = formCollection["CompReSetCheck"] ?? "0";
                    int sumcompsetbit = 0;
                    int sumcompresetbit = 0;
                    string Components = formCollection["Components"];
                    if (Components == "1")
                    {
                        if (compsetbit != "0")
                        {
                            string[] seperator = compsetbit.Split(',');
                            for (int i = 0; i < seperator.Length; i++)
                            {
                                sumcompsetbit += int.Parse(seperator[i]);
                                compsetbit = sumcompsetbit.ToString();
                            }
                        }
                        if (compunsetbit != "0")
                        {
                            string[] seperator = compunsetbit.Split(',');
                            for (int i = 0; i < seperator.Length; i++)
                            {
                                sumcompresetbit += int.Parse(seperator[i]);
                                compunsetbit = sumcompresetbit.ToString();
                            }
                        }
                    }
                    else if (Components == "2")
                    {
                        compunsetbit = "7";
                    }
                    var component = int.Parse(formCollection["SelectedComponent"]);
                    var componentid = db.components.Where(x => x.textno == component).SingleOrDefault();
                    formCollection["eventgroupid"] = formCollection["eventgroupid"] == "" ? "0" : formCollection["eventgroupid"];
                    formCollection["forwarddesktop"] = formCollection["forwarddesktop"] == "false" ? "0" : "1";
                    formCollection["forwardrule"] = formCollection["forwardrule"] == "false" ? "0" : "1";
                    String Query = "UPDATE eventbase SET setbit = '" + formCollection["SetBitMask"] + "', unsetbit = '" + formCollection["ResetBitMask"] + "', componentid = '" + componentid.componentid + "' ,compsetbit = '" + compsetbit + "', compunsetbit = '" + compunsetbit + "', target = '" + formCollection["target"] + "', forwarddesktop = '" + formCollection["forwarddesktop"] + "' , forwardrule = '" + formCollection["forwardrule"] + "', eventgroupid = '" + formCollection["eventgroupid"] + "', confidential = '" + formCollection["confidential"] + "', confidentialmask = '" + formCollection["confidentialmask"] + "', masktype = '" + formCollection["masktype"] + "' WHERE eventno = '" + formCollection["eventno"] + "'";
                    int output = db.Database.ExecuteSqlCommand(Query);

                    if (output > 0)
                    {
                        @TempData["OKMsg"] = "Event Updated Successfully!";
                        userlog.Info(Session["UserID"].ToString() + " : event updated successfully , event no = " + formCollection["eventno"]);
                        Log("Event Update", "", 10005001, "$eventid: " + formCollection["eventno"]);
                        return RedirectToAction("EventConfiguration", "Event");
                    }
                    else
                    {
                        @TempData["NoMsg"] = "Event Update Failed";
                        errorlog.Error(Session["UserID"].ToString() + " Event Update Failed " + output);
                        Log("Event Update Failed", "", 10005002, "$eventid: " + formCollection["eventno"]);
                        return RedirectToAction("EventConfiguration", "Event");
                    }

                }
                else if (formCollection["submitButton"] == "newEvent")
                {
                    activitylog.Info(Session["UserID"].ToString() + " is creaing new event, event no : " + formCollection["eventno"]);
                    formCollection["Neweventgroupid"] = formCollection["Neweventgroupid"] == "" ? "0" : formCollection["Neweventgroupid"];
                    formCollection["Newforwarddesktop1"] = formCollection["Newforwarddesktop1"] == "false" ? "0" : "1";
                    formCollection["Newforwardrule1"] = formCollection["Newforwardrule1"] == "false" ? "0" : "1";

                    int Tmessage0001 = db.Database.ExecuteSqlCommand("insert into message0001(textno,texttype,messagetext)" +
                        "Values('" + formCollection["Neweventno"].ToString() + "','1','" + formCollection["NewEventText"] + "')");

                    if (Tmessage0001 > 0)
                    {
                        int Teventbase = db.Database.ExecuteSqlCommand("insert into eventbase(eventno,textno,texttype,setbit,unsetbit,componentid,compsetbit,compunsetbit,target,forwarddesktop,forwardrule,eventgroupid,confidential,confidentialmask,masktype)" +
                           "Values('" + formCollection["Neweventno"].ToString() + "','" + formCollection["Neweventno"].ToString() + "','1','0', '0' ,'0','0','0','" + formCollection["Newtarget"] + "','" + formCollection["Newforwarddesktop1"] + "','" + formCollection["Newforwardrule1"] + "','" + formCollection["Neweventgroupid"] + "','0','NULL','-1')");
                        if (Teventbase > 0)
                        {
                            userlog.Info(Session["UserID"].ToString() + " Event created Successfully , Event No = " + formCollection["Neweventno"].ToString());
                            @TempData["OKMsg"] = "Event created";
                            activitylog.Info(Session["UserID"].ToString() + "Event configured");
                            Log("Event Update", "", 10005001, "$eventid: " + formCollection["eventno"]);
                            return RedirectToAction("EventConfiguration", "Event");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Event configuration failed";
                            errorlog.Error(Session["UserID"].ToString() + "Event configuration failed in eventbase");
                            Log("Event Update Failed", "", 10005002, "$eventid: " + formCollection["eventno"]);
                            return RedirectToAction("EventConfiguration", "Event");
                        }

                    }
                    else
                    {
                        @TempData["NoMsg"] = "Event configuration failed";
                        errorlog.Error(Session["UserID"].ToString() + "Event configuration failed while inserting in message0001");
                        Log("Event Update Failed", "", 10005002, "$eventid: " + formCollection["eventno"] + " $reason: Event configuration failed while inserting in message0001");
                        return RedirectToAction("EventConfiguration", "Event");
                    }
                }
                return RedirectToAction("EventConfiguration", "Event");

            }
            catch (Exception ex)
            {
                @TempData["NoMsg"] = "Event configuration failed";
                errorlog.Error(Session["UserID"].ToString() + "Event configuration failed");
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                Log("Event Update Failed", "", 10005002, "$eventid: " + formCollection["eventno"] + " $ex-msg: " + ex.Message);
                return RedirectToAction("EventConfiguration", "Event");
            }
        }

        //Delete Event
        [HttpPost]
        public ActionResult DeleteEvent(FormCollection formCollection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                var eventno = formCollection["Deleventno"].ToString();
                var eventText = formCollection["DelEventText"].ToString();

                int Fromeventbase = db.Database.ExecuteSqlCommand("DELETE FROM eventbase where textno= '" + eventno + "' AND texttype='1'");
                if (Fromeventbase > 0)
                {
                    int Frommessage001 = db.Database.ExecuteSqlCommand("DELETE FROM message0001 where textno= '" + eventno + "' AND messagetext ='" + eventText + "' AND texttype='1'");

                    if (Frommessage001 > 0)
                    {
                        @TempData["OKMsg"] = "Event Deleted successfully";
                        userlog.Info(Session["UserID"].ToString() + " Deleting event no " + eventno + " successfully");
                        Log("Event Delete", "", 10005003, "$event: " + eventno);
                        return RedirectToAction("EventConfiguration", "Event");
                    }
                    else
                    {
                        @TempData["OKMsg"] = "Deleting event from eventbase failed";
                        userlog.Info(Session["UserID"].ToString() + " Deleting event no from eventbase eventno" + eventno + " failed");
                        Log("Event Delete Failed", "", 10005004, "$event: " + eventno);
                        return RedirectToAction("EventConfiguration", "Event");
                    }

                }
                else
                {
                    @TempData["OKMsg"] = "Deleting event failed";
                    activitylog.Info(Session["UserID"].ToString() + " failed to delete event no " + eventno);
                    Log("Event Delete Failed", "", 10005004, "$event: " + eventno);
                    return RedirectToAction("EventConfiguration", "Event");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                return RedirectToAction("EventConfiguration", "Event");
            }
        }

        [HttpGet]
        public ActionResult PCstate(String id)
        {
            activitylog.Info(Session["UserID"].ToString() + " navigate to pc-state");
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string harddisk = obj.GetDiskDriveInfo(id, 912);
                string[] seperator = harddisk.Split(',');
                TempData["check"] = null;
                if (seperator[0] == "Success")
                {
                    string PhysicalMemory = obj.GetPhysicalMemory(id, 911);


                    string lanInfo = obj.GetLANInfo(id, 914);
                    string CPUInfo = obj.GetCPUInfo(id, 915);
                    string[] seperator2 = lanInfo.Split(',');
                    string[] seperator3 = CPUInfo.Split(',');
                    if (harddisk.Contains("Please find attached Drive Info"))
                    {
                        userlog.Info(Session["UserID"].ToString() + " fetching drive info successful of Device :" + id);

                        ViewBag.HDDspace = seperator;
                        ViewBag.lanInfo = seperator2;
                        ViewBag.CPUInfo = seperator3;
                        TempData["check"] = "1";
                        TempData["harddisk"] = "0";

                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error while fetching drive info  : " + harddisk);

                        ViewBag.HDDspace = "";
                        TempData["harddisk"] = "1";
                    }
                    if (lanInfo.Contains("Success,Please find attached LAN Info"))
                    {
                        userlog.Info(Session["UserID"].ToString() + " fetching lan info Successful of Device :" + id);
                        ViewBag.lanInfo = seperator2;
                        TempData["lanInfo"] = "0";
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error while fetching lan info  : " + lanInfo);

                        TempData["lanInfo"] = "1";
                    }
                    if (CPUInfo.Contains("Success,Please find attached"))
                    {
                        userlog.Info(Session["UserID"].ToString() + " fetching cpu info Successful of Device :" + id);

                        CPUInfo = CPUInfo.Substring(CPUInfo.IndexOf("1%%") + 3);
                        string[] seperator1 = CPUInfo.Split(',');
                        ViewBag.CPUInfo = seperator1;
                        TempData["CPUInfo"] = "0";
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error while fetching cpu info  : " + CPUInfo);

                        TempData["CPUInfo"] = "1";
                    }
                    if (PhysicalMemory.Contains("Success,Please find attached"))
                    {
                        userlog.Info(Session["UserID"].ToString() + " fetching physical memory info Successful of Device :" + id);

                        PhysicalMemory = PhysicalMemory.Substring(PhysicalMemory.IndexOf("1%%") + 3);
                        PhysicalMemory = PhysicalMemory.Replace(":BANK", "-");
                        //PhysicalMemory = PhysicalMemory.Remove(PhysicalMemory.LastIndexOf(':'), 1);
                        string[] seperator1 = PhysicalMemory.Split(',');
                        ViewBag.PhyMemory = seperator1;
                        TempData["PhysicalMemory"] = "0";
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error while fetching physical memory info  : " + PhysicalMemory);
                        ViewBag.PhyMemory = "";
                        TempData["PhysicalMemory"] = "1";
                    }
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching PC-state : " + harddisk);
                    TempData["check"] = "0";
                    TempData["ErrMsg"] = "Network issue";
                }

                return PartialView("_PCstate");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult GetServices(String id)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                TempData["check"] = "0";
                activitylog.Info(Session["UserID"].ToString() + " fetching services of device : " + id);

                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching Services Successful of Device :" + id);

                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching services: " + services);

                    TempData["ErrMsg"] = "Error occurred, Please check connectivity";
                    TempData["check"] = "0";
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult GetServicesSecond(String id)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                int commandno = 903;
                var services = obj.GetServicesSecond(id, commandno);
                //var services = "";
                TempData["check"] = "0";
                activitylog.Info(Session["UserID"].ToString() + " fetching services of device : " + id);

                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching Services Successful of Device :" + id);

                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching services: " + services);

                    TempData["ErrMsg"] = "Error occurred, Please check connectivity";
                    TempData["check"] = "0";
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult GetProcesses(String id)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string processes = obj.GetProcesses(id, 902);
                TempData["check"] = null;
                activitylog.Info(Session["UserID"].ToString() + " fetching process of device : " + id);

                if (processes.Contains("Success,Please find attached all Processes"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching Process Successful of Device :" + id);
                    processes = processes.Replace("Success,Please find attached all Processes: 1%%", "");
                    string[] seperator = processes.Split(',');
                    ViewBag.ProcessList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching services: " + processes);
                    TempData["ErrMsg"] = "Error occurred, Please check connectivity";
                    TempData["check"] = "0";
                }

                return PartialView("_ProcessList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }
        [HttpGet]
        public ActionResult GetProcessessSecond(String id)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string processes = obj.GetProcessesSecond(id, 902);
                //string processes = "";
                TempData["check"] = null;
                activitylog.Info(Session["UserID"].ToString() + " fetching process of device : " + id);

                if (processes.Contains("Success,Please find attached all Processes"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching Process Successful of Device :" + id);
                    processes = processes.Replace("Success,Please find attached all Processes: 1%%", "");
                    string[] seperator = processes.Split(',');
                    ViewBag.ProcessList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching services: " + processes);
                    TempData["ErrMsg"] = "Error occurred, Please check connectivity";
                    TempData["check"] = "0";
                }

                return PartialView("_ProcessList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult KillProcessSecond(String id, string Process)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string flag = obj.SecondAgentKillProcess(id, Process, 908);
                string processes = obj.GetProcessesSecond(id, 902);
                //string processes = "";
                TempData["check"] = null;
                activitylog.Info(Session["UserID"].ToString() + " fetching process of device : " + id);

                if (processes.Contains("Success,Please find attached all Processes"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching Process Successful of Device :" + id);
                    processes = processes.Replace("Success,Please find attached all Processes: 1%%", "");
                    string[] seperator = processes.Split(',');
                    ViewBag.ProcessList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching services: " + processes);
                    TempData["ErrMsg"] = "Error occurred, Please check connectivity";
                    TempData["check"] = "0";
                }

                return PartialView("_ProcessList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult PvInventory(String id)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " fetching inventory of device :" + id);

                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string VersionProp = obj.GetVersionProp(id, 909);
                string HWInvent = obj.GetHWInvent(id, 910);

                string OS = obj.GetOSInfo(id, 913);
                TempData["CheckVersionProp"] = "null";
                TempData["check"] = "null";
                TempData["CheckHWInvent"] = "null";
                TempData["CheckOS"] = "null";
                if (VersionProp.Contains("Success,Please find attached Version Properties"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching version properties Successful of Device :" + id);

                    VersionProp = VersionProp.Replace("Success,Please find attached Version Properties: 1%%", "");
                    string[] seperatorVP = VersionProp.Split(',');
                    TempData["CheckVersionProp"] = seperatorVP;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching version properties : " + VersionProp);

                    TempData["ErrMsg"] = "Connectivity issue";
                }
                if (HWInvent.Contains("Success,Please find attached Inventory"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching hardware inventory Successful of Device :" + id);

                    HWInvent = HWInvent.Replace("Success,Please find attached Inventory: 1%%", "");
                    string[] seperatorHWInvent = HWInvent.Split(',');
                    TempData["CheckHWInvent"] = seperatorHWInvent;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching hardware inventory : " + HWInvent);

                    TempData["ErrMsg"] = "Connectivity issue";

                }
                if (OS.Contains("Success,Please find attached Operating System Info"))
                {
                    userlog.Info(Session["UserID"].ToString() + " fetching  operating system info Successful of Device :" + id);

                    OS = OS.Replace("Success,Please find attached Operating System Info: 1%%", "");
                    string[] seperatorOS = OS.Split(',');
                    TempData["CheckOS"] = seperatorOS;
                    TempData["check"] = "1";

                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error fetching hardware inventory : " + OS);

                    TempData["ErrMsg"] = "Connectivity issue";
                }
                return PartialView("_HWInvent");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public ActionResult GetJournal(string id, string journal, string ServerPath)
        {
            try
            {
                TempData["DeviD"] = id;

                activitylog.Info(Session["UserID"].ToString() + " uploading journal file to server from device :" + id);
                string path = WebConfigurationManager.AppSettings["Upload"];
                if (ServerPath == "")
                {
                    path = WebConfigurationManager.AppSettings["Upload"];
                }
                else
                {
                    path = WebConfigurationManager.AppSettings["Upload"] + "\\" + ServerPath;
                }
                string html;
                IMSFService.IMSFService a = new IMSFService.IMSFService();
                string journalfile = a.DownloadJournalfile(id, "C:\\journal\\" + journal, 901);
                string[] seperator = journalfile.Split(',');
                if (!System.IO.Directory.Exists(path))
                {

                    html = "Upload failed , folder or path on server does not exists";

                }
                else
                {
                    if (seperator[0] == "Success")
                    {
                        if (journalfile.Contains("Success,Journal File Transferred with filename"))
                        {
                            userlog.Info(Session["UserID"].ToString() + " uploading journal file " + journal + " to server from device :" + id + " successful");
                            //Read file to byte array
                            byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                            //Begins the process of writing the byte array back to a file
                            path = path + '\\' + journal;
                            using (System.IO.Stream file = System.IO.File.OpenWrite(path))
                            {
                                file.Write(FileBytes, 0, FileBytes.Length);
                            }
                            html = "Uploaded successful";
                            Log("File Upload", "", 10006001, "$upload-path: " + path);
                        }
                        else
                        {
                            errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + journal + " , " + journalfile);
                            html = "file not transferred, Please check connectivity";
                            Log("File Upload Failed", "", 10006002, "$upload-path: " + path);
                        }
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + journal + " , " + journalfile);
                        html = "Connection to machine failed";
                    }
                }

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public ActionResult Status(StatusBar collection)
        {
            string id = ""; string fname = ""; string userid = "";
            collection.DeviceID = id;
            collection.FileName = fname;
            collection.UserID = userid;
            StatusBar objstat = new StatusBar();
            var statusdet = objstat;
            int b = 0;
            while (statusdet == null && b > 10)
            {
                Task.Delay(500);
                statusdet = db.StatusBars.Where(a => a.UserID == "Usman" && a.DeviceID == "0002" && a.FileName == "ABCD.txt").OrderByDescending(a => a.TimeStamp).FirstOrDefault();
                objstat = statusdet;
                b++;
            }
            return View("StatusBar", objstat);
        }

        public ActionResult DownloadFile(String fname, string id, string dir, string ServerPath)
        {
            try
            {
                string html = "";
                if (ServerPath == "")
                {
                    ServerPath = WebConfigurationManager.AppSettings["Upload"];
                }
                else
                {
                    ServerPath = WebConfigurationManager.AppSettings["Upload"] + "\\" + ServerPath;
                }

                if (!System.IO.Directory.Exists(ServerPath))
                {
                    html = "Invalid path";
                }
                else
                {
                    activitylog.Info(Session["UserID"].ToString() + " uploading file to server from device :" + id);

                    TempData["DeviD"] = id;

                    IMSFService.IMSFService web = new IMSFService.IMSFService();
                    string UserID = Session["UserID"].ToString();
                    //string File = web.Downloadfile(id, dir, fname, 320, UserID, DateTime.Now);
                    string File = web.Downloadfile(id, dir, fname, 320, UserID, DateTime.Now);

                    if (File.Contains("Success,File Transferred with filename"))
                    {
                        string[] seperator = File.Split(',');
                        if (seperator[0] == "Success")
                        {
                            if (seperator.Length > 2)
                            {
                                userlog.Info(Session["UserID"].ToString() + " uploading file " + fname + " to server from device :" + id + " successful");

                                //Read file to byte array
                                byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                                //string path = "D:\\IMS Server\\Upload\\" + fname;
                                string path = ServerPath + "\\" + fname;

                                //Begins the process of writing the byte array back to a file
                                using (System.IO.Stream file = System.IO.File.OpenWrite(path))
                                {
                                    file.Write(FileBytes, 0, FileBytes.Length);
                                }
                                html = "Uploaded successfull on path " + path;
                            }
                            else
                            {
                                errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fname + " , " + File);

                                html = "file transfer failed";
                            }
                        }
                        else
                        {
                            errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fname + " , " + File);

                            html = "Error uploading file";
                        }
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fname + " , " + File);

                        html = "Error uploading file";
                    }
                }
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public class UploadFilesResult
        {
            public string Name { get; set; }
            public int Length { get; set; }
            public string Type { get; set; }
        }

        public ActionResult UploadFile(string id, string dir, string Mpath)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " downloading file from server to device :" + id);
                string Spath = WebConfigurationManager.AppSettings["Download"] + "\\" + dir;
                FileAttributes attr = System.IO.File.GetAttributes(Spath);
                string html = "";
                Mpath = Mpath == "" ? "C:\\Proagent\\" : Mpath;
                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)

                {
                    html = "Directory is selecetd , Please select a file";
                    return Content(html, "text/html");
                }
                else
                {
                    System.IO.FileStream stream = System.IO.File.OpenRead(Spath);

                    //string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
                    //int counter = 0;
                    //decimal number = (decimal)stream.Length;
                    //while (Math.Round(number / 1024) >= 1)
                    //{
                    //    number = number / 1024;
                    //    counter++;
                    //}
                    //string length = string.Format("{0:n1}{1}", number, suffixes[counter]);

                    byte[] fileBytes = new byte[stream.Length];
                    stream.Read(fileBytes, 0, fileBytes.Length);
                    stream.Close();
                    string base64string = Convert.ToBase64String(fileBytes);




                    TempData["DeviD"] = id;
                    var Filename = dir.Substring(dir.LastIndexOf('\\') + 1);
                    IMSFService.IMSFService obj = new IMSFService.IMSFService();
                    string UserID = Session["UserID"].ToString();

                    obj.Timeout = -1;
                    string File = obj.Uploadfile(id, Mpath, Filename, 321, base64string, "y", UserID, DateTime.Now);

                    string[] seperator = File.Split(',');
                    if (File.Contains("Success,File Transferred with filename"))
                    {
                        userlog.Info(Session["UserID"].ToString() + " downloading file " + Filename + " to device : " + id + " successful");
                        Log("File Upload", id, 10006001, "$upload-path: " + Mpath);

                        html = Filename + " transferred successful";
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + Filename + " , " + File);

                        errorlog.Error("Error: " + File);
                        html = Filename + " sending failed";
                    }

                    return Content(html, "text/html");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);

                return Content("Error occured , check logs ", "text/html");

            }
        }

        public ActionResult ViewFile(String id, string dir, string fileName)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " Viewing file " + fileName + " from device device :" + id);

                TempData["DeviD"] = id;
                string html = "";
                IMSFService.IMSFService web = new IMSFService.IMSFService();
                string VFile = web.Downloadfile(id, dir, fileName, 320, "", DateTime.Now);

                if (VFile.Contains("Success,File Transferred with filename"))
                {
                    string[] seperator = VFile.Split(',');
                    if (seperator[0] == "Success")
                    {
                        if (seperator.Length > 2)
                        {
                            activitylog.Info(Session["UserID"].ToString() + " File View successful from device : " + id);
                            //Read file to byte array
                            byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                            var obj = new { name = fileName, data = seperator[2], data2 = System.Net.Mime.MediaTypeNames.Application.Octet };
                            Log("File Download", id, 10006003, "$file-download-filiename: " + fileName + "$file-download-dir: " + dir);
                            return Json(obj, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            errorlog.Error("User: " + Session["UserID"] + " Error viewing file : " + VFile);
                            html = "file transfer failed";
                            Log("File Download failed", id, 10006004, "$file-download-filiename: " + fileName + "$file-download-dir: " + dir);
                        }
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error viewing file : " + VFile);
                        html = "Error uploading file";
                        Log("File Download", id, 10006004, "$file-download-filiename: " + fileName + "$file-download-dir: " + dir);
                    }
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error viewing file : " + VFile);
                    html = "Error uploading file";
                    Log("File Download", id, 10006004, "$file-download-filiename: " + fileName + "$file-download-dir: " + dir);
                }


                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public ActionResult FileDownload(FormCollection form)
        {
            try
            {
                string html = "";
                var fileName = form["Rootdir"];
                var DrivePath = form["ViewFileFolder"];
                var mainRoot = form["mainRoot"];
                var id = form["DevID"];
                if (DrivePath == fileName)
                {
                    DrivePath = mainRoot;
                }
                activitylog.Info(Session["UserID"].ToString() + " downloading file from server to device :" + id);
                IMSFService.IMSFService web = new IMSFService.IMSFService();
                string Downloadfile = web.Downloadfile(id, DrivePath, fileName, 320, "", DateTime.Now);
                if (Downloadfile.Contains("Success,File Transferred with filename"))
                {
                    string[] seperator = Downloadfile.Split(',');
                    if (seperator[0] == "Success")
                    {
                        if (seperator.Length > 2)
                        {
                            userlog.Info(Session["UserID"].ToString() + " downloading file " + fileName + " from device : " + id + " successful");
                            //Read file to byte array
                            byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                            Log("File Download", id, 10006003, "$file-download: " + DrivePath + " $fileName: " + fileName);
                            return File(FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                        }
                        else
                        {
                            errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fileName + " , " + Downloadfile);
                            html = "file transfer failed";
                            Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                        }
                    }
                    else
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fileName + " , " + Downloadfile);
                        html = "file transfer failed";
                        Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                    }
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error uploading journal file " + fileName + " , " + Downloadfile);
                    html = "file transfer failed";
                    Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                }
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        public ActionResult StartProg(String Devid, string dir, string Fname, string mRoot)
        {
            try
            {
                string html = "";
                var fileName = Fname;
                var DrivePath = dir;
                var mainRoot = mRoot;
                var id = Devid;
                if (DrivePath == fileName)
                {
                    DrivePath = mainRoot;
                }
                else
                {
                    DrivePath = DrivePath + "\\" + fileName;
                }
                activitylog.Info(Session["UserID"].ToString() + " downloading file from server to device :" + id);
                IMSFService.IMSFService web = new IMSFService.IMSFService();
                //string Downloadfile = web.Downloadfile(id, DrivePath, fileName, 320, "", DateTime.Now);

                string StartProgram = web.ExecuteProgram(id, DrivePath, 149, 0);

                if (StartProgram.Contains("Success,Please find attached Execute Program Status: 1%%Last Message from client: 1%%Program Executed with Output: File does not exist"))
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error Executing file " + fileName + " , " + StartProgram);
                    html = "file does not exist";
                    //Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                    Log("File Execute failed", id, 10006004, "$file-execute: " + DrivePath + " $fileName: " + fileName);
                }
                else if (StartProgram.Contains("Success,Please find attached Execute Program Status: 1%%Last Message from client: 1%%Program Executed"))
                {

                    userlog.Info(Session["UserID"].ToString() + " Executing file " + fileName + " from device : " + id + " successful");
                    //Read file to byte array
                    //byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                    Log("File Execute", id, 10006003, "$file-execute successful: " + DrivePath + " $fileName: " + fileName);
                    //return File(FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    html = "File " + fileName + " executed successfully";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error Executing file " + fileName + " , " + StartProgram);
                    html = "file execute failed";
                    //Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                    Log("File Execute failed", id, 10006004, "$file-execute: " + DrivePath + " $fileName: " + fileName);
                }
                return Content(html, "text/html");
            }

            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                //return RedirectToAction("Error", "Error");                
                return Content("Error Executing File", "text/html");

            }
        }


        public ActionResult StartProgDirect(String Devid, string mRoot)
        {
            try
            {
                string html = "";
                var mainRoot = mRoot;
                var id = Devid;
                activitylog.Info(Session["UserID"].ToString() + " downloading file from server to device :" + id);
                IMSFService.IMSFService web = new IMSFService.IMSFService();
                //string Downloadfile = web.Downloadfile(id, DrivePath, fileName, 320, "", DateTime.Now);

                string StartProgram = web.ExecuteProgram(id, mRoot, 149, 0);
                if (StartProgram.Contains("Success,Please find attached Execute Program Status: 1%%Last Message from client: 1%%Program Executed with Output: File does not exist"))
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error Executing file " + mRoot);
                    html = "file does not exist";
                    //Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                    Log("File Execute failed", id, 10006004, "$file-execute: " + mRoot);
                }
                else if (StartProgram.Contains("Success,Please find attached Execute Program Status: 1%%Last Message from client: 1%%Program Executed"))
                {

                    userlog.Info(Session["UserID"].ToString() + " Executing file " + mainRoot + " from device : " + id + " successful");
                    //Read file to byte array
                    //byte[] FileBytes = Convert.FromBase64String(seperator[2]);
                    Log("File Execute", id, 10006003, "$file-execute successful: " + mainRoot);
                    //return File(FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    html = "File executed successfully";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error Executing file " + mainRoot);
                    html = "file execute failed";
                    //Log("File Download Failed", id, 10006004, "$file-download: " + DrivePath + " $fileName: " + fileName);
                    Log("File Execute failed", id, 10006004, "$file-execute: " + mainRoot);
                }
                return Content(html, "text/html");
            }

            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                //return RedirectToAction("Error", "Error");                
                return Content("Error Executing File", "text/html");

            }
        }

        [HttpGet]
        public ActionResult StartService(String id, string Service)
        {
            try
            {
                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.StartService(id, Service, 907);
                activitylog.Info(Session["UserID"].ToString() + " is starting service :" + Service);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " starting service " + Service + " of device : " + id + " successful");
                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Start Service", id, 10007001, "$service: " + Service + " $commandno: " + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed starting service " + Service + " of device : " + id);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                    Log("Start Service Failed", id, 10007002, "$service: " + Service + " $commandno: " + commandno);
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult StopService(String id, string Service)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is stopping service :" + Service);

                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.StopService(id, Service, 917);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " stopping service " + Service + " of device : " + id + " successful");

                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Stop Servcice", id, 10007003, "$service: " + Service + " $commandno: " + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed stopping service " + Service + " of device : " + id);
                    Log("Stop Servcice Failed", id, 10007004, "$service: " + Service + " $commandno: " + commandno);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                Log("Stop Servcice Failed", id, 10007004, "$service: " + Service + " $commandno: " + 903);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult StartServiceSecond(String id, string Service)
        {
            try
            {
                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.SecondAgentStartService(id, Service, 907);
                activitylog.Info(Session["UserID"].ToString() + " is starting service  via second agent  :" + Service);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                commandno = 907;
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " starting service via second agent " + Service + " of device : " + id + " successful");
                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Start Service Second Agent", id, 10007005, "$service: " + Service + " $commandno:" + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed starting service  via second agent  " + Service + " of device : " + id);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                    Log("Start Service Failed", id, 10007006, "$service: " + Service + " $commandno:" + commandno);
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }


        [HttpGet]
        public ActionResult StopServiceSecond(String id, string Service)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is stopping service :" + Service);

                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.SecondAgentStopService(id, Service, 919);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                commandno = 919;
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " stopping service via second agent " + Service + " of device : " + id + " successful");

                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Stop Servcice", id, 10007007, "$service: " + Service + " $commandno: " + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed stopping service " + Service + " of device : " + id);
                    Log("Stop Servcice Failed", id, 10007008, "$service: " + Service + " $commandno: " + commandno);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                Log("Stop Servcice Failed", id, 10007004, "$service: " + Service + " $commandno: " + 903);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult UpdateAgent(String id, string path, string filename)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is updating agent  :" + path + "\\" + filename);
                string CompletePath = WebConfigurationManager.AppSettings["Download"] + "\\" + path;

                System.IO.FileStream stream = System.IO.File.OpenRead(CompletePath + "\\" + filename);

                byte[] fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();
                string base64string = Convert.ToBase64String(fileBytes);

                CompletePath = CompletePath + "\\" + filename;
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.UploadSecondAgentUpdater(id, "..\\", filename, 100, base64string, "Y", Session["UserID"].ToString(), DateTime.Now);

                Log("Agent Update Started ", id, 10007009, "$Agent: " + filename + " $commandno: 100");
                return Content(result);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                Log("Agent Update Failed", id, 10007010, "$Agent: " + filename + " $commandno: 100");
                return Content("Error Updating");
            }
        }

        [HttpGet]
        public ActionResult RestartMachine(String id)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is Restarting Machine ID :" + id);

                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.RestartMachine(id, 135, 5);
                activitylog.Info(Session["UserID"].ToString() + " is Restarting Machine command result:" + result);
                Log("Restart Machine ", id, 10007011, "$deviceid: " + id + " $commandno: 135");
                return Content(result);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                Log("Restart Machine Failed ", id, 10007012, "$deviceid: " + id + " $commandno: 135");

                return Content("Error Updating");
            }
        }

        [HttpGet]
        public ActionResult IMSService(String id, string Status)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is sending " + Status + " IMSService command via second agent");
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.SecondAgentServiceControl(id, Status, 102);
                activitylog.Info(Session["UserID"].ToString() + " result of " + Status + " IMSService command via second agent : " + result);
                Log("Start Service Second Agent", id, 10007013, "$service: IMSService $commandno: 102");
                return Content(result);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                Log("Start Service Failed Second Agent", id, 10007014, "$service: IMSService $commandno: 102");
                return Content("Error Please Check Logs");
                //return RedirectToAction("Error", "Error");
            }
        }

        [HttpGet]
        public ActionResult PauseService(String id, string Service)
        {
            try
            {
                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.SecondAgentPauseService(id, Service, 919);
                activitylog.Info(Session["UserID"].ToString() + " is starting service :" + Service);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " starting service " + Service + " of device : " + id + " successful");
                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Start Service", id, 10007001, "$service: " + Service + " $commandno: " + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed starting service " + Service + " of device : " + id);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                    Log("Start Service Failed", id, 10007002, "$service: " + Service + " $commandno: " + commandno);
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }



        [HttpGet]
        public ActionResult RestartService(String id, string Service)
        {
            try
            {
                Service = Service.Trim();
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                string result = obj.SecondAgentRestartService(id, Service, 918);
                activitylog.Info(Session["UserID"].ToString() + " is starting service :" + Service);
                int commandno = 903;
                var services = obj.GetServices(id, commandno);
                TempData["check"] = "null";
                if (services.Contains("Success,Please find attached all Services"))
                {
                    userlog.Info(Session["UserID"].ToString() + " starting service " + Service + " of device : " + id + " successful");
                    services = services.Replace("Success,Please find attached all Services: 1%%", "");
                    Log("Start Service", id, 10007001, "$service: " + Service + " $commandno: " + commandno);
                    string[] seperator = services.Split(',');
                    ViewBag.ServiceList = seperator;
                    TempData["check"] = "1";
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " failed starting service " + Service + " of device : " + id);
                    TempData["ErrMsg"] = "Error occurred";
                    TempData["check"] = "0";
                    Log("Start Service Failed", id, 10007002, "$service: " + Service + " $commandno: " + commandno);
                }
                return PartialView("_ServicesList");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult VFDriveRoot(String id, string path)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                var subDir = obj.GetDirFiles(id, path, 906);
                if (subDir.Contains("Success,Please find attached all Directories & Files from path"))
                {
                    subDir = subDir.Substring(subDir.IndexOf("1%%") + 3);
                    string[] listDir = subDir.Split(new string[] { "\n", "," }, StringSplitOptions.None);
                    ViewBag.RootDir = listDir;
                }
                else
                {
                    ViewBag.RootDir = "Check connectivity";
                }


                return PartialView("_SubDirectoriesVF");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult DriveRoot(String id, string path)
        {
            try
            {
                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                var subDir = obj.GetDirFiles(id, path, 906);
                if (subDir.Contains("Success,Please find attached all Directories & Files from path"))
                {
                    subDir = subDir.Substring(subDir.IndexOf("1%%") + 3);
                    string[] listDir = subDir.Split(new string[] { "\n", "," }, StringSplitOptions.None);
                    ViewBag.RootDir = listDir;
                }
                else
                {
                    ViewBag.RootDir = "Check connectivity";
                }


                return PartialView("_SubDirectories");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult ServerRoot(string path)
        {
            try
            {
                path = WebConfigurationManager.AppSettings["Download"] + "\\" + path;
                //path = @"D:\IMS Server\Download\" + path;
                string[] folder = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                if (folder.Length != 0)
                {
                    ViewBag.folder = folder;
                }
                else
                {
                    ViewBag.folder = "";
                }
                if (files.Length != 0)
                {
                    string[] authors = new string[files.Length];

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Length < 248)
                        {
                            FileInfo info = new FileInfo(files[i]);
                            authors[i] = files[i] + "*" + info.Length.ToString() + " KB";
                        }

                    }
                    authors = authors.Where(c => c != null).ToArray();

                    ViewBag.files = authors;
                }
                else
                {
                    ViewBag.files = "";
                }
                return PartialView("_ServerSubDirectories");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }

        [HttpGet]
        public ActionResult SubDir(String id, string path)
        {

            path = path.Trim();
            path = path + "\\";
            IMSFService.IMSFService obj = new IMSFService.IMSFService();
            var subDir = obj.GetDirFiles(id, path, 906);
            if (subDir.Contains("Success,Please find attached all Directories & Files from path"))
            {
                subDir = subDir.Substring(subDir.IndexOf("1%%") + 3);
                string[] listDir = subDir.Split(new string[] { "\n", "," }, StringSplitOptions.None);
                ViewBag.RootDir = listDir;
            }
            else
            {
                ViewBag.RootDir = "Check connectivity";
            }


            return PartialView("_SubDirectories");
        }
        public ActionResult dntUpdate(FormCollection collection)
        {
            Device obj1 = new Device();

            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is trying to update device time of device : " + obj1.DeviceID);

                IMSFService.IMSFService obj = new IMSFService.IMSFService();
                obj.Timeout = -1;
                obj1.DeviceID = collection["DeviceID"];
                obj1.date = collection["dnt"] ?? "0";
                var date = collection["CDate"] ?? "0";
                var time = collection["CTime"] ?? "0";

                var Dresponse = obj.SetDate(obj1.DeviceID, date, 922);
                var Tresponse = obj.SetTime(obj1.DeviceID, time, 923);
                if (Dresponse.Contains("Success,Please find attached Set Date status: 1%% Date has been set successfully") && Tresponse.Contains("Success,Please find attached Set Time status: 1%% Time has been set successfully"))
                {
                    string response = obj.GetDateTime(obj1.DeviceID, 925);
                    //string response = "Success,Please find attached Date Time: 1%%Current Date: Thursday, August 13, 2020 Current Time: 3:11:48 PM Timezone: Pakistan Standard Time Universal Time 8/13/2020 10:11:48 AM";

                    if (response.Contains("Success,Please find attached Date Time: 1%%"))
                    {
                        response = response.Substring(response.IndexOf("1%%") + 3);
                        int FirstIndex = response.IndexOf("Current Date:");
                        int SecondIndex = response.IndexOf("Current Time:");
                        int ThirdIndex = response.IndexOf("Timezone:");
                        string date2 = response.Substring(FirstIndex, SecondIndex - 1); ;
                        string time2 = response.Substring(SecondIndex, ThirdIndex - SecondIndex);
                        date2 = date2.Substring(response.IndexOf(":") + 1);
                        time2 = time2.Substring(time2.IndexOf(":") + 1);
                        DateTime MachineDate2 = Convert.ToDateTime(date2 + " " + time2); ;
                        obj1.date = string.Concat(date2, time2);
                    }
                    else
                    {
                        errorlog.Error("Error Updating date and time of ATM:" + obj1.DeviceID + " Error " + response);
                        obj1.date = "Unable to fetch Date and Time";
                    }
                }

                TempData["Success"] = "Success Updating Time";
                return View("dntUpdate", obj1);
            }
            catch (Exception ex)
            {
                obj1.DeviceID = collection["DeviceID"];
                TempData["Fail"] = "Error Updating Time";
                errorlog.Error("Error: " + ex);
                return View("dntUpdate", obj1);

            }
        }
        public ActionResult Devicefail_More(String L, String Dev, String prev)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                ViewData["DeviD"] = Dev;

                String DeviceID = Dev;
                Int64 leave = 0;
                if (prev == "TRUE")
                {
                    leave = Convert.ToInt64(L) - 100;
                    if (leave < 0)
                    {
                        leave = 0;
                    }

                }
                else
                {
                    leave = Convert.ToInt64(L) + 100;

                }

                Int64 next = 100;

                ViewBag.Leave = leave.ToString();

                DataSet ds = new DataSet();
                string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "SELECT D.deviceid,D.componentid,(select m.messagetext from message0001 m where m.textno=C.textno) As component," +
                        "D.started,D.ended,D.eventno,(select m.messagetext from message0001 m where m.textno=D.eventno) As StartEvent," +
                        "D.endeventno,(select m.messagetext from message0001 m where m.textno=D.endeventno and m.texttype = '1') As Endmessage FROM devicefail D " +
                        "INNER JOIN component C ON (C.componentid= D.componentid) where deviceid='" + DeviceID + "' order by d.started desc OFFSET " + leave + " ROWS FETCH NEXT " + next + " ROWS ONLY";


                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            sda.Fill(ds, "Table 1");
                        }
                    }
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        TempData["RetainedCards"] = ds.Tables[0].Rows[0][0];
                    }
                    else
                    {
                        TempData["RetainedCards"] = "NULL";
                    }

                }
                return View("DeviceFail", ds);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }




        }

        //ATM Dir back

        [HttpGet]
        public ActionResult MachineBack(string path, string folderT)
        {
            try
            {
                int index = path.LastIndexOf('\\');
                path = path.Substring(0, index);

                if (folderT != "Upload")
                {
                    //string split = "Download";
                    //path = path.Substring(path.IndexOf(split) + split.Length + 1);
                    if (path == "Download")
                    {
                        path = WebConfigurationManager.AppSettings["Download"];
                    }
                    else
                    {
                        string toBeSearched = "Download";
                        int ix = path.IndexOf(toBeSearched);

                        if (ix != -1)
                        {
                            path = path.Substring(ix + toBeSearched.Length);
                            // do something here
                        }
                        path = WebConfigurationManager.AppSettings["Download"] + path;
                    }
                    string lastword = path.Split('\\').LastOrDefault();
                    if (lastword != "Download")
                    {
                        string split = "Download";
                        ViewBag.path = "Download\\" + path.Substring(path.IndexOf(split) + split.Length + 1);
                    }
                    else
                    {
                        ViewBag.path = "Download\\";
                    }
                }
                else
                {
                    //string split = "Download";
                    //path = path.Substring(path.IndexOf(split) + split.Length + 1);
                    if (path == "Upload")
                    {
                        path = WebConfigurationManager.AppSettings["Upload"];
                    }
                    else
                    {
                        string toBeSearched = "Upload";
                        int ix = path.IndexOf(toBeSearched);

                        if (ix != -1)
                        {
                            path = path.Substring(ix + toBeSearched.Length);
                            // do something here
                        }
                        path = WebConfigurationManager.AppSettings["Upload"] + path;
                    }
                    string lastword = path.Split('\\').LastOrDefault();
                    if (lastword != "Upload")
                    {
                        string split = "Upload";
                        ViewBag.path = "Upload\\" + path.Substring(path.IndexOf(split) + split.Length + 1);
                    }
                    else
                    {
                        ViewBag.path = "Upload\\";
                    }
                }

                //path = WebConfigurationManager.AppSettings["Download"] + "\\" + path;
                //path = @"D:\IMS Server\Download\" + path;

                string[] folder = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                if (folder.Length != 0)
                {
                    ViewBag.folder = folder;
                }
                else
                {
                    ViewBag.folder = "";
                }
                if (files.Length != 0)
                {
                    string[] authors = new string[files.Length];

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo info = new FileInfo(files[i]);
                        authors[i] = files[i] + "*" + info.Length.ToString() + " KB";

                    }

                    ViewBag.files = authors;
                }
                else
                {
                    ViewBag.files = "";
                }
                return PartialView("_ServerSubDir");
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");

            }
        }
        public void GetServerUploadDir()
        {
            try
            {
                string path = WebConfigurationManager.AppSettings["Download"];
                //string path = @"D:\IMS Server\Download\";
                string[] folder = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                if (folder.Length != 0)
                {
                    ViewBag.folder = folder;
                }
                else
                {
                    ViewBag.folder = "";
                }
                if (files.Length != 0)
                {
                    string[] authors = new string[files.Length];

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo info = new FileInfo(files[i]);
                        authors[i] = files[i] + "*" + info.Length.ToString() + " KB";

                    }

                    ViewBag.files = authors;
                }
                else
                {
                    ViewBag.files = "";
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }
        }


        #region Filter Job result new
        public ViewResult GetSqlJrALL(Int64 leave, Int64 next, DateTime? DTF, DateTime? DTT, String Rad, String JobID, int? Report, String tp)
        {
            using (SurveilAIEntities dbPro = new SurveilAIEntities())
            {
                dbPro.Database.CommandTimeout = 300;
                string type = "";
                if (tp == "All")
                {
                    //type = " ";
                    type = " and jr.result in ('0','1')";
                }
                else if (tp == "Success")
                {
                    type = " and jr.result = '0'";
                }
                if (tp == "Fail")
                {
                    type = " and jr.result = '1'";
                }
                IEnumerable<JobRData> result;
                if (DTF != null && DTT != null && Rad == "Range")//Since
                {
                    var cnt = dbPro.jobresults.Where(a => a.jobtsp >= DTF && a.jobtsp <= DTT).Count();
                    TempData["JrTCount"] = cnt;

                    var sql = "";
                    if (JobID == "ALL Job")
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr where jr.jobtsp between '" + DTF + "' and '" + DTT + "'"
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid ";
                    }
                    else
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr where jr.jobtsp between '" + DTF + "' and '" + DTT + "'AND jr.jobid='" + JobID + "'"
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid ";

                    }



                    result = dbPro.Database.SqlQuery<JobRData>(sql);
                    ViewBag.Date = Convert.ToDateTime(DTF).ToShortDateString();
                }
                else
                {
                    string type1 = "";
                    if (tp == "All")
                    {

                        type1 = "";
                    }
                    else if (tp == "Success")
                    {
                        type1 = " where jr.result = '1'";
                    }
                    if (tp == "Fail")
                    {
                        type1 = " where jr.result = '0'";
                    }
                    if (JobID != "ALL Job")
                    {
                        if (type1 != "")
                        {
                            type1 = type1 + " and jr.jobid='" + JobID + "'";
                        }
                        else
                        {
                            type1 = "where jr.jobid='" + JobID + "'";
                        }
                    }
                    var cnt = dbPro.jobresults.Count();
                    TempData["JrTCount"] = cnt;
                    var sql = "";
                    if (JobID == "ALL Job")
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr " + type1
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";
                    }
                    else
                    {
                        sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                                + " from jobresult jr " + type1
                                 + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                                + "  ORDER BY jr.timestamp desc, jr.resourceid "
                                + "OFFSET " + leave + " ROWS "
                                + "FETCH NEXT " + next + " ROWS ONLY";
                    }


                    result = dbPro.Database.SqlQuery<JobRData>(sql);
                    ViewBag.Date = DateTime.Now.ToShortDateString();

                }

                foreach (var x in result)
                {
                    String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string b = x.JobID.Trim();
                    string c = x.ResourceID.Trim();

                    evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
                }

                int z = 100 - evt.JrData.Count();
                TempData["JrCount"] = (leave + 100) - z;
                evt.deviceid = "ALL";


                return View("JobResult", evt);
            }


        }


        public ViewResult GetSqlJr(String DeviceID, Int64 leave, Int64 next, DateTime? DTF, DateTime? DTT, String Rad, String tp)
        {
            string type = "";
            if (tp == "All")
            {
                type = " ";
            }
            else if (tp == "Success")
            {
                type = " and jr.result = '0'";
            }
            if (tp == "Fail")
            {
                type = " and jr.result = '1'";
            }
            IEnumerable<JobRData> result;
            if (DTime != null && Rad == "Range")//Since
            {
                var cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID) && a.jobtsp >= DTF && a.jobtsp <= DTT).Count();
                TempData["JrTCount"] = cnt;

                var sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                            + " from jobresult jr where jr.resourceid='" + DeviceID + "' and  jr.jobtsp between '" + DTF + "' and  '" + DTT + "'"
                             + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                            + "  ORDER BY jr.timestamp desc, jr.resourceid "
                            + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";


                result = dbPro.Database.SqlQuery<JobRData>(sql);
                ViewBag.Date = Convert.ToDateTime(DTime).ToShortDateString();
            }
            else
            {
                var cnt = dbPro.jobresults.Where(a => a.resourceid.Equals(DeviceID)).Count();
                TempData["JrTCount"] = cnt;

                var sql = " select jr.jobid,jr.resourceid,case when jr.result in (0,1) then (select distinct s.f3 from Sheet1$ s join job j on j.command=CAST(s.F1 AS int) where s.F2='20' and j.commandno=jr.commandno and j.jobid=jr.jobid)  else null end as command   ,jr.commandno as 'commandno',repeatcount as 'Attempt',jr.jobtsp,case when jr.result=0 then 'Failed' else 'Success' end as 'Result', jr.errorspec as 'ResultDetails' "
                            + " from jobresult jr where jr.resourceid='" + DeviceID + "' "
                             + " group by jr.jobid,jr.commandno,jr.resourceid,jr.repeatcount,jr.jobtsp,jr.result,jr.errorspec,jr.timestamp"
                            + "  ORDER BY jr.timestamp desc, jr.resourceid "
                            + "OFFSET " + leave + " ROWS "
                            + "FETCH NEXT " + next + " ROWS ONLY";

                result = dbPro.Database.SqlQuery<JobRData>(sql);
                ViewBag.Date = DateTime.Now.ToShortDateString();

            }

            foreach (var x in result)
            {
                String a = x.jobtsp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string b = x.JobID.Trim();
                string c = x.ResourceID.Trim();

                evt.JrData.Add(new Tuple<string, string, string, int?, int, string, Tuple<string, string>>(b, c, x.Command, x.CommandNo, x.Attempt, a, new Tuple<string, string>(x.Result, x.ResultDetails)));
            }

            int z = 100 - evt.JrData.Count();
            TempData["JrCount"] = (leave + 100) - z;
            evt.deviceid = DeviceID;


            return View("JobResult", evt);
        }

        #endregion

        [HttpPost]
        public JsonResult WriteErrorLog(string errormsg)
        {
            if (Session["UserID"] == null)
            {
                return Json("Logout", JsonRequestBehavior.DenyGet);
            }
            try
            {
                errorlog.Error("User: " + Session["UserID"] + "Hierarchy View Error : " + errormsg);
                return Json("Done", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                return Json("Error,0", JsonRequestBehavior.DenyGet);
            }

        }


        private void Log(string adminaction, string device, int command, string vardata)
        {
            pvjournal pvjournal = new pvjournal();

            pvjournal.adminaction = adminaction;
            pvjournal.device = device;
            pvjournal.cmdstat = 0;
            pvjournal.command = command;
            pvjournal.desktopuser = Session["UserID"].ToString();
            pvjournal.errorcode = 0;
            pvjournal.functionid = 0;
            pvjournal.issuer = "EVENT_CONTROLLER";
            pvjournal.issuertype = 5;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";

            Audit audit = new Audit();
            audit.Log(pvjournal);
        }
    }
}