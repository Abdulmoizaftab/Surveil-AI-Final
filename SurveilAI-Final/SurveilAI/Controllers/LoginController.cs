using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
namespace SurveilAI.Controllers
{
    public class LoginController : Controller
    {
        ILogger userlog = LogManager.GetLogger("user");
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger errorlog = LogManager.GetLogger("error");
        //CryptoJsHelper crypto = new CryptoJsHelper();

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    // Do whatever here...
        //    String Checking = "I am here";
        //}


        SurveilAIEntities db = new SurveilAIEntities();
        // GET: Login
        public ActionResult Index()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2);
            string displayableVersion = $"{version} ({buildDate})";
            Assembly thisAssem = typeof(LoginController).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            Version ver = thisAssemName.Version;
            ViewBag.Version = string.Format("This is version {0:0.0.0.0} of Innomate.", ver);
            if (TempData["AlreadyLogged"] != null)
            {
                string Userid = TempData["UserId"].ToString();
                string Pword = TempData["Pword"].ToString();
                ViewBag.LoggedIn = "Yes";
                ViewBag.Userid = Userid;
                ViewBag.Pword = Pword;
                TempData["UserId"] = Userid;

            }
            TempData["AlreadyLogged"] = null;
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult EndUserSession()
        {
            try
            {
                string Userid = @TempData["UserId"].ToString();
                ViewBag.LoggedIn = "";
                db.Database.ExecuteSqlCommand("Update Users set IsOnline = 'false' where  UserID = '" + Userid + "'");
                db.Database.ExecuteSqlCommand("update usersession set sessionflag = 0 where username = '" + Userid + "'");
                userlog.Info(Session["UserID"] + " Log out");
                Session.Abandon();
                string response = "Logout";
                return Json(response, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Authorize(User user)
        {
            HttpContext httpContext = System.Web.HttpContext.Current;
            try
            {
                user.Password = CryptoJsHelper.DecryptStringAES(user.PasswordE);
                //DateTime LicenseDate = LicenseManagement.LicenseDateCheck();//start

                //if (LicenseDate == DateTime.ParseExact("01-01-2000", "dd-MM-yyyy", null))
                //{
                //    @TempData["signuperror"] = "Invalid License";
                //    userlog.Info(user.UserID + "Invalid License");
                //    Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment:Invalid License");
                //    return RedirectToAction("Index", "Login");
                //}
                //else if (DateTime.Now > LicenseDate)
                //{
                //    @TempData["signuperror"] = "License Expired";
                //    userlog.Info(user.UserID + " License Expired");
                //    Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment:License Expired");
                //    return RedirectToAction("Index", "Login");
                //} //end

                string URL = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                user.LastUrl = user.LastUrl == URL + "/" ? URL + @"/User/UserDashBoard" : (user.LastUrl = user.LastUrl ?? "Dashboard");


                user.LastUrl = ReplaceURL(user.LastUrl, "EJRepPost", "EJRep");
                user.LastUrl = ReplaceURL(user.LastUrl, "Event/Monitoring", "Event/Event");
                user.LastUrl = ReplaceURL(user.LastUrl, "DevAssignPost", "DevAssign");
                user.LastUrl = ReplaceURL(user.LastUrl, "Login/FPasswordChange", "User/Userdashboard");


                //ServerManager mgr = new ServerManager();
                //string SiteName = HostingEnvironment.ApplicationHost.GetSiteName();
                //Site currentSite = mgr.Sites[SiteName];

                //The following obtains the application name and application object
                //The application alias is just the application name with the "/" in front

                string ApplicationAlias = HostingEnvironment.ApplicationVirtualPath;
                string ApplicationName = ApplicationAlias.Substring(1);
                if (user.LastUrl == URL + "/" + ApplicationName + "/" || user.LastUrl == URL + "/" + ApplicationName)
                {
                    user.LastUrl = URL + "/" + ApplicationName + "/User/UserDashBoard";
                }

                string UserIdFromCookies = GetUserNameFromCookies();
                if (!string.IsNullOrEmpty(UserIdFromCookies) && UserIdFromCookies != user.UserID)
                {
                    user.LastUrl = URL + "/" + ApplicationName + "/User/UserDashBoard";
                }
                userlog.Info(user.UserID + " : trying to login");


                bool ldap = false;
                string usererror = "";
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    String pass = user.Encrypt(user.Password, "IPLIIS1234");

                    //string encryPass = "B1OXlQX9shQ27jngxnn+pw==";
                    //string password = user.Decrypt(encryPass, System.Configuration.ConfigurationManager.AppSettings["EncryptKey"].ToString());
                    var usrdetail = db.Users.Where(a => a.UserID.Equals(user.UserID)).FirstOrDefault();
                    if (usrdetail != null)
                    {
                        if (usrdetail.UserID != user.UserID)
                        {
                            usrdetail = null;
                        }
                    }

                    if (usrdetail != null)
                    {
                        var role = db.UserSecurities.Where(a => a.AccountType == usrdetail.AccountType).SingleOrDefault();
                        var CheckRole = db.UserPermissions.Where(a => a.Licenseon == true).Select(x => x.ID).ToList();
                        //var CheckRole = db.UserPermissions.Where(a => a.Licenseon == true).Select(x => x.Index).ToList();


                        if (usrdetail != null && usrdetail.Ldap == true)
                        {
                            ldap = LDAPLOGIN(user.UserID, user.Decrypt(pass, System.Configuration.ConfigurationManager.AppSettings["EncryptKey"].ToString()));

                            if (ldap == false)
                            {
                                usrdetail = null;
                                Session["LDAP"] = "false";
                            }
                            else
                            {
                                Session["LDAP"] = "true";
                            }
                        }
                        else
                        {
                            usrdetail = db.Users.Where(a => a.UserID.Equals(user.UserID) && a.Password.Equals(pass)).FirstOrDefault();
                            Session["LDAP"] = "false";
                        }
                        var passPolicy = role.Passwordpolicy.Split(',');
                        string minAgePw = passPolicy[5];
                        string maxAgePw = passPolicy[6];
                        string maxPwAttemps = passPolicy[4];

                        if (usrdetail != null || Session["LDAP"].ToString() == "true")
                        {

                            int Att = Convert.ToInt32(usrdetail.Attempts);

                            int? isonline = db.usersessions.Where(x => x.extuserid == user.UserID).Select(x => x.sessionflag).FirstOrDefault();
                            if (isonline != null && isonline == 1)
                            {
                                @TempData["UserId"] = user.UserID;
                                @TempData["Pword"] = CryptoJsHelper.EncryptStringAES(user.Password);
                                @TempData["AlreadyLogged"] = "Already Logged In!";
                                return RedirectToAction("Index", "Login");
                            }
                            //if (usrdetail.IsOnline == true)
                            //{
                            //    @TempData["signuperror"] = "Your Account Is Already Logged In";
                            //    return RedirectToAction("Index", "Login");
                            //}
                            //else 
                            if (Att > int.Parse(maxPwAttemps) || usrdetail.IsLocked == true)
                            {
                                Log("Login Failed", "", 10000002, "$User: " + user.UserID + " $Reason : Max Attempts or User is Locked ");
                                @TempData["signuperror"] = "Your Account Is Locked. Please Contact Your Administrator";
                                usererror = "Your Account Is Locked. Please Contact Your Administrator";
                                userlog.Info(user.UserID + " account Is Locked");
                                @TempData["AlreadyLogged"] = null;
                                return RedirectToAction("Index", "Login");
                            }
                            else
                            {

                                if (role.Access == null)
                                {
                                    Log("Login Failed", "", 10000002, "$User: " + user.UserID + " $Reason : No Roles Assigned ");
                                    @TempData["signuperror"] = "Account Role Issue. Please Contact System Administrator";
                                    userlog.Info(user.UserID + "Account roles not assigned.");
                                    @TempData["AlreadyLogged"] = null;
                                    return RedirectToAction("Index", "Login");
                                }
                                Session["UserID"] = usrdetail.UserID.ToString();
                                Session["UserSessionID"] = Session.SessionID;
                                Session["Role"] = usrdetail.AccountType.ToString();
                                Session["LastLogin"] = usrdetail.LastLogin.ToString();

                                List<string> acs = role.Access.Split(',').ToList();
                                acs = acs.OrderBy(x => int.Parse(x)).ToList();
                                List<object> sessions = new List<object>();

                                foreach (var x in acs)
                                {
                                    for (int i = 0; i < CheckRole.Count; i++)
                                    {

                                        if (CheckRole[i].ToString() == x)
                                        {
                                            sessions.Add(x);
                                        }
                                    }

                                }
                                Session["UserRole"] = sessions;

                                Session["Fview"] = role.Fview;
                                Session["Iview"] = role.Iview;
                                Session["Rview"] = role.Rview;
                                Session["Uview"] = role.Uview;
                                Session["Hview"] = role.Hview;
                                Session["Aview"] = role.Aview;
                                Session["AAview"] = role.AAview;
                                Session["SRview"] = role.SRview;
                                Session["R1view"] = role.R1view;
                                Session["R2view"] = role.R2view;
                                Session["R3view"] = role.R3view;
                                Session["Hadd"] = role.Hadd;
                                Session["Hdel"] = role.Hdel;
                                Session["Hupd"] = role.Hupd;
                                Session["Uadd"] = role.Uadd;
                                Session["Udel"] = role.Udel;
                                Session["Uupd"] = role.Uupd;
                                Session["Aadd"] = role.Aadd;
                                Session["Adel"] = role.Adel;
                                Session["Aupd"] = role.Aupd;
                                Session["EJview"] = role.EJview;
                                Session["REJview"] = role.REJview;
                                Session["DEJview"] = role.DEJview;
                                Session["Jview"] = role.Jview;
                                Session["JRview"] = role.JRview;
                                Session["Mview"] = role.Mview;



                                double day = (DateTime.Today.Date - Convert.ToDateTime(usrdetail.LastDateChange).Date).TotalDays;
                                DateTime d1 = Convert.ToDateTime(usrdetail.LastDateChange);
                                d1 = d1.AddDays(40);
                                if (usrdetail.PassUDPrompt == false)
                                {
                                    //if (day > 40 && Session["LDAP"].ToString() != "true")
                                    if (day > int.Parse(minAgePw) && Session["LDAP"].ToString() != "true")
                                    {
                                        //if (day > 45)
                                        if (day > int.Parse(maxAgePw))
                                        {
                                            Log("Login Failed", "", 10000002, "$User: " + user.UserID + " $Reason : Password Expired ");
                                            db.Database.ExecuteSqlCommand("Update Users set Attempts = '" + 0 + "' , IsOnline = '" + true + "' , LastLogin = '" + DateTime.Now + "' where  UserID = '" + user.UserID + "'");
                                            TempData["errormessage2"] = "We strongly recommend that you change your password";
                                            userlog.Info(user.UserID + " password expired!");
                                            return RedirectToAction("FPasswordChange", "Login");
                                        }
                                        db.Database.ExecuteSqlCommand("Update Users set Attempts = '" + 0 + "' , IsOnline = '" + true + "' , LastLogin = '" + DateTime.Now + "' where  UserID = '" + user.UserID + "'");
                                        TempData["PassAlert"] = "Consider Changing Your Password";
                                        Log("Login ", "", 10000001, "$User: " + user.UserID + " $Comment: User is loged in , Password about to expire ");
                                        userlog.Info(user.UserID + " password about to expired!");
                                        userlog.Info(user.UserID + " : log in");
                                        //return RedirectToAction("UserDashBoard", "User");
                                        UpdateSession(user.UserID, httpContext);
                                        return Redirect(user.LastUrl);
                                    }
                                    else
                                    {
                                        Log("Login ", "", 10000001, "$User: " + user.UserID + " $Comment: Logged in ");
                                        db.Database.ExecuteSqlCommand("Update Users set Attempts = '" + 0 + "' , IsOnline = '" + true + "' , LastLogin = '" + DateTime.Now + "' where  UserID = '" + user.UserID + "'");
                                        userlog.Info(user.UserID + " Log in");
                                        //return RedirectToAction("UserDashBoard", "User");
                                        UpdateSession(user.UserID, httpContext);

                                        return Redirect(user.LastUrl);
                                    }
                                }
                                else
                                {
                                    Log("Login Failed ", "", 0, "$User: " + user.UserID + " $Reason : Password Expired ");
                                    TempData["errormessage2"] = "Change your password";
                                    userlog.Info(user.UserID + " password expired!");
                                    userlog.Info(user.UserID + " navigate to password");
                                    return RedirectToAction("FPasswordChange", "Login");
                                }
                            }
                        }
                        else
                        {
                            var usrdetail2 = db.Users.Where(a => a.UserID.Equals(user.UserID)).FirstOrDefault();
                            if (usrdetail2 != null)
                            {
                                int x = Convert.ToInt32(usrdetail2.Attempts) + 1;
                                db.Database.ExecuteSqlCommand("Update Users set Attempts = '" + x + "' where  UserID = '" + user.UserID + "'");
                                if (x > 3)
                                {
                                    db.Database.ExecuteSqlCommand("Update Users set IsLocked = '" + true + "' where  UserID = '" + user.UserID + "'");
                                    userlog.Info(user.UserID + " is locked due to failed attempts more than 5!");
                                    Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment:  is locked due to failed multiple attempts");
                                    usererror = "Your Account Is Locked. Please Contact Your Administrator";
                                }
                                userlog.Info(user.UserID + " password fail attempt: " + x);
                            }
                            if (usrdetail != null && usrdetail2.Ldap == true)
                            {
                                Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment: Invalid Username or Password, Or LDAP server is down!");
                                @TempData["signuperror"] = "Invalid Username or Password, Or LDAP server is down!";
                                userlog.Info(user.UserID + " Invalid Username or Password, Or LDAP server is down!");
                                @TempData["AlreadyLogged"] = null;
                            }

                            else
                            {
                                if (usererror == "")
                                {
                                    @TempData["signuperror"] = "Invalid Username or Password";
                                    @TempData["AlreadyLogged"] = null;
                                }
                                else
                                {
                                    @TempData["signuperror"] = usererror;
                                }
                                userlog.Info(user.UserID + " Invalid Username or Password");
                                Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment:Invalid Username or Password");
                            }

                            return RedirectToAction("Index", "Login");
                        }
                    }
                    else
                    {
                        @TempData["signuperror"] = "User does not exist";
                        userlog.Info(user.UserID + " User does not exist");
                        Log("Login Failed ", "", 10000002, "$User: " + user.UserID + " $Comment:User does not exist");
                        return RedirectToAction("Index", "Login");
                    }

                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error505", "Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);                
            }

        }

        public bool LDAPLOGIN(String userID, String pass)
        {

            String adPath = Path.Combine("LDAP://", System.Configuration.ConfigurationManager.AppSettings["LdapDomain"].ToString());

            LdapAuthentication adAuth = new LdapAuthentication(adPath);

            if (adAuth.IsAuthenticated(System.Configuration.ConfigurationManager.AppSettings["LdapDomain"].ToString(), userID, pass) == true)
            {
                return true;

            }
            else
            {
                return false;

            }
        }

        public ActionResult LogOut()
        {
            try
            {
                SaveUserNameInCookies();
                if (Session["UserID"].ToString() == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                string username = Session["UserID"].ToString();
                Log("Logout", "", 10000003, "$User: " + username + " $Comment: logged out");
                db.Database.ExecuteSqlCommand("Update Users set IsOnline = 'false' where  UserID = '" + username + "'");
                db.Database.ExecuteSqlCommand("update usersession set sessionflag = 0 where username = '" + username + "'");
                userlog.Info(Session["UserID"] + " Log out");
                //User obj = new User();
                //obj.LastUserLogged = Session["UserID"].ToString();
                Session.Abandon();

                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                //Log("Logout", "", 10000004, "$User: " + Session["UserID"].ToString() + " $error: " + ex);

                errorlog.Error("Error: " + ex);
                return RedirectToAction("Index", "Login");
                //return RedirectToAction("Error505", "Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

        }

        public ActionResult PasswordChange()
        {
            if (IsSessionExpired())
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to password change");
            }
            catch (Exception ex)
            {

            }


            return View();
        }

        public ActionResult FPasswordChange()
        {
            if (TempData.ContainsKey("errormessage"))
            {
                ViewBag.errormessage = TempData["errormessage"].ToString();
            }


            activitylog.Info(Session["UserID"].ToString() + " navigate to password change");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(User user)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                string URL = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                user.Password = CryptoJsHelper.DecryptStringAES(user.PasswordE);
                user.newPassword = CryptoJsHelper.DecryptStringAES(user.newPasswordE);
                user.newPassword2 = CryptoJsHelper.DecryptStringAES(user.newPassword2E);


                activitylog.Info(Session["UserID"].ToString() + " navigate to F passworchange ");
                if (user.newPassword.Equals(user.newPassword2))
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        String pass = user.Encrypt(user.Password, "IPLIIS1234");
                        User obj = new User();
                        String extpass = obj.HashIt(user.Password);
                        String usr = Session["UserID"].ToString();
                        string pass2 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass2).FirstOrDefault();
                        string pass3 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass3).FirstOrDefault();
                        string pass4 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass4).FirstOrDefault();
                        var CheckPass = db.Users.Where(a => a.UserID.Equals(usr) && a.Password.Equals(pass)).FirstOrDefault();
                        if (CheckPass != null)
                        {
                            String newPass = user.Encrypt(user.newPassword, "IPLIIS1234");
                            var usrdetail = db.Users.Where(a => a.UserID.Equals(usr) && !(a.Pass1.Equals(newPass)) && !(a.Pass2.Equals(newPass)) && !(a.Pass3.Equals(newPass)) && !(a.Pass4.Equals(newPass)) && !(a.Pass5.Equals(newPass))).FirstOrDefault();
                            if (usrdetail != null)
                            {

                                int output = db.Database.ExecuteSqlCommand("Update Users set password = '" + newPass + "' , Pass1 = '" + newPass + "' , Pass2 = '" + pass + "' , Pass3 = '" + pass2 + "', Pass4 = '" + pass3 + "', Pass5 = '" + pass4 + "', LastDateChange = '" + DateTime.Now + "', PassUDPrompt = '" + false + "' where  UserID = '" + usr + "'");
                                if (output > 0)
                                {
                                    db.Database.ExecuteSqlCommand("Update extuser Set password = '" + extpass + "' , pwdchanged = '" + DateTime.Now + "' where extuserid = '" + usr + "'");
                                    Log("Password Update", "", 10000005, "$User: " + usr + " $Comment: Password changed successfully");
                                    @TempData["DoneMsg"] = "Password Updated Successfully!";
                                    activitylog.Info(usr + " Password Updated Successfully!");
                                    return RedirectToAction("PasswordChange", "Login");
                                }
                                else
                                {
                                    Log("Password Update Failed", "", 10000006, "$User: " + usr + " $Comment: Password changed unsuccessful");
                                    @TempData["errormessage2"] = "Password Updated UnSuccessfull!";
                                    activitylog.Info(usr + " Password Updated UnSuccessfull!");
                                }
                            }
                            else
                            {
                                Log("Password Update Failed", "", 10000006, "$User: " + usr + " $Comment: Repeated Password");
                                TempData["errormessage2"] = "You Cannot Repeat Your Previous 5 Passwords!";
                                userlog.Info(usr + " You Cannot Repeat Your Previous 5 Passwords!");
                            }
                            if (CheckPass.PassUDPrompt == true)
                            {
                                if (user.LastUrl == "PasswordChange")
                                {
                                    return RedirectToAction("PasswordChange", "Login");
                                }
                                else
                                {
                                    return RedirectToAction("FPasswordChange", "Login");
                                }

                            }
                            else
                            {
                                return RedirectToAction("PasswordChange", "Login");
                            }
                        }
                        else
                        {
                            Log("Password Update Failed", "", 10000006, "$User: " + usr + " $Comment: Incorrect password");
                            TempData["errormessage2"] = "Incorrect Password!";
                            userlog.Info(usr + " Incorrect Password!");
                            if (user.LastUrl == "PasswordChange")
                            {
                                return RedirectToAction("PasswordChange", "Login");
                            }
                            else
                            {
                                return RedirectToAction("FPasswordChange", "Login");
                            }
                        }

                    }
                }
                else
                {
                    Log("Password Update Failed", "", 10000006, "$User: " + user.UserID + " $Comment: passwords do not match");
                    TempData["errormessage2"] = "Passwords do not match";
                    userlog.Info(Session["UserID"].ToString() + " Passwords do not match");
                    if (user.LastUrl == "PasswordChange")
                    {
                        return RedirectToAction("PasswordChange", "Login");
                    }
                    else
                    {
                        return RedirectToAction("FPasswordChange", "Login");
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error505", "Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

        }

        public ActionResult DashBoard()
        {
            return RedirectToAction("UserDashBoard", "User");
        }

        public void UpdateSession(string UserId, HttpContext context)
        {

            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    DateTime? LastActivityTime = GetLastActivityTime(UserId);
                    DateTime LoginTime = DateTime.Now;
                    int SessionFlag = 1;
                    string ServerIP = GetLocalIpAddress();
                    string UserIp = context.Request.UserHostAddress;
                    string Desktop = "0"; //0 for web user, 1 for desktop user
                    int Port = 9992;
                    if (UserIp != null)
                    {
                        if (UserIp == "::1")
                        {
                            UserIp = "localhost";
                        }
                    }
                    else
                    {
                        UserIp = "";
                    }
                    string sessionId = context.Session.SessionID;
                    Tuple<bool, string> Results = GetSessionAndExtID(UserId);
                    string Domain = Results.Item2;
                    int SessionQryRslt = 0;
                    if (!Results.Item1)
                    {
                        SessionQryRslt = db
                            .Database
                            .ExecuteSqlCommand(@"insert into usersession 
                                (domain, extuserid, desktop, ipaddress, port, logintime, lastactiontime, terminalserver, sessionflag, username, sessionid)
                                values('" + Domain + "', '" + UserId + "', '" + Desktop + "', '" + UserIp + "', '" + Port + "', '" + LoginTime + "', '"
                                    + LastActivityTime + "', '" + ServerIP + "', '" + SessionFlag + "', '" + UserId + "', '" + sessionId + "')");
                    }
                    else
                    {

                        int count = db.usersessions.Where(x => x.extuserid == UserId).Count();
                        if (count > 1)
                        {
                            string querydel = "delete from usersession where extuserid = '" + UserId + "' and logintime not in (select max(logintime) from usersession where extuserid = '" + UserId + "')";
                            int res = db
                                    .Database
                                    .ExecuteSqlCommand(querydel);
                        }
                        string query = @"update usersession 
                                set domain = '" + Domain + "'," +
                                    "extuserid = '" + UserId + "'," +
                                    " desktop = '" + Desktop + "'," +
                                    " ipaddress = '" + UserIp + "'," +
                                    " port = '" + Port + "'," +
                                    " logintime = '" + LoginTime + "'," +
                                    " lastactiontime = '" + LastActivityTime + "'," +
                                    " terminalserver = '" + ServerIP + "'," +
                                    " sessionflag = '" + SessionFlag + "'," +
                                    " username = '" + UserId + "'," +
                                    " sessionid = '" + sessionId + "' where username = '" + UserId + "'";

                        SessionQryRslt = db
                            .Database
                            .ExecuteSqlCommand(query);
                    }
                    if (SessionQryRslt > 0)
                    {
                        Log("Login ", "", 10000001, "$User: " + UserId + " $Comment: User has successfully logged in with IP Address: " + UserIp);
                    }

                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }

        }


        public static string GetLocalIpAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        public Tuple<bool, string> GetSessionAndExtID(string UserId)
        {
            bool UserSession = false;
            string Domain = "";
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    var result = db
                            .usersessions
                            .Select(x => new { x.extuserid })
                            .Where(y => y.extuserid.Equals(UserId)).FirstOrDefault();
                    if (result != null)
                    {
                        UserSession = true;
                    }
                    var UsrDomain = db
                            .extusers
                            .Select(x => new { x.domain, x.extuserid })
                            .Where(y => y.extuserid.Equals(UserId)).FirstOrDefault();
                    Domain = UsrDomain.domain;

                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }

            return Tuple.Create(UserSession, Domain);
        }

        public DateTime? GetLastActivityTime(string UserId)
        {
            DateTime? LastActivityTime = DateTime.Now;
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    LastActivityTime = db
                        .pvjournals
                        .Select(x => new { serverdatetime = (DateTime?)x.serverdatetime, x.desktopuser })
                        .Where(y => y.desktopuser.Equals(UserId))
                        .Max(m => m.serverdatetime);
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }
            return LastActivityTime;
        }

        public static string ReplaceURL(string URL, string find, string replace)
        {
            if (URL.Contains(find))
            {
                string pattern = "\\b" + find + "\\b";
                URL = Regex.Replace(URL, pattern, replace);
                return URL;
            }
            return URL;
        }

        public bool IsSessionExpired()
        {
            if (Session["UserSessionID"] != null)
            {
                string userid = Session["UserID"].ToString();
                string CurrentSessionId = Session["UserSessionID"].ToString();
                CurrentSessionId = CurrentSessionId.Trim();
                string SessionIdFromDB = db.usersessions.Where(x => x.extuserid == userid).Select(x => x.sessionid).FirstOrDefault();

                if (CurrentSessionId == SessionIdFromDB.Trim() && (!string.IsNullOrEmpty(SessionIdFromDB)))
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

        #region Cookies
        public void SaveUserNameInCookies()
        {
            try
            {
                string UserName = Session["UserID"].ToString();
                HttpCookie userInfo = new HttpCookie("userInfo");
                userInfo["UserName"] = UserName;
                userInfo.Expires.Add(new TimeSpan(24, 0, 0));
                Response.Cookies.Add(userInfo);
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }
        }

        public string GetUserNameFromCookies()
        {
            string UserName = "";
            try
            {
                HttpCookie reqCookies = Request.Cookies["userInfo"];
                if (reqCookies != null)
                {
                    UserName = reqCookies["UserName"].ToString();
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
            }
            return UserName;
        }

        #endregion
        private void Log(string adminaction, string device, int command, string vardata)
        {
            pvjournal pvjournal = new pvjournal();

            pvjournal.adminaction = adminaction;
            pvjournal.device = device;
            pvjournal.cmdstat = 0;
            pvjournal.command = command;
            pvjournal.desktopuser = "";
            pvjournal.errorcode = 0;
            pvjournal.functionid = 0;
            pvjournal.issuer = "AUTHORIZATION";
            pvjournal.issuertype = 6;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "Authentication";

            Audit audit = new Audit();
            audit.Log(pvjournal);
        }

    }

    public class LogActionFilterAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                string Userid = filterContext.HttpContext.Session["UserID"].ToString();
                if (!string.IsNullOrEmpty(Userid))
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        DateTime dt = DateTime.Now;
                        string ActivityTimeStamp = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                        db.Database.ExecuteSqlCommand("Update Users set LastActivity = GETDATE() where  UserID = '" + Userid + "'");
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Log("OnActionExecuting", filterContext.RouteData);
        }
    }
}