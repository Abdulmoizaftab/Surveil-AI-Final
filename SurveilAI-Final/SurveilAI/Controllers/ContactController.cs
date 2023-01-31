using NLog;
using SurveilAI.DataContext;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SurveilAI.Controllers
{
    public class ContactController : Controller
    {
        SurveilAIEntities db = new SurveilAIEntities();
        ILogger userlog = LogManager.GetLogger("user");
        ILogger activitylog = LogManager.GetLogger("actvity");
        ILogger errorlog = LogManager.GetLogger("error");
        contact obj = new contact();

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
        // GET: Contact
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Contact");
            }
            else
            {
                var ret = Check("8");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Contact");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " : navigate to contacts");
                var mymodel = new contact();
                mymodel.data = db.contacts.ToList();
                var MailtempList = new SelectList(db.mailtemplates.ToList(), "", "id");
                ViewData["MailtempList"] = MailtempList;
                return View(mymodel);
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(contact collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("58");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " :  is creating new Contact");
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    String fname, company, phone, mobile, fax, language, sms, email, function;
                    String contactid = collection.contactid.ToString();
                    String lname = collection.lname.ToString();
                    if (collection.fname != null)
                    {
                        fname = collection.fname.ToString();
                    }
                    else
                    {
                        fname = "";
                    }
                    if (collection.company != null)
                    {
                        company = collection.company.ToString();
                    }
                    else
                    {
                        company = "";
                    }
                    if (collection.phone != null)
                    {
                        phone = collection.phone.ToString();
                    }
                    else
                    {
                        phone = "";
                    }
                    if (collection.mobile != null)
                    {
                        mobile = collection.mobile.ToString();
                    }
                    else
                    {
                        mobile = "";
                    }
                    if (collection.fax != null)
                    {
                        fax = collection.fax.ToString();
                    }
                    else
                    {
                        fax = "";
                    }
                    if (collection.e_mail != null)
                    {
                        email = collection.e_mail.ToString();
                    }
                    else
                    {
                        email = "";
                    }
                    if (collection.smsmailaddress != null)
                    {
                        sms = collection.smsmailaddress.ToString();
                    }
                    else
                    {
                        sms = "";
                    }
                    if (collection.cfunction != null)
                    {
                        function = collection.cfunction.ToString();
                    }
                    else
                    {
                        function = "";
                    }
                    if (collection.language != null)
                    {
                        language = collection.language.ToString();
                    }
                    else
                    {
                        language = "";
                    }
                    if (collection.check.ToString() == "new")
                    {
                        int output = db.Database.ExecuteSqlCommand("insert into contact(contactid,lname,fname,company,phone,mobile,fax,e_mail,smsmailaddress,cfunction,language,mailtemplate) " +
                        "Values('" + contactid.Trim() + "','" + lname.Trim() + "','" + fname.Trim() + "','" + company.Trim() + "', '" + phone.Trim() + "' ,'" + mobile.Trim() + "','" + fax.Trim() + "','" + email.Trim() + "','" + sms + "','" + function + "','" + language + "','')");
                        if (output > 0)
                        {
                            @TempData["OKMsg"] = "Contact Created Successfully!";
                            activitylog.Info(Session["UserID"].ToString() + " : Contact Created Successfully");
                            userlog.Info(Session["UserID"].ToString() + " : Contact " + contactid.Trim() + " Created Successfully");

                            Log("Contact Creation", "", 10002021, "$cid: " + contactid);
                            return RedirectToAction("Index", "Contact");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Contact Creation Unsuccessful!";
                            errorlog.Error(Session["UserID"].ToString() + " Contact Creation Unsuccessful");
                            errorlog.Error(Session["UserID"] + " Contact Creation Unsuccessful error : " + output);
                            Log("Contact Creation Failed", "", 10002022, "$cid: " + contactid);
                            return RedirectToAction("Index", "Contact");
                        }
                    }
                    else if (collection.check.ToString() == "edit")
                    {
                        String contactidOld = collection.ContactidOld.ToString();
                        activitylog.Info(Session["UserID"].ToString() + " :  is editing user " + collection.contactid);
                        int output = db.Database.ExecuteSqlCommand("Update contact Set contactid = '" + contactid + "' ,lname = '" + lname + "' ,fname = '" + fname + "' ,company = '" + company + "' ,phone = '" + phone + "' ,fax = '" + fax + "' , mobile = '" + mobile + "', e_mail = '" + email + "', smsmailaddress = '" + sms + "', cfunction = '" + function + "', language = '" + language + "', mailtemplate = '' where contactid = '" + contactidOld + "'");
                        if (output > 0)
                        {
                            @TempData["OKMsg"] = "Contact Updated Successfully!";
                            activitylog.Info(Session["UserID"].ToString() + " : Contact Updated Successfully");
                            userlog.Info(Session["UserID"].ToString() + " : Contact " + collection.contactid + " Created Successfully");
                            Log("Contact Updation", "", 10002023, "$cid: " + contactid);

                            return RedirectToAction("Index", "Contact");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Contact Update Unsuccessful!";
                            errorlog.Error(Session["UserID"].ToString() + " Contact " + collection.contactid + " Creation Unsuccessful");
                            errorlog.Error(Session["UserID"] + " Contact Creation Unsuccessful error : " + output);
                            Log("Contact Updation Failed", "", 10002024, "$cid: " + contactid);
                            return RedirectToAction("Index", "Contact");
                        }

                    }
                    else
                    {
                        return RedirectToAction("Index", "Contact");
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("Index", "Contact");
            }
        }


        // POST: User/Delete/5

        public ActionResult Delete(String id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("58");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    activitylog.Info(Session["UserID"].ToString() + " :  deleting contact " + id);
                    String contactid = id;
                    int output = db.Database.ExecuteSqlCommand("Delete from contact where contactid = '" + contactid + "'");
                    if (output > 0)
                    {
                        @TempData["OKMsg"] = "Contact Deleted Successfully!";
                        activitylog.Info(Session["UserID"].ToString() + " :  Contact Deleted Successfully " + id);
                        userlog.Info(Session["UserID"].ToString() + " :Contact Deleted Successfully " + id);
                        Log("Contact Deletion", "", 10002025, "$cid: " + contactid);
                        return RedirectToAction("Index", "Contact");
                    }
                    else
                    {
                        @TempData["NoMsg"] = "Contact Could not be Deleted ";
                        activitylog.Info(Session["UserID"].ToString() + " :  Contact Could not be Deleted " + id);
                        errorlog.Error("User: " + Session["UserID"] + " Error Contact Could not be Deleted " + output);
                        Log("Contact Deletion failed", "", 10002026, "$cid: " + contactid);
                        return RedirectToAction("Index", "Contact");
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("Index", "Contact");
            }
        }


        public ActionResult EditPost(string id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var obj = db.contacts.Where(x => x.contactid.Equals(id)).FirstOrDefault();
            obj.lname = obj.lname.Trim();
            obj.fname = obj.lname.Trim();

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(contact collection)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var ret = Check("58");
                if (ret == false)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            try
            {
                activitylog.Info(Session["UserID"].ToString() + " :  is updating Contact " + collection.contactid);
                using (SurveilAIEntities db = new SurveilAIEntities())
                {
                    String fname, company, phone, mobile, fax, language, sms, email, function;
                    String contactid = collection.contactid.ToString();
                    String lname = collection.lname.ToString();
                    if (collection.fname != null)
                    {
                        fname = collection.fname.ToString();
                    }
                    else
                    {
                        fname = "";
                    }
                    if (collection.company != null)
                    {
                        company = collection.company.ToString();
                    }
                    else
                    {
                        company = "";
                    }
                    if (collection.phone != null)
                    {
                        phone = collection.phone.ToString();
                    }
                    else
                    {
                        phone = "";
                    }
                    if (collection.mobile != null)
                    {
                        mobile = collection.mobile.ToString();
                    }
                    else
                    {
                        mobile = "";
                    }
                    if (collection.fax != null)
                    {
                        fax = collection.fax.ToString();
                    }
                    else
                    {
                        fax = "";
                    }
                    if (collection.e_mail != null)
                    {
                        email = collection.e_mail.ToString();
                    }
                    else
                    {
                        email = "";
                    }
                    if (collection.smsmailaddress != null)
                    {
                        sms = collection.smsmailaddress.ToString();
                    }
                    else
                    {
                        sms = "";
                    }
                    if (collection.cfunction != null)
                    {
                        function = collection.cfunction.ToString();
                    }
                    else
                    {
                        function = "";
                    }
                    if (collection.language != null)
                    {
                        language = collection.language.ToString();
                    }
                    else
                    {
                        language = "";
                    }
                    if (collection.check.ToString() == "update")
                    {
                        string query = String.Format("update contact Set contactid = '{0}', lname ='{1}',fname='{2}',company='{3}',phone='{4}',mobile='{5}',fax='{6}',e_mail='{7}',smsmailaddress='{8}',cfunction='{9}',language='{10}',mailtemplate='' where contactid='{0}'", contactid.Trim(), lname.Trim(), fname.Trim(), company.Trim(), phone.Trim(), mobile.Trim(), fax.Trim(), email.Trim(), sms.Trim(), function.Trim(), language.Trim());
                        int output = db.Database.ExecuteSqlCommand(query);
                        if (output > 0)
                        {
                            @TempData["OKMsg"] = "Contact Updated Successfully!";
                            activitylog.Info(Session["UserID"].ToString() + " : Contact update Successfully");
                            userlog.Info(Session["UserID"].ToString() + " : Contact " + collection.contactid + " updated Successfully");
                            return RedirectToAction("Index", "Contact");
                        }
                        else
                        {
                            @TempData["NoMsg"] = "Contact Update Unsuccessful!";
                            errorlog.Error(Session["UserID"].ToString() + " Contact update Unsuccessful");
                            errorlog.Error(Session["UserID"] + " Contact update Unsuccessful error : " + output);

                            return RedirectToAction("Index", "Contact");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Contact");
                    }
                }
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + Session["UserID"] + " Error: " + ex);
                @TempData["NoMsg"] = "Oops! Something went wrong Try Again!";
                return RedirectToAction("Index", "Contact");
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
            pvjournal.issuer = "CONTACT_CONTROLLER";
            pvjournal.issuertype = 13;
            pvjournal.vardata = vardata;
            pvjournal.Operation_Type = "FrontEnd Manager";
            Audit audit = new Audit();
            audit.Log(pvjournal);
        }
    }
}