using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;


namespace SurveilAI.Controllers
{

    public class UserController : Controller
    {
        SurveilAIEntities db = new SurveilAIEntities();
        // GET: User
        ILogger userlog = LogManager.GetLogger("user");
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger errorlog = LogManager.GetLogger("error");
        User obj = new User();
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

        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                userlog.Info("Session timed-out redirected to login page");
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("4");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigated to Users");
                var mymodel = new User();
                mymodel.users = db.Users.ToList();
                //foreach (var item in users)
                //{
                //    mymodel.UserData.Add(new Tuple<String, String, bool?, bool?, bool?, bool?, String, Tuple<String>>(item.UserID, item.AccountType, item.IsOnline, item.PassUDPrompt, item.IsLocked, item.Ldap, item.FirstName, new Tuple<String>(item.LastName)));
                //}

                var accTypeList = new SelectList(db.UserSecurities.ToList(), "", "AccountType");

                ViewData["DBUserAcc"] = accTypeList;
                return View(mymodel);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }


        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("5");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                if (collection.Ldap == false)
                {
                    collection.newPassword = CryptoJsHelper.DecryptStringAES(collection.newPasswordE);
                    collection.newPassword2 = CryptoJsHelper.DecryptStringAES(collection.newPassword2E);
                }

                String pass = null;
                String extpass = null;
                if (collection.Ldap == true || collection.newPassword.Equals(collection.newPassword2))
                {
                    activitylog.Info(Session["UserID"].ToString() + " is creating new user");
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        String usr = collection.UserID.ToString().Trim(' ');
                        if (collection.Ldap != true)
                        {
                            pass = collection.Encrypt(collection.newPassword, "IPLIIS1234");
                            extpass = obj.HashIt(collection.newPassword);
                        }
                        String accType = collection.AccountType.ToString();
                        int online, locked, passPrompt, ldap;
                        ldap = online = locked = 0;
                        if (collection.IsOnline == true)
                        {
                            online = 1;
                        }
                        else
                        {
                            online = 0;
                        }

                        if (collection.IsLocked == true)
                        {
                            locked = 1;
                        }
                        else
                        {
                            locked = 0;
                        }

                        if (collection.PassUDPrompt == true && collection.Ldap != true)
                        {
                            passPrompt = 1;
                        }
                        else
                        {
                            passPrompt = 0;
                        }
                        if (collection.Ldap == true)
                        {
                            ldap = 1;
                        }
                        else
                        {
                            ldap = 0;
                        }

                        string Query = string.Format("insert into users(UserID, Password, AccountType, IsOnline, LastDateChange, PassUDPrompt, IsLocked, Ldap)  Values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", usr, pass, accType, online, DateTime.Now, passPrompt, locked, ldap);

                        int output = db.Database.ExecuteSqlCommand(Query);
                        if (output > 0)
                        {
                            Log("User Creation", "", 10002001, "$uid: " + usr);
                            int output1 = db.Database.ExecuteSqlCommand("insert into extuser(domain,extuserid,userid,isuser,password,amethod,admin,locked,lockeduntil,failedlogins,pwdexpires,pwdchanged,policyid,validfrom,validto,contactid,lastlogontime,showonlogin,usergroup,failedtries) " +
                            "Values('','" + usr + "', 'user', '1','" + extpass + "', '0', '0', '0', null, '0', null,'" + DateTime.Now + "', '0', null, null, 'user', null, '0', null, '0')");
                            @TempData["OKMsg"] = "User Account Created Successfully!";
                            userlog.Info(Session["UserID"].ToString() + " : user successfully created account '" + usr + "'");
                            Log("User Account Creation", "", 10002003, "$uid: " + usr);
                            return RedirectToAction("Index", "User");
                        }
                        else
                        {
                            Log("User Creatation", "", 10002002, "$uid: " + usr);
                            Log("User Account Creatation", "", 10002004, "$uid: " + usr);
                            @TempData["NoMsg"] = "User Account Creation Unsuccessful!";
                            errorlog.Error(Session["UserID"].ToString() + " User Account Created Unsuccessful");
                            return RedirectToAction("Index", "User");
                        }
                    }
                }
                else
                {
                    TempData["PassError"] = "Passwords do not match";
                    errorlog.Error(Session["UserID"].ToString() + " Passwords do not match");
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                if (ex.ToString().Contains("Violation of PRIMARY KEY constraint"))
                {
                    @TempData["NoMsg"] = "Cannot Insert duplicate Records!";
                }
                else
                {
                    @TempData["NoMsg"] = "User was not Created!";
                }
                return RedirectToAction("Index", "User");
            }
        }

        //Checking if user Exists
        [HttpPost]
        public JsonResult AlreadyRegistered(string UserID, string initialUser)
        {

            return Json(UserExists(UserID, initialUser), JsonRequestBehavior.AllowGet);
        }

        public bool UserExists(string UserID, string initialUser)
        {
            string user = UserID;
            string user2 = initialUser;

            bool status = true;

            //List<Device> RegisterdDevice = new List<Device>();

            if (user2 == "NewUser")
            {
                var RegDevID = (from users in db.Users
                                where users.UserID.ToUpper() == user.ToUpper()
                                select new { users.UserID }).FirstOrDefault();


                if (RegDevID != null)
                {
                    //Already registered  
                    status = false;
                }
                else
                {
                    //Available to use  
                    status = true;
                }
            }
            else
            {
                status = true;
            }


            return status;
        }

        // GET: User/Edit/5
        public ActionResult Edit(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("6");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is editing user " + id);
                var accTypeList = db.UserSecurities.Select(a => a.AccountType).ToList();
                ViewBag.dbUserAcc = accTypeList;

                var acc = db.Users.Where(a => a.UserID.Equals(id)).FirstOrDefault();

                ViewBag.acc = acc.AccountType;

                obj.IsOnline = acc.IsOnline;
                obj.IsLocked = acc.IsLocked;
                obj.Ldap = acc.Ldap;
                obj.PassUDPrompt = acc.PassUDPrompt;
                obj.FirstName = acc.FirstName;
                obj.LastName = acc.LastName;

                ViewBag.id = id;

                return View(obj);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(User collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("6");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                if (collection.Ldap == false)
                {
                    collection.newPassword = CryptoJsHelper.DecryptStringAES(collection.newPasswordE);
                    collection.newPassword2 = CryptoJsHelper.DecryptStringAES(collection.newPassword2E);
                }
                String pass = null;
                String extpass = null;
                var accTypeList = new SelectList(db.UserSecurities.ToList(), "", "AccountType");
                ViewData["DBUserAcc"] = accTypeList;
                activitylog.Info(Session["UserID"].ToString() + " updated user data and is posting");
                String usr = collection.UserID.ToString();

                if (collection.Ldap == true || collection.newPassword.Equals(collection.newPassword2))
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        if (collection.Ldap != true)
                        {
                            pass = collection.Encrypt(collection.newPassword, "IPLIIS1234");
                            extpass = obj.HashIt(collection.newPassword);
                        }
                        String oldID = collection.idOld.ToString();
                        String accType = collection.AccountType.ToString();
                        activitylog.Info(Session["UserID"].ToString() + " is editing user " + collection.UserID);
                        var usrdetail = db.Users.Where(a => a.UserID.Equals(oldID) && !(a.Pass1.Equals(pass)) && !(a.Pass2.Equals(pass))).FirstOrDefault();

                        string pass1 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass1).FirstOrDefault();
                        string pass2 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass2).FirstOrDefault();
                        string pass3 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass3).FirstOrDefault();
                        string pass4 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.Pass4).FirstOrDefault();
                        if (usrdetail != null || collection.Ldap == true)
                        {
                            int online, locked, passPrompt, ldap;
                            ldap = online = locked = 0;
                            if (collection.IsOnline == true)
                            {
                                online = 1;
                            }
                            else
                            {
                                online = 0;
                            }

                            if (collection.IsLocked == true)
                            {
                                locked = 1;
                            }
                            else
                            {
                                locked = 0;
                            }

                            if (collection.PassUDPrompt == true && collection.Ldap != true)
                            {
                                passPrompt = 1;
                            }
                            else
                            {
                                passPrompt = 0;
                            }
                            if (collection.Ldap == true)
                            {
                                ldap = 1;
                            }
                            else
                            {
                                ldap = 0;
                            }


                            //int output = db.Database.ExecuteSqlCommand("Update Users Set UserID = '" + usr + "' ,Password = '" + pass + "' ,AccountType = '" + accType + "' ,IsOnline = '" + online + "' ,PassUDPrompt = '" + passPrompt + "' ,IsLocked = '" + locked + "' , Attempts = '" + 0 + "', Ldap = '" + ldap + "'  where UserID = '" + oldID + "'");
                            int output = db.Database.ExecuteSqlCommand("Update Users set password = '" + pass + "' , Pass1 = '" + pass + "',AccountType = '" + accType + "', IsOnline = '" + online + "', Ldap = '" + ldap + "',IsLocked = '" + locked + "',  Attempts = '" + 0 + "',Pass2 = '" + pass1 + "' , Pass3 = '" + pass2 + "', Pass4 = '" + pass3 + "', Pass5 = '" + pass4 + "', LastDateChange = '" + DateTime.Now + "', PassUDPrompt = '" + passPrompt + "' where UserID = '" + oldID + "'");
                            if (output > 0)
                            {
                                Log("User Updation", "", 10002005, "$uid: " + usr);
                                int output1 = db.Database.ExecuteSqlCommand("Update extuser Set extuserid = '" + usr + "' ,password = '" + extpass + "' where extuserid = '" + oldID + "'");
                                @TempData["OKMsg"] = "User Account Updated Successfully!";
                                userlog.Info(Session["UserID"].ToString() + " : user account updated successfully " + collection.UserID);
                                Log("User Account Updation", "", 10002007, "$uid: " + usr);
                                return RedirectToAction("Index", "User");
                            }
                            else
                            {
                                @TempData["NoMsg"] = "User Account Update Unsuccessful!";
                                Log("User Updation", "", 10002006, "$uid: " + usr);
                                Log("User Updation", "", 10002008, "$uid: " + usr);
                                errorlog.Error(Session["UserID"].ToString() + " User Account Update Unsuccessful " + collection.UserID + " " + output);
                                return RedirectToAction("Edit", "User", new { id = usr + "/" });
                            }
                        }
                        else
                        {
                            TempData["NoMsg"] = "You Cannot Repeat Previous 2 Passwords!";
                            errorlog.Error("User: " + Session["UserID"] + " Error: You Cannot Repeat Previous 2 Passwords for " + collection.UserID);
                            return RedirectToAction("Edit", "User", new { id = usr + "/" });
                        }

                    }
                }
                else
                {
                    TempData["PassError"] = "Passwords do not match";
                    errorlog.Error("User: " + Session["UserID"] + " Passwords do not match for " + collection.UserID);
                    return RedirectToAction("Edit", "User", new { id = usr + "/" });
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("Edit", "User");
            }
        }

        // POST: User/Delete/5
        [HttpGet]
        public ActionResult Delete(String id, User collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("7");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            if (id != Session["UserID"].ToString())
            {
                try
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        activitylog.Info(Session["UserID"].ToString() + " deleting user " + id);
                        String usr = id;
                        int output = db.Database.ExecuteSqlCommand("Delete from Users where UserID = '" + usr + "'");
                        if (output > 0)
                        {
                            Log("User Deletion", "", 10002009, "$uid: " + usr);
                            int output1 = db.Database.ExecuteSqlCommand("Delete from extuser where extuserid = '" + usr + "'");
                            Log("User Account Deletion", "", 10002011, "$uid: " + usr);
                            @TempData["OKMsg"] = "User Account Deleted Successfully!";
                            userlog.Info(Session["UserID"].ToString() + " : user account deleted successfully " + id);
                            return RedirectToAction("Index", "User");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "User Account Could not be Deleted ";
                            Log("User Deletion", "", 10002010, "$uid: " + usr);
                            Log("User Account Deletion", "", 10002012, "$uid: " + usr);
                            errorlog.Error(Session["UserID"].ToString() + " User Account Could not be Deleted " + id + " " + output);
                            return RedirectToAction("Index", "User");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    return RedirectToAction("Index", "User");
                }
            }
            else
            {
                @TempData["NoMsg"] = "User Account Could not be Deleted. It's Logged in.";
                activitylog.Info(Session["UserID"].ToString() + " User Account Could not be Deleted because you are logged in with it " + id);
                return RedirectToAction("Index", "User");
            }

        }


        [HttpPost]
        public JsonResult PasswordPolicyCheck(string newPassword2, string AccountType)
        {
            try
            {
                newPassword2 = newPassword2.Substring(0, newPassword2.Length - 1);
                newPassword2 = newPassword2.Substring(1, newPassword2.Length - 1);
                newPassword2 = CryptoJsHelper.DecryptStringAES(newPassword2);
                var role = db.UserSecurities.Where(a => a.AccountType == AccountType).Select(p => p.Passwordpolicy).FirstOrDefault();
                //string[]passPolicy positions (minlength,maxLength,minCapital,minDigit,maxAttempt,minAge,maxAge,Password,passwordExpiry)

                if (role == null)
                {
                    string errMsg = "The password policy is not assigned to this role";
                    return Json(errMsg, JsonRequestBehavior.AllowGet);
                }
                var passPolicy = role.Split(',');
                string minCapital = (passPolicy[2].Contains('.')) ? passPolicy[2].Substring(0, passPolicy[2].IndexOf(".")) : passPolicy[2];
                string minDigit = (passPolicy[3].Contains('.')) ? passPolicy[3].Substring(0, passPolicy[3].IndexOf(".")) : passPolicy[3];
                string minLength = (passPolicy[0].Contains('.')) ? passPolicy[0].Substring(0, passPolicy[0].IndexOf(".")) : passPolicy[0];
                string maxLength = (passPolicy[1].Contains('.')) ? passPolicy[1].Substring(0, passPolicy[1].IndexOf(".")) : passPolicy[1];

                Regex rg = new Regex(@"^(?=.*[a-z])*(?=(.*[A-Z]){" + minCapital + @"})(?=(.*\d){" + minDigit + @"})(?=.*[~!@#$%^&*()_+\-=?\/<>,.';:\[\]\{\}\\]).{" + minLength + @"," + maxLength + @"}$");
                MatchCollection Count = rg.Matches(newPassword2);
                if (Count.Count > 0)
                {

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string errMsg = "The password must contain atleast 1 symbol " + minDigit + " digit " + minCapital + " capital " + minLength + " min length and " + maxLength + " max length ";
                    return Json(errMsg, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public ActionResult UserDashBoard()
        {
            TempData["AlreadyLogged"] = null;
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            //else
            //{
            //    var ret = Check("41");
            //    if (ret == false)
            //    {
            //        return RedirectToAction("Index", "Login");
            //    }
            //}
            try
            {
                String usr = Session["UserID"].ToString();
                activitylog.Info(Session["UserID"].ToString() + " navigate to dashboard");
                var lastlogin = Session["LastLogin"].ToString();
                if (lastlogin == null || lastlogin == "")
                {
                    ViewData["lastlogin"] = "- - -";

                }
                else
                {
                    ViewData["lastlogin"] = lastlogin;

                }

                userlog.Info(Session["UserID"].ToString() + " Last Login: " + lastlogin);


                var passchanged = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.LastDateChange).First();

                if (Session["LDAP"].ToString() != "true")
                {
                    ViewData["passchanged"] = passchanged;

                    userlog.Info(Session["UserID"].ToString() + " Last password changed: " + passchanged);
                }
                else
                {
                    ViewData["passchanged"] = "LDAP";

                    userlog.Info(Session["UserID"].ToString() + " Last password changed: " + "LDAP User");
                }

                if (Session["LDAP"].ToString() != "true")
                {
                    DateTime dtfrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    DateTime dtTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    var diff = (dtTo - Convert.ToDateTime(passchanged)).TotalDays;

                    string diffd = String.Format("{0:N0}", diff);
                    int difftime = int.Parse(diffd);
                    ViewData["diff"] = 45 - difftime;
                    userlog.Info(Session["UserID"].ToString() + " Password change in: " + (45 - difftime).ToString());
                }
                else
                {

                    ViewData["diff"] = "LDAP";

                    userlog.Info(Session["UserID"].ToString() + " Password change in: " + "LDAP User");

                }
                string driveFromConfig = System.Configuration.ConfigurationManager.AppSettings["OnlyDrive"];
                if (Directory.Exists(driveFromConfig))
                {
                    DriveInfo driveInfo = new DriveInfo(System.Configuration.ConfigurationManager.AppSettings["OnlyDrive"]);
                    long FreeSpace = driveInfo.AvailableFreeSpace;
                    FreeSpace = FreeSpace / 1048576;
                    if (FreeSpace > 1024)
                    {
                        FreeSpace = FreeSpace / 1024;
                        ViewData["space"] = FreeSpace.ToString();
                        ViewData["spacetype"] = " GB";
                    }
                    else
                    {
                        ViewData["space"] = FreeSpace.ToString();
                        ViewData["spacetype"] = " MB";
                    }
                }
                else
                {
                    ViewData["drive-error"] = "Drive not configured!";
                }

            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Login");
            }


            return View();
        }
        public ActionResult AccRole()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("8");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to account role");
                var mymodel = new UserSecurity();
                //mymodel.usersss = db.Users.ToList();
                mymodel.usersec = db.UserSecurities.ToList();

                mymodel.Data = db.Users.GroupBy(r => r.AccountType)
                .Select(r => new UserCustom()
                {
                    AccType = r.Key,
                    Count = r.Count()
                }).ToList();

                return View(mymodel);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Role()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("8");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to account role");
                var mymodel = new UserCustom();
                mymodel.usersss = db.Users.ToList();
                mymodel.usersec = db.UserSecurities.ToList();

                mymodel.Data = db.Users.GroupBy(r => r.AccountType)
                .Select(r => new UserCustom()
                {
                    AccType = r.Key,
                    Count = r.Count()
                }).ToList();

                return View(mymodel);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole(UserSecurity collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("9");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                String Acc = collection.AccountType.ToString();
                if (Acc == "" || Acc == null)
                {
                    @TempData["NoMsg"] = "This field is required!";
                    return RedirectToAction("AccRole", "User");
                }
                Boolean init = false;
                string passwordpolicy = "";
                int output = db.Database.ExecuteSqlCommand("insert into UserSecurity(AccountType,Uview,Uadd,Uupd,Udel,Iview,Aview,Aadd,Aupd,Adel,AAview,R1view,R2view,R3view,Rview,SRview,Hview,Hadd,Hupd,Hdel,Fview,EJview,REJview,DEJview,Jview,JRview,Mview) values('" + Acc + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "','" + init + "')");
                if (output > 0)
                {
                    @TempData["OKMsg"] = "Account Role Created Successfully!";
                    Log("Account Role Creation", "", 10002013, "$acctype: " + Acc);
                    userlog.Info(Session["UserID"].ToString() + " : account role created successfully named " + Acc);
                    return RedirectToAction("AccRole", "User");
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + "Account Role Creation Unsuccessful named " + Acc);
                    @TempData["NoMsg"] = "Account Role Creation Unsuccessful!";
                    Log("Account Role Creation", "", 10002014, "$acctype: " + Acc);
                    return RedirectToAction("AccRole", "User");
                }

            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("AccRole", "User");
            }
        }

        [HttpGet]
        public ActionResult RemoveRole(String id, User collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("11");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is removing role : " + id);
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    String Acc = id;
                    var userCount = db.Users.Where(a => a.AccountType.Equals(id)).Count();
                    if (userCount > 0)
                    {

                        @TempData["NoMsg"] = "Account Role Assigned to User. Could not be Deleted!";
                        return RedirectToAction("AccRole", "User");
                    }
                    else
                    {
                        int output = db.Database.ExecuteSqlCommand("Delete from UserSecurity where AccountType = '" + Acc + "'");
                        if (output > 0)
                        {
                            Log("Account Role Deletion", "", 10002015, "$acctype: " + Acc);
                            userlog.Info(Session["UserID"].ToString() + " : role " + Acc + " deleted succesfully  ");
                            @TempData["OKMsg"] = "Account Role Deleted Successfully!";
                            return RedirectToAction("AccRole", "User");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Account Role Could not be Deleted ";
                            Log("Account Role Deletion", "", 10002016, "$acctype: " + Acc);
                            errorlog.Error("User: " + Session["UserID"] + " Error deleting role : " + output);
                            return RedirectToAction("AccRole", "User");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("AccRole", "User");
            }
        }

        //public ActionResult RoleEdit(String id)
        //{
        //    if (Session["UserID"] == null)
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //    else
        //    {
        //        var ret = Check("10");
        //        if (ret == false)
        //        {
        //            return RedirectToAction("Index", "Login");
        //        }
        //    }
        //    try
        //    {
        //        ViewBag.id = id;
        //        var data = db.UserSecurities.SqlQuery("Select * from UserSecurity where AccountType = '" + id + "'").ToList();
        //        activitylog.Info(Session["UserID"].ToString() + "Editing Account Role " + id);
        //        UserSecurity ForList = new UserSecurity();
        //        ForList.CheckList = data;
        //        return View(ForList);
        //    }
        //    catch (Exception ex)
        //    {
        //        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //        return View("Error");
        //        //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult RoleEdit(UserSecurity collection)
        //{
        //    if (Session["UserID"] == null)
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //    else
        //    {
        //        var ret = Check("10");
        //        if (ret == false)
        //        {
        //            return RedirectToAction("Index", "Login");
        //        }
        //    }
        //    try
        //    {
        //        String usr = collection.Store;
        //        ViewBag.id = collection.Store;
        //        //foreach (var item in collection.CheckList)
        //        //{
        //        //    collection.Uview = item.Uview;
        //        //    collection.Uadd = item.Uadd;
        //        //    collection.Uupd = item.Uupd;
        //        //    collection.Udel = item.Udel;
        //        //    collection.Iview = item.Iview;
        //        //    collection.Aview = item.Aview;
        //        //    collection.Aadd = item.Aadd;
        //        //    collection.Aupd = item.Aupd;
        //        //    collection.Adel = item.Adel;
        //        //    collection.AAview = item.AAview;
        //        //    collection.R1view = item.R1view;
        //        //    collection.R2view = item.R2view;
        //        //    collection.R3view = item.R3view;
        //        //    collection.Rview = item.Rview;
        //        //    collection.SRview = item.SRview;
        //        //    collection.Hview = item.Hview;
        //        //    collection.Hadd = item.Hadd;
        //        //    collection.Hupd = item.Hupd;
        //        //    collection.Hdel = item.Hdel;
        //        //    collection.Fview = item.Fview;
        //        //    collection.EJview = item.EJview;
        //        //    collection.REJview = item.REJview;
        //        //    collection.DEJview = item.DEJview;
        //        //    collection.Jview = item.Jview;
        //        //    collection.JRview = item.JRview;
        //        //    collection.Mview = item.Mview;
        //        //}


        //        activitylog.Info(Session["UserID"].ToString() + "Assigning Account Role " + usr);
        //        var data = db.UserSecurities.SqlQuery("Select * from UserSecurity where AccountType = '" + usr + "'").ToList();
        //        UserSecurity ForList = new UserSecurity();
        //        ForList.CheckList = data;

        //        //int output = db.Database.ExecuteSqlCommand("Update UserSecurity Set Uview = '" + collection.Uview + "' , Uadd = '" + collection.Uadd + "' , Uupd = '" + collection.Uupd + "' , Udel = '" + collection.Udel + "', Iview = '" + collection.Iview + "', Aview = '" + collection.Aview + "', Aadd = '" + collection.Aadd + "', Aupd = '" + collection.Aupd + "', Adel = '" + collection.Adel + "' , AAview = '" + collection.AAview + "', R1view = '" + collection.R1view + "', R2view = '" + collection.R2view + "', R3view = '" + collection.R3view + "', Rview = '" + collection.Rview + "', SRview = '" + collection.SRview + "' , Hview = '" + collection.Hview + "', Hadd = '" + collection.Hadd + "', Hupd = '" + collection.Hupd + "', Hdel = '" + collection.Hdel + "' , Fview = '" + collection.Fview + "' , EJview = '" + collection.EJview + "' , REJview = '" + collection.REJview + "' , DEJview = '" + collection.DEJview + "', Jview = '" + collection.Jview + "', JRview = '" + collection.JRview + "', Mview = '" + collection.Mview + "' where AccountType = '" + usr + "'");
        //        int output = 1;


        //        if (output > 0)
        //        {
        //            activitylog.Info(Session["UserID"].ToString() + "Account Role Assigned Successfully " + usr);
        //            @TempData["OKMsg"] = "Account Role Assigned Successfully!";
        //            Log("Account Role Updation", "", 10002017, "$acctype: " + usr);
        //            return View(ForList);
        //        }
        //        else
        //        {
        //            errorlog.Error("User: " + Session["UserID"] + " Account Role Assign Unsuccessful " + usr);
        //            @TempData["NoMsg"] = "Account Role Assign Unsuccessful!";
        //            Log("Account Role Updation", "", 10002018, "$acctype: " + usr);

        //            return View(ForList);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
        //        return RedirectToAction("AccRole", "User");
        //    }
        //}

        public ActionResult EditRole(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("10");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                ViewBag.id = id;
                var data = db.UserPermissions.SqlQuery("Select * from UserPermission  where Licenseon =1 order by Hierarchy").ToList();

                UserPermission ForList = new UserPermission();
                ForList.Role = db.UserSecurities.Where(x => x.AccountType == id).Select(y => y.Access).SingleOrDefault();
                ForList.data = data;
                return View(ForList);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult EditRole(UserPermission collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("10");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {

                String Acc = collection.Store.ToString();
                String Acs = "NULL";
                if (collection.Role != null)
                {
                    Acs = collection.Role.ToString();
                }

                int output = db.Database.ExecuteSqlCommand("Update UserSecurity Set Access = '" + Acs + "' where AccountType = '" + Acc + "'");

                if (output > 0)
                {

                    var role = Session["Role"].ToString();
                    var sql = db.UserSecurities.Where(a => a.AccountType == role).SingleOrDefault();
                    if (sql != null)
                    {
                        var acs = sql.Access.Split(',');

                        List<object> sessions = new List<object>();
                        foreach (var x in acs)
                        {
                            sessions.Add(x);
                        }
                        Session["UserRole"] = sessions;
                    }
                    Log("Update Account Role", "", 10002017, "$acctype: " + Acc);

                    userlog.Info(Session["UserID"].ToString() + " : account role assigned successfully " + Acc);
                    @TempData["OKMsg"] = "Account Role Assigned Successfully!";
                    return RedirectToAction("EditRole", "User");
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Account Role Assign Unsuccessful " + Acc + " " + output);
                    Log("Update Account Role", "", 10002018, "$acctype: " + Acc);
                    @TempData["NoMsg"] = "Account Role Assign Unsuccessful!";
                    return RedirectToAction("EditRole", "User");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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
            pvjournal.issuer = "USER_CONTROLLER";
            pvjournal.issuertype = 7;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";
            Audit audit = new Audit();
            audit.Log(pvjournal);
        }
    }
}