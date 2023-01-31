using Dapper;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using SurveilAI_APIs.DataContext;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;


namespace SurveilAI_APIs.Controllers
{
    public class DesktopApiController : ApiController
    {
        SqlConnection con = new SqlConnection("Data Source=192.168.1.100; Initial Catalog = SurveilAI; User ID = sa; Password=Dev@2022");
        Queue LoggedDetail = new Queue();
        Queue<string> Device = new Queue<string>();
        MappedLogin map = new MappedLogin();
        MappedHier hier = new MappedHier();
        Queue<string> HierName = new Queue<string>();
        Queue<string> HierID = new Queue<string>();
        Queue<string> HierDevice = new Queue<string>();
        string pattern = "['\"]";
        int count = 0;
        string[] channel = new string[1000];
        string HierLevl = "";
        string DeviceType = "";
        dynamic rs1;
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> Login(UserPassword mapp)
        {
            try
            {
                var getlogin = await GetLogin(mapp.user, mapp.password);
                if (getlogin.Any(x => x.ATM.Contains("DeviceID")))
                {
                    if (getlogin.Count > 0)
                    {
                        foreach (var item in getlogin)
                        {
                            var sp = item.ATM.Split();
                            map.UserID = item.UserID;
                            map.AccountType = item.AccountType;
                            var final = RemoveFirstAndLast(sp[3]);
                            if (final.Contains(','))
                            {
                                var fin = final.Split(',');
                                var a = 0;
                                foreach (var final1 in fin)
                                {

                                    LoggedDetail.Enqueue(Regex.Replace(final1, pattern, string.Empty));
                                    if (final1.Contains('.'))
                                    {

                                        var cnvsplit = final1.Split('.');
                                        for (int j = 0; j < cnvsplit.Length; j++)
                                        {
                                            channel[a] = cnvsplit[0];
                                            count++;
                                            a++;
                                        }
                                    }
                                    else
                                    {
                                        channel[count] = final1;
                                        count++;
                                    }

                                }

                            }
                            else
                            {
                                LoggedDetail.Enqueue(Regex.Replace(final, pattern, string.Empty));
                            }
                            map.ATM = LoggedDetail;
                            map.Message = "Successfully Login";
                            foreach (var channels in channel)
                            {
                                if (channels != null)
                                {
                                    string val = channels;
                                    Device.Enqueue(Regex.Replace(val, pattern, string.Empty));
                                }
                                else
                                {
                                    break;
                                }
                            }
                            Device = new Queue<string>(Device.Distinct());
                            map.Devices = Device;
                        }
                    }
                    else
                    {
                        map.Message = "Invalid User Or Password";
                    }
                }
                
                else
                {
                    
                    foreach (var item in getlogin)
                    {
                        var sp = item.ATM.Split();
                        map.UserID = item.UserID;
                        map.AccountType = item.AccountType;
                        var final = RemoveFirstAndLast(sp[2]);
                        HierLevl = Regex.Replace(final, pattern, string.Empty);
                        DeviceType = RemoveHierLevl(sp[5]);
                    }
                    //var query = @"select h.HierName,d.DeviceType,d.DeviceID from Device as d 
                    //                Inner join Hierarchy as h on d.HierLevel=h.Hierlevel
                    //                where d.Hierlevel IN(" + HierLevl + ") and d.DeviceType IN(" + DeviceType + ")";
                    var query = @"select h.Hierlevel,h.HierName,ac.DeviceID as Devices,d.BranchName,ac.DeviceType,
                                ac.Cam1,ac.Cam2,ac.Cam3,ac.Cam4,ac.Cam5,ac.Cam6,
                                ac.Cam7,ac.Cam8,ac.Cam9 from AddCameras as ac
                                INNER JOIN Hierarchy as h on h.Hierlevel=ac.ACHierLevel
                                Inner JOIn Device as d on d.DeviceID=ac.DeviceID
                                where ac.ACHierLevel in(" + HierLevl + ") and ac.DeviceType in(" + DeviceType + ")";
                    con.Open();
                    rs1 = con.Query<MappedHier>(query);
                    //rs1.Add(new MappedHier { Message = "Successfully Login", UserID = map.UserID, AccountType = map.UserID });
                    con.Close();
                    map.HierAssignLevl = rs1;
                    //if (rs.Count() > 1)
                    //{
                    //    foreach (var item in rs)
                    //    {
                    //        HierName.Enqueue(item.HierName);
                    //        HierID.Enqueue(item.DeviceID);
                    //        HierDevice.Enqueue(item.DeviceType);
                    //    }
                    //}
                    //map.HierName = HierName;
                    //map.Devices = HierID;
                    //map.DeviceType = HierDevice;
                    
                    map.Message = "Successfully Login";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { Status = 200, Data = map });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Status = 500, Message = "Connectivity Problem", Error = ex });
            }
        }
        public async Task<List<UserDetailWithDevicesAndHierLevel>> GetLogin(string email, string password)
        {

            try
            {
                var query = @"select UserID,AccountType,ATM from Users where UserID='" + email + "' and Password='" + password + "'";
                con.Open();
                var rs = await con.QueryAsync<UserDetailWithDevicesAndHierLevel>(query);
                con.Close();
                return rs.ToList();
            }

            catch (Exception ex)
            {
                con.Close();
                throw ex;
            }

        }
        public string RemoveFirstAndLast(string str)
        {
            str = str.Substring(0, str.Length - 2);
            str = str.Substring(2);
            return str;
        }
        public string RemoveHierLevl(string str)
        {
            str = str.Substring(0, str.Length - 1);
            str=str.Substring(2);
            str = str.Substring(1);
            return str;
        }
    }
    public class MappedLogin
    {

        public string UserID { get; set; }
        public Queue ATM { get; set; }
        public string AccountType { get; set; }
        public Queue<string> Devices { get; set; }
        public string Message { get; set; }
        public Queue<string> DeviceType { get; set; }
        public Queue<string> HierName { get; set; }
        public List<MappedHier> HierAssignLevl { get; set; }
    }
    public class UserPassword
    {
        public string user { get; set; }
        public string password { get; set; }
    }
    public class MappedHier
    {
        public string Devices { get; set; }
        public string DeviceType { get; set; }
        public string Hierlevel { get; set; }
        public string HierName { get; set; }
        public string BranchName { get; set; }
        public string Cam1 { get; set; }
        public string Cam2 { get; set; }
        public string Cam3 { get; set; }
        public string Cam4 { get; set; }
        public string Cam5 { get; set; }
        public string Cam6 { get; set; }
        public string Cam7 { get; set; }
        public string Cam8 { get; set; }
        public string Cam9 { get; set; }
       
    }
}