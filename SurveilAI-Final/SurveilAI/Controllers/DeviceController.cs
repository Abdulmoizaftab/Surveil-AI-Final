using Microsoft.Ajax.Utilities;
using MongoDB.Driver.Builders;
using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Web.Mvc;

namespace SurveilAI.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class DeviceController : Controller
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

        public void GetIP()
        {
            var ip = db.NDVRs.ToList();
            ViewBag.IP = new SelectList(ip,"NDip", "NDip");
        }
        // GET: Device
        public ActionResult Device()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("13");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to device");
                HierList();
                GetIP();
                var hierarchyList = new SelectList(db.Hierarchies.ToList(), "", "HierName");
                ViewData["DBHierList"] = hierarchyList;
                List<int> myVal = new List<int>(new int[] { 1, 2, 3, 4 });
                ViewData["NumOfCass"] = myVal;
                var mymodel = new Device();
                mymodel.Devicess = db.Devices.ToList();

                String DirectDir = "C:/Videoarchive/";

                DirectoryInfo dirInfo = new DirectoryInfo(DirectDir);

                if (dirInfo.GetDirectories().Length > 0)
                {
                    foreach (var dir in dirInfo.GetDirectories())
                    {
                        mymodel.DevFolder.Add(dir.Name);
                    }
                    mymodel.DevFolder.RemoveAll(r => mymodel.Devicess.Any(a => a.DeviceID == r));
                }
                var trans = db.transports.ToList();
                if (trans != null)
                {
                    foreach (var x in trans)
                    {
                        mymodel.DevFolder.Add(x.deviceid.Trim());
                    }
                    mymodel.DevFolder.RemoveAll(r => mymodel.Devicess.Any(a => a.DeviceID == r));
                }

                return View(mymodel);
            }
            catch (Exception ex)
            {
                if (ex.ToString().ToLower().Contains("could not find a part of the path"))
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                    return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Videoarchive path not found!');
                       </script>
                     </body> ");
                }
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        // POST: Device/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Device collection)
        //{
        //    if (Session["UserID"] == null)
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //    else
        //    {
        //        var ret = Check("14");
        //        if (ret == false)
        //        {
        //            return RedirectToAction("Index", "Login");
        //        }
        //    }
        //    try
        //    {
        //        using (IISContext db = new IISContext())
        //        {
        //            activitylog.Info(Session["UserID"].ToString() + " is adding new device");

        //            String DeviceID = collection.DeviceID;
        //            DeviceID = DeviceID.Trim();
        //            String BranchName = collection.BranchName;
        //            String HierName = collection.HierName;
        //            String IP = collection.IP;
        //            String DeviceType = collection.DeviceType;
        //            String a = null;
        //            if (DeviceType == "Wincor")
        //            {
        //                a = "2";
        //            }
        //            else if (DeviceType == "NCR")
        //            {
        //                a = "1000";
        //            }
        //            else if (DeviceType == "Diebold")
        //            {
        //                a = "1001";
        //            }
        //            String Cassette = collection.LastCardNo;
        //            String x = "0";
        //            DateTime y = Convert.ToDateTime("2017-08-16 00:00:00");//dummy date inserted
        //            var hlevel = db.Hierarchies.Where(h => h.HierName.Equals(HierName)).Select(h => h.Hierlevel).Single();

        //            using (var dbCtxTxn = db.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    var query = string.Format("exec insertDevice @DeviceID = '{0}', @BranchName = '{1}', @hlevel = '{2}', @IP = '{3}', @DeviceType = '{4}', @Date = '2017-08-16 00:00:00', @Cassete = '{6}'", DeviceID, BranchName, hlevel, IP, a, y, Cassette);
        //                    var output = db.Database.ExecuteSqlCommand(query);


        //                    if (output > 0)
        //                    {
        //                        @TempData["OKMsg"] = "Device Added Successfully!";


        //                    }
        //                    else
        //                    {
        //                        @TempData["NoMsg"] = "Device Add Unsuccessful!";

        //                    }
        //                    //var output = db.Database.ExecuteSqlCommand("insert into Device(DeviceID,BranchName,HierLevel,IP,DeviceType,Obsolete) " +
        //                    //     "Values('" + DeviceID + "','" + BranchName + "','" + hlevel + "','" + IP + "','" + a + "','0')");

        //                    //if (output > 0)
        //                    //{
        //                    //    var output2 = db.Database.ExecuteSqlCommand("insert into DeviceLastImage(DeviceID,LastImage,ImageDay,ImageNight,Connected,LastConTime) " +
        //                    //                  "Values('" + DeviceID + "','" + y + "','" + x + "','" + x + "','" + x + "','" + y + "')");

        //                    //    if (output2 > 0)
        //                    //    {
        //                    //        var output3 = db.Database.ExecuteSqlCommand("insert into CASHPHYS_NEW(DEVICEID,DEVICETYPE,REPORTCASSETTTE) " +
        //                    //                      "Values('" + DeviceID + "','" + a + "','" + Cassette + "')");

        //                    //        if (output3 > 0)
        //                    //        {
        //                    //            var output4 = db.Database.ExecuteSqlCommand("INSERT basedata ([id], [idtype], [reference], [value]) VALUES ('" + DeviceID + "', 1, 999904,'" + DeviceType + "'),('" + DeviceID + "', 1, 999988, ''),('" + DeviceID + "', 1, 999998,''),('" + DeviceID + "', 1, 999905,''),('" + DeviceID + "', 1, 999992,''),('" + DeviceID + "', 1, 999997,''),('" + DeviceID + "', 1, 999996,''),('" + DeviceID + "', 1, 999995,''),('" + DeviceID + "', 1, 999978,''),('" + DeviceID + "', 1, 999979,''),('" + DeviceID + "', 1, 999915,''),('" + DeviceID + "', 1, 999916,''),('" + DeviceID + "', 1, 999912,''),('" + DeviceID + "', 1, 999913,''),('" + DeviceID + "', 1, 999914,''),('" + DeviceID + "', 1, 999977,''),('" + DeviceID + "', 1, 1,''),('" + DeviceID + "', 1, 999999,''),('" + DeviceID + "', 1, 999994,''),('" + DeviceID + "', 1, 999993,''),('" + DeviceID + "', 1, 999991,''),('" + DeviceID + "', 1, 999990,''),('" + DeviceID + "', 1, 999986,''),('" + DeviceID + "', 1, 999901,''),('" + DeviceID + "', 1, 999902,''),('" + DeviceID + "', 1, 999903,'')");
        //                    //            var output6 = db.Database.ExecuteSqlCommand("insert into lastevent(deviceid,timestamp,messageno) " +
        //                    //                "values('" + DeviceID + "', getdate(), '99999989')");
        //                    //            var output8 = db.Database.ExecuteSqlCommand("insert into state(deviceid,devicestate,eventid,timestamp,connected,LastConTime,Network,NetworkConTime) " +
        //                    //                "values('" + DeviceID + "', '0', null, null, 1, null, null, null)");

        //                    //            if (output4 > 0 && output6 > 0 && output8 > 0)
        //                    //            {
        //                    //                var compid = db.components.Where(z => z.ForMonitoring == true).ToList();
        //                    //                foreach (var id in compid)
        //                    //                {
        //                    //                    var output5 = db.Database.ExecuteSqlCommand("INSERT into lastcomponentevent(deviceid,timestamp,messageno,orgmessage,servertimestamp,devicestate,eventno,eventcount,eventgroupid,componentid,compstate,compeventcount) values('" + DeviceID + "',GETDATE(),'0','UNKNOWN',GETDATE(),'0','0','0',NULL,'" + id.componentid + "','0','0')");
        //                    //                    //var output7 = db.Database.ExecuteSqlCommand("insert into componentstate(deviceid,componentid,compstate,timestamp) " +
        //                    //                    //"values('" + DeviceID + "','" + id.componentid + "','0', null)");

        //                    //                    var output7 = db.Database.ExecuteSqlCommand("insert into componentstate(deviceid,componentid,compstate,timestamp) " +
        //                    //                "values('" + DeviceID + "','" + id.componentid + "','0', null)");


        //                    //                }

        //                    //                dbCtxTxn.Commit();
        //                    //                @TempData["OKMsg"] = "Device Added Successfully!";
        //                    //            }
        //                    //            else
        //                    //            {
        //                    //                dbCtxTxn.Rollback();
        //                    //                @TempData["NoMsg"] = "Device Add Unsuccessful!";
        //                    //            }
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            dbCtxTxn.Rollback();
        //                    //            @TempData["NoMsg"] = "Device Add Unsuccessful!";
        //                    //        }
        //                    //    }
        //                    //    else
        //                    //    {
        //                    //        dbCtxTxn.Rollback();
        //                    //        @TempData["NoMsg"] = "Device Add Unsuccessful!";
        //                    //    }
        //                    //}
        //                    //else
        //                    //{
        //                    //    dbCtxTxn.Rollback();
        //                    //    @TempData["NoMsg"] = "Device Add Unsuccessful!";
        //                    //}

        //                }
        //                catch (Exception ex)
        //                {
        //                    //dbCtxTxn.Rollback();
        //                    errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //                    @TempData["NoMsg"] = "Device Add Unsuccessful! Check Logs";
        //                }
        //            }
        //            return RedirectToAction("Device", "Device");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
        //        return RedirectToAction("Device", "Device");
        //    }
        //}

        // GET: User/Edit/5
        [HttpPost]
        public ActionResult NDVRReg(NDVR ndvr)
        {
            try
            {
                var query = string.Format("insert into NDVR(NDName,NDip,NDusername,NDpassword,Ndate) values('" + ndvr.NDName + "','" + ndvr.NDip + "','" + ndvr.NDusername + "','" + ndvr.NDpassword + "',CURRENT_TIMESTAMP)");
                var output = db.Database.ExecuteSqlCommand(query);
                if (output > 0)
                {
                    Log("NVR/DVR/CAMERA", ndvr.NDid.ToString(), 10003001, "$devid: " + ndvr.NDid.ToString());
                    userlog.Info(Session["UserID"].ToString() + " : successfully added device : " + ndvr.NDName);
                    @TempData["OKMsg"] = ndvr.NDName+"Added Successfully!";


                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error failed adding device : " + ndvr.NDid.ToString());
                    @TempData["NoMsg"] = "Device Add Unsuccessful!";
                    Log(ndvr.NDName+"Creation Failed", ndvr.NDid.ToString(), 10003002, "$devid: " + ndvr.NDid.ToString() + "$step: 4");

                }
            }
            catch(Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                Log("Device Creation", ndvr.NDid.ToString(), 10003002, "$devid: " + ndvr.NDid.ToString() + "$ex-msg: " + ex.Message);
                @TempData["NoMsg"] = ndvr.NDName+" "+"Add Unsuccessful! Check Logs";
            }
            return RedirectToAction("Device", "Device");
        }
        public void HierList()
        {
            var HierLvl = db.Hierarchies.ToList();
            ViewBag.HierLvll = new SelectList(HierLvl, "Hierlevel", "HierName");
        }
        [HttpPost]
        public JsonResult GetBranches(int? id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var Branches = db.Devices.Where(x => x.HierLevel == id && x.DeviceType == "NVR" || x.DeviceType == "DVR").ToList();
                return Json(Branches, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public JsonResult GetDevicess(double? id)
        {
            try
            {
                var padding = id.ToString().PadLeft(10, '0');
                db.Configuration.ProxyCreationEnabled = false;
                var allDevices = db.Devices.ToList();
                var Devicess = db.Devices.Where(x => x.DeviceID == padding.ToString()).ToList();
                return Json(Devicess, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Channels(int? id)
        {
            AllCamerasChannels acc = new AllCamerasChannels();
            int count = 1;
            var padding = id.ToString().PadLeft(10, '0');
            db.Configuration.ProxyCreationEnabled = false;
            var Channels = db.Devices.Where(x => x.DeviceID == padding.ToString()).ToList();
            ViewBag.Channels = Channels;
            var rs = @"select CamName,CamType from Cameras WHERE DeviceID ='" + padding + "'";
            var GetCam = db.Database.SqlQuery<TypeCameras>(rs).ToList();
            foreach(var item in GetCam)
            {
                if(count==1)
                {
                    acc.Cam1 = item.CamName.Split('.')[0];
                    acc.Cam1Type = item.CamType;
                    count++;
                }
                else if(count==2)
                {
                    acc.Cam2 = item.CamName.Split('.')[0];
                    acc.Cam2Type = item.CamType;
                    count++;
                }
                else if (count == 3)
                {
                    acc.Cam3 = item.CamName.Split('.')[0];
                    acc.Cam3Type = item.CamType;
                    count++;
                }
                else if (count == 4)
                {
                    acc.Cam4 = item.CamName.Split('.')[0];
                    acc.Cam4Type = item.CamType;
                    count++;
                }
                else if (count == 5)
                {
                    acc.Cam5 = item.CamName.Split('.')[0];
                    acc.Cam5Type = item.CamType;
                    count++;
                }
                else if (count == 6)
                {
                    acc.Cam6 = item.CamName.Split('.')[0];
                    acc.Cam6Type = item.CamType;
                    count++;
                }
                else if (count == 7)
                {
                    acc.Cam7 = item.CamName.Split('.')[0];
                    acc.Cam7Type = item.CamType;
                    count++;
                }
                else if (count == 8)
                {
                    acc.Cam8 = item.CamName.Split('.')[0];
                    acc.Cam8Type = item.CamType;
                    count++;
                }
                else if (count == 9)
                {
                    acc.Cam9 = item.CamName.Split('.')[0];
                    acc.Cam9Type = item.CamType;
                    count++;
                }
                else if (count == 10)
                {
                    acc.Cam10 = item.CamName.Split('.')[0];
                    acc.Cam10Type = item.CamType;
                    count++;
                }
                else if (count == 11)
                {
                    acc.Cam11 = item.CamName.Split('.')[0];
                    acc.Cam11Type = item.CamType;
                    count++;
                }
                else if (count == 12)
                {
                    acc.Cam12 = item.CamName.Split('.')[0];
                    acc.Cam12Type = item.CamType;
                    count++;
                }
                else if (count == 13)
                {
                    acc.Cam13 = item.CamName.Split('.')[0];
                    acc.Cam13Type = item.CamType;
                    count++;
                }
                else if (count == 14)
                {
                    acc.Cam14 = item.CamName.Split('.')[0];
                    acc.Cam14Type = item.CamType;
                    count++;
                }
                else if (count == 15)
                {
                    acc.Cam15 = item.CamName.Split('.')[0];
                    acc.Cam15Type = item.CamType;
                    count++;
                }
                else 
                {
                    acc.Cam16 = item.CamName.Split('.')[0];
                    acc.Cam16Type = item.CamType;
                    count++;
                }

            }
            Queue<dynamic> ChannelAndTypes = new Queue<dynamic>();
            ChannelAndTypes.Enqueue(Channels);
            ChannelAndTypes.Enqueue(acc);
            //ChannelAndTypes.Enqueue(TempData["Types"]);
            return Json(ChannelAndTypes, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Create(Device collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("14");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    activitylog.Info(Session["UserID"].ToString() + " is adding new device");
                    
                    String DeviceID = collection.DeviceID.ToString().PadLeft(10, '0');
                    DeviceID = DeviceID.Trim();
                    String BranchName = collection.BranchName;
                    String HierName = collection.HierName;
                    String IP = collection.IP;
                    String DeviceType = collection.DeviceType;
                    String CamType = collection.CamType;
                    String a = null;
                    //var ipCheck = false;
                    var ipCheck = db.Devices.Any(x=>x.IP== IP);



                    //if (DeviceType == "Wincor")
                    //{
                    //    a = "2";
                    //}
                    //else if (DeviceType == "NCR")
                    //{
                    //    a = "1";
                    //}
                    //else if (DeviceType == "Diebold")
                    //{
                    //    a = "3";
                    //}
                    String Cassette = collection.LastCardNo;
                    //String x = "0";
                    DateTime y = Convert.ToDateTime("2017-08-16 00:00:00");//dummy date inserted


                    var hlevel = db.Hierarchies.Where(h => h.HierName.Equals(HierName)).Select(h => h.Hierlevel).Single();

                    try
                    {
                       
                        var query = string.Format("insert into Device(DeviceID,BranchName,HierLevel,IP,DeviceType,Channels) values('"+DeviceID+"','"+BranchName+"',"+ hlevel + ",'"+IP+"','"+DeviceType+"','"+Cassette+"')");
                        var query2 = string.Format("insert into AddCameras(ACHierLevel,DeviceID,DeviceType) values('" + hlevel + "','" + DeviceID + "','" + DeviceType + "')");
                        if (ipCheck == false)
                        {
                            var output = db.Database.ExecuteSqlCommand(query);
                            var output2 = db.Database.ExecuteSqlCommand(query2);
                            if (output > 0)
                            {
                                Log("Device Creation", DeviceID, 10003001, "$devid: " + DeviceID);
                                userlog.Info(Session["UserID"].ToString() + " : successfully added device : " + DeviceID);
                                @TempData["OKMsg"] = "Device Added Successfully!";


                            }
                            else
                            {
                                errorlog.Error("User: " + Session["UserID"] + " Error failed adding device : " + DeviceID);
                                @TempData["NoMsg"] = "Device Add Unsuccessful!";
                                Log("Device Creation Failed", DeviceID, 10003002, "$devid: " + DeviceID + "$step: 4");

                            }
                        }
                        else
                        {
                            TempData["On"] = "1";
                            TempData["IPErrorMsg"] = "IP Alread Assigned. Device Add Unsuccessful!";
                        }
                        if (collection.DeviceType == "Camera" && ipCheck==false)
                        {
                            query = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + hlevel + "','" + DeviceID + "','" + DeviceType + "','Camera.0' ,'" + CamType + "')");
                            db.Database.ExecuteSqlCommand(query);
                        }


                    }
                    catch (Exception ex)
                    {
                        //dbCtxTxn.Rollback();
                        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                        Log("Device Creation", DeviceID, 10003002, "$devid: " + DeviceID + "$ex-msg: " + ex.Message);
                        string errorMsg = ex.Message;

                        if (errorMsg.Contains("Violation of PRIMARY KEY"))
                        {
                            @TempData["NoMsg"] = "DeviceId Already Exists. Device Add Unsuccessful!  ";
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Device Add Unsuccessful! Check Logs";
                        }
                    }
                }
                return RedirectToAction("Device", "Device");

            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Device Creation", "", 10003002, "$ex-msg: " + ex.Message);
                return RedirectToAction("Device", "Device");
            }
        }






        public ActionResult AddCameras(TypeCameras collection)
        {
            var rs = db.Database.SqlQuery<TypeCameras>("select * from cameras").ToList();


            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("14");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    activitylog.Info(Session["UserID"].ToString() + " is adding new camera to nvr");

                    var ACHierLevel = collection.ACHierLevel;
                    String DeviceID = collection.DeviceID;
                    var DeviceType = db.Devices.Where(h => h.DeviceID.Equals(DeviceID)).Select(h => h.DeviceType).Single();
                    String cam1 = collection.Cam1==null?"NULL":collection.Cam1.Replace(" ","")+".0";
                    String cam2 = collection.Cam2 == null ? "NULL" : collection.Cam2.Replace(" ", "") + ".1";
                    String cam3 = collection.Cam3 == null ? "NULL" : collection.Cam3.Replace(" ", "") + ".2";
                    String cam4 = collection.Cam4 == null ? "NULL" : collection.Cam4.Replace(" ", "") + ".3";
                    String cam5 = collection.Cam5 == null ? "NULL" : collection.Cam5.Replace(" ", "") + ".4";
                    String cam6 = collection.Cam6 == null ? "NULL" : collection.Cam6.Replace(" ", "") + ".5";
                    String cam7 = collection.Cam7 == null ? "NULL" : collection.Cam7.Replace(" ", "") + ".6";
                    String cam8 = collection.Cam8 == null ? "NULL" : collection.Cam8.Replace(" ", "") + ".7";
                    String cam9 = collection.Cam9 == null ? "NULL" : collection.Cam9.Replace(" ", "") + ".8";
                    String cam10 = collection.Cam10 == null ? "NULL" : collection.Cam10.Replace(" ", "") + ".9";
                    String cam11 = collection.Cam11 == null ? "NULL" : collection.Cam11.Replace(" ", "") + ".10";
                    String cam12 = collection.Cam12 == null ? "NULL" : collection.Cam12.Replace(" ", "") + ".11";
                    String cam13 = collection.Cam13 == null ? "NULL" : collection.Cam13.Replace(" ", "") + ".12";
                    String cam14 = collection.Cam14 == null ? "NULL" : collection.Cam14.Replace(" ", "") + ".13";
                    String cam15 = collection.Cam15 == null ? "NULL" : collection.Cam15.Replace(" ", "") + ".14";
                    String cam16 = collection.Cam16 == null ? "NULL" : collection.Cam16.Replace(" ", "") + ".15";
                    var duplicateCheck = db.AddCameras.Any(x=>x.DeviceID == collection.DeviceID);
                    var duplicateCheck2 = db.Database.SqlQuery<TypeCameras>("select * from Cameras WHERE DeviceID =" + DeviceID+";").Count();
                    var myCameras = db.Database.SqlQuery<TypeCameras>("select CamName from Cameras WHERE DeviceID =" + DeviceID + ";").ToList();


                    //var duplicateCheck2 = db.Database.ExecuteSqlCommand(query0).ToString();

                    string query;
                    string query2;

                    try
                    {
                        if (duplicateCheck2 > 0)
                        {
                            if (cam1 != "NULL")
                            {
                                if (duplicateCheck2 >= 1)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam1 + "',CamType='" + collection.Cam1Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[0].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam1 + "','" + collection.Cam1Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam2 != "NULL")
                            {
                                if(duplicateCheck2 >= 2)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam2 + "',CamType='" + collection.Cam2Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[1].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam2 + "','" + collection.Cam2Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam3 != "NULL")
                            {
                                if(duplicateCheck2 >= 3)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam3 + "',CamType='" + collection.Cam3Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[2].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam3 + "','" + collection.Cam3Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam4 != "NULL")
                            {
                                if(duplicateCheck2 >= 4)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam4 + "',CamType='" + collection.Cam4Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[3].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam4 + "','" + collection.Cam4Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam5 != "NULL")
                            {
                                if(duplicateCheck2 >= 5)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam5 + "',CamType='" + collection.Cam5Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[4].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam5 + "','" + collection.Cam5Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam6 != "NULL")
                            {
                                if(duplicateCheck2 >= 6)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam6 + "',CamType='" + collection.Cam6Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[5].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam6 + "','" + collection.Cam6Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam7 != "NULL")
                            {
                                if(duplicateCheck2 >= 7)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam7 + "',CamType='" + collection.Cam7Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[6].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam7 + "','" + collection.Cam7Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam8 != "NULL")
                            {
                                if(duplicateCheck2 >= 8)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam8 + "',CamType='" + collection.Cam8Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[7].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam8 + "','" + collection.Cam8Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam9 != "NULL")
                            {
                                if(duplicateCheck2 >= 9)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam9 + "',CamType='" + collection.Cam9Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[8].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam9 + "','" + collection.Cam9Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam10 != "NULL")
                            {
                                if(duplicateCheck2 >= 10)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam10 + "',CamType='" + collection.Cam10Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[9].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam10 + "','" + collection.Cam10Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam11 != "NULL")
                            {
                                if(duplicateCheck2 >= 11)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam11 + "',CamType='" + collection.Cam11Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[10].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam11 + "','" + collection.Cam11Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam12 != "NULL")
                            {
                                if(duplicateCheck2 >= 12)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam12 + "',CamType='" + collection.Cam12Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[11].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam12 + "','" + collection.Cam12Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam13 != "NULL")
                            {
                                if(duplicateCheck2 >= 13)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam13 + "',CamType='" + collection.Cam13Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[12].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam13 + "','" + collection.Cam13Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam14 != "NULL")
                            {
                                if(duplicateCheck2 >= 14)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam14 + "',CamType='" + collection.Cam14Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[13].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam14 + "','" + collection.Cam14Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam15 != "NULL")
                            {
                                if(duplicateCheck2 >= 15)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam15 + "',CamType='" + collection.Cam15Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[14].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam15 + "','" + collection.Cam15Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                            if (cam16 != "NULL")
                            {
                                if(duplicateCheck2 >= 16)
                                {
                                    query2 = string.Format("UPDATE Cameras SET CHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', CamName='" + cam16 + "',CamType='" + collection.Cam16Type + "' WHERE DeviceID =" + DeviceID + " and CamName ='" + myCameras[15].CamName + "';");
                                    db.Database.ExecuteSqlCommand(query2);
                                }
                                else
                                {
                                    query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam16 + "','" + collection.Cam16Type + "')");
                                    db.Database.ExecuteSqlCommand(query2);
                                }

                            }
                        }
                        else
                        {
                            if (cam1 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam1 + "','" + collection.Cam1Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam2 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam2 + "','" + collection.Cam2Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam3 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam3 + "','" + collection.Cam3Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam4 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam4 + "','" + collection.Cam4Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam5 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam5 + "','" + collection.Cam5Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam6 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam6 + "','" + collection.Cam6Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam7 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam7 + "','" + collection.Cam7Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam8 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam8 + "','" + collection.Cam8Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam9 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam9 + "','" + collection.Cam9Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam10 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam10 + "','" + collection.Cam10Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam11 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam11 + "','" + collection.Cam11Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam12 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam12 + "','" + collection.Cam12Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam13 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam13 + "','" + collection.Cam13Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam14 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam14 + "','" + collection.Cam14Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam15 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam15 + "','" + collection.Cam15Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                            if (cam16 != "NULL")
                            {
                                query2 = string.Format("insert into Cameras(CHierLevel,DeviceID,DeviceType,CamName,CamType) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam16 + "','" + collection.Cam16Type + "')");
                                db.Database.ExecuteSqlCommand(query2);
                            }
                        }
                        
                        if (duplicateCheck)
                        {
                            query = string.Format("UPDATE AddCameras SET ACHierLevel =" + ACHierLevel + " , DeviceID= '" + DeviceID + "', DeviceType= '" + DeviceType + "', Cam1='" + cam1 + "',Cam2='" + cam2 + "',Cam3='" + cam3 + "', Cam4='" + cam4 + "',Cam5='" + cam5 + "',Cam6='" + cam6 + "', Cam7='" + cam7 + "',Cam8='" + cam8 + "',Cam9='" + cam9 + "', Cam10='" + cam10 + "',Cam11='" + cam11 + "',Cam12='" + cam12 + "', Cam13='" + cam13 + "',Cam14='" + cam14 + "',Cam15='" + cam15 + "',Cam16='" + cam16 + "' WHERE DeviceID =" + DeviceID+";");        
                        }
                        else
                        {
                            query = string.Format("insert into AddCameras(ACHierLevel,DeviceID,DeviceType,Cam1,Cam2,Cam3,Cam4,Cam5,Cam6,Cam7,Cam8,Cam9,Cam10,Cam11,Cam12,Cam13,Cam14,Cam15,Cam16) values('" + ACHierLevel + "','" + DeviceID + "','" + DeviceType + "','" + cam1 + "','" + cam2 + "','" + cam3 + "','" + cam4 + "','" + cam5 + "','" + cam6 + "','" + cam7 + "','" + cam8 + "','" + cam9 + "','" + cam10 + "','" + cam11 + "','" + cam12 + "','" + cam13 + "','" + cam14 + "','" + cam15 + "','" + cam16 + "')");     
                        }
                        var output = db.Database.ExecuteSqlCommand(query);
                        if (output > 0)
                        {
                            Log("Camera Addition to NVR", DeviceID, 10003001, "$devid: " + DeviceID);
                            userlog.Info(Session["UserID"].ToString() + " : successfully added device : " + DeviceID);
                            @TempData["OKMsg"] = "Camera Added Successfully!";


                        }
                        else
                        {
                            errorlog.Error("User: " + Session["UserID"] + " Error failed adding camera : " + DeviceID);
                            @TempData["NoMsg"] = "Camera Add Unsuccessful!";
                            Log("Camera Addition Failed", DeviceID, 10003002, "$devid: " + DeviceID + "$step: 4");

                        }


                    }
                    catch (Exception ex)
                    {
                        //dbCtxTxn.Rollback();
                        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                        //Log("Device Creation", DeviceID, 10003002, "$devid: " + DeviceID + "$ex-msg: " + ex.Message);
                        @TempData["NoMsg"] = "Camera Add Unsuccessful! Check Logs";
                    }
                }
                return RedirectToAction("Device", "Device");

            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Camera Addition", "", 10003002, "$ex-msg: " + ex.Message);
                return RedirectToAction("Device", "Device");
            }
        }




        public ActionResult DevEdit(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("15");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is editing device : " + id);
                Device obj = new Device();
                var hierarchyList = db.Hierarchies.ToList();
                ViewBag.Hier = hierarchyList.Select(a => a.HierName).ToList();

                //var hlevel = db.Devices.Where(h => h.DeviceID.Equals(id)).Select(h => new {bname=h.BranchName,ip=h.IP,dtype=h.DeviceType,hl=h.HierLevel}).ToList();
                var check = db.Devices.SqlQuery("select * from Device where DeviceID='" + id + "'").FirstOrDefault();

                obj.BranchName = check.BranchName;
                obj.IP = check.IP;
                obj.HierName = hierarchyList.Where(a => a.Hierlevel.Equals(check.HierLevel)).Select(a => a.HierName).FirstOrDefault();
                obj.DeviceType = check.DeviceType;
                obj.Channels= check.Channels;
                if (obj.DeviceType == "3")
                {
                    obj.DeviceType = "Diebold";
                }
                else if (obj.DeviceType == "2")
                {
                    obj.DeviceType = "Wincor";
                }
                else if (obj.DeviceType == "1")
                {
                    obj.DeviceType = "NCR";
                }


                ViewBag.Channels = obj.Channels;
                ViewBag.DeviceID = id;
                return View(obj);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult DevBasedata(String id)
        {
            string DeviceID = id;
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("17");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is editing basedata : " + id);
                DataSet ds = new DataSet();
                string constr = ConfigurationManager.ConnectionStrings["IISContext1"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select distinct id, (select Top 1 value from basedata where reference = 999904 and id = '" + DeviceID + "') as Profile, " +
                   "(select Top 1 value from basedata where reference = 999988 and id = '" + DeviceID + "') as Timezone, " +
                   "(select Top 1 value from basedata where reference = 999905 and id = '" + DeviceID + "') as URL, " +
                   "(select Top 1 value from basedata where reference = 999998 and id = '" + DeviceID + "') as Location, " +
                   "(select Top 1 value from basedata where reference = 999992 and id = '" + DeviceID + "') as Description, " +
                   "(select Top 1 value from basedata where reference = 999997 and id = '" + DeviceID + "') as Street, " +
                   "(select Top 1 value from basedata where reference = 999995 and id = '" + DeviceID + "') as City, " +
                   "(select Top 1 value from basedata where reference = 999996 and id = '" + DeviceID + "') as Zip, " +
                   "(select Top 1 value from basedata where reference = 999978 and id = '" + DeviceID + "') as State, " +
                   "(select Top 1 value from basedata where reference = 999979 and id = '" + DeviceID + "') as Country, " +
             "(select Top 1 value from basedata where reference = 999915 and id = '" + DeviceID + "') as Longitude, " +
             "(select Top 1 value from basedata where reference = 999916 and id = '" + DeviceID + "') as Latitude, " +
             "(select Top 1 value from basedata where reference = 999912 and id = '" + DeviceID + "') as Vendor, " +
             "(select Top 1 value from basedata where reference = 999913 and id = '" + DeviceID + "') as DeviceModel, " +
             "(select Top 1 value from basedata where reference = 999914 and id = '" + DeviceID + "') as DeviceType, " +
             "(select Top 1 value from basedata where reference = 999977 and id = '" + DeviceID + "') as Branch, " +
             "(select Top 1 value from basedata where reference = 1 and id = '" + DeviceID + "') as BranchCode, " +
             "(select Top 1 value from basedata where reference = 999999 and id = '" + DeviceID + "') as Orginization, " +
             "(select Top 1 value from basedata where reference = 999994 and id = '" + DeviceID + "') as SoftwareVer, " +
             "(select Top 1 value from basedata where reference = 999993 and id = '" + DeviceID + "') as InstDate, " +
             "(select Top 1 value from basedata where reference = 999991 and id = '" + DeviceID + "') as SerialNo, " +
             "(select Top 1 value from basedata where reference = 999990 and id = '" + DeviceID + "') as System, " +
             "(select Top 1 value from basedata where reference = 999986 and id = '" + DeviceID + "') as AgentVer, " +
             "(select Top 1 value from basedata where reference = 999901 and id = '" + DeviceID + "') as Contact1, " +
             "(select Top 1 value from basedata where reference = 999902 and id = '" + DeviceID + "') as Contact2, " +
             "(select Top 1 value from basedata where reference = 999903 and id = '" + DeviceID + "') as Contact3 " +
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

                string C1 = "", C2 = "", C3 = "";
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    C1 = row["Contact1"].ToString();
                    C2 = row["Contact2"].ToString();
                    C3 = row["Contact3"].ToString();
                }

                List<string> contacts = db.contacts.Select(x => x.contactid).ToList();
                List<SelectListItem> ContactList = UserCustom.ToListItem(contacts);
                ViewBag.ContactList = ContactList;
                ViewBag.Contact1 = C1;
                ViewBag.Contact2 = C2;
                ViewBag.Contact3 = C3;
                return View(ds);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }




        // POST: Device/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Device collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("15");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    //activitylog.Info(Session["UserID"].ToString() + " is editing device " + collection.DevIdOld);

                    String Oldid = collection.DevIdOld;
                    String DeviceID = collection.DeviceID;
                    String BranchName = collection.BranchName;
                    String HierName = collection.HierName;
                    String ip = collection.IP;
                    String DeviceType = collection.DeviceType;
                    //bool isValid = IPAddress.TryParse(IP, out IPAddress _);

                    String a = null;
                    if (DeviceType == "Wincor")
                    {
                        a = "2";
                    }
                    else if (DeviceType == "NCR")
                    {
                        a = "1000";
                    }
                    else if (DeviceType == "Diebold")
                    {
                        a = "1001";
                    }
                    String Channels = collection.LastCardNo;


                    if (DeviceID == null || DeviceID == "")
                    {
                        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                        return RedirectToAction("Device", "Device");
                    }
                    if (BranchName == null || BranchName == "")
                    {
                        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                        return RedirectToAction("Device", "Device");
                    }
                    if (HierName == null || HierName == "")
                    {
                        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                        return RedirectToAction("Device", "Device");
                    }
                    if (ip == null || ip == "")
                    {
                        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                        return RedirectToAction("Device", "Device");
                    }
                    if (DeviceType == null || DeviceType == "")
                    {
                        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";

                        return RedirectToAction("Device", "Device");
                    }

                    var hlevel = db.Hierarchies.Where(h => h.HierName.Equals(HierName)).Select(h => h.Hierlevel).Single();


                    using (var dbCtxTxn = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var output = db.Database.ExecuteSqlCommand("Update Device Set DeviceID = '" + DeviceID + "', BranchName= '" + BranchName + "' , HierLevel='" + hlevel + "', IP='" + ip + "', DeviceType='" + a + "' where DeviceID='" + Oldid + "'");

                            if (output > 0)
                            {
                                var output2 = db.Database.ExecuteSqlCommand("UPDATE CASHPHYS_NEW Set DEVICEID = '" + DeviceID + "', DEVICETYPE ='" + a + "', REPORTCASSETTTE='" + Channels + "' where DEVICEID='" + Oldid + "'");
                                if (output2 > 0)
                                {
                                    var output3 = db.Database.ExecuteSqlCommand("Update DeviceLastImage Set DeviceID = '" + DeviceID + "' Where DeviceID = '" + Oldid + "'");

                                    if (output3 > 0)
                                    {
                                        var output4 = db.Database.ExecuteSqlCommand("Update basedata Set id = '" + DeviceID + "' Where id = '" + Oldid + "'");

                                        if (output4 > 0)
                                        {
                                            var output5 = db.Database.ExecuteSqlCommand("Update lastevent Set deviceid = '" + DeviceID + "' where deviceid = '" + Oldid + "'");
                                            var output6 = UserCustom.EditAssignDevice(DeviceID, Oldid);
                                            var output7 = db.Database.ExecuteSqlCommand("Update lastcomponentevent Set deviceid = '" + DeviceID + "' where deviceid = '" + Oldid + "'");
                                            var output8 = db.Database.ExecuteSqlCommand("update transport set remotedte='" + ip + "' where deviceid='" + DeviceID + "'");

                                            if (output5 > 0)
                                            {
                                                userlog.Info(Session["UserID"].ToString() + " : editing device " + DeviceID + " succesfully  ");
                                                dbCtxTxn.Commit();
                                                @TempData["OKMsg"] = "Device Updated Successfully!";
                                            }
                                            else
                                            {
                                                errorlog.Error("User: " + Session["UserID"] + " Error  editing device " + DeviceID + " unsuccessful :" + output5);
                                                dbCtxTxn.Commit();
                                                @TempData["OKMsg"] = "Device Updated Successfully!";
                                            }
                                            Log("Device Updation", DeviceID, 10003003, "$devid: " + DeviceID);
                                        }
                                        else
                                        {
                                            errorlog.Error("User: " + Session["UserID"] + " Error  editing device " + DeviceID + " unsuccessful :" + output4);
                                            dbCtxTxn.Rollback();
                                            @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                                            Log("Device Updation Failed", DeviceID, 10003004, "$devid: " + DeviceID + "$step: 4");
                                        }
                                    }
                                    else
                                    {
                                        errorlog.Error("User: " + Session["UserID"] + " Error  editing device " + DeviceID + " unsuccessful :" + output3);
                                        dbCtxTxn.Rollback();
                                        @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                                        Log("Device Updation", DeviceID, 10003004, "$devid: " + DeviceID + "$step: 3");
                                    }
                                }
                                else
                                {
                                    errorlog.Error("User: " + Session["UserID"] + " Error  editing device " + DeviceID + " unsuccessful :" + output2);
                                    dbCtxTxn.Rollback();
                                    @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                                    Log("Device Updation", DeviceID, 10003004, "$devid: " + DeviceID + "$step: 2");
                                }
                            }
                            else
                            {
                                errorlog.Error("User: " + Session["UserID"] + " Error  editing device " + DeviceID + " unsuccessful :" + output);
                                dbCtxTxn.Rollback();
                                @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                                Log("Device Updation", DeviceID, 10003004, "$devid: " + DeviceID + "$step: 1");
                            }

                        }
                        catch (Exception ex)
                        {
                            dbCtxTxn.Rollback();
                            errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                            Log("Device Updation", DeviceID, 10003004, "$devid: " + DeviceID + "$ex-msg: " + ex.Message);
                            @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                        }
                    }
                    return RedirectToAction("Device", "Device");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Device Updation", "", 10003004, "$ex-msg: " + ex.Message);
                return RedirectToAction("Device", "Device");
            }
        }

        [HttpPost]
        public ActionResult EditBasedata(basedata collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("17");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            basedata obj = new basedata();

            obj.idold = collection.idold;
            obj.id = collection.id;
            obj.profile = collection.profile;
            obj.timezone = collection.timezone;
            obj.location = collection.location;
            obj.url = collection.url;
            obj.description = collection.description;
            obj.street = collection.street;
            obj.zip = collection.zip;
            obj.city = collection.city;
            obj.state = collection.state;
            obj.country = collection.country;
            obj.longitude = collection.longitude;
            obj.latitude = collection.latitude;
            obj.vendor = collection.vendor;
            obj.devicemodel = collection.devicemodel;
            obj.devicetype = collection.devicetype;
            obj.branch = collection.branch;
            obj.branchcode = collection.branchcode;
            obj.orginization = collection.orginization;
            obj.softwarever = collection.softwarever;
            obj.instdate = collection.instdate;
            obj.serialno = collection.serialno;
            obj.system = collection.system;
            obj.agentver = collection.agentver;
            obj.contact1 = collection.contact1;
            obj.contact2 = collection.contact2;
            obj.contact3 = collection.contact3;

            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    activitylog.Info(Session["UserID"].ToString() + " is editing device master data " + collection.idold);

                    string query = "Update basedata set value = case reference when '999904' then '" + obj.profile + "' when '999988' then '" + obj.timezone + "' " +
                                 "when '999998' then '" + obj.location + "' " +
                                 "when '999905' then '" + obj.url + "' " +
                                 "when '999992' then '" + obj.description + "' " +
                                 "when '999997' then '" + obj.street + "' " +
                                 "when '999996' then '" + obj.zip + "' " +
                                 "when '999995' then '" + obj.city + "' " +
                                 "when '999978' then '" + obj.state + "' " +
                                 "when '999979' then '" + obj.country + "' " +
                                 "when '999915' then '" + obj.longitude + "' " +
                                 "when '999916' then '" + obj.latitude + "' " +
                                 "when '999912' then '" + obj.vendor + "' " +
                                 "when '999913' then '" + obj.devicemodel + "' " +
                                 "when '999914' then '" + obj.devicetype + "' " +
                                 "when '999977' then '" + obj.branch + "' " +
                                 "when '1' then '" + obj.branchcode + "' " +
                                 "when '999999' then '" + obj.orginization + "' " +
                                 "when '999994' then '" + obj.softwarever + "' " +
                                 "when '999993' then '" + obj.instdate + "' " +
                                 "when '999991' then '" + obj.serialno + "' " +
                                 "when '999990' then '" + obj.system + "' " +
                                 "when '999986' then '" + obj.agentver + "' " +
                                 "when '999901' then '" + obj.contact1 + "' " +
                                 "when '999902' then '" + obj.contact2 + "' " +
                                 "when '999903' then '" + obj.contact3 + "' " +
                            "end " +
                    "where id = '" + obj.id + "'";


                    int output = db.Database.ExecuteSqlCommand("Update basedata set value = case reference when '999904' then '" + obj.profile + "' when '999988' then '" + obj.timezone + "' " +
                                 "when '999998' then '" + obj.location + "' " +
                                 "when '999905' then '" + obj.url + "' " +
                                 "when '999992' then '" + obj.description + "' " +
                                 "when '999997' then '" + obj.street + "' " +
                                 "when '999996' then '" + obj.zip + "' " +
                                 "when '999995' then '" + obj.city + "' " +
                                 "when '999978' then '" + obj.state + "' " +
                                 "when '999979' then '" + obj.country + "' " +
                                 "when '999915' then '" + obj.longitude + "' " +
                                 "when '999916' then '" + obj.latitude + "' " +
                                 "when '999912' then '" + obj.vendor + "' " +
                                 "when '999913' then '" + obj.devicemodel + "' " +
                                 "when '999914' then '" + obj.devicetype + "' " +
                                 "when '999977' then '" + obj.branch + "' " +
                                 "when '1' then '" + obj.branchcode + "' " +
                                 "when '999999' then '" + obj.orginization + "' " +
                                 "when '999994' then '" + obj.softwarever + "' " +
                                 "when '999993' then '" + obj.instdate + "' " +
                                 "when '999991' then '" + obj.serialno + "' " +
                                 "when '999990' then '" + obj.system + "' " +
                                 "when '999986' then '" + obj.agentver + "' " +
                                 "when '999901' then '" + obj.contact1 + "' " +
                                 "when '999902' then '" + obj.contact2 + "' " +
                                 "when '999903' then '" + obj.contact3 + "' " +
                            "end " +
                    "where id = '" + obj.id + "'");

                    if (output > 0)
                    {

                        @TempData["OKMsg"] = "Device Updated Successfully!";
                        Log("Update Basedata", obj.id, 10003007, "$dId: " + obj.id);

                        userlog.Info(Session["UserID"].ToString() + " Device Master Data Updated Successfully " + obj.idold);
                        return RedirectToAction("DevBasedata", new { id = obj.id });
                    }
                    else
                    {
                        userlog.Info(Session["UserID"].ToString() + " Device Master Data Updated Unsuccessful " + obj.idold);
                        @TempData["NoMsg"] = "Device Udpate Unsuccessful!";
                        Log("Update Basedata Failed", obj.id, 10003008, "$dId: " + obj.id);
                        return RedirectToAction("DevBasedata", new { id = obj.id });
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("DevBasedata", new { id = obj.id });
            }
        }

        //[HttpGet]
        //public ActionResult devDelete(String id)
        //{
        //    if (Session["UserID"] == null)
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //    else
        //    {
        //        var ret = Check("16");
        //        if (ret == false)
        //        {
        //            return RedirectToAction("Index", "Login");
        //        }
        //    }
        //    try
        //    {
        //        using (IISContext db = new IISContext())
        //        {
        //            try
        //            {
        //                activitylog.Info(Session["UserID"].ToString() + " is deleting device " + id);
        //                String Devid = id;
        //                var result = db.Database.ExecuteSqlCommand("exec DELETEDEVICE @deviceid='" + id + "'");
        //                if (result >= 0)
        //                {
        //                    @TempData["OKMsg"] = "Device Deleted Successfully!";
        //                    activitylog.Info(Session["UserID"].ToString() + " Device Deleted Successfully " + id);


        //                }
        //                else
        //                {
        //                    @TempData["NoMsg"] = "Device Could not be Deleted ";
        //                    errorlog.Error(Session["UserID"].ToString() + " Device Could not be Deleted " + id);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //                @TempData["NoMsg"] = "Device Could not be Deleted ";
        //            }


        //            return RedirectToAction("Device", "Device");
        //        }


        //    //activitylog.Info(Session["UserID"].ToString() + " is deleting device " + id);
        //    //String Devid = id;
        //    //var result = db.Database.ExecuteSqlCommand("exec DELETEDEVICE @deviceid='" + id + "'");

        //    //using (var dbCtxTxn = db.Database.BeginTransaction())
        //    //{
        //    //    try
        //    //    {
        //    //        var output = db.Database.ExecuteSqlCommand("Delete from Device where DeviceID = '" + Devid + "'");

        //    //        if (output > 0)
        //    //        {
        //    //            db.Database.ExecuteSqlCommand("Delete from componentstate where DeviceID = '" + Devid + "'");
        //    //            var output2 = db.Database.ExecuteSqlCommand("Delete from DeviceLastImage where DeviceID = '" + Devid + "'");
        //    //            if (output2 >= 0)
        //    //            {
        //    //                var output3 = db.Database.ExecuteSqlCommand("Delete from CASHPHYS_NEW where DEVICEID = '" + Devid + "'");

        //    //                if (output3 >= 0)
        //    //                {
        //    //                    var output4 = db.Database.ExecuteSqlCommand("Delete from basedata where id = '" + Devid + "'");

        //    //                    if (output4 >= 0)
        //    //                    {
        //    //                        var output5 = db.Database.ExecuteSqlCommand("Delete from lastevent where deviceid = '" + Devid + "'");
        //    //                        var output6 = UserCustom.RemoveAssignDevice(Devid);
        //    //                        var output7 = db.Database.ExecuteSqlCommand("Delete from lastcomponentevent where deviceid = '" + Devid + "'");


        //    //                        if (output5 >= 0)
        //    //                        {
        //    //                            dbCtxTxn.Commit();
        //    //                            @TempData["OKMsg"] = "Device Deleted Successfully!";
        //    //                            activitylog.Info(Session["UserID"].ToString() + " Device Deleted Successfully " + id);
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            dbCtxTxn.Commit();
        //    //                            @TempData["OKMsg"] = "Device Deleted Successfully!";
        //    //                            activitylog.Info(Session["UserID"].ToString() + " Device Deleted Successfully " + id);
        //    //                        }
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        dbCtxTxn.Rollback();
        //    //                        @TempData["NoMsg"] = "Device Could not be Deleted ";
        //    //                        activitylog.Info(Session["UserID"].ToString() + " Device Could not be Deleted " + id);
        //    //                    }
        //    //                }
        //    //                else
        //    //                {
        //    //                    dbCtxTxn.Rollback();
        //    //                    @TempData["NoMsg"] = "Device Could not be Deleted ";
        //    //                    activitylog.Info(Session["UserID"].ToString() + " Device Could not be Deleted " + id);
        //    //                }
        //    //            }
        //    //            else
        //    //            {
        //    //                dbCtxTxn.Rollback();
        //    //                @TempData["NoMsg"] = "Device Could not be Deleted ";
        //    //                activitylog.Info(Session["UserID"].ToString() + " Device Could not be Deleted " + id);
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            dbCtxTxn.Rollback();
        //    //            @TempData["NoMsg"] = "Device Could not be Deleted ";
        //    //            activitylog.Info(Session["UserID"].ToString() + " Device Could not be Deleted " + id);
        //    //        }

        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        dbCtxTxn.Rollback();
        //    //        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //    //        @TempData["NoMsg"] = "Device Could not be Deleted ";
        //    //    }
        //    //}


        //    }
        //    catch (Exception ex)
        //    {
        //        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
        //        @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
        //        return RedirectToAction("Device", "Device");
        //    }
        //}

        [HttpGet]
        public ActionResult devDelete(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("16");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    try
                    {
                        activitylog.Info(Session["UserID"].ToString() + " is deleting device " + id);
                        String Devid = id;
                        var result = db.Database.ExecuteSqlCommand("exec DELETEDEVICE @deviceid='" + id + "'");
                        if (result >= 0)
                        {
                            Log("Delete Device", Devid, 10003005, "$dId: " + Devid);
                            @TempData["OKMsg"] = "Device Deleted Successfully!";
                            userlog.Info(Session["UserID"].ToString() + " : device deleted successfully " + id);


                        }
                        else
                        {
                            Log("Delete Device", Devid, 10003006, "$dId: " + Devid);
                            @TempData["NoMsg"] = "Device Could not be Deleted ";
                            errorlog.Error(Session["UserID"].ToString() + " : device could not be deleted " + id);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                        @TempData["NoMsg"] = "Device Could not be Deleted ";
                        Log("Delete Device", "", 10003006, "$ex-msg: " + ex.Message);
                    }


                    return RedirectToAction("Device", "Device");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Delete Device", "", 10003006, "$ex-msg: " + ex.Message);
                return RedirectToAction("Device", "Device");
            }
        }

        public ActionResult DevAssign()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("18");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to device assign");

                var userList = new SelectList(db.Users.ToList(), "", "UserID");
                ViewData["DBUserList"] = userList;

                var obj = new Device();
                // obj.Devicess = db.Devices.ToList();

                obj.myDevice = UserCustom.GetAssignDevice(Session["UserID"].ToString());


                return View(obj);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return View("Error");
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }


        public JsonResult DevAssignUser(string id)
        {

            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to device assign");

                var userList = new SelectList(db.Users.ToList(), "", "UserID");
                ViewData["DBUserList"] = userList;

                var obj = new Device();
                obj.Devicess = db.Devices.ToList();

                obj.myDevice = UserCustom.GetAssignDevice(id.ToString());

                return Json(obj.myDevice, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult DevAssignPost(User collect)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("19");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                ViewData["Cameras"] = db.AddCameras.ToList();
                var obj = new Device();
                UserCustom assignedDevices = new UserCustom();
                String usr = collect.UserID;
                TempData["usrID"] = usr;
                TempData.Keep();
                var Devices = db.Devices.ToList();
                //var Devices = assignedDevices.GetAssignDevice(usr).ToList();
                var Hierarchy = db.Hierarchies.ToList();
                //obj.data = Devices;

                obj.hierarchies = Hierarchy;
                obj.Devicess = Devices;
                obj.arry = db.Users.Where(a => a.UserID.Equals(usr)).Select(a => a.ATM).FirstOrDefault();

                userlog.Info(Session["UserID"].ToString() + " : is assigning devices to user " + usr);
                return View(obj);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("DevAssign", "Device");
            }
        }//yeh bhi check kro

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult DevAssignLast(FormCollection collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("19");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                List<String> getdata = new List<string>();
                var UserID = collection["UserID"];
                var condition = collection["condition"];
                var SelectedRadioBtn = collection["Radio"];
                var DeviceType = collection["Dtype"];
                var hier = collection["hierarchy"];
                var devices = collection["Alldevices"];

                if (SelectedRadioBtn == "hierarchyDiv" && DeviceType != null)
                {
                    hier = hier.Replace(",", "'',''");
                    hier = "''" + hier + "''";
                    DeviceType = DeviceType.Replace(",", "'',''");
                    DeviceType = "''" + DeviceType + "''";

                    condition = string.Format("HierLevel IN ({0}) AND DeviceType IN({1})", hier, DeviceType);
                }
                else if (SelectedRadioBtn == "hierarchyDiv")
                {
                    hier = hier.Replace(",", "'',''");
                    hier = "''" + hier + "''";
                    condition = string.Format("HierLevel IN ({0})", hier);


                }
                else if (SelectedRadioBtn == "deviceDiv")
                {
                    devices = devices.Replace(",", "'',''");
                    devices = "''" + devices + "''";
                    condition = string.Format(" DeviceID IN ({0})", devices);

                }
                string query = string.Format("Update Users Set ATM = '{0}' where UserID = '{1}'", condition, collection["UserID"]);
                int output = db.Database.ExecuteSqlCommand(query);
                if (output > 0)
                {
                    userlog.Info(Session["UserID"].ToString() + " : device assigned successfully to user " + collection["UserID"]);
                    @TempData["OKMsg"] = "Device Assigned Successfully!";
                    Log("Device Assigned", "", 10003101, "$data: " + condition + " to $userId: " + UserID);
                    return RedirectToAction("DevAssign", "Device");
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error: " + output);
                    TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    Log("Device Assigned", "", 10003102, "$data: " + condition + " to $userId: " + UserID);
                    return RedirectToAction("DevAssign", "Device");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Device Assigned", "", 10003101, "$ex-msg: " + ex.Message);
                return RedirectToAction("DevAssign", "Device");
            }

        }

        public ActionResult Hierarchy()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("20");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " navigate to hierarchy");
                var mymodel = new Hierarchy();
                mymodel.HierarchyList = db.Hierarchies.ToList();
                //ViewData["HierParent"] = new SelectList(mymodel.HierarchyList.Select(a => a.HierName));
                List<SelectListItem> Hlist = new List<SelectListItem>();

                foreach (var item in mymodel.HierarchyList)
                {
                    SelectListItem li = new SelectListItem
                    {
                        Text = item.HierName,
                        Value = item.HierId
                    };
                    Hlist.Add(li);
                }
                ViewData["HierParent"] = Hlist;
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
        public ActionResult addHierarchy(Hierarchy collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("21");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is creating new hierarchy ");
                var HierName = collection.HierName;
                var Parent = collection.Parent;
                var addHierarchy = "";
                if (Parent == true)
                {
                    var HierParent = collection.Hierlist.ToString();
                    addHierarchy = UserCustom.AddHierarchy(HierName, HierParent);//add hierarchy class
                }
                else
                {
                    addHierarchy = UserCustom.AddHierarchy(HierName);//add hierarchy class
                }

                if (addHierarchy == "Success")
                {
                    userlog.Info(Session["UserID"].ToString() + " : Hierarchy added Successfully " + collection.HierName);
                    @TempData["OKMsg"] = "Hierarchy added Successfully!";
                    Log("Add Hierarchy", "", 10003501, "$hiername: " + collection.HierName);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (addHierarchy == "Fail")
                {
                    errorlog.Info(Session["UserID"].ToString() + " Hierarchy added Unsuccessfully " + collection.HierName);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    Log("Add Hierarchy Failed", "", 10003502, "$hiername: " + collection.HierName);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (addHierarchy == "Exist")
                {
                    errorlog.Info(Session["UserID"].ToString() + ": Hierarchy Name Already Exists ");
                    @TempData["NoMsg"] = "Hierarchy Name Already Exists";
                    Log("Hierarchy Name Already Exists", "", 10003502, "$hiername: " + addHierarchy);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else
                {
                    errorlog.Error("User: " + Session["UserID"] + addHierarchy);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    Log("Add Hierarchy Failed", "", 10003502, "$hiername: " + addHierarchy);
                    return RedirectToAction("Hierarchy", "Device");
                }


                //var Hierlevel = collection.Hierlevel;
                //var checklevel = db.Hierarchies.SqlQuery("select * from Hierarchy where Hierlevel='" + Hierlevel + "'").ToList();
                //var checkName = db.Hierarchies.SqlQuery("select * from Hierarchy where HierName='" + HierName + "'").ToList();

                //if (checklevel.Count < 1)
                //{
                //    if (checkName.Count < 1)
                //    {
                //        int addHir = db.Database.ExecuteSqlCommand("insert into hierarchy (HierName,Hierlevel)values('" + HierName + "','" + Hierlevel + "')");
                //        if (addHir > 0)
                //        {
                //            activitylog.Info(Session["UserID"].ToString() + " Hierarchy added Successfully " + collection.HierName);
                //            @TempData["OKMsg"] = "Hierarchy added Successfully!";
                //            return RedirectToAction("Hierarchy", "Device");
                //        }
                //        else
                //        {
                //            activitylog.Info(Session["UserID"].ToString() + " Hierarchy added Unsuccessfully " + collection.HierName);
                //            @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                //            return RedirectToAction("Hierarchy", "Device");
                //        }
                //    }
                //    else
                //    {
                //        activitylog.Info(Session["UserID"].ToString() + " Hierarchy Name Already Exists " + collection.HierName);

                //        @TempData["NoMsg"] = "Hierarchy Name Already Exists";
                //        return RedirectToAction("Hierarchy", "Device");
                //    }

                //}
                //else
                //{
                //    activitylog.Info(Session["UserID"].ToString() + " Hierarchy Level Already Exists " + collection.HierName);

                //    @TempData["NoMsg"] = "Hierarchy Level Already Exists";
                //    return RedirectToAction("Hierarchy", "Device");
                //}

            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Add Hierarchy Failed", "", 10003502, "$hiername: " + collection.HierName + "$ex-msg: " + ex.Message);
                return RedirectToAction("Hierarchy", "Device");
            }
        }

        public ActionResult hierDelete(String name, int level)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("22");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is deleting hierarchy");

                var DelHierarachy = UserCustom.DelHierarchy(name, level);

                if (DelHierarachy == "Success")
                {
                    userlog.Info(Session["UserID"].ToString() + " : hierarchy deleted successfully " + name);
                    @TempData["OKMsg"] = "Hierarchy Deleted Successfully!";
                    Log("Delete Hierarchy", "", 10003503, "$hiername: " + name);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (DelHierarachy == "ContainsDevice")
                {
                    errorlog.Info(Session["UserID"].ToString() + DelHierarachy);
                    errorlog.Error(Session["UserID"].ToString() + DelHierarachy);
                    @TempData["NoMsg"] = "Unsuccessfull! Devices are assigned to this Hierarchy!";
                    Log("Delete Hierarchy Failed", "", 10003504, "$hiername: " + name + "$reason: Devices are assigned to this Hierarchy");
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (DelHierarachy == "ContainsSubHierarchy")
                {
                    errorlog.Info(Session["UserID"].ToString() + DelHierarachy);
                    @TempData["NoMsg"] = "Unsuccessfull! Hierarchy contains sub-hierarchy!";
                    Log("Delete Hierarchy Failed", "", 10003504, "$hiername: " + name + "$reason: Hierarchy contains sub-hierarchy");
                    return RedirectToAction("Hierarchy", "Device");
                }
                else
                {
                    errorlog.Info(Session["UserID"].ToString() + " Hierarchy Deleted Unsuccessfully " + name);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    Log("Delete Hierarchy Failed", "", 10003504, "$hiername: " + name + "$reason: something went wrong");
                    return RedirectToAction("Hierarchy", "Device");
                }
                //int deleteHir = db.Database.ExecuteSqlCommand("DELETE FROM hierarchy WHERE HierName = '" + name + "' AND Hierlevel = '" + level + "'");
                //if (deleteHir > 0)
                //{
                //    activitylog.Info(Session["UserID"].ToString() + " Hierarchy Deleted Successfully " + name);
                //    @TempData["OKMsg"] = "Hierarchy Deleted Successfully!";
                //    return RedirectToAction("Hierarchy", "Device");
                //}
                //else
                //{
                //    activitylog.Info(Session["UserID"].ToString() + " Hierarchy Deleted Unsuccessfully " + name);
                //    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                //    return RedirectToAction("Hierarchy", "Device");
                //}
            }
            catch (Exception ex)
            {
                activitylog.Info(Session["UserID"].ToString() + " Hierarchy Deleted Unsuccessfully " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Delete Hierarchy Failed", "", 10003504, "$hiername: " + name + "ex-msg: " + ex.Message);
                return RedirectToAction("Hierarchy", "Device");
            }
        }

        [HttpPost]
        public ActionResult EditHierarchy(Hierarchy collection)
        {
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " is Editing hierarchy " + collection.OldHierlevel);
                var HierName = collection.HierNameEdit;
                var Parent = collection.ParentEdit;
                var addHierarchy = "";
                if (Parent == true)
                {
                    var HierParent = collection.HierlistEdit.ToString();
                    addHierarchy = UserCustom.EditHierarchy(HierName, collection.OldHierlevel.ToString(), HierParent);//edit hierarchy class
                }
                else
                {
                    addHierarchy = UserCustom.EditHierarchy(HierName, collection.OldHierlevel.ToString());//edit hierarchy class
                }

                if (addHierarchy == "Success")
                {
                    userlog.Info(Session["UserID"].ToString() + " : Hierarchy added Successfully " + collection.HierNameEdit);
                    @TempData["OKMsg"] = "Hierarchy added Successfully!";
                    Log("Edit Hierarchy", "", 10003505, "$hiername: " + collection.HierNameEdit);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (addHierarchy == "Fail")
                {
                    errorlog.Info(Session["UserID"].ToString() + " : Hierarchy added Unsuccessfully " + collection.HierNameEdit);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    Log("Edit Hierarchy Failed", "", 10003506, "$hiername: " + collection.HierNameEdit);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (addHierarchy == "Exist")
                {
                    @TempData["NoMsg"] = "Hierarchy Already Exists";
                    Log("Hierarchy Already Exists", "", 10003506, "$hiername: " + collection.HierNameEdit);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else if (addHierarchy == "ContainsSubHierarchy")
                {
                    errorlog.Info(Session["UserID"].ToString() + " : Hierarchy added Unsuccessfully " + collection.HierNameEdit);
                    errorlog.Error(Session["UserID"].ToString() + " : Hierarchy added Unsuccessfully " + collection.HierNameEdit);
                    @TempData["NoMsg"] = "Contains Sub-Hierarchy!";
                    Log("Edit Hierarchy Failed", "", 10003506, "$hiername: " + collection.HierNameEdit);
                    return RedirectToAction("Hierarchy", "Device");
                }
                else
                {
                    Log("Edit Hierarchy Failed", "", 10003506, "$hiername: " + collection.HierNameEdit);
                    errorlog.Error("User: " + Session["UserID"] + addHierarchy);
                    @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                    return RedirectToAction("Hierarchy", "Device");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Edit Hierarchy Failed", "", 10003506, "$hiername: " + collection.HierNameEdit + "$reason: " + ex.Message);
                return RedirectToAction("Hierarchy", "Device");
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
            pvjournal.issuer = "DEVICE_CONTROLLER";
            pvjournal.issuertype = 10;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";

            Audit audit = new Audit();
            audit.Log(pvjournal);
        }

    }
}
