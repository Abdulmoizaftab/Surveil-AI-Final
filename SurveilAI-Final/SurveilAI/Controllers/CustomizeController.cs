using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;


namespace SurveilAI.Controllers
{
    public class CustomizeController : Controller
    {
        SurveilAIEntities db = new SurveilAIEntities();
        SurveilAIEntities dbPro = new SurveilAIEntities();
        stb obj = new stb();
        ILogger errorlog = LogManager.GetLogger("error");
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger userlog = LogManager.GetLogger("user");


        pvgroup obj1 = new pvgroup();
        pvgroupcondition obj2 = new pvgroupcondition();

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

        // GET: Customize
        public ActionResult StateRepresentation()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("57");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            errorlog.Error(Session["UserID"] + " navigaet to customize page ");

            String sql = "select s.*,m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 order by prio";

            var result = dbPro.Database.SqlQuery<Data>(sql);
            var iu = "";
            var d = "";
            foreach (var x in result)
            {
                if (x.Internalused == 1)
                {
                    iu = "Yes";
                }
                else
                {
                    iu = "No";
                }
                if (x.Display == 1)
                {
                    d = "Yes";
                }
                else
                {
                    d = "No";
                }
                string[] words = x.Color.Split(' ');
                string col = "rgb(" + words[0] + "," + words[1] + "," + words[2] + ")";
                col.Replace(' '.ToString(), String.Empty);
                obj.StbALL.Add(new Tuple<string, string, string, int, string, int>(iu, col, d, x.Prio, x.Messagetext, x.Bit));
            }
            var Prio = dbPro.stbs.Where(a => a.type == 0).OrderBy(a => a.prio).Select(a => a.prio).ToList();
            ViewData["Prior"] = new SelectList(Prio);

            return View(obj);
        }

