using SurveilAI.Models;
using SurveilAI.DataContext;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveilAI.Controllers
{
    public class DeletionController : Controller
    {
        SurveilAIEntities db = new SurveilAIEntities();
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger errorlog = LogManager.GetLogger("error");
        UserCustom UserCustom = new UserCustom();
        ILogger userlog = LogManager.GetLogger("user");

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

        // GET: Deletion
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("24");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {

                activitylog.Info(Session["UserID"].ToString() + " navigate to folder deletion");
                String usr = Session["UserID"].ToString();
                var ATMID = UserCustom.GetAssignDevice(usr);
                if (ATMID != null)
                {
                    List<string> AllAtm = ATMID.Select(a => a.DeviceID).ToList();

                    var mymodel = new Device();
                    var result = from Device in db.Devices
                                 join Hierarchy in db.Hierarchies on Device.HierLevel equals Hierarchy.Hierlevel
                                 where (AllAtm).Contains(Device.DeviceID)
                                 orderby Device.DeviceID
                                 select new { Device.DeviceID, Device.BranchName, Hierarchy.HierName, Device.IP, Device.DeviceType };

                    foreach (var a in result)
                    {

                        mymodel.devicesss.Add(new Tuple<string, string, string, string, string>(a.DeviceID.ToString(), a.BranchName.ToString(), a.HierName.ToString(), a.IP.ToString(), a.DeviceType.ToString()));
                    }

                    return View(mymodel);
                }
                else
                {
                    var atms = new SelectList("");
                    ViewData["ATMID"] = atms;
                    @TempData["NoMsg"] = "No Devices Assigned For This User!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                var atms = new SelectList("");
                ViewData["ATMID"] = atms;
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return View();
            }

        }

        [HttpGet]
        public ActionResult folderDelete(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("24");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                string path = System.Configuration.ConfigurationManager.AppSettings["DirectDirectory"].ToString() + id.ToString();

                activitylog.Info(Session["UserID"].ToString() + " is deleting device");
                if (Directory.Exists(path) == true)
                {
                    Directory.Delete(path, true);
                    @TempData["OKMsg"] = "Folder Deleted Successfully!";
                    userlog.Info(Session["UserID"].ToString() + " Folder Deleted Successfully " + id.ToString());
                    Log("Delete Folder", id, 10004001, "$did:" + id);
                    return RedirectToAction("Index", "Deletion");
                }
                else
                {
                    // lblDate0.Visible = true;
                    @TempData["NoMsg"] = "Folder Not Found!";
                    errorlog.Error(Session["UserID"].ToString() + " Folder Deleted Unsuccessfully " + id.ToString());

                    Log("Delete Folder Failed", id, 10004002, "$did:" + id);
                    return RedirectToAction("Index", "Deletion");
                }

            }
            catch (Exception ex)
            {
                @TempData["NoMsg"] = "Error While Deleting Folder!";
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                Log("Delete Folder Failed", id, 10004002, "$did:" + id + " $ex-msg: " + ex.Message);
                return RedirectToAction("Index", "Deletion");
            }

        }

        public ActionResult folderDeleteall()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("24");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                String usr = Session["UserID"].ToString();
                activitylog.Info(Session["UserID"].ToString() + " is deleting all assigined folders");

                var ATMID = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).First();
                if (ATMID != null)
                {
                    var atms = new SelectList(ATMID.Split('%').ToList());

                    var mymodel1 = new User();
                    var ATMID1 = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).First();
                    if (ATMID1 != "" && ATMID1 != null && ATMID1 != "|")
                    {
                        var atms1 = ATMID.Split('%');
                        foreach (var a in atms1)
                        {
                            var id = a.Split('|')[0];
                            mymodel1.atmss.Add(id);
                        }

                    }


                    foreach (var b in mymodel1.atmss)
                    {
                        string path = System.Configuration.ConfigurationManager.AppSettings["DirectDirectory"].ToString() + b.ToString();

                        if (Directory.Exists(path) == true)
                        {
                            Directory.Delete(path, true);
                        }

                    }
                    @TempData["hadATM"] = "All folder deleted!";
                    userlog.Info(Session["UserID"].ToString() + " : All folders are deleted ");
                    Log("Delete Folder", "", 10004001, "$delete All:");
                    return RedirectToAction("Index", "Deletion");
                }
                else
                {
                    var atms = new SelectList("");
                    ViewData["ATMID"] = atms;
                    @TempData["noATM"] = "No ATM is assigned to you!";
                    errorlog.Info(Session["UserID"].ToString() + " :  No ATM is Assigned");

                    Log("Delete Folder Failed", "", 10004002, "$delete All");
                    return RedirectToAction("Index", "Deletion");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                var atms = new SelectList("");
                ViewData["ATMID"] = atms;
                @TempData["noATM"] = "Error while deleting folders!";
                Log("Delete Folder Failed", "", 10004002, "$delete All $ex-msg: " + ex.Message);

                return RedirectToAction("Index", "Deletion");
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
            pvjournal.issuer = "DELETION_CONTROLLER";
            pvjournal.issuertype = 12;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";

            Audit audit = new Audit();
            audit.Log(pvjournal);
        }
    }
}