        public class Data
        {
            public int Internalused { get; set; }
            public string Color { get; set; }
            public int Display { get; set; }
            public int Prio { get; set; }
            public string Messagetext { get; set; }
            public int Bit { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StateUpdate(stb collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("57");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            errorlog.Error(Session["UserID"] + " is updating state ");

            String color = collection.color;
            int prior = Convert.ToInt32(collection.prio);
            String txtmsg = collection.messagetext;
            String disp = collection.disp.ToString();
            String data = collection.data;

            if (data == null)
            {
                return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Error!');
                       </script>
                     </body> ");
            }

            String[] ip = data.Split(',');

            String intused = ip[0];
            int last = Convert.ToInt16(ip[1]);
            int bit = Convert.ToInt16(ip[2]);

            StringBuilder sb = new StringBuilder(color);
            sb.Remove(0, 4);
            sb.Remove((sb.Length - 1), 1);
            color = sb.ToString();
            String[] col = color.Split(',');
            color = col[0].Trim() + " " + col[1].Trim() + " " + col[2].Trim();

            if (disp == "True")
            {
                disp = "1";
            }
            else if (disp == "False")
            {
                disp = "0";
            }

            if (intused == "Yes")
            {
                intused = "1";
            }
            else if (intused == "No")
            {
                intused = "0";
            }

            if (last == prior)
            {
                using (var dbCtxTxn = dbPro.Database.BeginTransaction())
                {
                    try
                    {
                        var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
                        if (sql > 0)
                        {
                            //log likhwao scene done hai
                            var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
                            var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
                            if (sql2 > 0)
                            {
                                userlog.Info(Session["UserID"].ToString() + " successfully updated state : " + bit);
                                dbCtxTxn.Commit();
                                Log("message update", "", 10000201, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                                TempData["Response"] = "Ok";
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                Log("message update Failed", "", 10000202, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                                TempData["Response"] = "Error";
                            }
                        }
                        else
                        {
                            //bad scene hai
                            dbCtxTxn.Rollback();
                            Log("color update", "", 10000301, "$bit: " + bit + "$type: '0'");
                            TempData["Response"] = "Error";
                        }
                    }
                    catch
                    {
                        dbCtxTxn.Rollback();
                        TempData["Response"] = "Error";
                    }
                }
            }
            else if (last > prior)
            {
                using (var dbCtxTxn = dbPro.Database.BeginTransaction())
                {
                    try
                    {
                        var upd = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = prio + 1 where prio >= '" + prior + "' and prio < '" + last + "' and type = '0'");

                        var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
                        if (sql > 0)
                        {
                            //log likhwao scene done hai
                            var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
                            var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
                            if (sql2 > 0)
                            {
                                userlog.Info(Session["UserID"].ToString() + " successfully updated priority : " + prior);
                                dbCtxTxn.Commit();
                                Log("message update", "", 10000201, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                Log("message update", "", 10000202, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                            }
                        }
                        else
                        {
                            //bad scene hai
                            dbCtxTxn.Rollback();
                            Log("color update", "", 10000301, "$bit: " + bit + "$type: '0'");
                        }
                    }
                    catch (Exception ex)
                    {
                        dbCtxTxn.Rollback();
                        Log("color update", "", 10000301, "$ex: " + ex.Message);
                    }
                }
            }
            else if (last < prior)
            {
                using (var dbCtxTxn = dbPro.Database.BeginTransaction())
                {
                    try
                    {
                        var upd = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = prio - 1 where prio <= '" + prior + "' and prio > '" + last + "' and type = '0'");

                        var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
                        if (sql > 0)
                        {
                            var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
                            var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
                            if (sql2 > 0)
                            {
                                userlog.Info(Session["UserID"].ToString() + " successfully updated priority : " + prior);

                                //log
                                dbCtxTxn.Commit();
                                Log("message update", "", 10000201, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                            }
                            else
                            {
                                //log
                                dbCtxTxn.Rollback();
                                Log("message update", "", 10000202, "$messageId: " + mesg.textno + "$messageType: " + mesg.texttype);
                            }
                        }
                        else
                        {
                            //bad scene hai
                            dbCtxTxn.Rollback();
                            Log("color update", "", 10000301, "$bit: " + bit + " $type:'0'");
                        }
                    }
                    catch (Exception ex)
                    {
                        dbCtxTxn.Rollback();
                        Log("color update", "", 10000301, "$ex: " + ex.Message);
                    }
                }
            }

            return RedirectToAction("StateRepresentation", "Customize");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StateCreate(stb collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("57");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  navigate to state create");

            String color = collection.color;
            int prior = Convert.ToInt32(collection.prio);
            String txtmsg = collection.messagetext;
            String disp = collection.disp.ToString();
            String data = collection.data;

            if (data == null)
            {
                return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Error!');
                       </script>
                     </body> ");
            }

            String[] ip = data.Split(',');

            String intused = ip[0];
            int last = Convert.ToInt16(ip[1]);
            int bit = Convert.ToInt16(ip[2]);

            StringBuilder sb = new StringBuilder(color);
            sb.Remove(0, 4);
            sb.Remove((sb.Length - 1), 1);
            color = sb.ToString();
            String[] col = color.Split(',');
            color = col[0].Trim() + " " + col[1].Trim() + " " + col[2].Trim();

            if (disp == "True")
            {
                disp = "1";
            }
            else if (disp == "False")
            {
                disp = "0";
            }

            if (intused == "Yes")
            {
                intused = "1";
            }
            else if (intused == "No")
            {
                intused = "0";
            }

            //if (last == prior)
            //{
            //    using (var dbCtxTxn = dbPro.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
            //            if (sql > 0)
            //            {
            //                //log likhwao scene done hai
            //                var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
            //                var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
            //                if (sql2 > 0)
            //                {
            //                    dbCtxTxn.Commit();
            //                }
            //                else
            //                {
            //                    dbCtxTxn.Rollback();
            //                }
            //            }
            //            else
            //            {
            //                //bad scene hai
            //                dbCtxTxn.Rollback();
            //            }
            //        }
            //        catch
            //        {
            //            dbCtxTxn.Rollback();
            //        }
            //    }
            //}
            //else if (last > prior)
            //{
            //    using (var dbCtxTxn = dbPro.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            var upd = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = prio + 1 where prio >= '" + prior + "' and prio < '" + last + "' and type = '0'");

            //            var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
            //            if (sql > 0)
            //            {
            //                //log likhwao scene done hai
            //                var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
            //                var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
            //                if (sql2 > 0)
            //                {
            //                    dbCtxTxn.Commit();
            //                }
            //                else
            //                {
            //                    dbCtxTxn.Rollback();
            //                }
            //            }
            //            else
            //            {
            //                //bad scene hai
            //                dbCtxTxn.Rollback();
            //            }
            //        }
            //        catch
            //        {
            //            dbCtxTxn.Rollback();
            //        }
            //    }
            //}
            //else if (last < prior)
            //{
            //    using (var dbCtxTxn = dbPro.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            var upd = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = prio - 1 where prio <= '" + prior + "' and prio > '" + last + "' and type = '0'");

            //            var sql = dbPro.Database.ExecuteSqlCommand("UPDATE stb SET prio = '" + prior + "', internalused = '" + intused + "', color = '" + color + "', display = '" + disp + "' where bit = '" + bit + "' and type = '0'");
            //            if (sql > 0)
            //            {
            //                var mesg = dbPro.stbs.Where(a => a.bit.Equals(bit)).Select(a => new { a.textno, a.texttype }).FirstOrDefault();
            //                var sql2 = dbPro.Database.ExecuteSqlCommand("UPDATE message0001 SET messagetext = '" + txtmsg + "' where textno = '" + mesg.textno + "' AND texttype = '" + mesg.texttype + "'");
            //                if (sql2 > 0)
            //                {
            //                    //log
            //                    dbCtxTxn.Commit();
            //                }
            //                else
            //                {
            //                    //log
            //                    dbCtxTxn.Rollback();
            //                }
            //            }
            //            else
            //            {
            //                //bad scene hai
            //                dbCtxTxn.Rollback();
            //            }
            //        }
            //        catch
            //        {
            //            dbCtxTxn.Rollback();
            //        }
            //    }
            //}

            return RedirectToAction("StateRepresentation", "Customize");
        }


        public ActionResult Rules()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("34");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            pmschedule obj = new pmschedule();
            activitylog.Info(Session["UserID"].ToString() + " :  navigate to rules");
            var RuleList = new SelectList(db.pmactions.Where(a => a.type == 7).Distinct().ToList(), "", "pmid");
            //galat hai yeh
            var sql = "select s.bit, s.color, m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype=3 order by prio";
            var result = dbPro.Database.SqlQuery<SurveilAI.Models.Data2>(sql);

            foreach (var x in result)
            {
                obj.state.Add(x);
            }
            //galat hai yeh
            ViewData["Contact"] = new SelectList(db.contacts.Select(a => a.contactid).ToList());
            ViewData["Rule"] = RuleList;
            //obj.events = db.message0001.Where(a => a.texttype == 3 && a.textno.ToString().StartsWith("9999")).ToList();
            //obj.events = db.eventbases.Where(a => a.forwardrule.Equals(1)).Join(db.message0001, e => e.textno, m => m.textno, (e, m) => m).ToList();
            //changed  event getting 

            List<message0001> Eventconf = new List<message0001>();

            var d = from t1 in db.eventbases
                    join t2 in db.message0001 on new { t1.textno, t1.texttype } equals new { t2.textno, t2.texttype }
                    orderby t2.textno
                    select new { t2.textno, t2.messagetext };

            foreach (var x in d.Skip(1))
            {

                Eventconf.Add(new message0001 { textno = x.textno, messagetext = x.messagetext });
            }

            obj.events = Eventconf;
            //obj.devices = db.Devices.Select(a => a.DeviceID).ToList();
            obj.data = db.pmschedules.ToList();
            //obj.Hierar = db.Hierarchies.ToList();

            UserCustom uc = new UserCustom();
            List<Device> getDevice = uc.GetAssignDevice(Session["UserID"].ToString());
            List<string> UsrAssigendDev = getDevice.Select(x => x.DeviceID).ToList<string>();
            obj.devices = UsrAssigendDev;
            obj.Hierar = db.Hierarchies.ToList();
            obj.Hierar = obj.Hierar.Where(x => getDevice.Any(y => y.HierLevel == x.Hierlevel)).ToList();

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RulePost(pmschedule collect)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("35");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " : is creating a rule ");

            ///////////////Default////////////////////////
            pmschedule obj = new pmschedule();
            pmaction act = new pmaction();

            if (collect.pmid.Equals(" "))
            {
                return RedirectToAction("Index", "Login");

            }

            var RuleList = new SelectList(db.pmactions.Where(a => a.type == 7).Distinct().ToList(), "", "pmid");
            var sql = "select s.bit, s.color, m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype=3 order by prio";
            var result = dbPro.Database.SqlQuery<SurveilAI.Models.Data2>(sql);

            foreach (var x in result)
            {
                obj.state.Add(x);
            }
            ViewData["Contact"] = new SelectList(db.contacts.Select(a => a.contactid).ToList());
            ViewData["Rule"] = RuleList;
            //obj.events = db.message0001.Where(a => a.texttype == 3 && a.textno.ToString().StartsWith("9999")).ToList();
            obj.events = db.eventbases.Where(a => a.forwardrule.Equals(1)).Join(db.message0001, e => e.textno, m => m.textno, (e, m) => m).ToList();

            obj.devices = db.Devices.Select(a => a.DeviceID).ToList();
            obj.data = db.pmschedules.ToList();
            obj.Hierar = db.Hierarchies.ToList();

            ///////////////Default////////////////////////

            obj.pmid = collect.pmid;
            obj.check = collect.check;
            obj.description = collect.description;
            obj.resultneeded = collect.resultneeded;
            obj.journalneeded = collect.journalneeded;
            obj.dayofweek = collect.dayofweek;
            //if(collect.condition != null)
            //{
            //    obj.condition = collect.condition.Split().Where(x => x.StartsWith("(") && x.EndsWith(")")).ToString();
            //    obj.condition = obj.condition.Remove('\'');
            //    //var getchild = db.Hierarchies.Where(a => obj.condition.Contains(a.Hierlevel.ToString()))
            //}
            obj.condition = collect.condition;
            obj.triggertype = collect.triggertype;

            obj.noevents = collect.noevents;
            obj.eventno = collect.eventno;//pmevents

            obj.leveltriggered = collect.leveltriggered;
            obj.setreset = collect.setreset;
            obj.statebitmask = collect.statebitmask;

            obj.problemevent = collect.problemevent;
            obj.classification = collect.classification;
            obj.problempmid = collect.problempmid;
            obj.timetype = collect.timetype;
            obj.timetoescalate = collect.timetoescalate;
            obj.escalation = collect.escalation;

            obj.Actdata = collect.Actdata[0].Split('^');//pmaction
            var actioncount = 0;
            if (obj.Actdata != null)
            {
                actioncount = (obj.Actdata.Length - 1) / 2;
            }
            if (obj.check == "Set")
            {
                var check = db.pmschedules.Where(a => a.pmid.Equals(collect.pmid)).ToList();

                if (check.Count != 0)
                {
                    TempData["Err"] = "Rule already exists";
                    return RedirectToAction("Rules", "Customize");
                }

                using (var dbCtxTxn = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (obj.triggertype == 1)//Event
                        {
                            userlog.Info(Session["UserID"].ToString() + " is creating a rule :" + obj.pmid);
                            var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, null, null, '" + obj.noevents + "', '0', '0', null, null, null, null, ' ', ' ', null, '0', '0', '0', null, null, '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                            if (output > 0)
                            {
                                if (obj.eventno != null)
                                {
                                    //var Eventdata = obj.eventno.Split(',');
                                    var Eventdata = obj.eventno.Split('|');

                                    string eventcheck = "No";
                                    if (obj.triggertype == 1)
                                    {
                                        if (AddEvents(Eventdata, obj.pmid))
                                        {
                                            eventcheck = "OK";
                                        }
                                        else
                                        {
                                            dbCtxTxn.Rollback();
                                            Log("Event Create", "", 10000402, "$pmid: " + obj.pmid);
                                        }
                                    }
                                    if (obj.triggertype == 1 && eventcheck == "OK")
                                    {
                                        int i = 1;
                                        if (obj.Actdata.Length - 1 > 0)
                                        {
                                            for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                            {
                                                if (AddAction(obj.Actdata, obj.pmid, z, i))
                                                {
                                                    i++;
                                                    if (z == (obj.Actdata.Length - 3))
                                                    {
                                                        dbCtxTxn.Commit();
                                                        @TempData["OKMsg"] = "Rule Created !";
                                                        Log("Event Create", "", 10000401, "$pmid: " + obj.pmid);
                                                    }
                                                }
                                                else
                                                {
                                                    dbCtxTxn.Rollback();
                                                    @TempData["NoMsg"] = "Rule Creation Failed";
                                                    Log("Event Create", "", 10000402, "$pmid: " + obj.pmid);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dbCtxTxn.Commit();
                                            @TempData["OKMsg"] = "Rule Created !";
                                            Log("Event Create", "", 10000401, "$pmid: " + obj.pmid);
                                        }

                                    }
                                }
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                @TempData["NoMsg"] = "Rule Creation Failed";
                                Log("Event Create", "", 10000402, "$pmid: " + obj.pmid);
                            }
                        }
                        else if (obj.triggertype == 2)//State
                        {
                            userlog.Info(Session["UserID"].ToString() + " is creating a rule :" + obj.pmid);
                            var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', '" + obj.statebitmask + "', '" + obj.setreset + "', '0', '0', '0', '" + obj.leveltriggered + "', null, null, null, '0', '0', ' ', ' ', null, null, ' ', ' ', null, '0', '0', '0', null, null, '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                            if (output > 0)
                            {
                                int i = 1;
                                if (obj.Actdata.Length - 1 > 0)
                                {
                                    for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                    {
                                        if (AddAction(obj.Actdata, obj.pmid, z, i))
                                        {
                                            i++;
                                            if (z == (obj.Actdata.Length - 3))
                                            {
                                                dbCtxTxn.Commit();
                                                @TempData["OKMsg"] = "Rule Created !";
                                                Log("State Creation", "", 10000501, "$pmid: " + obj.pmid);
                                            }
                                        }
                                        else
                                        {
                                            dbCtxTxn.Rollback();
                                            @TempData["NoMsg"] = "Rule Creation Failed";
                                            Log("State Creation", "", 10000502, "$pmid: " + obj.pmid);
                                        }
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Commit();
                                    @TempData["OKMsg"] = "Rule Created !";
                                    Log("State Creation", "", 10000501, "$pmid: " + obj.pmid);
                                }
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                @TempData["NoMsg"] = "Rule Creation Failed";

                                Log("State Creation", "", 10000502, "$pmid: " + obj.pmid);
                            }
                        }
                        else if (obj.triggertype == 3)//Incident
                        {
                            userlog.Info(Session["UserID"].ToString() + " is creating a rule :" + obj.pmid);
                            if (obj.problemevent == 4)//Change priority
                            {
                                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                    "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, '" + obj.problemevent + "', '" + obj.classification + "', null, '0', '0', ' ', ' ', '" + obj.timetoescalate + "', '" + obj.escalation + "', '" + obj.problempmid + "', ' ', '" + obj.timetype + "', '0', '0', '0', '0', '0', '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                                if (output > 0)
                                {
                                    int i = 1;
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();

                                                    @TempData["OKMsg"] = "Rule Created !";
                                                    Log("Incident Creation - Change Pirority", "", 10000603, "$pmid: " + obj.pmid);
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                @TempData["NoMsg"] = "Rule Creation Failed";

                                                Log("Incident Creation - Change Pirority Failed", "", 10000604, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();

                                        @TempData["OKMsg"] = "Rule Created !";
                                        Log("Incident Creation - Change Pirority", "", 10000603, "$pmid: " + obj.pmid);
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    @TempData["NoMsg"] = "Rule Creation Failed";
                                    Log("Incident Creation - Change Pirority", "", 10000603, "$pmid: " + obj.pmid);
                                }
                            }
                            else if (obj.problemevent == 8)//Close incident
                            {
                                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                    "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, '" + obj.problemevent + "', null, null, '0', '0', ' ', ' ', null, null, ' ', ' ', null, '0', '0', '0', '0', '0', '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                                if (output > 0)
                                {
                                    int i = 1;
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();

                                                    @TempData["OKMsg"] = "Rule Created !";
                                                    Log("Close Incident", "", 10000605, "$pmid: " + obj.pmid);
                                                }
                                            }
                                            else
                                            {
                                                @TempData["NoMsg"] = "Rule Creation Failed";

                                                dbCtxTxn.Rollback();
                                                Log("Close Incident", "", 10000606, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();

                                        @TempData["OKMsg"] = "Rule Created !";
                                        Log("Close Incident", "", 10000605, "$pmid: " + obj.pmid);
                                    }
                                }
                                else
                                {
                                    @TempData["NoMsg"] = "Rule Creation Failed";

                                    dbCtxTxn.Rollback();
                                    Log("Close Incident", "", 10000606, "$pmid: " + obj.pmid);
                                }
                            }
                            else if (obj.problemevent == 1)//New incident
                            {
                                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                    "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, '" + obj.problemevent + "', '1', null, '0', '0', ' ', ' ', '" + obj.timetoescalate + "', '" + obj.escalation + "', '" + obj.problempmid + "', ' ', '" + obj.timetype + "', '0', '0', '0', '0', '0', '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                                if (output > 0)
                                {
                                    int i = 1;
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();

                                                    @TempData["OKMsg"] = "Rule Created !";
                                                    Log("Create Incident", "", 10000601, "$pmid: " + obj.pmid);
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                @TempData["NoMsg"] = "Rule Creation Failed";

                                                Log("Create Incident", "", 10000602, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();

                                        @TempData["OKMsg"] = "Rule Created !";
                                        Log("Create Incident", "", 10000601, "$pmid: " + obj.pmid);
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    @TempData["NoMsg"] = "Rule Creation Failed";

                                    Log("Create Incident", "", 10000602, "$pmid: " + obj.pmid);
                                }
                            }
                            else if (obj.problemevent == 9)//Re-open incident
                            {
                                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                                    "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, '" + obj.problemevent + "', null, null, '0', '0', ' ', ' ', null, null, ' ', ' ', null, '0', '0', '0', '0', '0', '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                                if (output > 0)
                                {
                                    int i = 1;
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();

                                                    @TempData["OKMsg"] = "Rule Created !";
                                                    Log("Re-Open Incident", "", 10000607, "$pmid: " + obj.pmid);
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                @TempData["NoMsg"] = "Rule Creation Failed";

                                                Log("Re-Open Incident Failed", "", 10000608, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();

                                        @TempData["OKMsg"] = "Rule Created !";
                                        Log("Re-Open Incident", "", 10000607, "$pmid: " + obj.pmid);
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    @TempData["NoMsg"] = "Rule Creation Failed";

                                    Log("Re-Open Incident", "", 10000608, "$pmid: " + obj.pmid);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dbCtxTxn.Rollback();
                        @TempData["NoMsg"] = "Rule Creation Failed";
                        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                        Log("Error in Rule Creation", "", 10000701, "$ex-msg: " + ex.Message);
                    }
                }
            }
            else if (obj.check == "Edit")
            {
                int xx = 0;
                using (var dbCtxTxn = db.Database.BeginTransaction())
                {
                    try
                    {
                        UserCustom cc = new UserCustom();
                        string jobCondtion = db.pmschedules.Where(x => x.pmid == obj.pmid).Select(x => x.condition).FirstOrDefault<string>();
                        obj.condition = cc.FilterJobDevices(jobCondtion, obj.condition, Session["UserID"].ToString());
                        if (obj.triggertype == 1)//Event
                        {
                            userlog.Info(Session["UserID"].ToString() + " : is updating a rule :" + obj.pmid);
                            var output = db.Database.ExecuteSqlCommand("UPDATE pmschedule " +
                                "SET pmid = '" + obj.pmid + "', description = '" + obj.description + "', actioncount = '" + actioncount + "', dayofweek = '" + obj.dayofweek + "', condition = '" + obj.condition + "', triggertype = '" + obj.triggertype + "', problemevent = NULL, classification = NULL, noevents = '" + obj.noevents + "', nodevents = '0', delay = '0', timetoescalate = NULL, escalation = NULL, problempmid = '', resultneeded = '" + obj.resultneeded + "', journalneeded = '" + obj.journalneeded + "' " +
                                "WHERE pmid = '" + obj.pmid + "'");
                            if (output > 0)
                            {
                                Log("Update Event", "", 10000403, "$pmid: " + obj.pmid);
                                if (obj.eventno != null)
                                {
                                    //var Eventdata = obj.eventno.Split(',');
                                    var Eventdata = obj.eventno.Split('|');

                                    string eventcheck = "No";
                                    if (obj.triggertype == 1)
                                    {
                                        int DeleteEvent = db.Database.ExecuteSqlCommand("DELETE From pmevents Where pmid = '" + obj.pmid + "'");
                                        if (AddEvents(Eventdata, obj.pmid))
                                        {
                                            eventcheck = "OK";
                                        }
                                        else
                                        {
                                            dbCtxTxn.Rollback();
                                            Log("Delete Event", "", 10000406, "$pmid: " + obj.pmid);
                                        }
                                    }
                                    if (obj.triggertype == 1 && eventcheck == "OK")
                                    {
                                        int i = 1;
                                        int DeleteAction = db.Database.ExecuteSqlCommand("DELETE From pmaction Where pmid = '" + obj.pmid + "'");
                                        if (obj.Actdata.Length - 1 > 0)
                                        {
                                            for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                            {
                                                if (AddAction(obj.Actdata, obj.pmid, z, i))
                                                {
                                                    i++;
                                                    if (z == (obj.Actdata.Length - 3))
                                                    {
                                                        dbCtxTxn.Commit();
                                                        Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                                        xx = 1;
                                                    }
                                                }
                                                else
                                                {
                                                    dbCtxTxn.Rollback();
                                                    Log("Delete Action Failed", "", 10000808, "$pmid: " + obj.pmid);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dbCtxTxn.Commit();
                                            Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                            xx = 1;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                Log("Update Event Failed", "", 10000404, "$pmid: " + obj.pmid);
                            }
                        }
                        else if (obj.triggertype == 2)//State
                        {
                            userlog.Info(Session["UserID"].ToString() + " is updating a rule :" + obj.pmid);
                            var output = db.Database.ExecuteSqlCommand("UPDATE pmschedule " +
                                "SET pmid = '" + obj.pmid + "', description = '" + obj.description + "', actioncount = '" + actioncount + "', dayofweek = '" + obj.dayofweek + "', condition = '" + obj.condition + "', triggertype = '" + obj.triggertype + "', statebitmask = '" + obj.statebitmask + "', setreset = '" + obj.setreset + "', leveltriggered = '" + obj.leveltriggered + "', resultneeded = '" + obj.resultneeded + "', journalneeded = '" + obj.journalneeded + "' " +
                                "WHERE pmid = '" + obj.pmid + "'");
                            if (output > 0)
                            {
                                int i = 1;
                                int DeleteAction = db.Database.ExecuteSqlCommand("DELETE From pmaction Where pmid = '" + obj.pmid + "'");
                                if (obj.Actdata.Length - 1 > 0)
                                {
                                    for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                    {
                                        if (AddAction(obj.Actdata, obj.pmid, z, i))
                                        {
                                            i++;
                                            if (z == (obj.Actdata.Length - 3))
                                            {
                                                dbCtxTxn.Commit();
                                                Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                                xx = 1;
                                            }
                                        }
                                        else
                                        {
                                            dbCtxTxn.Rollback();
                                            Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);
                                        }
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Commit();
                                    Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                    xx = 1;
                                }
                            }
                            else
                            {
                                dbCtxTxn.Rollback();
                                Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);
                            }
                        }
                        else if (obj.triggertype == 3)//Incident
                        {
                            if (obj.problemevent == 4)//Change priority
                            {
                                userlog.Info(Session["UserID"].ToString() + " is updating a rule :" + obj.pmid);
                                var output = db.Database.ExecuteSqlCommand("UPDATE pmschedule " +
                                    "SET pmid = '" + obj.pmid + "', description = '" + obj.description + "', actioncount = '" + actioncount + "', dayofweek = '" + obj.dayofweek + "', condition = '" + obj.condition + "', triggertype = '" + obj.triggertype + "', problemevent = '" + obj.problemevent + "', classification = '" + obj.classification + "', noevents = NULL, timetoescalate = '" + obj.timetoescalate + "', escalation = '" + obj.escalation + "', problempmid = '" + obj.problempmid + "', timetype = '" + obj.timetype + "', resultneeded = '" + obj.resultneeded + "', journalneeded = '" + obj.journalneeded + "' " +
                                    "WHERE pmid = '" + obj.pmid + "'");
                                if (output > 0)
                                {
                                    Log("Update Action", "", 10000809, "$pmid: " + obj.pmid);
                                    int i = 1;
                                    int DeleteAction = db.Database.ExecuteSqlCommand("DELETE From pmaction Where pmid = '" + obj.pmid + "'");
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();
                                                    Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);

                                                    xx = 1;
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();
                                        Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);

                                        xx = 1;
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    Log("Update Action", "", 10000810, "$pmid: " + obj.pmid);
                                    Log("Update Action", "", 10000810, "$pmid: " + obj.pmid);

                                }
                            }
                            else if (obj.problemevent == 8)//Close incident
                            {
                                var output = db.Database.ExecuteSqlCommand("UPDATE pmschedule " +
                                    "SET pmid = '" + obj.pmid + "', description = '" + obj.description + "', actioncount = '" + actioncount + "', dayofweek = '" + obj.dayofweek + "', condition = '" + obj.condition + "', triggertype = '" + obj.triggertype + "', problemevent = '" + obj.problemevent + "', classification = NULL, noevents = NULL, timetoescalate = NULL, escalation = NULL, problempmid = '', resultneeded = '" + obj.resultneeded + "', journalneeded = '" + obj.journalneeded + "' " +
                                    "WHERE pmid = '" + obj.pmid + "'");
                                if (output > 0)
                                {
                                    int i = 1;
                                    int DeleteAction = db.Database.ExecuteSqlCommand("DELETE From pmaction Where pmid = '" + obj.pmid + "'");
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();
                                                    Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                                    xx = 1;
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();
                                        Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                        xx = 1;
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    Log("Update Action", "", 10000810, "$pmid: " + obj.pmid);
                                }
                            }
                            else if (obj.problemevent == 1)//New incident
                            {
                                var output = db.Database.ExecuteSqlCommand("UPDATE pmschedule " +
                                    "SET pmid = '" + obj.pmid + "', description = '" + obj.description + "', actioncount = '" + actioncount + "', dayofweek = '" + obj.dayofweek + "', condition = '" + obj.condition + "', triggertype = '" + obj.triggertype + "', problemevent = '" + obj.problemevent + "', classification = '1', noevents = NULL, timetoescalate = '" + obj.timetoescalate + "', escalation = '" + obj.escalation + "', problempmid = '" + obj.problempmid + "', timetype = '" + obj.timetype + "', resultneeded = '" + obj.resultneeded + "', journalneeded = '" + obj.journalneeded + "' " +
                                    "WHERE pmid = '" + obj.pmid + "'");
                                if (output > 0)
                                {
                                    Log("Update Incident", "", 10000609, "$pmid: " + obj.pmid);
                                    int i = 1;
                                    int DeleteAction = db.Database.ExecuteSqlCommand("DELETE From pmaction Where pmid = '" + obj.pmid + "'");
                                    if (obj.Actdata.Length - 1 > 0)
                                    {
                                        for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                                        {
                                            if (AddAction(obj.Actdata, obj.pmid, z, i))
                                            {
                                                i++;
                                                if (z == (obj.Actdata.Length - 3))
                                                {
                                                    dbCtxTxn.Commit();
                                                    Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                                    xx = 1;
                                                }
                                            }
                                            else
                                            {
                                                dbCtxTxn.Rollback();
                                                Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbCtxTxn.Commit();
                                        Log("Delete Action", "", 10000807, "$pmid: " + obj.pmid);
                                        xx = 1;
                                    }
                                }
                                else
                                {
                                    dbCtxTxn.Rollback();
                                    Log("Delete Action", "", 10000808, "$pmid: " + obj.pmid);
                                }
                            }
                            //else if (obj.problemevent == 9)//Re-open incident
                            //{
                            //    var output = db.Database.ExecuteSqlCommand("INSERT INTO pmschedule(pmid,pmuserid,description,active,actioncount,startit,stopit,dayofweek,condition,triggertype,statebitmask,setreset,compid,compstbmask,compsetreset,leveltriggered,problemevent,classification,noevents,nodevents,delay,calname,caluserid,timetoescalate,escalation,problempmid,problempmuserid,timetype,econsameevent,echascorrelcond,echascancelcond,eccanceltrigger,eccheckonlyonce,hierlevel,eccheckbeforefire,resultneeded,journalneeded) " +
                            //        "VALUES('" + obj.pmid + "', ' ', '" + obj.description + "', '1', '" + actioncount + "', '00000000000000', '00000000000000', '" + obj.dayofweek + "', '" + obj.condition + "', '" + obj.triggertype + "', null, null, null, null, null, null, '" + obj.problemevent + "', null, null, '0', '0', ' ', ' ', null, null, ' ', ' ', null, '0', '0', '0', '0', '0', '0', '0', '" + obj.resultneeded + "', '" + obj.journalneeded + "')");
                            //    if (output > 0)
                            //    {
                            //        int i = 1;
                            //        if (obj.Actdata.Length - 1 > 0)
                            //        {
                            //            for (var z = 0; z < obj.Actdata.Length - 1; z += 2)
                            //            {
                            //                if (AddAction(obj.Actdata, obj.pmid, z, i))
                            //                {
                            //                    i++;
                            //                    if (z == (obj.Actdata.Length - 3))
                            //                    {
                            //                        dbCtxTxn.Commit();
                            //                    }
                            //                }
                            //                else
                            //                {
                            //                    dbCtxTxn.Rollback();
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            dbCtxTxn.Commit();
                            //        }
                            //    }
                            //    else
                            //    {
                            //        dbCtxTxn.Rollback();
                            //    }
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                        dbCtxTxn.Rollback();
                        Log("Error in Rule Creation", "", 10000701, "$ex-msg: " + ex.Message);
                        return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Error: Rule Not Updated');
                         window.close();
                       </script>
                       </body> ");
                    }
                }
                if (xx == 1)
                {
                    return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Rule Updated');
                         window.close();
                       </script>
                       </body> ");
                }
                else
                {
                    return Content(@"<body>
                       <script type='text/javascript'>
                         alert('Rule Not Updated');
                         window.close();
                       </script>
                       </body> ");
                }
            }

            //return RedirectToAction("Rules", "Customize", obj);
            return RedirectToAction("Rules", "Customize");
        }

        public ActionResult RuleEdit(string Pmid)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("36");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  is editing rule : " + Pmid);
            pmschedule objs = new pmschedule();
            pmaction obja = new pmaction();
            var id = Pmid;
            objs.pmid = Pmid;
            var scheduleData = db.pmschedules.Where(a => a.pmid.Equals(id)).ToList();
            var actionData = db.pmactions.Where(a => a.pmid.Equals(id)).ToList();
            var eventData = db.pmevents.Where(a => a.pmid.Equals(id)).ToList();
            var RuleList = new SelectList(db.pmactions.Where(a => a.type == 7).Distinct().ToList(), "", "pmid");

            //objs.events = db.message0001.Where(a => a.texttype == 3 && a.textno.ToString().StartsWith("99999")).ToList();
            List<message0001> Eventconf = new List<message0001>();

            var d = from t1 in db.eventbases
                    join t2 in db.message0001 on new { t1.textno, t1.texttype } equals new { t2.textno, t2.texttype }
                    orderby t2.textno
                    select new { t2.textno, t2.messagetext };

            foreach (var x in d.Skip(1))
            {

                Eventconf.Add(new message0001 { textno = x.textno, messagetext = x.messagetext });
            }

            //objs.events = db.eventbases.Where(a => a.forwardrule.Equals(1)).Join(db.message0001, e => e.textno, m => m.textno, (e, m) => m).ToList();
            objs.events = Eventconf;

            //objs.devices = db.Devices.Select(a => a.DeviceID).ToList();
            //objs.Hierar = db.Hierarchies.ToList();

            UserCustom uc = new UserCustom();
            List<Device> getDevice = uc.GetAssignDevice(Session["UserID"].ToString());
            List<string> UsrAssigendDev = getDevice.Select(x => x.DeviceID).ToList<string>();
            objs.devices = UsrAssigendDev;
            objs.Hierar = db.Hierarchies.ToList();
            objs.Hierar = objs.Hierar.Where(x => getDevice.Any(y => y.HierLevel == x.Hierlevel)).ToList();


            var sql = "select s.bit, s.color, m.messagetext from stb s join message0001 m on m.textno=s.textno and s.type=0 and m.texttype=3 order by prio";
            var result = dbPro.Database.SqlQuery<SurveilAI.Models.Data2>(sql);
            foreach (var x in result)
            {
                objs.state.Add(x);
            }

            ViewData["Contact"] = new SelectList(db.contacts.Select(a => a.contactid).ToList());
            ViewData["Rule"] = RuleList;

            foreach (var item in scheduleData)
            {
                objs.description = item.description;
                objs.active = item.active;
                objs.actioncount = actionData.Count();
                objs.dayofweek = item.dayofweek;
                objs.condition = item.condition;
                objs.triggertype = item.triggertype;
                objs.statebitmask = item.statebitmask;
                objs.setreset = item.setreset;
                objs.leveltriggered = item.leveltriggered;
                objs.problemevent = item.problemevent;
                objs.classification = item.classification;
                objs.noevents = item.noevents;
                objs.timetoescalate = item.timetoescalate;
                objs.escalation = item.escalation;
                objs.problempmid = item.problempmid;
                objs.timetype = item.timetype;
                objs.resultneeded = item.resultneeded;
                objs.journalneeded = item.journalneeded;
            }
            if (objs.triggertype == 1)
            {
                String[] EventNums = new string[eventData.Count];
                //                var eventnos = "";
                for (int i = 0; i < eventData.Count; i++)
                {
                    EventNums[i] = eventData[i].eventno.ToString();
                }
                //foreach (var data in eventData)
                //{
                //    eventnos += data.eventno + ",";
                //}
                //eventnos = eventnos.TrimEnd(',');
                ////String[] EventNumS = eventnos.Split(',');
                objs.Sendevents = db.message0001.Where(a => a.texttype == 1 && EventNums.Contains(a.textno.ToString())).ToList();

            }
            if (objs.actioncount > 0)
            {
                foreach (var data in actionData)
                {
                    obja.step = data.step;
                    obja.deactivated = data.deactivated;
                    obja.onlyonerror = data.onlyonerror;
                    obja.type = data.type;
                    obja.jobid = data.jobid;
                    obja.contactid = data.contactid;
                    obja.adress = data.adress;
                    obja.classification = data.classification;
                    obja.reporttopic = data.reporttopic;
                    obja.mt_subject = data.mt_subject;
                    obja.mt_message = data.mt_message;
                    if (obja.type == 7)
                    {
                        var x = obja.classification + "~" + obja.reporttopic + "~" + obja.deactivated + "~" + obja.onlyonerror;
                        objs.action.Add("Create incident");
                        objs.action.Add(x);
                    }
                    if (obja.type == 3)
                    {
                        if (obja.contactid.Contains("$CONT$"))
                        {
                            var x = obja.contactid + "~" + obja.mt_subject + "~" + obja.mt_message + "~" + obja.deactivated + "~" + obja.onlyonerror;
                            objs.action.Add("Send e-mail");
                            objs.action.Add(x);
                        }
                        else if (obja.contactid != null && obja.contactid.Trim() != "")
                        {
                            var x = obja.contactid + "~" + obja.mt_subject + "~" + obja.mt_message + "~" + obja.deactivated + "~" + obja.onlyonerror;
                            objs.action.Add("Send e-mail");
                            objs.action.Add(x);
                        }
                        else
                        {
                            var x = "$emails$" + obja.adress + "~" + obja.mt_subject + "~" + obja.mt_message + "~" + obja.deactivated + "~" + obja.onlyonerror;
                            objs.action.Add("Send e-mail");
                            objs.action.Add(x);
                        }

                    }
                    if (obja.type == 11)
                    {
                        var x = obja.jobid + "~" + obja.deactivated + "~" + obja.onlyonerror;
                        objs.action.Add("Close incident");
                        objs.action.Add(x);
                    }
                    if (obja.type == 4)
                    {
                        AMTCustom EncryptionHelper = new AMTCustom();
                        string EncryKey = "@Innomate$&*";
                        obja.mt_message = EncryptionHelper.Decrypt(obja.mt_message, EncryKey);
                        var x = obja.mt_message.ToString();
                        objs.action.Add("Set Password");
                        objs.action.Add(x);
                    }
                    if (obja.type == 8)
                    {
                        obja.mt_message = obja.mt_message;
                        var x = obja.mt_message.ToString();
                        objs.action.Add("Reboot Device");
                        objs.action.Add(x);
                    }
                }
            }
            //var firstNotSecond = list1.Except(list2).ToList();
            //objs.Sendevents = objs.events.Except(objs.Sendevents).ToList();

            //var abcd = objs.events.Where(x => !objs.Sendevents.Any(y => y.textno == x.textno));

            HashSet<int> diffids = new HashSet<int>(objs.Sendevents.Select(s => s.textno));
            //You will have the difference here
            objs.events = objs.events.Where(m => !diffids.Contains(m.textno)).ToList();

            return View(objs);
        }

        public bool AddAction(string[] data, string pmid, int z, int i)
        {
            pmschedule obj = new pmschedule();
            pmaction act = new pmaction();

            act.pmid = pmid;


            string[] arr = data[z + 1].Split('~');

            act.step = i;
            i++;

            if (data[z].Contains("Create"))//Create incident
            {
                act.type = 7;
                act.classification = Convert.ToInt32(arr[0]);
                act.reporttopic = arr[1];
                act.deactivated = Convert.ToInt32(arr[2]);
                act.onlyonerror = Convert.ToInt32(arr[3]);
                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmaction(pmid,pmuserid,step,deactivated,onlyonerror,type,jobid,contactid,adress,classification,reporttopic,mt_subject,mt_message,command,vardata) " +
                    "VALUES('" + act.pmid + "', ' ', '" + act.step + "', '" + act.deactivated + "', '" + act.onlyonerror + "', '" + act.type + "', ' ', null, null, '" + act.classification + "', '" + act.reporttopic + "', null, null, null, null)");
                if (output > 0)
                {
                    Log("Create Action", "", 10000803, "$pmid: " + act.pmid);
                    return true;
                }
                else
                {
                    Log("Close Action Failed", "", 10000806, "$pmid: " + act.pmid);
                    return false;
                }
            }
            else if (data[z].Contains("Close"))//Close incident
            {
                act.type = 11;
                act.jobid = arr[0];
                act.deactivated = Convert.ToInt32(arr[1]);
                act.onlyonerror = Convert.ToInt32(arr[2]);
                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmaction(pmid,pmuserid,step,deactivated,onlyonerror,type,jobid,contactid,adress,classification,reporttopic,mt_subject,mt_message,command,vardata) " +
                    "VALUES('" + act.pmid + "', ' ', '" + act.step + "', '" + act.deactivated + "', '" + act.onlyonerror + "', '" + act.type + "', '" + act.jobid + "', null, null, null, null, null, null, null, null)");
                if (output > 0)
                {
                    Log("Create Action", "", 10000801, "$pmid: " + act.pmid);

                    return true;
                }
                else
                {
                    Log("Close Action Failed", "", 10000806, "$pmid: " + act.pmid);

                    return false;
                }
            }
            else if (data[z].Contains("Send"))//Send E-mail
            {
                act.type = 3;
                if (arr[0].Contains("$emails$"))
                {
                    arr[0] = arr[0].Remove(0, 8);//removing $emails$
                    act.adress = arr[0];
                    act.contactid = null;
                }
                else
                {
                    act.adress = null;
                    act.contactid = arr[0];
                }
                act.mt_subject = arr[1];
                act.mt_message = arr[2];
                act.deactivated = Convert.ToInt32(arr[3]);
                act.onlyonerror = Convert.ToInt32(arr[4]);
                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmaction(pmid,pmuserid,step,deactivated,onlyonerror,type,jobid,contactid,adress,classification,reporttopic,mt_subject,mt_message,command,vardata) " +
                    "VALUES('" + act.pmid + "', ' ', '" + act.step + "', '" + act.deactivated + "', '" + act.onlyonerror + "', '" + act.type + "', ' ', '" + act.contactid + "', '" + act.adress + "', null, null, '" + act.mt_subject + "', '" + act.mt_message + "', null, null)");
                if (output > 0)
                {
                    Log("Create Action", "", 10000801, "$pmid: " + act.pmid);
                    return true;
                }
                else
                {
                    Log("Create Action", "", 10000802, "$pmid: " + act.pmid);
                    return false;
                }
            }
            else if (data[z].Contains("Set Password"))//Set Bios Password
            {
                AMTCustom EncryptionHelper = new AMTCustom();
                string EncryKey = "@Innomate$&*";

                act.type = 4;
                act.adress = null;
                act.contactid = null;
                act.mt_subject = "";
                act.mt_message = arr[0];
                act.mt_message = EncryptionHelper.Encrypt(act.mt_message, EncryKey);
                act.deactivated = 0;
                act.onlyonerror = 0;
                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmaction(pmid,pmuserid,step,deactivated,onlyonerror,type,jobid,contactid,adress,classification,reporttopic,mt_subject,mt_message,command,vardata) " +
                    "VALUES('" + act.pmid + "', ' ', '" + act.step + "', '" + act.deactivated + "', '" + act.onlyonerror + "', '" + act.type + "', ' ', '" + act.contactid + "', '" + act.adress + "', null, null, '" + act.mt_subject + "', '" + act.mt_message + "', null, null)");
                if (output > 0)
                {
                    Log("Create Action", "", 10000801, "$pmid: " + act.pmid);
                    return true;
                }
                else
                {
                    Log("Create Action", "", 10000802, "$pmid: " + act.pmid);
                    return false;
                }
            }
            else if (data[z].Contains("Reboot Device"))//Reboot Device
            {
                act.type = 8;
                act.adress = null;
                act.contactid = null;
                act.mt_subject = "";
                act.mt_message = arr[0];
                act.mt_message = act.mt_message;
                act.deactivated = 0;
                act.onlyonerror = 0;
                var output = db.Database.ExecuteSqlCommand("INSERT INTO pmaction(pmid,pmuserid,step,deactivated,onlyonerror,type,jobid,contactid,adress,classification,reporttopic,mt_subject,mt_message,command,vardata) " +
                    "VALUES('" + act.pmid + "', ' ', '" + act.step + "', '" + act.deactivated + "', '" + act.onlyonerror + "', '" + act.type + "', ' ', '" + act.contactid + "', '" + act.adress + "', null, null, '" + act.mt_subject + "', '" + act.mt_message + "', null, null)");
                if (output > 0)
                {
                    Log("Create Action", "", 10000801, "$pmid: " + act.pmid);
                    return true;
                }
                else
                {
                    Log("Create Action", "", 10000802, "$pmid: " + act.pmid);
                    return false;
                }
            }

            return true;
        }

        public bool AddEvents(string[] data, string id)
        {
            var z = 0;
            try
            {
                data = data.Take(data.Count() - 1).ToArray();
                int i = 0;

                for (z = 0; z < data.Length; z++)
                {

                    int Comma = data[z].IndexOf(',');
                    if (Comma == 0)
                    {
                        data[z] = data[z].Substring(1);
                    }
                    String[] arr = data[z].Split(',');
                    var output = db.Database.ExecuteSqlCommand("INSERT INTO pmevents(pmid,pmuserid,delay,eventno) " +
                        "VALUES('" + id + "', ' ', '0', '" + arr[0] + "')");
                    if (output > 0)
                    {
                        i++;
                        Log("Create Event", "", 10000401, "$pmid: " + id);
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }

            //if (z / 2 == i)
            if (z == data.Length)
            {
                return true;
            }

            return false;
        }

        [HttpGet]
        public ActionResult getescalatetime(string pmid)
        {
            var x = db.pmschedules.Where(a => a.problempmid.Equals(pmid)).OrderByDescending(a => a.timetoescalate).Select(a => a.timetoescalate).FirstOrDefault();

            return Content(x.ToString());
        }

        public ActionResult Calender()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  navigate to calendar ");
            calendar obj = new calendar();

            obj.CalData = db.calendars.Select(a => a.calname).Distinct().ToList();


            if (TempData["RepMsgOk"] != null)
            {
                ViewBag.RepMsgOk = "Calendar deleted succesfully";

            }
            if (TempData["RepMsgno"] != null)
            {
                ViewBag.RepMsgNo = "Calendar not deleted";
            }


            return View(obj);
        }

        [HttpPost]
        [ValidateInput(true)]
        public ActionResult AddCalender(calendar collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            var obj = collection;
            if (obj.type == 2)
            {
                obj.dayofweek = -1;
            }
            activitylog.Info(Session["UserID"].ToString() + " :  is adding calendar : " + obj.calname);
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {

                    int output = db.Database.ExecuteSqlCommand("insert into calendar(calname,caluserid,type,day,dayofweek,period) " +
                            "Values('" + obj.calname + "','          ','" + obj.type + "','" + obj.day + "','" + obj.dayofweek + "','" + obj.period + "')");
                    userlog.Info(Session["UserID"].ToString() + " : calendar " + obj.calname + " added succesfully  ");
                    Log("Add Calender", "", 10000901, "$calname: " + obj.calname);
                    if (output > 0)
                    {
                        TempData["OKMsg"] = "Calender entry added.";
                    }
                    else
                    {
                        TempData["NoMsg"] = "Calender entry not added.";
                    }

                }

            }
            catch (Exception ex)
            {
                TempData["NoMsg"] = "Calender entry not added.";
                errorlog.Error("User: " + Session["UserID"] + " Error :" + ex);
                return RedirectToAction("Error", "Error");

            }

            return RedirectToAction("Calender", "Customize");

        }

        [HttpGet]
        public ActionResult CalendarEntries(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  selected calendar : " + id);
            calendar selectedCalender = new calendar();
            //{
            //    Data = db.calendars.ToList()
            //};

            var caldata = from cal in db.calendars
                          select new
                          {
                              cal.calname,
                              cal.type,
                              cal.day,
                              cal.dayofweek,
                              cal.period
                          };
            var callist = caldata.Where(c => c.calname == id).OrderBy(c => c.day).ThenBy(c => c.dayofweek).ThenBy(c => c.period).ToList();

            foreach (var x in callist)
            {
                selectedCalender.dataCal.Add(new Tuple<string, Int32, DateTime?, Int32?, string>(x.calname, x.type, x.day, x.dayofweek, x.period));
            }


            return PartialView("_CalendarTable", selectedCalender);
        }

        [HttpGet]
        public JsonResult getCalendar(String id)
        {
            var caldata = from cal in db.calendars
                          select new
                          {
                              cal.calname,
                              cal.type,
                              cal.day,
                              cal.dayofweek,
                              cal.period
                          };
            var callist = caldata.Where(c => c.calname == id).ToList();
            return Json(callist, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CalExist(string Calname)
        {
            try
            {
                calendar selectedCalender = new calendar();
                selectedCalender.Data = db.calendars.Where(a => a.calname == Calname).ToList();
                var Count = selectedCalender.Data.Count;
                bool isExist = false;
                if (Count > 0)
                {
                    isExist = true;
                }

                return Json(!isExist, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                throw;
            }

        }


        [HttpGet]
        public JsonResult DeleteCalendar(string id)
        {
            if (Session["UserID"] == null)
            {
                return Json("Logout", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return Json("Logout", JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " :  is deleting calendar : " + id);
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    string abc = "Delete from calendar where calname = '" + id + "'";

                    int output = db.Database.ExecuteSqlCommand("Delete from calendar where calname = '" + id + "'");
                    if (output > 0)
                    {
                        Log("Delete Calender", "", 10000903, "$calname: " + id);
                        userlog.Info(Session["UserID"].ToString() + " : calendar " + id + " deleted succesfully  ");

                        TempData["RepMsgOk"] = "Calendar deleted succesfully";
                        return Json("Ok", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Log("Delete Calender Failed", "", 10000904, "$calname: " + id);
                        errorlog.Error("User: " + Session["UserID"] + " Error deletinng calendar file  " + id + " " + output);
                        TempData["RepMsgNo"] = "Calendar not deleted";

                        return Json("No", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Delete Calender", "", 10000904, "$calname: " + id + " $ex-msg:" + ex.Message);
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult EditEntries(string name, string period, string day, string day2)
        //{
        //    return View();
        //}

        [HttpPost]
        public ActionResult EditEntries(string name, string period, string day, string day2)
        {
            return Redirect("Customize/Calender");
        }

        public ActionResult DeleteEntries(string name, string period, string day, string day2)
        {
            //var remove = from calendar in db.calendars
            //             where calendar.calname == name && calendar.period == period && calendar.day.ToString()==day &&  calendar.dayofweek.ToString() == day2
            //             //where calendar.calname == name
            //             select calendar;


            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("34");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    string abc = "Delete from calendar where calname = '" + name + "' AND period='" + period + "' AND day ='" + day + "' AND dayofweek='" + day2 + "'";
                    int output = db.Database.ExecuteSqlCommand("Delete from calendar where calname = '" + name + "' AND period='" + period + "' AND day ='" + day + "' AND dayofweek='" + day2 + "'");
                    if (output > 0)
                    {
                        @TempData["OKMsg"] = "Calendar Entry deleted!";
                        userlog.Info(Session["UserID"].ToString() + " : calendar entry deleted succesfully  ");
                        Log("Delete Calender", "", 10000903, "$calname: " + name);
                        return RedirectToAction("Calender", "Customize");
                    }
                    else
                    {

                        @TempData["NoMsg"] = "Calendar Could not be Deleted ";
                        errorlog.Error("User: " + Session["UserID"] + " Error Calendar entry could not not be Deleted " + output);

                        return RedirectToAction("Calender", "Customize");
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                Log("Delete Calender", "", 10000904, "$calname: " + name + "ex.msg: " + ex.Message);
                return RedirectToAction("Calender", "Customize");
            }

        }

        public ActionResult IncidentView()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("38");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  navigate to incident view ");
            problem obj = new problem();

            obj.data = db.problems.Where(a => a.status == 1).OrderByDescending(x => x.starttime).ToList();

            return View(obj);
        }

        public ActionResult ViewIncident(int? id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("38");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            activitylog.Info(Session["UserID"].ToString() + " :  viewing incident : " + id);
            var xx = db.problems.Where(a => a.repnumber == id).FirstOrDefault();
            string qry = @"select * from problemdata where  repnumber = " + id + " and remtime = (select max(remtime) from problemdata where  repnumber = " + id + ")";

            var remarks = db.problemdatas.SqlQuery(qry).ToList();

            foreach (var item in remarks)
            {
                xx.remark = item.remark;
            }
            return View(xx);
        }

        [HttpPost]
        public ActionResult UpdateIncident(problem collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("40");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            problem obj = new problem();
            activitylog.Info(Session["UserID"].ToString() + " : : updating incident : " + collection.repnumber);
            obj.repnumber = collection.repnumber;
            obj.status = collection.status;
            obj.classif = collection.classif;
            obj.remark = collection.remark;
            using (var dbProTxn = db.Database.BeginTransaction())
            {
                try
                {
                    if (obj.status == 2)
                    {

                        var pri = dbPro.Database.ExecuteSqlCommand("Update problem Set status = '" + obj.status + "', endtime = GETDATE() where repnumber = '" + obj.repnumber + "'");
                        if (pri > 0)
                        {
                            Log("Update Incident", "", 10000609, "$repnumber: " + obj.repnumber);
                            var pmev = dbPro.Database.ExecuteSqlCommand("Delete from pmeventstore where counter = '" + obj.repnumber + "'");
                            var count = dbPro.problemdatas.Where(a => a.repnumber == obj.repnumber).OrderByDescending(b => b.remnumber).Select(c => c.remnumber).FirstOrDefault();
                            if (count == 0)
                            {
                                count = 1;
                            }
                            else
                            {
                                count++;
                            }
                            string sd = "Insert into problemdata(remnumber,remuser,remextuser,remtime,remark) values ('" + obj.repnumber + "','','',GETDATE(),'" + obj.remark + "')";
                            var prod = dbPro.Database.ExecuteSqlCommand("Insert into problemdata(repnumber,remuser,remextuser,remtime,remark) values ('" + obj.repnumber + "','','',GETDATE(),'" + obj.remark + "')");
                            if (prod > 0)
                            {
                                Log("Create Incident", "", 10000601, "$repnumber: " + obj.repnumber);


                                dbProTxn.Commit();
                            }
                            else
                            {
                                dbProTxn.Rollback();
                                Log("Create Incident Failed", "", 10000602, "$repnumber: " + obj.repnumber);
                            }
                        }
                    }

                    else
                    {


                        var sta = dbPro.Database.ExecuteSqlCommand("Update problem Set classif = '" + obj.classif + "' where repnumber = '" + obj.repnumber + "'");
                        if (sta > 0)
                        {
                            Log("Update Incident", "", 10000609, "$repnumber: " + obj.repnumber);
                            var count = dbPro.problemdatas.Where(a => a.repnumber == obj.repnumber).OrderByDescending(b => b.remnumber).Select(c => c.remnumber).FirstOrDefault();
                            if (count == 0)
                            {
                                count = 1;
                            }
                            else
                            {
                                count++;
                            }
                            var prod = dbPro.Database.ExecuteSqlCommand("Insert into problemdata(repnumber,remuser,remextuser,remtime,remark) values ('" + obj.repnumber + "','','',GETDATE(),'" + obj.remark + "')");
                            if (prod > 0)
                            {
                                Log("Create Incident", "", 10000601, "$repnumber: " + obj.repnumber);

                                dbProTxn.Commit();
                            }
                            else
                            {
                                Log("Create Incident", "", 10000602, "$repnumber: " + obj.repnumber);

                                dbProTxn.Rollback();
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    errorlog.Error("User: " + Session["UserID"] + " Error : " + ex);
                    Log("Create Incident", "", 10000602, "$repnumber: " + obj.repnumber + " $ex-msg: " + ex.Message);


                    dbProTxn.Rollback();
                }
            }

            return RedirectToAction("IncidentView", "Customize");
        }


        public ActionResult GroupDevice()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("57");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            obj1.StaticGroups = db.pvgroups.Where(a => a.grouptype == 1).Select(a => a.groupname).ToList();
            obj1.DynamicGroups = db.pvgroups.Where(a => a.grouptype == 0).Select(a => a.groupname).ToList();

            return View("DeviceGroup", obj1);
        }

        public ActionResult ParViewDir(string gt)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("57");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                List<string> contacts = db.contacts.Select(a => a.contactid).ToList();
                List<SelectListItem> contactListItem = ContactListItem(contacts);

                ViewData["Contact"] = contactListItem;
                //ViewData["Contact"] = new SelectList(db.contacts.Select(a => a.contactid).ToList());
                obj1.Devices = db.Devices.Select(a => a.DeviceID).ToList();
                //obj1.Hierarchies = db.Hierarchies.Select(a => a.DeviceID).ToList();
                var Devices = db.Devices.ToList();
                var Hierarchy = db.Hierarchies.ToList();
                obj1.HierarchyList = Hierarchy;
                obj1.DeviceList = Devices;

                if (gt == "0")
                {
                    return PartialView("_CreateStaticGroup", obj1);
                }
                else
                {
                    return PartialView("_CreateDynamicGroup", obj1);
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("GroupDevice", "Customize");
            }

        }

        public ActionResult StaticDevGrpPost(FormCollection collection)
        {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                pvgroup pvGroup = GetGroupDetails(collection);
                string postType = collection["PostType"];
                var groups = db.pvgroups.OrderByDescending(x => x.groupid).Select(x => x.groupid).FirstOrDefault();
                //int maxHid = hids.Select(v => int.Parse(v.Substring(0))).Max();
                int groupid = groups.ToString() == "" ? 0 : groups;
                groupid++;
                if (postType != "Edit")
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        var queryGroup = db.pvgroups.Select(x => new { x.groupname }).Where(x => x.groupname.Equals(pvGroup.groupname)).FirstOrDefault();
                        if (queryGroup != null)
                        {
                            TempData["NoMsg"] = "Group Name Already Exists!";
                            return RedirectToAction("GroupDevice", "Customize");
                        }
                        else
                        {
                            string query = "INSERT INTO[dbo].[pvgroup] ([groupname],[description],[grouptype],[criteria],[contactid1],[contactid2],[contactid3]) " +
                        "VALUES('" + pvGroup.groupname + "','" + pvGroup.description + "','1','" + pvGroup.criteria + "','" + pvGroup.contactid1 + "','" + pvGroup.contactid2 + "','" + pvGroup.contactid3 + "')";

                            var pri = db.Database.ExecuteSqlCommand(query);
                            Log("Create Group", "", 10001001, "$GroupName: " + pvGroup.groupname);

                        }
                    }
                    @TempData["OKMsg"] = "Group Created Successfully!";
                }
                else
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        var groupQry = db.pvgroups
                            .Select(x => new { x.groupid })
                            .Where(x => x.groupid.Equals(pvGroup.groupid)).SingleOrDefault();
                        if (groupQry != null)
                        {
                            db.Database.
                                   ExecuteSqlCommand(@"update pvgroup set 
                                groupname = '" + pvGroup.groupname + "', " +
                                               "description = '" + pvGroup.description + "', " +
                                               "criteria = '" + pvGroup.criteria + "'," +
                                               " contactid1 = '" + pvGroup.contactid1 + "'," +
                                               " contactid2 = '" + pvGroup.contactid2 + "'," +
                                               " contactid3 = '" + pvGroup.contactid3 + "' where groupid = '" + pvGroup.groupid + "'");
                            Log("Update Group", "", 10001001, "$GroupName: " + pvGroup.groupname);
                            @TempData["OKMsg"] = "Device Group updated successfully!";
                        }
                        else
                        {
                            TempData["NoMsg"] = "Device Group does not exist!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Device group not updated!";





            }

            return RedirectToAction("GroupDevice", "Customize");
        }

        [HttpPost]
        public ActionResult DynamicDevGrpPost(pvgroup collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                if (collection.Contact11 != null)
                {
                    collection.contactid1 = collection.Contact11;
                }
                if (collection.Contact21 != null)
                {
                    collection.contactid2 = collection.Contact21;
                }
                if (collection.Contact31 != null)
                {
                    collection.contactid3 = collection.Contact31;
                }
                //obj1.Devices = db.Devices.Select(a => a.DeviceID).ToList();
                string postType = collection.PostType;
                var groups = db.pvgroups.OrderByDescending(x => x.groupid).Select(x => x.groupid).FirstOrDefault();
                //int maxHid = hids.Select(v => int.Parse(v.Substring(0))).Max();
                int groupid = groups.ToString() == "" ? 0 : groups;
                groupid++;
                if (postType != "Edit")
                {
                    var searchGroup = db.pvgroups.Select(x => new { x.groupname }).Where(x => x.groupname.Equals(collection.groupname)).FirstOrDefault();
                    if (searchGroup != null)
                    {
                        TempData["NoMsg"] = "Group Name Already Exists!";
                        return RedirectToAction("GroupDevice", "Customize");
                    }
                    else
                    {
                        using (SurveilAIEntities db = new SurveilAIEntities())
                        {
                            string querygrpconditions = "";

                            int pri = db.Database.ExecuteSqlCommand("INSERT INTO[dbo].[pvgroup] ([groupname],[description],[grouptype],[criteria],[contactid1],[contactid2],[contactid3]) " +
                                    "VALUES('" + collection.groupname + "','" + collection.description + "','0','" + collection.criteria + "','" + collection.contactid1 + "','" + collection.contactid2 + "','" + collection.contactid3 + "')");
                            if (pri > 0)
                            {
                                var queryGroup = db.pvgroups.Select(x => new { x.groupid, x.groupname }).Where(x => x.groupname.Equals(collection.groupname)).FirstOrDefault();
                                if (queryGroup != null)
                                {
                                    if (collection.Profile11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999904', '" + collection.Profile11 + "') ";
                                    }
                                    if (collection.Location11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999998', '" + collection.Location11 + "') ";
                                    }
                                    if (collection.Street11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999997', '" + collection.Street11 + "') ";
                                    }
                                    if (collection.City11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999995', '" + collection.City11 + "') ";
                                    }
                                    if (collection.Country11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999979', '" + collection.Country11 + "') ";
                                    }
                                    if (collection.Longitude11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999915', '" + collection.Longitude11 + "') ";
                                    }
                                    if (collection.Latitude11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999916', '" + collection.Latitude11 + "') ";
                                    }
                                    if (collection.Vendor11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999912', '" + collection.Vendor11 + "') ";
                                    }
                                    if (collection.DevType11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999914', '" + collection.DevType11 + "') ";
                                    }
                                    if (collection.Orgnization11 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999999', '" + collection.Orgnization11 + "') ";
                                    }
                                    if (collection.Contact1 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999901', '" + collection.Contact1 + "') ";
                                    }
                                    if (collection.Contact2 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999902', '" + collection.Contact2 + "') ";
                                    }
                                    if (collection.Contact3 != null)
                                    {
                                        querygrpconditions += "insert into pvgroupcondition values('" + queryGroup.groupid + "', '999903', '" + collection.Contact3 + "') ";
                                    }
                                    if (querygrpconditions != "")
                                    {
                                        int pri1 = db.Database.ExecuteSqlCommand(querygrpconditions);
                                    }



                                    if (pri > 0)
                                    {
                                        Log("Create Group", "", 10001001, "$groupName: " + collection.groupname);




                                        Log("Create Group Condition", "", 10001003, "$groupName: " + collection.groupname);
                                    }
                                }
                            }
                        }
                        @TempData["OKMsg"] = "Device Group created successfully!";
                    }
                }
                else
                {
                    using (SurveilAIEntities db = new SurveilAIEntities())
                    {
                        var groupQry = db.pvgroups
                            .Select(x => new { x.groupid })
                            .Where(x => x.groupid.Equals(collection.groupid)).SingleOrDefault();
                        if (groupQry != null)
                        {
                            int pri = db.Database.ExecuteSqlCommand(@"update pvgroup set 
                                                groupname = '" + collection.groupname + "'," +
                                                    " description = '" + collection.description + "', " +
                                                    "criteria = '" + collection.criteria + "', " +
                                                    "contactid1 = '" + collection.contactid1 + "'," +
                                                    " contactid2 = '" + collection.contactid2 + "'," +
                                                    " contactid3 = '" + collection.contactid3 + "'" +
                                                    " where groupid = '" + collection.groupid + "'");
                            if (pri > 0)
                            {
                                var queryGroup = db.pvgroups.Select(x => new { x.groupid, x.groupname }).Where(x => x.groupid.Equals(collection.groupid)).FirstOrDefault();
                                if (queryGroup != null)
                                {
                                    if (collection.Profile11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999904");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999904", collection.Profile11);
                                    }
                                    if (collection.Location11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999998");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999998", collection.Location11);
                                    }
                                    if (collection.Street11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999997");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999997", collection.Street11);
                                    }
                                    if (collection.City11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999995");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999995", collection.City11);
                                    }
                                    if (collection.Country11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999979");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999979", collection.Country11);
                                    }
                                    if (collection.Longitude11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999915");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999915", collection.Longitude11);
                                    }
                                    if (collection.Latitude11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999916");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999916", collection.Latitude11);
                                    }
                                    if (collection.Vendor11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999912");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999912", collection.Vendor11);
                                    }
                                    if (collection.DevType11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999914");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999914", collection.DevType11);
                                    }
                                    if (collection.Orgnization11 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999999");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999999", collection.Orgnization11);
                                    }
                                    if (collection.Contact1 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999901");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999901", collection.Contact1);
                                    }
                                    if (collection.Contact2 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999902");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999902", collection.Contact2);
                                    }
                                    if (collection.Contact3 == null)
                                    {
                                        DeleteDynamicCritera(queryGroup.groupid, "999903");
                                    }
                                    else
                                    {
                                        UpdateDynamicGroup(queryGroup.groupid, "999903", collection.Contact3);
                                    }
                                    if (pri > 0)
                                    {
                                        Log("Group Update", "", 10001001, "$groupName: " + collection.groupname);

                                        Log("Update Group Condition", "", 10001003, "$groupName: " + collection.groupname);
                                    }
                                }
                            }
                            @TempData["OKMsg"] = "Device Group updated successfully!";
                        }
                        else
                        {
                            TempData["NoMsg"] = "Device Group does not exist!";
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Device Group not updated!";
            }
            return RedirectToAction("GroupDevice", "Customize");
        }

        public ActionResult DeviceGroupEditView(string gt)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                using (var context = new SurveilAIEntities())
                {
                    var GroupDetails = context.pvgroups.Where(a => a.groupname == (gt.Trim())).SingleOrDefault();
                    List<string> contacts = context.contacts.Select(a => a.contactid).ToList();
                    List<SelectListItem> contactListItem = ContactListItemEdit(contacts);
                    ViewBag.contactList = contactListItem;

                    ViewData["Contact"] = contactListItem;
                    //ViewData["Contact"] = new SelectList(db.contacts.Select(a => a.contactid).ToList());
                    obj1.Devices = context.Devices.Select(a => a.DeviceID).ToList();
                    obj1.groupid = GroupDetails.groupid;
                    obj1.groupname = GroupDetails.groupname.Trim();
                    obj1.description = GroupDetails.description;







                    obj1.criteria = GroupDetails.criteria;
                    if (GroupDetails.contactid1.Contains("@"))

                    {
                        obj1.contactid1 = GroupDetails.contactid1;
                        obj1.Contact11 = "";
                    }
                    else
                    {
                        obj1.Contact11 = GroupDetails.contactid1;
                    }
                    if (GroupDetails.contactid2.Contains("@"))
                    {
                        obj1.contactid2 = GroupDetails.contactid2;
                        obj1.Contact21 = "";
                    }
                    else
                    {
                        obj1.Contact21 = GroupDetails.contactid2;
                    }
                    if (GroupDetails.contactid3.Contains("@"))
                    {
                        obj1.contactid3 = GroupDetails.contactid3;
                        obj1.Contact31 = "";
                    }
                    else
                    {
                        obj1.Contact31 = GroupDetails.contactid3;
                    }




                    List<SelectListItem> L1 = ContactListItemEdit(contacts);
                    List<SelectListItem> L2 = ContactListItemEdit(contacts);
                    List<SelectListItem> L3 = ContactListItemEdit(contacts);
                    foreach (var item in L1)
                    {
                        if (item.Text == obj1.Contact11.Trim())
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                    foreach (var item in L2)
                    {
                        if (item.Text == obj1.Contact21.Trim())
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                    foreach (var item in L3)
                    {
                        if (item.Text == obj1.Contact31.Trim())
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                    ViewBag.L1 = L1;
                    ViewData["L2"] = L2;
                    ViewData["L3"] = L3;
                    //obj1.contactid1 = RemoveSpaceAndComma(GroupDetails.contactid1);
                    //obj1.contactid2 = RemoveSpaceAndComma(GroupDetails.contactid2);
                    //obj1.contactid3 = RemoveSpaceAndComma(GroupDetails.contactid3);

                    var Devices = context.Devices.ToList();
                    var Hierarchy = context.Hierarchies.ToList();
                    obj1.HierarchyList = Hierarchy;
                    obj1.DeviceList = Devices;

                    if (obj1.Contact1 != null)
                    {
                        obj1.Contact1 = obj1.Contact1.Trim();

                    }
                    if (obj1.Contact2 != null)
                    {
                        obj1.Contact2 = obj1.Contact2.Trim();

                    }
                    if (obj1.Contact3 != null)
                    {
                        obj1.Contact3 = obj1.Contact3.Trim();

                    }
                    if (obj1.Contact11 != null)
                    {
                        obj1.Contact11 = obj1.Contact11.Trim();

                    }
                    if (obj1.Contact21 != null)
                    {
                        obj1.Contact21 = obj1.Contact21.Trim();

                    }
                    if (obj1.Contact31 != null)
                    {
                        obj1.Contact31 = obj1.Contact31.Trim();
                    }

                    if (GroupDetails.grouptype.ToString() == "1")
                    {
                        return PartialView("_ViewStaticGroup", obj1);
                    }
                    else
                    {
                        var grpcond = db.pvgroupconditions.Where(a => a.groupid == GroupDetails.groupid).ToList();

                        foreach (var item in grpcond)
                        {
                            if (item.basedatano.ToString() == "999904")
                            {
                                obj1.Profile11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999998")
                            {
                                obj1.Location11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999997")
                            {
                                obj1.Street11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999995")
                            {
                                obj1.City11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999979")
                            {
                                obj1.Country11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999915")
                            {
                                obj1.Longitude11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999916")
                            {
                                obj1.Latitude11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999912")
                            {
                                obj1.Vendor11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999914")
                            {
                                obj1.DevType11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999999")
                            {
                                obj1.Orgnization11 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999901")
                            {
                                obj1.Contact1 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999902")
                            {
                                obj1.Contact2 = item.criteria.ToString();
                            }
                            if (item.basedatano.ToString() == "999903")
                            {
                                obj1.Contact3 = item.criteria.ToString();
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
            }
            return PartialView("_ViewDynamicGroup", obj1);
        }

        public ActionResult GrpDevDel(string gt)
        {
            int groupId = Convert.ToInt32(gt);
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    var searchGroup = db.pvgroups
                        .Select(x => new { x.groupid, x.grouptype })
                        .Where(x => x.groupid.Equals(groupId)).SingleOrDefault();

                    if (searchGroup.grouptype == 0)
                    {
                        int result = db.Database.ExecuteSqlCommand("Delete from pvgroupcondition where groupid = '" + groupId + "'");
                        Log("Delete Group Condition", "", 10001005, "$gid: " + groupId);
                        int result2 = db.Database.ExecuteSqlCommand("Delete from pvgroup where groupid = '" + groupId + "'");
                        Log("Delete Group", "", 10001007, "$gid: " + groupId);
                        @TempData["OKMsg"] = "Device Group deleted successfully!";
                    }
                    else
                    {
                        int output = db.Database.ExecuteSqlCommand("Delete from pvgroup where groupid = '" + groupId + "'");
                        Log("Delete Group", "", 10001007, "$gid: " + groupId);
                        @TempData["OKMsg"] = "Device Group deleted successfully!";
                    }
                }
            }
            catch (Exception ex)
            {

                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                TempData["NoMsg"] = "Device Group not deleted!";
            }
            return RedirectToAction("GroupDevice", "Customize");
        }
        //Delete Event
        [HttpPost]
        public ActionResult DeleteRule(FormCollection formCollection)
        {
            var pmid = formCollection["Ruleid"].ToString();
            activitylog.Info(Session["UserID"].ToString() + " :  is deleting rule");
            using (var context = new SurveilAIEntities())
            {
                using (var dbTran = context.Database.BeginTransaction())
                {
                    try
                    {

                        //job FromJobs = 
                        context.pmevents.Where(x => x.pmid == pmid).ToList().ForEach(p => context.pmevents.Remove(p));
                        context.SaveChanges();

                        context.pmactions.Where(x => x.pmid == pmid).ToList().ForEach(p => context.pmactions.Remove(p));
                        context.SaveChanges();

                        context.pmexecutions.Where(x => x.pmid == pmid).ToList().ForEach(p => context.pmexecutions.Remove(p));
                        context.SaveChanges();

                        context.pmschedactions.Where(x => x.pmid == pmid).ToList().ForEach(p => context.pmschedactions.Remove(p));
                        context.SaveChanges();

                        context.pmschedules.Where(x => x.pmid == pmid).ToList().ForEach(p => context.pmschedules.Remove(p));
                        context.SaveChanges();

                        @TempData["OKMsg"] = "Rule Deleted Successfully!";

                        userlog.Info(Session["UserID"].ToString() + " : deleted rule " + pmid + " succesfully");

                        dbTran.Commit();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        @TempData["NoMsg"] = "Rule Delete Unsuccessful!";
                        activitylog.Info(Session["UserID"].ToString() + " :  failed delete to rule " + pmid);
                        errorlog.Error(Session["UserID"].ToString() + " deleting rule failed " + ex);
                        return RedirectToAction("Rules", "Customize");

                        throw;

                    }
                }
                return RedirectToAction("Rules", "Customize");


            }



        }

        public ActionResult PasswordPolicy()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                var AccType = db.UserSecurities.Select(x => x.AccountType).Distinct();
                ViewBag.AccType = AccType;
                return View();
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");
            }

        }

        public ActionResult PasswordPolicyPost(FormCollection formValues)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("59");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            string AccType = formValues["AccType"] == "" ? "0" : formValues["AccType"];
            if (AccType == "0" || AccType == null)
            {
                @TempData["NoMsg"] = "Select Account Type";
                return RedirectToAction("PasswordPolicy", "Customize");

            }
            formValues["minLength"] = formValues["minLength"] == "" ? "0" : formValues["minLength"];
            formValues["maxLength"] = formValues["maxLength"] == "" ? "0" : formValues["maxLength"];
            formValues["minCapital"] = formValues["minCapital"] == "" ? "0" : formValues["minCapital"];
            formValues["minDigit"] = formValues["minDigit"] == "" ? "0" : formValues["minDigit"];

            formValues["maxAttempt"] = formValues["maxAttempt"] == "" ? "0" : formValues["maxAttempt"];
            formValues["minAge"] = formValues["minAge"] == "" ? "0" : formValues["minAge"];
            formValues["maxAge"] = formValues["maxAge"] == "" ? "0" : formValues["maxAge"];
            formValues["Password"] = formValues["Password"] == "" ? "0" : formValues["Password"];
            formValues["passwordExpiry"] = formValues["passwordExpiry"] == "" ? "0" : formValues["passwordExpiry"];

            string policyValue = string.Format("{0},{1},{2},{3},{4},{5},{6},0,0", formValues["minLength"], formValues["maxLength"], formValues["minCapital"], formValues["minDigit"], formValues["maxAttempt"], formValues["minAge"], formValues["maxAge"], formValues["Password"], formValues["passwordExpiry"]);
            //string regex= @"^(?=.*[a-z])(?=.*[A-Z]{"+ formValues["minCapital"] + @"})(?=(.*\d){" + formValues["minCapital"] + @"})(?=.*[~!@#$%^&*()_+\-=?\/<>,.';:\[\]\{\}\\]).{" + formValues["minLength"] + ","+ formValues["maxLength"] + "}$";
            //db.UserSecurities.Add();
            try
            {
                UserSecurity pwPolicyUser = db.UserSecurities.Single(x => x.AccountType == AccType);
                pwPolicyUser.Passwordpolicy = policyValue;
                db.SaveChanges();
                @TempData["OKMsg"] = "Password Policy Updated";
                return RedirectToAction("PasswordPolicy", "Customize");

            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");
            }


        }

        [HttpGet]
        public ActionResult getPwPolicy(String Acctype)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                var acctype = db.UserSecurities.Where(x => x.AccountType == Acctype).Select(x => x.Passwordpolicy).FirstOrDefault();

                if (acctype != "")
                {
                    //  Send "false"
                    //return Json(new { success = false, responseText = acctype }, JsonRequestBehavior.AllowGet);
                    return Json(acctype, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //  Send "Success"
                    return Json("no password policy", JsonRequestBehavior.AllowGet);
                    //return Json(new { success = true, responseText = "no password policy" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return RedirectToAction("Error", "Error");
            }

        }

        #region Archive

        public string ConvertKBToGB(string KBSize)
        {
            KBSize = KBSize.Replace("KB", "");
            KBSize = KBSize.Trim();
            double size = Convert.ToInt32(KBSize);
            size = size / 1024;
            size = Math.Round(size, 2);
            return size.ToString() + " MB";
        }


        public List<TableArchive> GetTableSize()
        {
            List<TableArchive> TL = new List<TableArchive>();
            try
            {
                List<string> tableNames = new List<string>
                {
                    "pvjournal",
                    "devicefail",
                    "event",
                    "DEVICE_COMP_TEMP",
                    "DEVICE_STATE_TEMP",
                    "jobresult",
                    "abccc",
                    "Performance"
                };
                using (SurveilAIEntities dbContext = new SurveilAIEntities())
                {
                    foreach (var tblname in tableNames)
                    {
                        TableArchive spaceInfo = dbContext.Database.SqlQuery<TableArchive>("exec sp_spaceused @objname = '" + tblname + "'").FirstOrDefault<TableArchive>();
                        TL.Add(spaceInfo);
                    }
                }

                foreach (var item in TL)
                {
                    item.reserved = ConvertKBToGB(item.reserved);
                    item.data = ConvertKBToGB(item.data);
                    item.index_size = ConvertKBToGB(item.index_size);
                    item.unused = ConvertKBToGB(item.unused);
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            return TL;
        }

        public Archive GetArchiveStatus()
        {
            List<TableArchive> TL = new List<TableArchive>();
            Archive ArchiveObj = new Archive();

            try
            {
                using (SurveilAIEntities dbContext = new SurveilAIEntities())
                {
                    List<Archive> ArchiveStatus = dbContext.Archives.ToList();

                    foreach (var item in ArchiveStatus)
                    {
                        if (item.ArchivePolicy == true)
                        {
                            item.ByDays = true;
                            item.BySize = false;
                        }
                        else
                        {
                            item.BySize = true;
                            item.ByDays = false;
                        }
                        if (item.PurgePolicy == true)
                        {
                            item.ArchivePolicy = false;
                        }
                        else
                        {
                            item.ArchivePolicy = true;
                        }


                    }
                    ArchiveObj.Archives = ArchiveStatus;
                    ArchiveObj.tableArchives = GetTableSize();
                    List<SelectListItem> listItems = new List<SelectListItem>();
                    foreach (var listItem in ArchiveStatus)
                    {
                        listItems.Add(new SelectListItem
                        {
                            Text = listItem.Archive1,
                            Value = listItem.Archive1
                        });
                    }
                    ViewBag.ddArchive = listItems;
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            return ArchiveObj;
        }
        public ActionResult Archive()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("178");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            Archive ArchiveObj = new Archive();
            try
            {
                ArchiveObj = GetArchiveStatus();
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            return View(ArchiveObj);
        }

        [HttpPost]
        public ActionResult UpdateArchive(Archive model)
        {
            if (Session["UserID"] == null)
            {
                return PartialView("~/Views/User/_RedirectToLogin.cshtml");
            }
            else
            {
                var ret = Check("183");
                if (ret == false)
                {
                    return PartialView("~/Views/User/_RedirectToLogin.cshtml");
                }
            }
            var result = 0;
            Archive ArchiveObj = new Archive();
            try
            {
                ArchiveObj.Archive1 = model.Archive1;
                ArchiveObj.NoOfDays = model.NoOfDays;
                ArchiveObj.Size = model.Size;
                if (model.ByDays)
                {
                    ArchiveObj.ArchivePolicy = true;
                }
                else
                {
                    ArchiveObj.ArchivePolicy = false;
                }

                if (model.PurgePolicy)
                {
                    ArchiveObj.PurgePolicy = true;
                }
                else
                {
                    ArchiveObj.PurgePolicy = false;
                }
                ArchiveObj.IsActive = model.IsActive;
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    result = db.Database.ExecuteSqlCommand("update Archive set NoOfDays = '" + ArchiveObj.NoOfDays + "', Size = '" + ArchiveObj.Size + "', ArchivePolicy = '" + ArchiveObj.ArchivePolicy + "', PurgePolicy = '" + ArchiveObj.PurgePolicy + "', IsActive = '" + ArchiveObj.IsActive + "'  where Archive = '" + ArchiveObj.Archive1 + "'");
                }
                ArchiveObj = GetArchiveStatus();
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            if (result <= 0)
            {
                ArchiveObj.Msg = "Archive policy not updated!";
            }
            else
            {
                ArchiveObj.Msg = "Archive policy has been updated!";
            }

            return PartialView("_ArchiveStatus", ArchiveObj);
        }


        public ActionResult Archive_Create(Archive collection)
        {
            try
            {
                int output = db.Database.ExecuteSqlCommand("update archive set [NoOfDays] = '" + (String.IsNullOrEmpty(collection.NoOfDays.ToString()) ? 0 : collection.NoOfDays) + "', [Size] = '" + (String.IsNullOrEmpty(collection.Size.ToString()) ? 0 : collection.Size) + "',[ArchivePolicy] = '" + collection.ArchivePolicy + "',[PurgePolicy] = '" + collection.PurgePolicy + "', [IsActive] ='" + collection.IsActive + "' from Archive where Archive = '" + collection.Archive1 + "'");
                if (output > 0)
                {
                    Log("Archive Creatation", "", 10002001, "");
                    @TempData["OKMsg"] = "Archive Updated Successfully!";
                    activitylog.Info(Session["UserID"].ToString() + " Archive Updated Successfully");
                }
                else
                {
                    Log("Archive Creatation", "", 10002002, "");
                    @TempData["NoMsg"] = "Archive Updation Unsuccessful!";
                    errorlog.Error(Session["UserID"].ToString() + "Archive Updation Unsuccessful");
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }

            return RedirectToAction("Archive");
        }
        #endregion

        //Create SelectListItem for Contact
        public static List<SelectListItem> ContactListItem(List<string> entitiesList)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            int id = 1;
            foreach (var entity in entitiesList)
            {
                string value = entity.Trim();
                items.Add(new SelectListItem { Text = value, Value = value });
                id++;
            }
            items.TrimExcess();
            return items;
        }
        public static List<SelectListItem> ContactListItemEdit(List<string> entitiesList)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "-Select Contact-", Value = "-Select Contact-" });
            int id = 1;
            foreach (var entity in entitiesList)
            {
                string value = entity.Trim();
                items.Add(new SelectListItem { Text = value, Value = value });
                id++;
            }
            items.TrimExcess();
            return items;
        }

        //GroupForm
        public static pvgroup GetGroupDetails(FormCollection collection)
        {
            pvgroup pvGroup = new pvgroup();
            var hier = collection["hierarchy"];
            var devices = collection["Alldevices"];
            var condition = collection["condition"];
            pvGroup.criteria = collection["condition"];
            var SelectedRadioBtn = collection["Radio"];
            var DeviceType = collection["Dtype"];
            if (SelectedRadioBtn == "hierarchyDiv" && DeviceType != null)
            {
                hier = hier.Replace(",", "'',''");
                hier = "''" + hier + "''";
                DeviceType = DeviceType.Replace(",", "'',''");
                DeviceType = "''" + DeviceType + "''";

                condition = string.Format("HierLevel IN ({0}) AND DeviceType IN({1})", hier, DeviceType);
            }
            else if (SelectedRadioBtn == "hierarchyDiv" && hier != null)
            {
                hier = hier.Replace(",", "'',''");
                hier = "''" + hier + "''";
                condition = string.Format("HierLevel IN ({0})", hier);


            }
            else if (SelectedRadioBtn == "deviceDiv" && devices != null)
            {
                devices = devices.Replace(",", "'',''");
                devices = "''" + devices + "''";
                condition = string.Format(" DeviceID IN ({0})", devices);
            }
            string contactid1 = collection["contactid1"];
            string contactid2 = collection["contactid2"];
            string contactid3 = collection["contactid3"];
            string Contact11 = collection["Contact11"];
            string Contact21 = collection["Contact21"];
            string Contact31 = collection["Contact31"];
            if (Contact11 != "" && Contact11 != "-Select Contact-")
            {
                contactid1 = Contact11;
            }
            if (Contact21 != "" && Contact21 != "-Select Contact-")
            {
                contactid2 = Contact21;
            }
            if (Contact31 != "" && Contact31 != "-Select Contact-")
            {
                contactid3 = Contact31;
            }
            pvGroup.criteria = condition;
            pvGroup.groupid = Convert.ToInt32(collection["groupid"]);
            pvGroup.groupname = collection["groupname"];
            pvGroup.groupname = pvGroup.groupname.Trim();
            pvGroup.description = collection["description"];
            pvGroup.contactid1 = RemoveSpaceAndComma(contactid1);
            pvGroup.contactid2 = RemoveSpaceAndComma(contactid2);
            pvGroup.contactid3 = RemoveSpaceAndComma(contactid3);
            return pvGroup;
        }

        public static string RemoveSpaceAndComma(string data)
        {
            data = data.Replace(",", "");
            data = data.Trim();
            return data;
        }

        public static int UpdateDynamicGroup(int groupId, string baseDataNo, string criteriaName)
        {
            using (SurveilAIEntities db = new SurveilAIEntities())
            {
                string querygrpconditions = "";
                int baseData = Convert.ToInt32(baseDataNo);
                var searchGroup = db.pvgroupconditions.Select(x => new { x.groupid, x.basedatano }).Where(x => x.groupid.Equals(groupId) && x.basedatano.Equals(baseData)).FirstOrDefault();
                if (searchGroup != null)
                {
                    querygrpconditions += "update pvgroupcondition set criteria = '" + criteriaName + "' where groupid = '" + groupId + "' and basedatano = '" + baseDataNo + "'";
                }
                else
                {
                    querygrpconditions += "insert into pvgroupcondition values('" + groupId + "', '" + baseDataNo + "', '" + criteriaName + "') ";
                }
                int result = db.Database.ExecuteSqlCommand(querygrpconditions);
                return result;
            }
        }

        public static int DeleteDynamicCritera(int groupId, string baseDataNo)
        {
            int result = 0;
            using (SurveilAIEntities db = new SurveilAIEntities())
            {
                int baseData = Convert.ToInt32(baseDataNo);
                var searchGroup = db.pvgroupconditions.Select(x => new { x.groupid, x.basedatano }).Where(x => x.groupid.Equals(groupId) && x.basedatano.Equals(baseData)).FirstOrDefault();
                if (searchGroup != null)
                {
                    result = db.Database.ExecuteSqlCommand("Delete from pvgroupcondition where groupid = '" + groupId + "' and basedatano = '" + baseData + "'");
                }
            }
            return result;
        }

        #region calendar Events
        public ActionResult CalendarEvents()
        {
            List<CashForeCast_Events> events = new List<CashForeCast_Events>();
            try
            {
                events = GetEventsList();
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }

            return View(events);
        }
        public JsonResult GetEvents()
        {
            List<FullCalendar> CalendarEvents = new List<FullCalendar>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {

                    int? Adjustment = db.CashForeCast_Events.Where(x => x.Event_Name.Equals("Adjustment")).Select(x => x.Growth_).FirstOrDefault();
                    if (Adjustment != null)
                    {
                        List<CashForeCast_Events> events = db.CashForeCast_Events.Where(x => x.Event_Name != "Adjustment").ToList();
                        foreach (var EventDetail in events)
                        {
                            if (string.IsNullOrEmpty(EventDetail.Lunar_Day))
                            {
                                FullCalendar CalEvent = new FullCalendar { Subject = EventDetail.Event_Name, Start = (DateTime)EventDetail.Event_Date, End = (DateTime)EventDetail.Event_Date, isFullDay = true, ThemeColor = "blue", Description = EventDetail.Event_Name };
                                CalendarEvents.Add(CalEvent);
                            }
                            else if (!string.IsNullOrEmpty(EventDetail.Lunar_Day))
                            {
                                FullCalendar CalEvent = new FullCalendar { Subject = EventDetail.Event_Name, Start = (DateTime)EventDetail.Event_Date, End = (DateTime)EventDetail.Event_Date, isFullDay = true, ThemeColor = "green", Description = EventDetail.Event_Name };
                                CalendarEvents.Add(CalEvent);
                            }

                        }

                        List<LunarCalendar> HijriDates = GetHijriCalendar(Convert.ToInt32(Adjustment));
                        foreach (var Hdate in HijriDates)
                        {
                            Hdate.LunarDate = GetDate(Hdate.LunarDate);
                        }
                        CalendarEvents[0].HijriDates = HijriDates;
                    }


                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            return new JsonResult { Data = CalendarEvents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public static List<LunarCalendar> GetHijriCalendar(int Adjustment)
        {
            DateTime today = DateTime.Today;
            today = today.AddDays(-30);
            System.Globalization.HijriCalendar hej = new System.Globalization.HijriCalendar();
            hej.HijriAdjustment = Adjustment;
            List<LunarCalendar> HijriCal = new List<LunarCalendar>();
            for (int i = 0; i < 365; i++)
            {

                string m = hej.GetMonth(today).ToString();
                string y = hej.GetYear(today).ToString();
                string d = hej.GetDayOfMonth(today).ToString();
                LunarCalendar cal = new LunarCalendar { LunarDate = y + "-" + m + "-" + d, SolarDate = today };
                HijriCal.Add(cal);
                today = today.AddDays(1);
            }
            return HijriCal;
        }

        public JsonResult AdjustCalendar(int adjustment)
        {
            List<FullCalendar> CalendarEvents = new List<FullCalendar>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    var output = db.Database.ExecuteSqlCommand("update CashForeCast_Events set [Growth%] = '" + adjustment + "' where Event_Name = 'Adjustment'");
                    if (output > 0)
                    {
                        List<CashForeCast_Events> events = db.CashForeCast_Events.Where(x => x.Event_Name != "Adjustment").ToList();
                        foreach (var EventDetail in events)
                        {
                            if (string.IsNullOrEmpty(EventDetail.Lunar_Day))
                            {
                                FullCalendar CalEvent = new FullCalendar { Subject = EventDetail.Event_Name, Start = (DateTime)EventDetail.Event_Date, End = (DateTime)EventDetail.Event_Date, isFullDay = true, ThemeColor = "blue", Description = EventDetail.Event_Name };
                                CalendarEvents.Add(CalEvent);
                            }
                            else if (!string.IsNullOrEmpty(EventDetail.Lunar_Day))
                            {
                                FullCalendar CalEvent = new FullCalendar { Subject = EventDetail.Event_Name, Start = (DateTime)EventDetail.Event_Date, End = (DateTime)EventDetail.Event_Date, isFullDay = true, ThemeColor = "green", Description = EventDetail.Event_Name };
                                CalendarEvents.Add(CalEvent);
                            }

                        }

                        List<LunarCalendar> HijriDates = GetHijriCalendar(adjustment);
                        foreach (var Hdate in HijriDates)
                        {
                            Hdate.LunarDate = GetDate(Hdate.LunarDate);
                        }
                        CalendarEvents[0].HijriDates = HijriDates;
                    }

                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
            }
            return new JsonResult { Data = CalendarEvents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public static string GetDate(string HijriDate)
        {
            //string[] datelist = HijriDate.Split('-');
            //return datelist[2];

            string[] datelist = HijriDate.Split('-');
            string month = GetLunarMonth(Convert.ToInt32(datelist[1]));
            return datelist[2] + "-" + month;
        }

        [HttpPost]
        public ActionResult CreateCalEvent(CashForeCast_Events model)
        {
            var result = 0;
            List<CashForeCast_Events> CalendarEvents = new List<CashForeCast_Events>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {

                    string Event_Date = model.EventDate + " " + model.EventMonth;
                    if (model.CalendarType == "Solar")
                    {
                        var now = DateTime.Now;
                        int month = DateTime.ParseExact(model.EventMonth, "MMMM", CultureInfo.CurrentCulture).Month;
                        DateTime date = new DateTime(now.Year, month, Convert.ToInt32(model.EventDate));
                        result = db.Database.ExecuteSqlCommand("insert into CashForeCast_Events values('" + model.Event_Name + "', '" + Event_Date + "', NULL, '" + date + "', NULL )");
                    }
                    else if (model.CalendarType == "Lunar")
                    {
                        DateTime date = LunarToSolar(model);
                        result = db.Database.ExecuteSqlCommand("insert into CashForeCast_Events values('" + model.Event_Name + "', NULL, '" + Event_Date + "', '" + date + "', NULL )");
                    }

                }
                CalendarEvents = GetEventsList();
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Event not created!";
                CalendarEvents = GetEventsList();
                return PartialView("_CalendarEvents", CalendarEvents);
            }
            if (result <= 0)
            {
                @TempData["NoMsg"] = "Event not created!";
            }
            else
            {
                @TempData["OkMsg"] = "Event has been created!";
            }
            return PartialView("_CalendarEvents", CalendarEvents);
        }


        [HttpPost]
        public ActionResult UpdateCalEvent(CashForeCast_Events model)
        {
            var result = 0;
            List<CashForeCast_Events> CalendarEvents = new List<CashForeCast_Events>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {

                    string Event_Date = model.EventDate + " " + model.EventMonth;
                    if (model.CalendarType == "Solar")
                    {

                        result = db.Database.ExecuteSqlCommand("update CashForeCast_Events set Event_Name = '" + model.Event_Name + "', Solar_Day = '" + Event_Date + "', Event_Date = '" + model.Event_Date + "' where Event_Name = '" + model.OldEventName + "'");
                    }
                    else if (model.CalendarType == "Lunar")
                    {
                        DateTime date = LunarToSolar(model);
                        result = db.Database.ExecuteSqlCommand("update CashForeCast_Events set Event_Name = '" + model.Event_Name + "', Lunar_Day = '" + Event_Date + "', Event_Date = '" + model.Event_Date + "' where Event_Name = '" + model.OldEventName + "'");
                    }
                    CalendarEvents = GetEventsList();
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Event not updated!";
                CalendarEvents = GetEventsList();
                return PartialView("_CalendarEvents", CalendarEvents);
            }
            if (result <= 0)
            {
                @TempData["NoMsg"] = "Event not updated!";
            }
            else
            {
                @TempData["OkMsg"] = "Event has been updated!";
            }
            return PartialView("_CalendarEvents", CalendarEvents);
        }

        [HttpPost]
        public ActionResult DeleteCalEvent(CashForeCast_Events model)
        {
            var result = 0;
            List<CashForeCast_Events> CalendarEvents = new List<CashForeCast_Events>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    result = db.Database.ExecuteSqlCommand("delete from CashForeCast_Events where Event_Name = '" + model.Event_Name + "'");
                    CalendarEvents = GetEventsList();
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Event not deleted!";
                CalendarEvents = GetEventsList();
                return PartialView("_CalendarEvents", CalendarEvents);
            }
            if (result <= 0)
            {
                @TempData["NoMsg"] = "Event not deleted!";
            }
            else
            {
                @TempData["OkMsg"] = "Event has been deleted!";
            }
            return PartialView("_CalendarEvents", CalendarEvents);
        }

        public static List<CashForeCast_Events> GetEventsList()
        {
            List<CashForeCast_Events> events = new List<CashForeCast_Events>();
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    int? Adjustment = db.CashForeCast_Events.Where(x => x.Event_Name.Equals("Adjustment")).Select(x => x.Growth_).FirstOrDefault();
                    if (Adjustment == null)
                    {
                        var output = db.Database.ExecuteSqlCommand("insert into CashForeCast_Events (Event_Name, [Growth%]) values ('Adjustment', 0);");
                    }
                    events = db.CashForeCast_Events.Where(x => x.Event_Name != "Adjustment").ToList();


                    foreach (var e in events)
                    {
                        DateTime a = (DateTime)e.Event_Date;
                    }

                }
                //For Next year

                foreach (var CalEvent in events)
                {
                    if (!string.IsNullOrEmpty(CalEvent.Solar_Day))
                    {
                        CalEvent.NextYear = CalEvent.Event_Date.Value.AddYears(1).ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        CalEvent.NextYear = NextYearLunar(CalEvent);
                    }

                }
            }
            catch (Exception ex)
            {
            }

            return events;
        }

        public static DateTime LunarToSolar(CashForeCast_Events LunarModel)
        {
            int month = GetLunarMonthNum(LunarModel.EventMonth);
            DateTime today = DateTime.Today;
            System.Globalization.HijriCalendar hej = new System.Globalization.HijriCalendar();
            string y = hej.GetYear(today).ToString();
            DateTime SolarDate = new DateTime(Convert.ToInt32(y), month, Convert.ToInt32(LunarModel.EventDate), hej);
            return SolarDate;
        }

        public static string NextYearLunar(CashForeCast_Events LunarModel)
        {
            try
            {
                string[] dateList = LunarModel.Lunar_Day.Split(' ');
                int month = GetLunarMonthNum(dateList[1]);
                DateTime today = DateTime.Today;
                today = today.AddYears(1);
                System.Globalization.HijriCalendar hej = new System.Globalization.HijriCalendar();
                string y = hej.GetYear(today).ToString();
                DateTime SolarDate = new DateTime(Convert.ToInt32(y), month, Convert.ToInt32(dateList[0]), hej);
                return SolarDate.ToString("MM-dd-yyyy");
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        public static int GetLunarMonthNum(string month)
        {
            int val = 0;
            switch (month)
            {
                case "Muharram":
                    val = 1;
                    break;
                case "Safar":
                    val = 2;
                    break;
                case "Rabiul-Awwal":
                    val = 3;
                    break;
                case "Rabi-us-Sani":
                    val = 4;
                    break;
                case "Jamadi-ul-Awwal":
                    val = 5;
                    break;
                case "Jamadi-us-Sani":
                    val = 6;
                    break;
                case "Rajab":
                    val = 7;
                    break;
                case "Shaban":
                    val = 8;
                    break;
                case "Ramadan":
                    val = 9;
                    break;
                case "Shawal":
                    val = 10;
                    break;
                case "Zil-Qadah":
                    val = 11;
                    break;
                case "Zul-Hijah":
                    val = 12;
                    break;

            }
            return val;
        }

        public static string GetLunarMonth(int month)
        {
            string val = "";
            switch (month)
            {
                case 1:
                    val = "Muharram";
                    break;
                case 2:
                    val = "Safar";
                    break;
                case 3:
                    val = "Rabiul-Awwal";
                    break;
                case 4:
                    val = "Rabi-us-Sani";
                    break;
                case 5:
                    val = "Jamadi-ul-Awwal";
                    break;
                case 6:
                    val = "Jamadi-us-Sani";
                    break;
                case 7:
                    val = "Rajab";
                    break;
                case 8:
                    val = "Shaban";
                    break;
                case 9:
                    val = "Ramadan";
                    break;
                case 10:
                    val = "Shawal";
                    break;
                case 11:
                    val = "Zil-Qadah";
                    break;
                case 12:
                    val = "Zul-Hijah";
                    break;

            }
            return val;
        }
        #endregion

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
            pvjournal.issuer = "CUSTOMIZATION_CONTROLLER";
            pvjournal.issuertype = 11;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";
            Audit audit = new Audit();
            audit.Log(pvjournal);
        }

    }

    internal class AMTCustom
    {
        internal string Decrypt(string mt_message, string encryKey)
        {
            throw new NotImplementedException();
        }

        internal string Encrypt(string mt_message, string encryKey)
        {
            throw new NotImplementedException();
        }
    }
}

