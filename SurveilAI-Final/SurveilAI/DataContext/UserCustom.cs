using NLog;
using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SurveilAI.DataContext
{
    public class UserCustom
    {
        public IEnumerable<User> usersss { get; set; }

        public IEnumerable<UserSecurity> usersec { get; set; }

        public string AccType { get; set; }
        public int Count { get; set; }

        public List<UserCustom> Data = new List<UserCustom>();

        ILogger errorlog = LogManager.GetLogger("error");
        ILogger activitylog = LogManager.GetLogger("activity");
        SurveilAIEntities db = new SurveilAIEntities();

        public string SetAssignDevice(string user, string device)
        {
            try
            {
                int output = db.Database.ExecuteSqlCommand("Update Users Set ATM = '" + device + "' where UserID='" + user + "'");
                if (output > 0)
                {
                    return "Success";
                }
                else
                {
                    return "Fail";
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public List<Device> GetAssignDevice(string user)
        {

            List<Device> noData = new List<Device>();
            try
            {
                var obj = new Device();


                var ATMID = db.Users.Where(a => a.UserID.Equals(user)).Select(a => a.ATM).First();

                if (ATMID != "" && ATMID != null)
                {
                    List<Device> AllDev = new List<Device>();
                    if (ATMID.Contains("DeviceID"))
                    {
                        ATMID = ATMID.Remove(0, 13);//removing DeviceID IN
                        string temp = ATMID;

                        AllDev = db.Devices.Where(a => ATMID.Contains(a.DeviceID)).ToList();
                    }
                    else if (ATMID.Contains("HierLevel"))
                    {
                        ATMID = ATMID.Remove(0, 14);//removing HierLevel IN
                        AllDev = db.Devices.Where(a => ATMID.Contains(a.HierLevel.ToString())).ToList();
                    }


                    return AllDev;
                }
                else
                {
                    return noData;
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return noData;
            }
        }

        public string AddHierarchy(string name)
        {
            try
            {
                var HierName = name;

                //Getting max HierID
                //var hids = db.Hierarchies.Where(a => !a.HierId.Contains(".")).Select(x => x.HierId).ToList();
                var hids = db.Hierarchies.Where(a => !a.HierId.Contains(".")).OrderByDescending(x => x.HierId).Select(x => x.HierId).FirstOrDefault();
                //int maxHid = hids.Select(v => int.Parse(v.Substring(0))).Max();
                //int maxHid = hids=="" ? 0 : int.Parse(hids) ;
                string maxHid = hids ?? "0";

                var checklevel = db.Hierarchies.Max(a => a.Hierlevel);
                checklevel++;
                //var checkhier = db.Hierarchies.Where(a => !a.HierId.Contains(".")).Max(b => b.HierId);
                //var hierID = Convert.ToInt32(checkhier);
                //hierID++;
                var checkName = db.Hierarchies.SqlQuery("select * from Hierarchy where HierName='" + HierName + "'").ToList();
                int newHierId = Convert.ToInt32(maxHid) + 1;
                if (checkName.Count < 1)
                {
                    int addHir = db.Database.ExecuteSqlCommand("insert into hierarchy (HierName,Hierlevel,HierId)values('" + HierName + "','" + checklevel + "','" + newHierId + "')");
                    if (addHir > 0)
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Fail";
                    }
                }
                else
                {
                    return "Exist";
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }

        }

        public string AddHierarchy(string name, string parent)
        {
            try
            {
                var HierName = name;
                var hierarchies = db.Hierarchies.ToList();
                var checklevel = hierarchies.Max(a => a.Hierlevel);
                checklevel++;

                string hierID = "";

                parent = hierarchies.Where(a => a.HierId.Equals(parent)).Select(b => b.HierId).SingleOrDefault();
                var dotCount = parent.Count(a => a == '.');
                dotCount++;
                hierarchies = hierarchies.Where(a => a.HierId.StartsWith(parent) && a.HierId.Count(f => f == '.') == dotCount).OrderBy(a => a.HierId).ToList();
                var list = "";
                string newHierId = "";
                if (hierarchies.Count() > 0)
                {
                    //list = hierarchies.Last().HierId;
                    //list = list.TrimEnd(list[list.Length - 1]) + (Convert.ToInt32(list.Split('.').Last()) + 1).ToString();
                    //hierID = list;


                    List<string> nl = new List<string>();
                    foreach (var item in hierarchies)
                    {
                        nl.Add(item.HierId.Substring(item.HierId.LastIndexOf('.') + 1));

                    }
                    var maxNumber = nl.Select(v => int.Parse(v.Substring(0))).Max();
                    newHierId = parent + "." + (maxNumber + 1).ToString();
                    //list = hierarchies.Last().HierId;
                    //list = list.TrimEnd(list[list.Length - 1]) + (Convert.ToInt32(list.Split('.').Last()) + 1).ToString();
                    hierID = newHierId;

                }
                else
                {
                    hierID = parent;
                    hierID += ".1";
                }

                var checkName = db.Hierarchies.SqlQuery("select * from Hierarchy where HierName='" + HierName + "'").ToList();

                if (checkName.Count < 1)
                {
                    int addHir = db.Database.ExecuteSqlCommand("insert into Hierarchy (HierName,Hierlevel,HierId)values('" + HierName + "','" + checklevel + "','" + hierID + "')");
                    if (addHir > 0)
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Fail";
                    }
                }
                else
                {
                    return "Exist";
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public string DelHierarchy(string name, int level)//contains device cannot delete
        {
            try
            {
                var check = db.Devices.Where(a => a.HierLevel == level).ToList();
                var check2 = db.Hierarchies.SqlQuery("Select * from Hierarchy where HierId LIKE ((Select HierId from Hierarchy where HierName = '" + name + "' AND Hierlevel = '" + level + "')+'%')").ToList();
                if (check.Count() > 0)
                {
                    return "ContainsDevice";
                }
                if (check2.Count() > 1)
                {
                    return "ContainsSubHierarchy";
                }
                int deleteHir = db.Database.ExecuteSqlCommand("DELETE FROM hierarchy WHERE HierName = '" + name + "' AND Hierlevel = '" + level + "'");
                if (deleteHir > 0)
                {
                    return "Success";
                }
                else
                {
                    return "Fail";
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public string EditHierarchy(string name, string level)
        {
            try
            {
                var HierName = name;
                var checklevel = db.Hierarchies.Where(a => a.Hierlevel.ToString() == level).Select(a => a.HierId).FirstOrDefault();
                var checkhier = db.Hierarchies.Where(a => !a.HierId.Contains(".")).Max(b => b.HierId);
                var hierID = Convert.ToInt32(checkhier);
                if (hierID.ToString() != checklevel)
                {
                    hierID++;

                    int addHir = db.Database.ExecuteSqlCommand("UPDATE Hierarchy SET HierName = '" + HierName + "', HierId = '" + hierID + "' Where Hierlevel = '" + level + "'");
                    if (addHir > 0)
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Fail";
                    }
                }
                else
                {
                    return "Exist";
                }


            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }

        }

        public string EditHierarchy(string name, string level, string parent)
        {
            try
            {
                //var HierName = name;
                //var checklevel = db.Hierarchies.Where(a => a.Hierlevel.ToString() == level).Select(a => a.HierId).FirstOrDefault();
                //var checkhier = db.Hierarchies.Where(a => !a.HierId.Contains(".")).Max(b => b.HierId);
                //var hierID = Convert.ToInt32(checkhier);

                var HierName = name;
                var hierarchies = db.Hierarchies.ToList();

                string hierID = "";
                var checkSameParent = hierarchies.Where(a => a.HierId.Equals(parent)).Select(a => a.Hierlevel).FirstOrDefault();
                var check2 = db.Hierarchies.SqlQuery("Select * from Hierarchy where HierId LIKE ((Select HierId from Hierarchy where HierName = '" + name + "' AND Hierlevel = '" + level + "')+'%')").ToList();
                if (check2.Count() > 1)
                {
                    return "ContainsSubHierarchy";
                }
                if (checkSameParent.ToString() != level)
                {
                    parent = hierarchies.Where(a => a.HierId.Equals(parent)).Select(b => b.HierId).SingleOrDefault();
                    var dotCount = parent.Count(a => a == '.');
                    dotCount++;
                    hierarchies = hierarchies.Where(a => a.HierId.StartsWith(parent) && a.HierId.Count(f => f == '.') == dotCount).OrderBy(a => a.HierId).ToList();


                    //var list = "";
                    string newHierId = "";
                    if (hierarchies.Count() > 0)
                    {
                        List<string> nl = new List<string>();
                        foreach (var item in hierarchies)
                        {
                            nl.Add(item.HierId.Substring(item.HierId.LastIndexOf('.') + 1));

                        }
                        var maxNumber = nl.Select(v => int.Parse(v.Substring(0))).Max();
                        newHierId = parent + "." + (maxNumber + 1).ToString();
                        //list = hierarchies.Last().HierId;
                        //list = list.TrimEnd(list[list.Length - 1]) + (Convert.ToInt32(list.Split('.').Last()) + 1).ToString();
                        hierID = newHierId;

                    }
                    else
                    {
                        hierID = parent;
                        hierID += ".1";
                    }


                    int addHir = db.Database.ExecuteSqlCommand("UPDATE Hierarchy SET HierName = '" + HierName + "', HierId = '" + hierID + "' Where Hierlevel = '" + level + "'");
                    if (addHir > 0)
                    {
                        return "Success";
                    }
                    else
                    {
                        return "Fail";
                    }
                }
                else
                {
                    return "Exist";
                }


            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public string RemoveAssignDevice(string device)
        {
            try
            {

                //Update Users Set ATM = REPLACE(ATM, '''7777''', '') Where ATM LIKE '%''7777''%'

                int UDusr = db.Database.ExecuteSqlCommand("Update Users Set ATM = REPLACE(ATM, '''" + device + "''', '') Where ATM LIKE '%''" + device + "''%'");

                //Update Users SET ATM = REPLACE(REPLACE(REPLACE(ATM, '(,', '('), ',,', ','), ',)', ')') WHERE ATM LIKE '%(,%' OR ATM LIKE '%,,%' OR ATM LIKE '%,)%'

                int UDusr2 = db.Database.ExecuteSqlCommand("Update Users SET ATM = REPLACE(REPLACE(REPLACE(ATM, '(,', '('), ',,', ','), ',)', ')') WHERE ATM LIKE '%(,%' OR ATM LIKE '%,,%' OR ATM LIKE '%,)%'");

                return ("Result: " + UDusr + " && " + UDusr2);

            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public string EditAssignDevice(string device, string oldname)
        {
            try
            {

                //Update Users Set ATM = REPLACE(ATM, '''7777''', '''8888''') Where ATM LIKE '%''7777''%'

                int UDusr = db.Database.ExecuteSqlCommand("Update Users Set ATM = REPLACE(ATM, '''" + oldname + "''', '''" + device + "''') Where ATM LIKE '%''" + oldname + "''%'");

                return ("Result: " + UDusr);

            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return e;
            }
        }

        public stb GetStateByBit(int? state)
        {
            try
            {
                int statebit = Convert.ToInt32(state);
                stb obj = new stb();
                if (statebit < 1)
                {
                    obj.messagetext = "Operational";
                    obj.color = "0 255 0";
                    return obj;
                }
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
                var stbs = (from st in db.stbs
                            join msg in db.message0001
                            on st.textno equals msg.textno
                            where st.type.Equals(0) && s.Contains(st.bit)
                            orderby st.prio ascending
                            select new
                            {
                                st,
                                messagetext = msg.messagetext
                            }).FirstOrDefault();

                obj = stbs.st;
                obj.messagetext = stbs.messagetext;
                return obj;
            }
            catch (Exception ex)
            {
                //var e = "Error: " + ex;
                errorlog.Error(" Error: " + ex);
                return null;
            }
        }

        public static string GetBase64String(string image1)
        {
            try
            {
                string base64String = string.Empty;
                using (var img = System.Drawing.Image.FromFile(image1)) // Image Path from File Upload Controller
                {
                    using (var memStream = new MemoryStream())
                    {
                        img.Save(memStream, img.RawFormat);
                        byte[] imageBytes = memStream.ToArray();

                        // Convert byte[] to Base64 String
                        base64String = Convert.ToBase64String(imageBytes);

                        return base64String;
                        //   return base64String;
                    }
                }
            }
            catch (Exception)
            {
                return @"/9j/4AAQSkZJRgABAQEAAAAAAAD/4QCuRXhpZgAATU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAAZKGAAcAAAB6AAAALAAAAABVTklDT0RFAABDAFIARQBBAFQATwBSADoAIABnAGQALQBqAHAAZQBnACAAdgAxAC4AMAAgACgAdQBzAGkAbgBnACAASQBKAEcAIABKAFAARQBHACAAdgA2ADIAKQAsACAAcQB1AGEAbABpAHQAeQAgAD0AIAA5ADUACv/bAEMAAgEBAQEBAgEBAQICAgICBAMCAgICBQQEAwQGBQYGBgUGBgYHCQgGBwkHBgYICwgJCgoKCgoGCAsMCwoMCQoKCv/bAEMBAgICAgICBQMDBQoHBgcKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCv/AABEIAc8CZwMBIgACEQEDEQH/xAAfAAABBQEBAQEBAQAAAAAAAAAAAQIDBAUGBwgJCgv/xAC1EAACAQMDAgQDBQUEBAAAAX0BAgMABBEFEiExQQYTUWEHInEUMoGRoQgjQrHBFVLR8CQzYnKCCQoWFxgZGiUmJygpKjQ1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4eLj5OXm5+jp6vHy8/T19vf4+fr/xAAfAQADAQEBAQEBAQEBAAAAAAAAAQIDBAUGBwgJCgv/xAC1EQACAQIEBAMEBwUEBAABAncAAQIDEQQFITEGEkFRB2FxEyIygQgUQpGhscEJIzNS8BVictEKFiQ04SXxFxgZGiYnKCkqNTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqCg4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2dri4+Tl5ufo6ery8/T19vf4+fr/2gAMAwEAAhEDEQA/APk3Jz909aAf9n9KXNAIoATdjotGcfwmnZFIDQAhPH3TQST/AA/pS5Hp+lBI70AJk9lNG4/3aXI/yKXNADQcZ+U0E99tOzSZ7UAIST/D+lGf9k/lS55oyKAE3H+7Rnn7pp2aM0ANyc52np6UZJ/h/SlyM96M0AIPXafyoye60ueaXNADST1KmjJyDg/lTs0mRnvQAmTj7tGf9k/lSk4oyM0AID/sUZ9U/SlyP8ijNACZ4Hynr6UZP939KXINGfagBAT1wfyoB/2KXIoyO38qAEz/ALH6UdiNp/KlzQSCKAEyc/dNHP8Ad/Slz7UZHWgBM9fkozz9z9KXI7fyozQAnr8v6UZPoaUkY/8ArUZoATk9V/SjPP3P0pc8UZH+RQAmT/c/SgHnO39KXNGR/kUAJkgAYP5Uf8ApQRjijcMZoATPbaRQDx92lyP8ijI5oAQHH8NGfRe9LkUbhQAn/AKM4H3cUu4daMjqaAEB/wBk0Z5+5S5GTRkdqAEz/s0d87D1pdwoDA0AJnH8OKAcH7ppdwPJoyM0AJnn7tGf9ntS7h2o3DNACf8AAKM852Gl3A0ZGaAEz/snpQTn+Gl3LxRkUAJnn7ho6j7hpdwzRuFACZ5zsozz900uRnmjcP1oAQn1SjP+yaXIoLCgBM56pR/wCl3CgkdDQAhP+yaCc/wGlLCjIoATIxjbRnJ+4etKWGKNwoATtjZQT/s0pIoyv6elACZz/CaM8fdpdw4zRkYoATPbYaKXcKKAG7TnpRtPTFO3DPB/SjcPX9KAG7W9KNp9B1p28etG8ev6UANKkc4oKn0p28etIWHY/pQAhU+lGw+lO3D1/Sk3D1/SgBCrelBVvSnbh6/pQWB7/mKAG7W/u0bT/dp24ev6Um4eo/KgBNp9KNrf3aXcB1/lS7h6/pQA3a392jafSl3D1H5Ubh6j8qAE2n+7RtPXbS7hnqPyo3D2/KgBNrdMUbW/u07cPX9KTcM9f0oATafSja392l3DHUflRuGeSPyoATafSja3pS7hjqPyo3D1H5UAJtb+7RtPpS7h6j34o3D1H5UAJtb+7RtPpS7h6j8qXcPX9KAG7W9KNrY+7S7hjqPyo3D1H5UAJtb0o2tj7tLuHqPyoLe4/KgBNrelG1vSnbh6/pRuHr+lADdrf3aNp9KXcMdvyo3DOcj8qAE2t/do2t/dpdw7EflQGHqPyoATafSja392nbh6/pSbh6j8qAE2n0o2t6Uu4ccj8qCw9ePpQAm1vSja3pTtw9f0o3j1/SgBu0+lG1um2nbhnrSBx6/pQAm1vSja3pTtwzwf0pAw9f0oATa3pRtb0pwYev6Ubh60AN2t/do2t6Uu8Z6/pSlh2P6UAN2t6UbW9KXcPX9KXcM9f0oAbtPpRtb+7Ttw9RSbxnr+lACbG9KNrelOLjHBpNwz1/SgBNrelBU+lO3Ad/0oLDsRQA3a2c7RRsb0pS4zwf0pd4x1oAbtb0o2t6Uu4Z5P6Uu4ev6UAN2n0o2t/dp24djSbxxzQAm1vSja3pTt49aTd6n9KAE2t6UbW9KduHr+lG4etADdrelG1j2pd4x15pd49aAG7W9KNrelLu9/0pdw65/SgBu1vSja3pTtw9f0pNwxyRQAm1v7tFODjuaKAEwc8kdaMH1FLn3FGT7UAJg+oo2n1FLn3FGfcUAJtPqKTb/tD86UnjqKMn1WgA2nOQRRtPqKXPPUUZ9SKAE2nHJFIV9SPzpc9elKSR3FADdpB+8PzpQuPSjJPORRnHcUAGD6ijacdRS59SKM+hFADdp9RRt/2h+dOzzjIpNxPcUAG057UYPqKM89RS59xQAm0+oo2kHqKXPoRRnnqKAG7f8AaH50bT6j86XOeMijJz1FACbSOcijYT0Ipd3uKAx9RQAm30Io299w/Olz6kUbuoyKAE2n1H50BT1BFLn3FBY45IoATaT0Io2+4/Ol3H1FGT6igBCp65FG046j86UtzwRRk+q0AJsPqKNpP8Q/OnbuvI9qTd6EUAJt9xQVPqKXJ9RRu5GCOaAE2nHUfnRsJ6EUuTjqKXPuKAG7Sf4h+dGwjqRShu+RS5PqKAG7T6il2n2o3cDBFGeO1ABtPTijacdqAT6ijPuKADYc9qNp9qAcdxRn1x1oANpHpRtPtS5HTIpAT3xQABT7Umw+1LnnkigHHcUAG0+1G0+1GTz0pcgHqKAE2n2o2nPajJ9vajPPUUAJsPtS7T7UZweooyc5yKADaevFG05zxRnvxRnntQAbD7UFT6ijJ9RQTzkEUAG0jsKNh9qXPPUUmeM8UAG05yMUbT7UE89qM+460ABU+o6UbT7UZ9xQST3FABtJ9KNpPpRnvkdaCfTHvQAFSfSjaT6UE+4oznuKADacdqNpPpRuJHUUZyeooANp6cUbT7UE+mKM8dR0oANp9qAp9qM8dRRnjGRQAbT04ooz2OOlFABsHqaNg9TS5Ht+dJnpwKAF2Ck2D1NAb0Hb1oBB7DrQAFB2o2CjIxwP1oJHoKAF2D1NGwd6Tcue1Ln0H60AJsFGwUufYfnSEgdqADYKXYPU0m4Z6ClB64xQAbAetJsFLntj9aM+g/WgBNg96NgxQSM9BRuHcCgBdg9TSbKUN1xijPt+tACbBQUGe9KT7frSEjPQUAGwUbB6mjIPYUAg9h+dABsHel2CkBXoFFAIP8IoANg96NgoyMDgUbh6CgA2DvRsFAI9BRkf3R1oAXYKTYPejI9BRkAdB1oANgo2CjcOwFGQOw496ADYKNg9TQSBnKijIHG0UAGwUbBRkYPAo3D0FABsFGwUZHoPzoJGeQOlABsHqaNgzRkf3R0oBHoKADYP8mjYKAw9B+dGc84/WgA2Cl2LSZzjjvRwR939aADYKNg96A3PCigHnp39aADYKNgoznnH60ZyMbaAF2LSbBRxk/KPzoBxxtoANg96Ngozz0/WjOTjH60AGwe9KEFJu4xt/WjPP3R+dABsFGwe9AIHG2gnnkdvWgA2Cl2LSE54x+tAOONv60AAQUbBRkcfL29aCQOq0AGwe9GwUE88j9aC3bA/OgBdi0mwUZwcbf1o44+UdfWgA2CjYPejI67aCeOn60AGwepo2CjOOMfrRnH8P60AGwUuxaQkc5Xv60HHXaKADYPel2Ckz8vT9aN2Ow/OgA2CjYKM452/rRnqdvb1oAXYtJsHvRx/d/WjORwKAF2CikBxzj9aKAF2jPT9aNo9P1pMr6CjcBzigBQo9P1o2j0/WkyB2oyOuKAFKj0/WjaOmP1pMjuP1pCV9KAHbR6frRt9v1pNwz0o3AdqAF2j0/WgqOuP1pMjpiglfSgBdoHQfrQFHpSZUdBRkelAC7R6frRtHcfrSbgD0o3KO1AC7QOg/WjaOwpNw7ikyvpQA7aM9P1o2j0/WkyPSjK+nagBdo7j9aNq56Um4dcUbhnkUALgdcc/WjaM9P1puVx0pcj0oAUKP7v60bRnp+tNyMcgUuV9BQAu0elG0dcUmRgcUmV9BQA7aM9P1o2j+739aTI9KQEY6UAO2j0/WjaOuKTK+goyMdKAF2juKCo9P1pMr6CjI9KAF2jn5f1oKjsP1puRzxS5X0FAC7R6UbR0I/WkyOaMrnoKAFIGOn60bRnp+tNyOmKXcM9BQAu0en60bR6frSZXHSjcM5oAXaPSjA7A/nTcrxxS7h6UALtHpRgdcGkyvSgFR2oAUKB2owPf86TK+lJkYx/WgB20en60bR6frSblHQUAgcUALtHpQFHXFICoPQUmVHagB2B7/nRtGen60mRRlR0AoAXaD1H60bR6UmR+dGVznAoAXaM5xRge/wCdJlfTvRkf5NAC4Hofzo2g9f503KjoKXIznFAC7R6UFQe1NyvpSkr6UALge/50YHXB/Ok3DOaTKgdKAHbR/k0bQO1JuGc4pCV/yaAHFR0x+tGB7/nSEr6CjI9P1oAXA9D+dG0en60mV64pMj0oAdtHpQVHTFISp/8A10ZX0FAC4GOn60YHv+dNyMdP1pcrnJH60ALgen60bQO1JkHtRlT/AProAUqPSgADoP1pMrjoKTI6Y/WgB20dOaKTKnqP1ooAXAz36+lGB6fpTc+uetGfQGgBQAex/KjA7g9fSkz6ZoyfQ/nQA44xnH6UhAHY/lSE59aCfr+dADsDpj9KMA9v0pufrRkj1oAXg9j+VBA9D+VJyBjBoLfX86AFwCeh/KjAx0/SkJ+v50Z69aAHYHpRgZxj9Kbk+9GfY0ALgE9/yowPQ/lSE845oJ+v50AOwOeP0oAHpTQfrRn60AOIHTH6UEDPT9Kbn2NBOPWgBcD0P5UYA7Hp6UmeO/50Z9z0oAUAccUAAHofypM/WjPfmgBQBgcGjA9D+VJnAoz9fzoAUAAjj9KABjoaTP1ozx3oAdgA8CkwOeD19KTPfmjP160AKQM9D+VAA9P0pM9+fzoz7nrQAuBycGggdcfpSZ470Zz60ALgZPB/KjAHY/lSZ6mjPpn86AFwMdD+VLgZPBpuevX86M9etAC4GM4P5UYGeR29KTOfWjP8qAFwPQ/lS8Y6fpTQc+v50Z9j+dADsA+v5UYBHT9KQHtg0A/WgBcD1P5UDGOnf0poPYZpc+x60AKMen6UYHT+lNB9j+dAP1oAcAPT9KAB05/KkB5xzSZ9M0AO456/lQMZ6fpSZ69aQHnoaAHcf5FGB0/pTQec80oOD3oAXA9/yo4zj+lNzg8Zpc4PQ9KAFwvp+lHGf/rU3PsfzozznBoAdge/5UED/IpCcetISM8ZoAdxn/61JhfQ/lRnnoaTPsfzoAdxn/61GBn/AOtTc89DSk9+aAFwB/8AqowBjr+VNJ785xS59jQAuF9P0o49P0ppPPQ0E+x/OgBxA68/lQQOv9KQnPrQSDgnNAAQAO/5UpA9P0pueOQaCeehoAdx6fpQQOvP5U3Oex/Olzx3oAUgf5FGBjOT+VNJGOc0ufY0ALgen6UU0njkGigB3HTd3pBg/wAVGWzwe/pRlvegBRj1/Wjj1pMt7/lQC3v+VAC8dd1Icf3qDn3/ACoJb1P5UALxnrQcev60mT7/AJUZbtn8qAF4x1oI96TJx3/KglvU/lQAcf3qOORuoy3v+VALep/KgBePX9aOP7360mW9/wAqMt7/AJUAHGcbqOMfepMtnqfypct6n8qAAYzy1Lxj7360mW9T+VGWx3/KgBePX9aTjON1GW6c/lSZbPU/lQAvH96jjP3qMt6n8qPmz1P5UAKMcfNRxn71IC2O/wCVHzZzz+VABkY+9Rxz81JluuTS5b1P5UAHH96jjH3u9euRfsN/tHzRpKnhK2KsAQf7Wg6H/gdOH7DH7SX/AEKNr/4NoP8A4ugDyI4z1pOMfer17/hhf9pL/oUbb/wbQf8AxdB/YY/aR/6FG1/8G0H/AMXQB5Dxn71HGPvV69/wwx+0l/0KNr/4NoP/AIug/sMftI/9Cjbf+DaD/wCLoA8i45+ag4/vV67/AMMMftJZP/FI2v8A4NoP/i6D+wx+0l/0KNt/4NoP/i6APIeOfmo/4FXrx/YY/aSP/Mo2v/g2g/8Ai6P+GGP2keP+KRtf/BtB/wDF0AeQ8f3qXjP3u1eu/wDDDH7SWP8AkUbX/wAG0H/xdH/DDH7SOf8AkUbb/wAG0H/xdAHkXGPvUnGfvV69/wAMMftJf9Cjbf8Ag2g/+Lo/4YY/aS6f8Ija9P8AoLQf/F0AeQ8f3qX5fX9a9d/4YY/aSx/yKNr/AODaD/4ugfsMftJf9Cja/wDg2g/+LoA8i47NRxjOa9dH7DH7SX/QoWv/AINoP/i6P+GGP2ksf8iha/8Ag2g/+LoA8iyv96jK+v6167/wwx+0l/0KFr/4NYP/AIuj/hhj9pL/AKFG2/8ABtB/8XQB5Fx6/rRkHo1euj9hj9pL/oUbX/wbQf8AxdA/YY/aS/6FC1/8G0H/AMXQB5Fx60mR/er14fsMftJf9Cha/wDg1g/+LoH7DH7SX/QoWv8A4NYP/i6APIsqeh/Wj5fX9a9Q8Sfsc/H3wl4evvFGueF7eKz0+1e4upF1OFisaKWY4DZPAPAry4bs9T+VAC8djQMZxmky3v8AlRls9f0oAMr13UuV/vfrSZb1PX0o+bnk/lQAvy+v60ZX1pOc9T+VBLZ6/pQAvH96jI/vUmW9e3pQS3qfyoAXK/3v1o49f1pOc9T+VHzdyfyoAX5c4z+tHH96kJbPU/lRluOf0oAXjP3v1oyv979aTLep6elGW45P5UAL8vr+tHy/3v1pPm9T+VBLDufyoAU4/vUHA7/rSZb17+lGW9f0oAXK/wB79aPl9f1pCWx1P5UHdngnr6UALlR3/Wg4/vUnzY4J/KjLEdf0oAU7fX9aMr3akJb1/SjLY6n8qAF+X1/WikO7sT+VFADs+9JnJ4xSbT/d/Wk2t/d/WgB4IPSkyfam7W9KXa3p+tADs0hI9qTa3939aTa3pQA/vijNNw2cgfrSFW9P1oAdnjNBIHJpNrYxj9aCpPb9aAFzz2oB603a2ckfrS4b0/WgB2aOM0za3pS7W9P1oAXOOtGR7U3afT9aNrZ5FADgeTS5pu0+n60Yb0oAdxmkzg4pu1sdKXax6j9aAF3DGeKM8/hTdrelKVPYfrQAoP0ozxTdrY6frRtb0/WgB2enNGR7U3a2MY/Wgq3pQB+pmnn/AECD/riv8hUwPfioLBSbCDA/5Yr39hUu1sYx+tADwaTdxnPem7W9P1o2tjGP1oAdkUbvfvTdrdh+tLtOOn60ALnr0pQfpTNrY6frRtb0oAdnryKMjNN2t6frRtbsP1oAdnjjFGeeKQKQOn60m0+n60AOyPajPOKbtb0o2sD0/WgB2RxS8+gpm1vT9aUKf7v60AOH4UnPtSAMP4f1o2t/doAdn6fnRTdren60bT/d/WgB3PoKTn2pApz939aAGH8P60AO79qMn2/Om4b+7+tG1vT9aAOR/aC/5IX4w/7Fu8/9EtX5vc+1fo/+0Ep/4UX4wyv/ADLd53/6YtX5v7T/AHf1oAXnPOKOfakw2fu/rRhs52/rQA7JoFNw3Zf1pNp/u/rQA7nsB+dHOe1N2n+7+tLg5+7+tAC5+lLk00hj/D+tGG/u/rQA7vSc9sU3ac8r+tG1uy/rQA7n0FLk9qbg54X9aMN/d/WgBc/Sl59Kbhv7v60m1v7v60APOe1Ic+gpNp7L+tG04xt/WgBxJ7UfSmkMf4f1ow3939aAF59B+dLz2pm1v7tLtP8Ad/WgBeccgUuTTdpx939aMNjG39aAF59qPypNrf3f1pNrf3f1oAfzRTdp/u/rRQAuRnv1oz9aTa2c5o2t60ALn3P5UZ9z+VJtb+9Rtb+9QAZ9zQT9aNpx1o2nu1ACk/X8qM/X8qTac43UbW7mgBc/X8qCeO/5Um0/3u9GDj71AC59z+VAPHekCn+9RtbGM0ALn60Z+tJtb1o2n1oAXPPekzx1NG1v73agKcfeoAUHr1ozj1pNrf3qNrY60ALnHPNBPPekKn+9RtbPLUAGfc0uee9IFJ/io2tnhqAANx3ozzjJoCtjhv1o2nP3qADPTGaXP1/KkCkfxUbTn71AH6l6ef8AQYOv+pX+QqUHtz161Dp6n7DAQ3/LFf5Cptrf3v1oAM896M8dTQVbP3qNp5O6gBc89/ypM/WgqSfvUbT2agAz160E89TRtOT81BVu7UAGfc0Z6daNrZzuo2njDUABP1pc4Pek2H+9RtbP3qADPGcmjPuaNpxy1G1s/eoAM8d6XPsaTaePmo2H+9QAueO9APfmkCsOjUBT/eoAUH2NAOexpNrd2o2nu1AC59j+VGfrSbT/AHqArDo1ACjr3oB9jSbWzndRtb+9QByH7QZ/4sX4w4P/ACLd5/6Javzfzz0NfpB+0Ep/4UX4wy3/ADLd5/6Javze2t/eoAUH1zRnJ70mwjoaNrZzuoAUH2NGeehpNrf3qNrf3qAFz7H8qM89/wAqTYf71Gw5zuoAUnnHNGcHoaTax6tRtb+9QAueehozjsaTa2fvUbD/AHqAFzz0P5UE/Wk2HOd1G1j1agBc47E0Z9jSbW7NRtb+9QApP1oz7Gk2n+9RsPrQApPoDRnHqaQqxGC1G1v71AC546GjP1pNrf3qNp/vUALnjofyoJ46Gk2H1o2t03UALnjvRnI4BpNrY+9Rtb+9QAue+DRSbT/eooAN3PagMO+KULz2/Kk2n1/SgABPqKN3HbrRs9/0o2n1/SgA3H2oLZ9KNnv+lGz3oAM89qM89RRs/wA4o2n/ACKAAt64oLcHkUbT/kUFP84oACx9RQGPbFGz3oCHv/KgAz7ilyfak2n/ACKNp7/yoAN3PajdxjijYc9f0o2e9ABu57UZ9xQEOef5UbT/AJFAC5PtSbue1G0/5FBQ56/pQAbuMcUBue1Gz3o2GgA3cdqN2D2oCHH/ANajYf8AIoAA3TpRuIz0o2HFGz3oA/UrT2xYQdP9Sv8AIVNu+lQ6ehNhB/1xX+QqYKcUAG76Ubvp1oKH/Io2HFABu57UbuO1Gz3o2GgD5m/4KI+MvFvhSfwiPC3inUdNE63/AJ32C+kh8zb9n27thGcZOM9Mmvmr/hcPxZ/6Kj4h/wDB3cf/ABdfQf8AwUuU+f4MH+xqH87avlrYf8igDov+Fw/FrJx8UfEP/g7uP/i6+o/+Ceni3xT4q8P+JpvE/iW/1Joby2ELX95JMUBR8hd5OM4HT0r462Gvrb/gmsn/ABTfisZ/5fbX/wBAkoA+mt30o3c9qNhoCHNABu47UbueMdKNhx/9ajYc0AG7p0r8/Pjp8U/ibpvxn8V6fp3xF12CCDxDeJDBBq8yJGomYBVUNgADsK/QMIe9fm9+0Cn/ABfPxhz/AMzLe9v+mz0AUv8AhcHxa/6Kj4h/8Hdx/wDF0f8AC4PizjP/AAtDxF/4O7j/AOLrndn+cUbTj/61AH6YfCS7ub34VeGb29uXmmm8PWTzTSuWeRzAhLMTySTySa6DPcAda5v4NL/xaDwof+pasf8A0nSuk2e/f0oAAfpQDx2o2e/6UBOKAANyTxRuzzxRtPr+lAX1FAHIftBkH4F+MOB/yLd5/wCiWr83gR7V+kP7QS/8WL8Yf9i3edv+mLV+b2znOf0oAM/SgHJ7UbKNn+cUAG7Pp1oyPQdKNvr6+lGz/OKADPrijPPajZ7/AKUbMH/61ABnnHFBbOeRRs7f0o2n/IoAMg8YFGfXFLt57flSeX70AGee1Geg4o2c8/yo2n/IoAC3bjp3oyOnBo2dv6Ubf84oAM/Sgn6UFPf9KNnv+lAATjjilLc4496Qr6evpRt5/wDrUAGRjtRnHpRt4/8ArUFM9/0oACfTFG7HpRs9/wBKCnp/KgA3Yx0oyB6UbT/kUbeP/rUABPsKKNnv+lFAAGGehoz6CjFGDQAbsdu1G7PajafSlwaAEznsaAefumjHHSjHtQAFvagt/smlwaTFAAWHQCgt/smlwaQg0AGefumgN14P50Y9qMe1ABu44WjI9DRj2oIoAM8/dNGf9k+3NGPajHtQABueFP50buOFox7UY9qADcOuDRn/AGaMcUAe1ABn/ZP50bufun86Me1AHt+lAAG/2aN3P3T+dGPajHbFACZ/2aUn/ZP50Ae36UY9qAP1K09sWMHH/LFf5Cpg3bbUOnj/AECDj/livb2FTY9qAAt3waM/7NGO2KMe36UABP8AsmjcP7v60Y9qMe36f/WoA+Y/+CivhfxP4ln8Inw54bv9QEK3/nfYrN5dmfs+N20HGcHGfQ180H4YfEzt8Ode/wDBRP8A/E1+mePajHtQB+Zn/Cr/AImf9E417/wUT/8AxNfU3/BPDw34j8N+HvE8fiHw9fWDS3tsYlvbV4i4CPkjcBmvo3HtRj2oATIx92l3c5C0Y9v0/wDrUY9qADcMfdP50buc7aMcZxRjHagAB/2fyr89/jt8O/iDf/GnxZfWHgPWp4ZvEN48U0OmTMjqZmIIIXBBHcV+hGPalwaAPzLHww+Jn/ROde/8FE//AMTQPhf8TO/w517/AMFE/wD8TX6abaNp9KAOd+EdvcWXwo8MWd5byRTReHrJJYpFKsjCBAVIPIIPUV0Ib/ZPWjB9KMGgAB74NAb/AGTS4NG2gBARnoaN3saXafSkwR2oA4/9oI/8WL8YfL/zLd5/6JavzfDA8gHrX6Q/tBDHwL8Ycf8AMt3n/olq/N/BoAQN32n86M89DRijafSgAyPQ0bv9k9KNp9KMH0oANw9DRnn7ppcGkwc0AG72NBPPQ0bT6UbfagA3c8CjcDxg0YNLg0AJnn7po3exowc0bT6UAGfY0bvY0bfajB60AG4Hgg0E/wCyaXBpMH0oAC2eMGjcPQ0pU+lG32oAQntg0bueQetGD6UuDQAhOOxo3cYwaMH0pdpoATIx0oLcdDRtPpRg+lABux/CaKMGigAD+p/Sjd3zS455I60BT2IoAQNn+L9KN3+1+lLtPrRjvkUAJu/2v0pC3OQ36U4gkdRSEY5yKAAtz96jd/tUuDnrRgnvQAhb/a/Sgt/tfpS4OOTQRxkmgBN3IO79KQHA+9S7e+RS4PqKAE3f7X6UFu279KXBPcUYPrQAm4ZPPak3cD5qdjnqOlJjjORQAmeT81Lu/wBr9KXB55FGD6igBC3+1RuGQd1Lg+tGOc5FADd3GN3ejPOd3alxkdRS4Oeo6UANB6c0bsH7xNKB7il98igBobgZPSgt15pQMc8UYyTyKAP1K08/6DB83/LFf5VLnjqetR6eD9gg5H+pX+QqbHuKAG7uc7v0ozx1707HPUUmBjORQAbuTzSZ4A3U7GT1FJjjGRQAhPXmlJ77j+VLjryKMdsigBu7rzS7uRz2oxnJyKMZI5FACZ4xk0E8/e7Uu3jGRS4560ANzx978MUbuetOx2yKTGT1FAAG4HNG7/a/SjHA5FLg9CaAE3f7X6Ubu5P6UuD60YPTIoATf7/pQG9T+lLg56ijBAxmgBA3+1+lG7/a/SlAPc0bT60AIGz1P6Ub/f8ASlwQeoowQeo9qAOO/aCbPwL8Yc/8y3edv+mLV+b+7nr39K/SH9oIEfAvxh/2Ld5/6JavzfwQeTQAm7/a/Sjdz1/Sl2n1owRzkUAJv9/0o3c9f0pSD6jrQAc5zQAm7nr+lG7/AGv0pcHuaMH1oATd7/pQX9D+lLg9c0EH1FACbhnr+lBb37elLg5zmgg9zQAhb/a/Sjd7/pS4PUGjaaAE3e/6UFvQ/pSkEjORRg+1ACFh6/pRu/2v0pSCe9GCeQaAE3e/6UbsfxfpSlSaME96AE38df0o3D+9+lG0kdqUhvWgBN3+1+lG7Hf9KXHoaNp9aAE3D+9+lG/jr+lLg9MikwfUUAG7j736UUpB7GigBMt3Wj5uy0bRnpRjpQADOPuUc9Ngpdo9KNo9KAEyf7v6UnP9z9KcVHpSED0oACT/AHKMn+5S7RnpRtHpQAnzf3R+VHOPu/pS7R6UhXjpQAnPXZ+lKN2PuijaOmKNoweKADJ/uUc/3BS7R6UYHpQAnOeF7UnP939KXaM8CgAY5FAAC390UfN/doCjml2gjpQAnP8Ac/SjnP3aUgelG0Z6UAN5/u/pSgnP3P0owMcijaAeR2oABu/u/pR82fuCjAwDilCjPSgBvPHy96Oem39KUKOOKNvtQB+pWnk/YYPl/wCWK9vYVN83Zf0qLT1H2GAn/niv8hUuARwO9AAc5+4PyoOeflpdo9KTaPTvQAEnPKUnPTaPypSvPSjaOvvQAHOSdv6UHd/dowDkAUuAT0oAQ5/u0cj+H9KNo54o289KAE5H8P6Upzn7lG0YpcDOMUAJ82Pu0c/3KNoPGKNoJ6dqAE5x939KXJ/ufpRt6cUu0elACcn+Cj5sfd/Sl2ijaP8AIoAQbuu39KOf7nel2j0o2j0oATJ/ufpRyf4P0pQo9KNo9KAEG7+7+lA3ddv6Uu0f5FG0elAHHftBZ/4UX4w+X/mW7z/0S1fm/k/3P0r9If2glH/Ci/GHH/Mt3n/olq/N/aPSgBMn+5+lA3Z+7+lLtHpRtGf/AK1ACDd/d/Sjn+52pdoz0o2j0oATJ/ufpRkk/c/Sl2j0o2j0oAT5s/dH5UfMTwv6Uu0UFR6UAJk5+5QM9Nv6Uu0elGB6UAJk9k/Sj5v7v6UpUelG0dqAEO4n7v6UZb+7SlR6UbR6UAJkj+H9KCT/AHP0pdo9KNox0oAQ7v7v6UHd02/pS7RRtHpQAnzYwV/SjJz939KXaMdKNo9KAEyf7n6UfN12/pS7R6UbRQAh3Y+7+lHzDgrS7Rj/AOtRtGOlACHP9zt6UUu0elFADe/Xv60vHc/rSjGfvfrQcD+KgBP89aP89aOP7360ZH96gAPIxn9aTn1/Wncf3v1pOP71ABxnqfzo+p/Wl4B5NJxnG6gBPx/Wg89D+ZpeP7360HGM7qAE59f1o5xyf1peP7360ZH96gA/H9aPx/Wg46bqXj+9+tADTn1/Wjn1/Wl4/vUZH96gBOe5/Wl7Y/rS5H979aQ4H8VACHp1/Wg5znP60vH979aOM/e/WgBAO2f1o59f1pQR/eoyP71ACfj+tHfr+tLx/e60cZ+9QAg+v60c+v60uRwd1GR/eoA/UrT8/YYOf+WK9/apee5/Wo9PI+wQfN/yxX+QqXj+9QAn4/rR9D+tLx/eoyOu6gBOc9f1o59f1pcjP3qMj+9QAnPr+tHfr+tLx/erE8a/EnwD8ObP7f448X2OmRlSUW6nAeT/AHE+8/4A0AbX4/rRznOf1r588b/8FE/hforPb+CfDWo63Ipws0pFrC30LBn/ADQV5b4m/wCChvxn1Zmj8PaZo+kxn7jJbtNIPqzttP8A3zQB9q844P60c56/rX56ax+1h+0PrbFrz4rajHntZhLcD/v0q1gXfxk+Lt+xa9+KniOX1363OR/6HQB+lX4/rR36/rX5mD4mfEYHevxC1vPr/a03/wAVVuy+Nfxi09gbL4r+I48c4XW58flvxQB+lGD6/rRz6/rX586L+15+0XoTKbX4oXcyjqt7BFOD9TIpP613Xhf/AIKK/FbTGVPFPhnR9ViH3miV7eVv+BAsv/jlAH2aD7/rSDpjNeE+Bf8AgoL8HPEbJbeLbDUdAmbhpJo/tEAP+/H835oBXsvhbxl4T8bacNX8IeJbLUrY4/e2VysgU+hweD7HmgDSB9/1oyfXvS8dd360cf3v1oAaM9c9vWlB9T+tLx/e/WgYPf8AWgBB7n9aQemf1p3GetHHUN+tAHHftBE/8KL8Yc/8y3ed/wDpi1fm9nnOe/rX6RftBY/4UX4wwf8AmW7zv/0xavzf47N+tADQfU/rSjrnP60owf4v1o4z1oAb0/P1pec9f5UvHr+tHH979aAG8nqf1oyc9f1p3H979aOCcBv1oAb3zn9aD1JH86ccdjRwehoATJz1/Wk56Z/Wncf3v1oGP7360ANyc9f1o7//AF6dxn7360HHrQA0+ue3rS59/wBaXj1o46bufrQA3J9f1oyR3/WnfL/e/WjgdW/WgBp57/rS9wc/rSkD1o46ZoAbyR1pcnPX9aXju360cZ5b9aAG8jof1oPTg/rTjgdW/WjA9f1oATt1/WjJ9f1peB1ajgdW/WgBpJx17UU7ju360UAJznr+lHP979KMD+73oGP7ooAPm9f0o5/vd/SlAGMhaTA/u0ABz/e/Sjn+8fyo4H8Io4/uj86AF5z979KMMP4v0owM/dowP7tACc4+8fypSD1LUnGD8tKQOyigBBn+9+lHPr+lHGfuj86BjB+UUALg5+9+lJz1yfyoAH90UYH90fnQAc5+929KBn+9+lHGfujp60cf3R+dABz6/pRg9mP5UADJ+WgAf3RQAEH+9+lBzkfN+lBH+yKOM/d/WgA5/v8A6Uc56/pQcY+6PzowCfu9qADnA5/Sgbs9f0o4xnb+tH/AR09aADnH3u/pRz2b9KOOPlH50cf3RQB+pen5+wQc/wDLFe3sKl5x97v6VFp+DYQfL/yxX+QqXAx939aAD5s5z+lBzg/N+lHU/dH502WWKCFp5mVERSzuzABQOSSe1ADuc/e/SuX+KHxm+Hfwf0oap468SRWzOCbezT555z6JGOT6Z4A7kV4l+0L+3jpugvP4S+ChhvbxcpNrsihoIj0PlKeJT/tH5eOA2ePlHxD4j17xbq82v+JtWuL+9uH3TXN1MXdj9T29B0HagD3T4vft+/ELxa02lfDO0Hh+wOV+1NiS7kX1z92PPooJHZq8G1bVtX13UJNW1vVLi8upm3S3N1M0kjn1LNkmq+Bz8v60AZ4CfrQAHPPzfpRzx8/6V6H8PP2WPjh8SlS60TwPPa2kmCL7VD9niwf4hv8Amce6g17L4P8A+CbS7En+IHxIw3G+20e04/CWT/4igD5X5x979KOc9e3pX3ZoX7CH7O2joBe+H7/U2X+O/wBUkGfqItg/Sumsv2Wv2e9PAWD4TaU2OnnxmU/+Pk0AfnZ82Ov6UvOfvfpX6OH9nT4Dldv/AAqDw99Rpkf88VR1D9lH9nfU1KXPwo05cjn7O0kR/wDHGFAH54jOB836UfN6/pX3F4g/YB+AWsIx0uDVtKY52mz1HeB+EwckfjXm3jP/AIJu+JLRHuPAPxAtLwdVttUt2gbHpvQuCfwUUAfM3P8Ae/StDw34p8TeDtUTWvCuv3enXcf3bizmaNsehKnkex4NdF8Qf2fvi/8AC/fN4x8B3sFsh5voVE1vj1MiEqPoSD7VxuB/doA+lfg//wAFCvEelNFo/wAYdI/tK34X+1rCNY50Hq8fCP8AhtP1NfUPgT4jeCviboi+IPA3iS21C2bAcwN80ZP8LqcMjexANfmTgZ+7Wx4H8e+MPhxrsfiTwRr1xp93GceZA/Dr/ddTw6+xBFAH6cfN6/pRz/e/SvB/2d/22vC3xKeDwn8Rkg0bXHwkM+7FreN0AUk/u3P90nBPQ5OK94AH939aAD5v736Uc/3j+VGAT9yjAP8AD+tAHIftBZ/4UX4w5/5lu87f9MWr83uf736V+kH7QQH/AAovxh8v/Mt3n/olq/N/avZf1oAOf736Uc9m/SjAz939aMDONtABz/eP5UZP979KMD+7+tGB02/rQAc/3v0o+b1/Sjav939aCBn7tABz2b9KOf736UED+5QQOfl/WgA56Z/Sjn+9+lGADjbRtX+7+tAC85+9+lJz/e/Sggf3aMD+7QAc/wB79KOR3/SjA/u0YA/h/WgA5/vfpRz6/pRtX+7+tBA/u0AHP94/lR83c/pRgY+7S4H92gBOe5/SjnP3v0owMfd/Wjavdf1oAOfX9KOf7x/KjA/u0YAH3aAD5v736Uc92/SjA/u0EDqV/WgA5/vfpRRgd1/WigAxk0YFG4E9e9AYepoAMDHSjC9h+tG4epo3DPU0AGB3pMCl3D1oLjpk0AGB3o4/yaNw9TRuGeTQAYX/ACaCB3oLD1NBYdMmgBMClwBRvHrRuHqaADgf/ro4zRuHel3D1oATAzSYFLuHPJ6UbwO5oAMAGjjr/WjcM9TRuHvQAYHajAzml3D1pNwz940AJgUYFLuAHU0bhnqaAEwOKMD1pdwx1o3DPU0AJgUYFLuBxyaNwHc0AfqVp4H2CA/9MV/kKlwMVHp7D7BByf8AUr/IVLuGOtAFDxP4m8P+DdCufE3ifVYbKwtIy9xcTthVH8yT0AHJJAHJr4m/aW/a98SfGK4m8K+EXm0zw0rbTEG2y32P4pSDwvpGOPXJxh37b3xY8X+K/i5qHgC/1Ax6Roc6paWUOQrOY1Yyv/ef5iAew4HUk+K7h3J60ABAoVC5CopJJwABnNdJ8MPhT43+L/iRfDPgfSHuJeDcTv8ALDbJ/fkfoo/U9ACeK+0vgL+yL8PPg1FDrWoxJrOvqAW1G5i+S3b0hQ8L/vHLH1AOKAPnb4MfsN/Ez4jpFrXi/PhzSnwwa6iJupV/2Yjjbn1cj1ANfUXws/Zi+DnwkWO48O+F47m/TB/tTUgJp8+qk8J/wALXoO4etBYetACYFKQKCw5GTRuGepoATAowKXcCOpo3DPWgBMCjApd/HWjcOmTQAYGKMCjcOOTRuHqaAEZVZSrAEEcg9DXlnxU/Y8+C3xQWW8/sEaNqL5Iv9HVYst6vH9x+epwGPqK9U3j3pdw9aAPgT40/sf8AxT+Dyy6vHaDWtGjyTqWnRkmJfWWPlk+oyo/vV5TgDrX6oFlIweQe2K8J+P8A+xH4L+I6T+JPh0IND1s5d4VTbaXbf7Sgfu2P95RjrkEnNAHxKAM8Cvoj9mH9tXU/BbW3gP4t3st5o2RHaaq+Xmsh0Cv3kjH/AH0vbIwB4b408FeKfh54gn8L+MtFnsb63OHhmX7w7MpHDKezAkGskOPU0AfqVYX1jqllFqWm3cVxb3EYkgnhcMkiEZDAg4II5zUwAr5D/wCCfHxY8X/8JlN8Jrq/NxozWEt1bwzZJtpFZc+WeytuOV6Z5GCTn693jrmgDjv2ggP+FF+MP+xbvP8A0S1fm/xmv0h/aCYf8KL8Yc/8y3ef+iWr83t49TQAYGKTAzS7x6ml3jPWgBoAzS4H+TS7x1zRvHrQAmBRgc0bx6mjePU0AJgUEDNO3jPWjcPWgBMDP/16Plpd49aTePU0AGBmkwKXePejePWgBMClwOKXcM9aTePWgA+WjAo3j1NBcepoAQgUEClLj1pdw9aAEwMYoOPT9aN49aXevrQAmB1oIHejePU0bxjrQAmBilwMUu8Ubh0z+NACYH+TRS7x60UAIB6k0dO9Lk56ik3H1FABgdiaMduaUHPcUgY+ooAPxNGMHqaXd7ik3HPUUAHfOTRgeppdx9RRk+o/OgBCPc0EehNLuPqKQsfUUABGDwTRjjqaNx9RRuPqKADHqTRj3NLk+oo3e4oATHPU0Y7gmjcc9RRuPYigAA68mjGepNAY+opcnHUUAJj3NBHPU0u7HcUm7ngigAxxnJzRjJ5Jo3H1FG4+ooAMAgZJoAHbNG4kDkUbj6igAxwKMcck0bj6ijccdRQB+pWnjNhByf8AUr/IVNgYxk9ai09j9ggGR/qV/kKl3HHUUAfnr+1qP+MjPFXX/j+T/wBEpUf7Pv7O3i349eIja6dus9ItZB/aWrSJlYh12IP45COg7dTgV6F4y+A3iL48/tfeKdFsWa20y1v45NW1LbkQRmJMKvYu2CFH1J4Br628EeCvDPw68MWvhDwhpkdpY2ibYo06k92Y9WYnkk8k0AVfhr8MfBvwm8MReE/BWlLbW8eDI55knfHMkjYyzH8h0AAAFb+Pel3H1FG4+ooAMDBGTRgA8Zo3HnkUbj/eFABjr1oxnuaNx55FG4+ooATHbNKB7mjccdRRuOeooAMD3oxznmjcf7wo3HPUUAAHuaMY70BjxkilLccEUAJgdMmjA6ZNKG9SKTccdRQADrjJoA7ZP4Uu73FG4+ooA4z4z/A7wR8b/DTaF4qs9lxEpNhqUKjzrVz3U91PdTwfqAR8G/GL4M+Mfgl4rfwv4stiUbLWN9ED5V3Hn7yH19VPIP4E/pLu9xXL/Fr4T+EvjN4On8H+LrUMjjda3SAeZay44kQ9iO46EZB4NAHyN/wT8A/4X8eT/wAgO5/9Cjr7gwAcZNfI37K/wq8U/Bv9rK58GeKoBvi0S5e1uUB8u5iLJtkQ+hxyOxBB5FfXO7jqKAOP/aCH/Fi/GHJ/5Fu9/wDRLV+b+Oepr9If2gj/AMWL8Ycj/kW7z/0S1fm/u9xQAnbhjQQM9TQGPcijdz1GKADAHc0d+ppQ3qRSbjjqOlAAV9Cfaj1wTS7vcUm456igAIHXJoIA5yaC3oRS7uvIoAQ9eCaCo9TS7vcUBvUigBB15JowO5NG456igt0wRQAYGMgmg9uTS7vcUbvcUAIQOuTRj1Jpd3uKQsR3FABgHqTRgepoLccEUu7nqKAE7feNGM9zRu9xS7ueooATHbNGAe5oLHsRRu46igAwPU0djyaN3HUUu70IoAQjI6mijcfUUUALt56Ck2/7Io3ZP3e9GRjpQAYP90flS7f9n9KTcO4NG4dwaADH+yKCvsKMjsDSEg9V/WgB232H5Um3/Z/Sgkdlo3Drg0ALj2H5UbfYflSbh6UEjGMGgA2+w/KjH+yPypM88r+tKGAydtABj/ZpdvsPypMj0NG7vjvQAbfYflRtz2H5Um4f3e3rRn1X9aAFx/sj8qMf7NAIznb+tG4elAC7fYflRt56D8qTcPSjdz92gA29sD8qNv8As9qTP+z+tGR/d/WgBdvsKNvPQUgOcDFLuGeAfzoANp/uijHsPypMjHC96N3+z+tAH6maeP8AQIOP+WK/yFS7TjoKh08j7DBx/wAsl/kKlznjHegCCy0fS9MmuZ9O02CB7yfzrt4ogpmk2hd7EfeOFAyewFWNvsKCw9P1pCRz8tADsew/Kk2+wpN3X5f1oyP7v60ALt9h1o24HQUhbrxSkgdv1oANvsKXHsPypuR6dqN3+z29aAF2+wo2+wpMj+7+tLuwenagA298CjafQdKMgDp+tAYdcUALj2H5UbfYflTQeny/rS7vY/nQAu3PYflSbfb9KN3oDQGz2P50ALt9h+VGPYflSBsnoaA3oP1oAXb7D8qAvsPypA3sfzoDD0NAFebR9LuNTg1mfTYHu7aN47e5aIGSNHxuVW6gHauR3wKsbfYflQGyeh/OgNzjB/OgDkP2gh/xYvxhwP8AkW73t/0xavzf257D8q/R/wDaCb/ixfjDj/mW73v/ANMWr8393PI/WgBdvsPyo289B+VJuHoeKM84wfzoANvsPypcc9B+VJu5xg0bueh/OgA29sfpS7fYflSbvUfrQWGcEGgBdvsPypNvsPyo3c9D+dBbHGD+dAC49h+VJj2H5UFueh/Ojd6igBdvsPyo247D8qTdzjB/Ogt7GgA2+w/KjHsPyoLY6g/nRu6cGgBcew/Kk247fpRu9v1oLcdD+dAClfYflRt9h+VIW9AaC2Ox/OgA24HQflS49h+VIW4zg0bvb9aADb7fpS7fYflSbhjOD+dG7jgGgAx7D8qNvsPyo3YGcH86N3GcGgBcew/Kik3e360UAG32ox7Ub6N3Q/1oAMGjb7UF6A3vQAY9qMe1Ju96N3uaAHYNJgmjd70FqADBoK4HSjd70Fv8g0AGPb9KMe1Jv+tG+gBcGgg+lG7A4P60bsDigAxz0ox7Um76/nRv+tAC456fpRjik30u7jr+tAAQcdKMe1G7ik3c5/rQAuPajHoP0pN31/OlDc0AGPajHtRv/wA5o30AGPb9KMe1Ju/zmjd9fzoA/UvTx/oEGB/yxXt7Cpse1Q6e3+gwf9cV/kKm3/5zQAY9qMe36Ub+elJuz6/nQAuPajHt+lJu+v50bqAFwfSjHtQX6/40b6ADHtRj2pC+aN31/OgBce36f/WowT2pN31pd/8AnNABj2oxz0o3/wCc0b6ADHtS7T6Cm7sev50u7/OaAF2n0pMH0o3ZH/16N3vQAbT6Uu32pN3vRuyP/r0ALtPoKNp9KTf7frRu4/8Ar0ALtPpSbfajdjvRu96AOQ/aCX/ixfjD/sW7z/0S1fm9tPoK/SD9oJv+LF+MP+xbvP8A0S1fm/u5/wDr0AG0+lG0+lAbP/66A3vQAbfal2+1IG96N/8AKgA2n0FG0+lG/wDzmgPz/wDXoANp9KNvtSbsdz+dLu560ALt9qTafQUb+aN3H/16ADafSjafSjfz/wDXo3c9f1oANp9KNvtRu96N3vQAbT6CjafSjd/nNBf/ADmgA2n0o2n0oLc9fyNG73oANp9KXb7Uhb3o3c//AF6ADafSjafSjf8A5zQW9/1oANp9KNp9KN/vRu96AF2+1FJv7/1ooAXHqaMc9aTnP3O9HPZaADB65FGPcUDd/do5/ufpQAEdckUEe4oJb+7+lHJ/g/SgBcc9aD7kUnJP3KPmzwgoAXtjIpCPU0HP9wUEt/doAXb70gB7EUZb+5+lHzY+7+lAC49xRg+opPm7JRz/AHBQAuOTyKQAY6ij5s/dH5UnP9z9KAFx7il/EUnzf3f0o5/uUAGOOopSOetIc/3BR8wP3R+VAAAMdRS4IPUUnPTZ+lALZ+7QApByOaMHOM0gyMfLRyT9z9KADHTJFGOuCKBuGPl/Sk5/ufpQB9W2/wDwUn0+CCOD/hUEx2IFz/bg5wMf88aef+Clmn/9Efm/8Ho/+M18oAtn7tAz/d70AfV//Dyyw6f8Kfm/8Ho/+M0f8PLNP7/B+b/wej/4zXyhkk/c/Sk+bH3f0oA+r/8Ah5Zp+f8Akj83/g9H/wAZpf8Ah5Zp/wD0R6b/AMHo/wDjNfKB3Z+5+lHzf3f0oA+rz/wUs0/HPwfm/wDB6P8A4zR/w8ssP+iPzf8Ag9H/AMZr5QOefloJYn7lAH1f/wAPLNP7/B6b/wAHg/8AjNJ/w8s0/P8AyR+b/wAHo/8AjNfKPzc/KPyo+b+7+lAH1d/w8s0/H/JHpv8Awej/AOM0v/Dyyw/6I/N/4PR/8Zr5Q5/u/pRznO3tQB9X/wDDyzTwP+SPzf8Ag9H/AMZo/wCHllh/0R+b/wAHo/8AjNfKBLYxto+bP3BQB9Xf8PLNP4/4s9N7f8T0f/GaUf8ABSzT88fB6b/wej/4zXyh82Pu/pRk/wB39KAPq8f8FLNPxx8H5v8Awej/AOM0f8PLLA/80fm/8Ho/+M18oc/3KOf7goA+rx/wUssO3wfm/wDB6P8A4zR/w8t0/wD6I/N/4PR/8Zr5Q5H8P6Uc9NvegD6vH/BSzTwf+SPTf+D0f/GaB/wUs0/t8H5v/B6P/jNfKHzdNtHP9z9KAPq//h5ZYf8ARH5v/B6P/jNA/wCCllh2+D83/g9H/wAZr5Q5/uCjn+5+lAH0t8RP+CgVj468B6z4LT4Wy2rarpk1oLg6yHERkQru2+UM4znGRXzTg57Uc/3f0o+Yfw0ALg56ijBz1FJz/c/SjkH7lAC4Oeooxz1pOf7go5/u9vSgAIPcilwc9RScj+Gjn+5+lAC4PTIoIOeopOf7goOf7goAUg560hBxkkUc5+7Rzj7vagBSDnqKMH1FJz3T9KOT/AKAFIPqKCD3NJyf4KOf7v6UAGD6ijHA5FHPXbR83939KAFIPqKCD60nP9yjnP3BQApBxyaMHpkUnzY+7+lHJP3aAADK9RS4PXNJkn+H9KOf7lAC4OOoowcZJpOeyCjnsv6UAGD0yOlFHJ/h7UUALhfT9KTAH8J/KjIB/wDr0Ar/AJNAC4X+7+lGFPb9KTI6f1pcr/k0AJgY+7Rgf3T+VGR/k0ZXP/16AFIGfu0AD0/SkyP8mjI/yaADC4Py0pAx0/Skyvr+tBI6n+dAAAP7poAXpg0ZH+TSgqB/9egA+X0/SjAz92kyuf8A69GR0/rQAYAP3f0oAH900EjOf60ZXH/16AFAXPT9KPl6Y/SgEf5NJlf8mgBSB/doIGelJkf5NBI/yaAAAf3T+VGBnofyoyuKMj/JoAMDA4P5UuFz0/Skyv8Ak0ZUHj+dABgcfKfyowM/dNHy4H+NGR/k0AHy5HH6UfKByO9fePxI8PeAfiLat+z5faXZ2eo6t4UTUNGvVhVSJ42I6gZ4IRsDkqX9K+Ik0e/0Dxkvh/WbNobqz1IQXUEnVHWTayn6EGgDLwvp+lB246H8q9x/4KBWVhp/xytYLCzigQ+Hrc7IUCjPmzc4FdJ4T8MfDf8AZS+DGmfFnx74Tt9c8X+IFEmj6feAFLZCoYHBB27VKlmxuywUY5NAHza9vKiCZ7dwjdGK4B/GmfKRnafyr3q1/wCCg/xSmvvL8SeDvDt/pjnE+nC0dAU/uhi7c49Qw9qsftAfDX4fXfgjRP2p/gppEdtplxdxnVdI2Yjgl34B2jhAHUoyjg5UrwSaAPn3A54oIX0P5V73/wAN8+Kv+iR+Ev8AwEk/+Lr1L9p/9oK5+BmpaFZeH/h34fvF1XTTcTG8tDlGDAYG0jjnvQB8ZEDn5T+VGBx8pr0H43ftBar8b7fTrfU/B+j6UNOeVkOlwshk3hQd2WOcbePqa8+yv+TQAfL0wfyowM9D09K+m/2UPEFv4I/Zd8aePo9Asb+60vVGlgjvoAyt+5h4PfHJ6Guf/wCG+fFX/RI/CX/gJJ/8XQB4JhcYx+lKqGRgiIST0AWvTZb7xB+158b9K05NEsNImvY0tpRp0REUUMe93lIJOWC7u/JAFen/ABK/aA8F/sv6s/wn/Z+8CaWbzTlEera3qERkkkmx8y5UqXYdyTtByAoxQB8xyRPE3lyxMrDqrLgik4x0P5V9KfD39q7QPjjrdv8ADT9o3wFo13aapILe01S2gMb20rHC5JYlckgb0K7c8gjJHkX7Qfwhn+CPxOvfBZnea0KrcabcSfelt3ztz7ghlJ7lSe9AHEYHofyo+XHI/SvoT9u/T9O0+08BGxsYYfM0KQv5UYXccQ8nHWvAtL2HUrcEdZ0GD/vCgCABemP0o4A5Hf0r33/gofYafp3xf0iHT7KGBD4bjJSGMKCftE/PFexeBpfh1J8G/h98OPGehWnk+M/DrWQuzEocTCFCo3Y4LZbB67gvrQB8QAD0P5UYXHAP5Vu/EnwHq3wx8dal4F11f9I065aPzMYEqdUkHsykMPrXr/7ZVhp9l8PvhZJZ2UMTS+Gi0rRRhS58q15OOvU/nQB4INvPH6Ug29s/lX1R8LPHI+E/7E1v8RNM8MaXf3sOryRBdStt6sr3BU5wQeB05rlLL9tjQddmFl8Tf2ffC+o2MhxN9ktgsij1AkDgn2yPqKAPAuOeP0pQBnp+le4ftB/An4ft8P7T9oL4DzSN4bvXC3+nSMS1i5bbkZJIG/5CpJwSMEqePDsjP/16ADAx0P5UDGen6UZH+TRlc5/rQAADPQ/lS4Gen6UmR/jzSgr39PWgBOPQ/lRgen6UZX/JpcjP/wBegBAB6H8qMDPQ/lRlSen60EjqP50ALgZ6fpScf3T+VKCuf/r0mVxjH60ALhc9P0pAB6H8qXIz/wDXpMr6frQAELnofypcDjj9KTK9v50ZHf8AnQAcf3T+VGF9O3pS5X/JpMj/ACaADA9D19KCF9D+VBK/5NLlf8mgBCBjp+lKcZ+7+lJkd/50pK5z/WgBMDjg/lRgeh/KjK/5NBK9D/OgBSF9P0pMDHT9KMrjH9aMjv8AzoADj+6fyopcr/k0UAJ3xn9aOfX9aOevNHPbNAAPc/rRkev60oJo5oAT6H9aM/7X60pzScn1oAO/3h+dH4/rS89aOfWgBPx/Wgnjr+tHIoOfegAzz94fnQD3z+tGT0yaUZoAT8f1oJx3/WlyfWjmgBM/7Xb1oyfUfnRz0zRz0yaADv1/Wjr3/WlGc0ZNACE47/rRnnr+tLzRyO9ADcnH3v1pT/vdvWjkdzRz6mgA9Pm/WgZJ+9+tGT6mjJ9TQAdMfN+tGff9aMnHWjn3oA+lv2zfGOs/D/4veA/GmgT7LvTdCimiyeGxI2VPswypHoTWb+1T4P0fxRfeF/2lvAsOdL8TtAmoqv8AywugQBux3IVkP+1Ee7U3/goJn/hL/CnX/kWE/wDRjU/9jLxho3jXR9X/AGafHM2bPV0N3orscmG4TDsq57/KsgHTKN/eoApf8FBwjfHyyWVsKfD9tuOe3nTVq/8ABRx5Y/Gnhixh+W0i0Vzbqv3QTJhsfgqVk/8ABQ3P/C9rXk/8i7b/APo2autn03Sf21vgZo9lout2tv458KweXLaXUm37Um1VY+u19qMGxhWypwDmgD5byc9R+dfSHweY3f7A/jqDUzmGLVZPs+7oCFtWGP8AgfP1NcFpX7F/7R2pawukzeAWtFL4ku7q+hEKD+9lWJYf7oJ9q7r9ovxH4R+CnwPsf2XPBmtx6hqLzLP4luoTwrB/MKtjoxcLheqpGM9RQB85c4J3frX0V/wUMz/b3g/n/mAt3/2hXzrz6mvor/goWT/b3g//ALALf+hCgD51PGfm/WjJ/vdvWjJ55o5680AfUH7Jd74O0/8AZT8b3vxA0u4vtFj1VzqNpaORJLH5MPCncvOcfxCuZ/4WB/wT+/6Ij4r/APA1/wD5MrQ+BJJ/Yj+I/wD1+v8A+ioK+dcn1NAH0N+w4/hS/wD2ndbvfDNjJbaadLvZNGtrlsyQxG4iCKx3N8wjODye/J614l8Rpru5+IWvXGoOfPfWbpptx53mVs5/HNbPwB+KLfB34raX44ljeS1hkaK/iTq8Dja+B3IzuA7lRXrfx9/ZW1z4ieIJvjH8AZLXX9I15zdS29pdIrxSty5G4gMC2TjO5SSCOKAPnOKSSKRZYpCrqcqwPIPrX0d/wUWUP4n8J3dwgF3Jo0guOxwHBA/MvWZ8Ff2NfG1v4ig8a/G21t/D/h3SJBdXYvruPdOEOQpCkhEJHzFiOOmc5HGftVfGO1+NPxZudf0ZmOl2UC2emM6kGSNSSZMdtzMxHfGM9KAPQf2+cGz+H5B/5gMnOfaGvn3Sj/xM7f8A67p3/wBoV9OeOPB13+158AvCviX4bXcFz4h8MWn2PVNKknWN2JRFb7xwCTGHXJAIc85GK4j4SfsXfGPVfG1jceO/DJ0bR7S6SbULq8uY8tGhDMihWJJIGM9BnJPagDW/4KN4/wCFx6Pz/wAyzH3/AOniepP2i7280z9mz4Q6lp9y0Nxb2glgmjbDI6xREMD2IIBrjv2yfifo3xR+Ndzf+G7tbiw0yzj0+2uY2yk2xmZ2U9xvdgD3ABHBrrP2mc/8Mv8Awn/7B5/9Ex0AT/tE2Vn8f/gdon7S3h+2T+09OjXT/FMEI5Ug434HZXYEf7Ey5+7UH7aZ/wCLd/CgZ/5lk9/+mNrWJ+xl8TNN0Dxpd/Crxltl0DxjAbK4hmPyLOwKofbcGKH3ZT/DXXf8FB9Eh8MaX8PvDdtM0sen6Xc20cjjlljW3UE+5xQBVJ/410qM/wDMe/8Abo186AjPX9a+pfAHw88W/FH9g+Hwj4K05bu/l1p3SFp0jBVbkljucgdPeuF8P/sFfHvUrtU1+003R7YczXV3qKSBF7nEW7J+uPqKAOl/ZxZ7v9jf4mWWqNmyiW4e339BN9mU8f8AAljP1r5w+n86+gPjx8RPh58LPg/F+zL8H9aXVDJP5viXWImBWVwQxQFcgsWVc4JCqgXJJOPn/nNACZx3/Wlzg4z+tJk+9LznNACEjPXv60HrgH9aXnNGTmgAwex/Wkzjv+tHPqaXnNABnHQ/rSMR2P60c9eaU5oAD14P60Y9/wBaDmg5NACZ55P60Z75/Wl5pOT60AKSPX9aT0wf1pTmg5oAMep/Wk6dT+tLyaTn3oAM98/rSkjHJ/Wjk0c0AJnjr+tLg9Sf1o5xRk0AIcjqf1oyPX9aOQMc0ZPTmgBSeOT+tJng89/Wl5xRzQAmD3P60UuTRQAgHPTvRj2/lQMego49qADHt/KjHt/KjI9qOPbrQAEYGcUEYPT+VGR6CjIz0FABjnp/KjHt/KjjPAoyPQe1AAR7fyoIz0H8qDj2oOM9BQAY6cfyoAyOlGRnoPejgdQKADHtQR7UcdxS5HegBMe38qMe38qDjPIFBI9BQAAdRj+VGPajjPQUcelAAR7UY5xj+VLkGkOM5IoAMcdP5UYwen8qCRjoKOPQUAGO+P5UY9v5UcY6UZHoKADGe38qMdcD+VHGOgoyPQUAW9W1/Xdfljm17Wbu9aJNkTXdy0hRf7oLE4HtUNjfXumXceoabdy288Tbop4JCjofUMOQai464FHGOlAFrV9c1vxBdC+1/V7q+nCBBNeXDSuFGSBliTjk8e9R6fqGoaVdx6jpd7NbXETZint5SjofUMMEGocj0FHGOlAHVXfxz+NN/ZHTbz4r+I5IWXa0b6zMdw9D83I+tcqzM7b3JJJySTnJoyPQUcY6CgAxnPH8quav4g8QeIHjk17W7y+aJdsLXly0hRfQbicD2FUxjHSjI9BQAevHT6UY56fyo4wcCjI9BQBbtfEGv2OlzaLZa3eQ2dwc3FpFdMsUp4+8oOG6DqO1VMc4x/KjjHQUDHpQAY4zj+Va3hfx7448EyvJ4P8AF+p6WX5k+wX7wh/qFIB/GsnI9BQMZ4AoA2/FHxJ+IfjeNYfGHjfVtTjVspFfahJKin1CsSBWJj2/lQCB2FHHcCgC7oHiTxF4VvhqnhjXb3TroDAuLG6aFwPTchBxWt4g+MHxW8V6e2l+JfiPrd9auMPbXOqSvG31Utg/jXOcDsKOMdqADHGSKt3uva7qVjBpmpazd3FtaDFrbzXLOkI6YRScLxjpVTjPSjj0FACxySRSLLCxV1OVZTggjoQau6z4m8SeJDG3iLxBfX5hz5Rvbt5dgOM43E4zgfkKo5HcCjgdhQBq6V458baHZjTtF8YapZ26ElYLXUJI0BPJIVWAFN1Pxp4x12E22t+K9SvIz/yzur+SRfyYmswY9qMj2oAAPb+VGPb+VHHoKOO4FAAB7fyoxk9BRx2Ao4J7UAAGT0FGOen8qMj0HWjj0FABgen8qMc8j+VHHoKOOwFABjnGBRjnoKMj2oOOnFAARzjH8qPw/lRx6Cjj0FABjnkfyoI9qPl7AUcHjAoACO2BQR6D+VBI9qOPQUABAz0/lRj/AGf5Ucego4PYUAB9gKCMcYFGR6Cl4B5AoAQjjp/Kgj2/lQMY6Cjj0FAB/wAB/lQR6Cj5fQUZGMYFABjA6Cgjjp/Kjj0FHHPFABjjp/KijI9BRQAZOevf0o+brk/lSjg//WpPp/KgAGfX9KMn1P5UvbHP5Uf56UAIc+ufwoO7PU/lS+39KTn/ACKAF5B6/pRk+v6UH8fyo59T+VACZbHU/lQd3r+lAz0/pRz/AJFABk+p/KjJ56/lR34/lR/npQAvPqfyoGfX9KMn1P5UnP8AkUABLZ6n8qMn1P5Uc0f56UAAJz3/ACpecdT+VJ9f5UvPqfyoAOc9f0pCTnqfyoOf8ijmgAycdT+VHOe/5UY4/wDrUd+f5UAHzYHJ/KjLep/Kjn/Io57/AMqADLHHJ/KjJI5J/Kjn/Io/z0oAMt7/AJUfNjqfyo5/yKO2B/KgAy3qfyoyxHf8qOaP89KADLep/KjnHf8AKj/PSjnGP6UAHzc8/pRlvU/lR9P5Uc/5FABljkc/lRls9T+VH+elH+elABzjHP5UZbPU/lRzjH9KPp/KgA+bGOfyoy2ep/Kjn/Io5/yKAAbvU/lR83r+lH+elL+f5UAJlvX9KBn1P5UuTRzQAmWz1P5Uc4/+tS8+p/Kj8/yoAQZ6Z/SgFvf8qX8/yoyf8igBBnPU/lRlvU/lSjNHPqfyoATnHJ7+lHzep/Kl/E/lRn6/lQAmW9f0o5znJ/Kjkf8A6qUZzQAhJ9T+VLzzz+lHPqfyo/E/lQAnI7n8qMsD1/Sl5x3/ACo5zx/KgBOfU/lSkn1P5UmT70pz70AHOevb0pOexP5Uv4n8qPz/ACoATLev6Ucnufypeeo/lRz70AGT6n8qPm45/Sjn3o59T+VACc+p/KjLdf6UvfqfypOe38qAA5Pc/lS5PqfyoJNGT6mgBPm9f0oOc9T+VLzjv+VHfqfyoAT5vX9KCSf/ANVL/npSZNAC846n8qT5iOv6UuT6mjn1P5UAIc56/kKKXntn8qKAE79R1o4HpS4NGGoAQD0x+dJx7fnTsGjB/wAmgBOPQfnSce1Lg+lGDQAcZzx+dHB6Y/OlwaMGgBDg9cfnSHHt+dOwaMGgBvGeMUcc9KXBpcGgBOPb86DjvilwaMGgBOpPT8TSce1LhvT9aMGgBOMnp09aXjHb86XBowaAEOMc4o7446etLg0mDnpQAhx7Ud+3Slwe9GGoATjHajA9vzp2DijB96AGjHFBA9qXDen60YPQ0AJxkdKOMdqXB96XBxzQA3A9vzo4HX1p2D70mG9KAEIGecfnRxjtTsGkwaADjnOKQY9qcAe9GD70AN4Gf8aMD2/Olw3YUuDQA3jHal4z2owaUA96AGjHtRgA9unrS4PvRg9hQAnGM8fnS+3HT1owfSlwaAE79R+dA9sfnS4PrSYP+TQADGeMfnR+I/OlwaMH1oAT24/Oj24/OlwaMGgBB14x+dAxnt+dGDn/AOvS4NACfiPzo6enX1pcH1owaAE9sj86OM9vzpcGjBzQAnft19aD9R+dLg0YPrQAnT0596D9R+dLg0YOev60AIce350HHt+dGDS4NACH6j86Onp+dLg+tGDQAnTuPzo49vwNLg/5NBBoAQ4x26etH4j86XBowfWgBOnOR+dHbt09aXB9aMH/ACaAEOCO350e/H50pBAowc0AJ2xkfnR+I/OlwaMH1oATOeePzo4x2/OlwcUYPUUAJ27fnQOnb86XBowaAEx7j86KXB9aKAEzjqD1oB6daMjPUdaKADOOufzo59DRR/jQAE+maT6g0vHqKOM9qADJz0NB49aMe/60cUAJk4PWg+uDSnoaDQAnOeh/OjnBGDS4BPUUce1ABn13UZPvR+NH+eaAEz3welH4H86XjNHGO1ACAnPQ0oz33UcZwMUcUAJknpml79DQaDjOKAEzx3/OjJz0NLxjtRwD2oAQdOAaM+oNLx7Ud+MUAJ+Bo/A/nRwaXj1FACZPB5o/A0vHGKOOvFACc+h/Ojg9AaXjOOKTPXNAATyetHOO9Lx04o4x2oATueDRz3zS+tHHtQAnXoDRnnjPSl74NHHtQAnOOAaM89D+dLx14o79qAE59DRnPY0vHtR359KAEz060uT70DHtRgeooAMn0NAJ9DRn3FHGM5FAAM+ho5Hr1oyPUUe+aAAE+9GSB3owPUUdO4oAOQehoGc9DRx1JFHHqKADketGTnvR170ceooAOfejJznBo49RRwepFABznoaOffpRx6ijr3oAMn3oyc9DRwe4o+hFABk+hoOSeho4PcUceooAOc9DSZPvSjnvRx6igAyc96MnuDR36ijg8ZFAAcnqDRzxwaOPUUA+hoAMn3o596MDpkUcDuKAAk+hoJOOhoPoSKOAeooAOcYwaCTnvRn3FHGcEigAycd6Dnpg0fiKPxFABkkdDRzjoaOB3FAI9RQAEn3ooP1HNFABk/3qAT60YOcUc9qADPvRk+tAB6CjBoAMn1oyc/eoIx1oxzz+tABk5zRk/wB6jB9KMGgAyfWgk44NGDQRxg0AGTnOaMn+9RigDvigAyf71GT/AHqOc0YPpQAZOc7qMn+9RijFABk5zuoyf71AGT0o5oAMnsaMnP3qXB9KTHOaADJx96jJz96jHFGOenNABk46/rQCf71GB6UY5zQAZP8Aeoye5ox3oxQAZP8AeoBI7/rRj2ox3xQAZP8AeoJP96jFBHegAyc/eoycY3UY56UYoAMnnn9aMn+9QR6ijGeKAAk9moyf71BGaMc0AGTjG6gE9z+tGOKMe1ABk/3qMn+9RijGeMUAGT/eoyfWjHT9KXk96AEBPc0ZP96lGfWjBoATPfNG4460c0YNABk560ZPc0o54zSDNABk5zmjPqaXBz1pOaADcfWgk560YNLz0zQAmT3NGTnrRg5pefWgBM+9G488/Slwc0mCaAAk+tG4560Z96Ug560AJk9jRn3pSD1zQQe9ACbjnrxRk460YNHTvQAZOetGT60pB9aMGgBM+po3H1peeuaTBoANx9aMn1o6d6O2c0AGfejcfWlwfWjB9aAEycYBo3H1oxRyKADJx1o3e9L1Gc0DJ70AJk/3qM+hpcGkx70AG4+tFGD3NFABz6D8qMn+7+lGB0wevpRgeh/KgAHTp+lH4fpSjHb+VGB6H8qAEOfQflRg+g/Kggdx+lGB6H8qAD8B+VHPp+lLgZ6fpRgDt+lACc56D8qDz2H5UuB3B/KkIA7fpQAYPoPyo/4CPyowvofyowvofyoAB7j9KP8AgI/KlAA7H8qQgdcH8qAD8O3pRg+g/KjAyeD09KMD0P5UAH/Af0oHTkfpRhfQ/lS4HofyoAQ/QflR3+7+lKQD6/lSYGeh/KgAwfQflR36fpRgY6H8qML6H8qADB9B+VHPcD8qABjofyowueh/KgA59B+VHPoPyowOOD19KMDuD+VAAOMcfpRg+g/KjA9D+VAA7A/iKADn0H5UHP8AdH5UYX0P5UED0PX0oADnPQflR+H6UYGeh/KjA9D+VAAcnPH6UHPoPyowOcA/lRhfQ/lQAHP90flQc+g/KgheeD+VGB6H8qAD8B+VByT0H5UYHofyowucYP5UAGD0wPyo59B+VGB1wfyowueh6elABzgcfpRk+n6UADA4P5UuB6fpQAg+g/KgZ9P0pQF9P0oAHp+lACfUfpS5Pp+lGFx0/SjA/wAigBASO36UD/d/SlAXpj9KAB6fpQAgJz0/SgfT9KUAZ6fpQAvp+lABk+n6Ugznp+lKAPT8xRgeh/KgBPfH6Uc56fpRhff8qXAz0/SgBOfT9KXPoP0owM9P0pML6fpQAc+g/Kjqfu/pS4Gen6UYXPT9KAEOew/Sg5z0/SlwPT9KMDPT9KADPotIc+n44pcDPT9KTA9P0oADyen6Uc+n6UuFz0/SjA7D9KAE5/8Ar4pc/wCz+lGB6fpRhc9P0oATJ9P0o544/SlwPT9KCB6fpQAhz2H6Uc+n6UpA9P0oIHp+lABn2/SkP0/SlIX0/Sggf5FACZOOR+lHbGB+VKQMdP0owPT9KAE5x0/Slz6jP4UYGOn6UEL6fpQAmT6fpRSkDp/SigBOPXvRkUc5/H1o59f1oAM+/wCtHFKORn+tJz6/rQAZoz7/AOfzoOcdf1oOQcZ/WgA79aM+9KQc9f1oHI/+vQAhOe/60Z9/1/8Ar0c4PP60HI7/AK0AAI9aM46EUc8c9fejnk/1oAAfcUZpRkj/AOvSHOev60AGRnrRkUHPTPb1o5xnP60AHGeooz7ilGc4/rQuT3/WgBCfejPPWg8d/wBaMHOP60AGRRn3o5253frRyeB6etABnHejOT1owcDB/WjBzjP60AHAoyKOcA5/Wjnn5v1oAM980Z96Bk4/xowcdf1oAM5PWjjFGCDyf1o5A5Pf1oAM85zRx60YOcZ/WgE469/WgAz70E+9GDg5P60YYdT+tABx60ZFGDzz+tHOev60AHBHWjPfP60c88/rSgHPJ/WgBM8UfjRhsZ3frRyT17etABkUZ96OeOf1oweRn9aAAH3oz70fj39aBnnn9aADPuKM470fj+tH49/WgAz70Z96DkHr29aOcHnp70AGfejPvR36/rRz6/rQAZ96M+9HPPP60pBz1/WgBM+9GeetAyeM9vWjnPX9aADPvRnHQ0fj+tAye/b1oAM+9Gfeg9jn9aB15P60AGfejPvRzxk/rS/j+tACZx3FGfejuOf1oIOM5/WgAz70Z96OQeT+tByO/f1oAM570Z96Dnuf1o59e/rQAZ96M8daXBwef1pOeue3rQAE+9Gfeg5Gef1o5/yaADPuKM570Hp1/WlxzjP60AJn3oz70DOM5/Wg59e3rQAZ96PxFHbOf1o59f1oAM+9FLj37etFAH//2Q==";
            }
        }

        public static List<object[]> GetUserSessionInfo()
        {
            List<object[]> _pvUsersList = new List<object[]>();
            using (SurveilAIEntities db_context = new SurveilAIEntities())
            {
                //var activeUsers = db_context.pvjournals.Select(pv => pv.desktopuser).Distinct().ToList();
                //foreach (var userid in activeUsers)
                //{
                //    if (userid != "")
                //    {
                //        var lastSession = (from u in db_context.usersessions
                //                           where u.extuserid.Equals(userid)
                //                           select u).FirstOrDefault();
                //        string isOnline = lastSession.sessionflag == 0 ? "Offline" : "Online";

                //        Object[] obj = new object[]
                //       {
                //    userid  ?? "" as object,
                //    lastSession.logintime  ?? "" as object,
                //    isOnline ?? "" as object,
                //    lastSession.ipaddress ?? "" as object,
                //    lastSession.terminalserver ?? "" as object,
                //       };
                //        _pvUsersList.Add(obj);
                //    }
                //}
                foreach (var usersessions in db_context.usersessions)
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

            }
            return _pvUsersList;
        }

        public string FilterJobDevices(string JobDevices, string UpdateDevices, string UserId)
        {
            try
            {
                string FilteredDevices = "";
                if (JobDevices.Contains("DeviceID") && UpdateDevices.Contains("HierLevel"))
                {
                    FilteredDevices = UpdateDevices;
                }
                else if (JobDevices.Contains("HierLevel") && UpdateDevices.Contains("DeviceID"))
                {
                    FilteredDevices = UpdateDevices;
                }
                else
                {
                    List<Device> Devices = new List<Device>();
                    List<Hierarchy> hierarchies = new List<Hierarchy>();

                    if (!string.IsNullOrEmpty(JobDevices))
                    {
                        List<Device> AllDev = new List<Device>();
                        bool UpdateByDevice = false;
                        if (UpdateDevices.Contains("DeviceID")) { UpdateByDevice = true; }
                        if (JobDevices.Contains("DeviceID"))
                        {
                            List<string> SplitList = JobDevices.Split(new string[] { "AND" }, StringSplitOptions.None).ToList();
                            string HierLvls = SplitList[0].Trim();
                            HierLvls = HierLvls.Replace("d.DeviceID IN (", "");
                            HierLvls = HierLvls.Remove(HierLvls.Length - 1);
                            HierLvls = HierLvls.Substring(1, HierLvls.Length - 1);
                            HierLvls = HierLvls.Replace("'", "");
                            List<string> Hlist = HierLvls.Split(',').ToList();

                            //Hierachies to Update
                            List<string> SplitListUpd = new List<string>();
                            string DeviceType = "";
                            if (UpdateDevices.Contains("AND"))
                            {
                                SplitListUpd = UpdateDevices.Split(new string[] { "AND" }, StringSplitOptions.None).ToList();
                                UpdateDevices = SplitListUpd[0].Trim();
                                DeviceType = SplitListUpd[1].Trim();
                            }

                            UpdateDevices = UpdateDevices.Replace("d.DeviceID IN (", "");//removing HierLevel IN
                            UpdateDevices = UpdateDevices.Trim();
                            UpdateDevices = UpdateDevices.Remove(UpdateDevices.Length - 1);
                            UpdateDevices = UpdateDevices.Replace("''", "");
                            UpdateDevices = UpdateDevices.Replace("'", "");
                            List<string> HListUpdate = UpdateDevices.Split(',').ToList();


                            //Hier to add
                            List<string> HierNotAddedCurrently = HListUpdate.Where(p => Hlist.All(p2 => p2 != p)).ToList();


                            //Get User Assigned Hierachies
                            List<Device> UserAssignedDevices = GetAssignDevice(UserId);
                            List<string> AllAssignedHier = new List<string>();
                            if (UpdateByDevice)
                            {
                                UserAssignedDevices = UserAssignedDevices.Where(x => UserAssignedDevices.Any(y => y.DeviceID == x.DeviceID)).ToList();
                                AllAssignedHier = UserAssignedDevices.Select(x => x.DeviceID.ToString()).ToList();
                            }
                            else
                            {
                                List<Hierarchy> AllHierarchies = db.Hierarchies.ToList();
                                AllHierarchies = AllHierarchies.Where(x => UserAssignedDevices.Any(y => y.HierLevel == x.Hierlevel)).ToList();
                                AllAssignedHier = AllHierarchies.Select(x => x.Hierlevel.ToString()).ToList();
                            }


                            //Hier to remove
                            List<string> HierToRemove = AllAssignedHier.Where(p => !HListUpdate.Any(e => e == p)).ToList();

                            List<string> UpdatedHierList = Hlist.Where(p => !HierToRemove.Any(e => e == p)).ToList();//removing
                            UpdatedHierList.AddRange(HierNotAddedCurrently);//Addding

                            string HierIds = "";
                            foreach (var item in UpdatedHierList)
                            {
                                HierIds += "''" + item + "'',";
                            }
                            if (HierIds.EndsWith(","))
                            {
                                HierIds = HierIds.Remove(HierIds.Length - 1);
                            }

                            if (DeviceType != "")
                            {
                                FilteredDevices = " d.DeviceID IN (" + HierIds + ")  AND " + DeviceType;
                            }
                            else
                            {
                                FilteredDevices = " d.DeviceID IN (" + HierIds + ") ";
                            }
                        }
                        else if (JobDevices.Contains("HierLevel"))
                        {
                            //Hierachies from Jobs
                            List<string> SplitList = JobDevices.Split(new string[] { "AND" }, StringSplitOptions.None).ToList();
                            string HierLvls = SplitList[0].Trim();
                            HierLvls = HierLvls.Replace("d.HierLevel IN (", "");
                            HierLvls = HierLvls.Remove(HierLvls.Length - 1);
                            HierLvls = HierLvls.Substring(1, HierLvls.Length - 1);
                            HierLvls = HierLvls.Replace("'", "");
                            List<string> Hlist = HierLvls.Split(',').ToList();

                            //Hierachies to Update
                            List<string> SplitListUpd = new List<string>();
                            string DeviceType = "";
                            if (UpdateDevices.Contains("AND"))
                            {
                                SplitListUpd = UpdateDevices.Split(new string[] { "AND" }, StringSplitOptions.None).ToList();
                                UpdateDevices = SplitListUpd[0];
                                DeviceType = SplitListUpd[1].Trim();
                            }

                            UpdateDevices = UpdateDevices.Remove(0, 17);//removing HierLevel IN
                            UpdateDevices = UpdateDevices.Remove(UpdateDevices.Length - 3);
                            UpdateDevices = UpdateDevices.Replace("''", "");
                            UpdateDevices = UpdateDevices.Replace("'", "");
                            List<string> HListUpdate = UpdateDevices.Split(',').ToList();


                            //Hier to add
                            List<string> HierNotAddedCurrently = HListUpdate.Where(p => Hlist.All(p2 => p2 != p)).ToList();


                            //Get User Assigned Hierachies
                            List<Device> UserAssignedDevices = GetAssignDevice(UserId);
                            List<Hierarchy> AllHierarchies = db.Hierarchies.ToList();
                            AllHierarchies = AllHierarchies.Where(x => UserAssignedDevices.Any(y => y.HierLevel == x.Hierlevel)).ToList();
                            List<string> AllAssignedHier = AllHierarchies.Select(x => x.Hierlevel.ToString()).ToList();

                            //Hier to remove
                            List<string> HierToRemove = AllAssignedHier.Where(p => !HListUpdate.Any(e => e == p)).ToList();

                            List<string> UpdatedHierList = Hlist.Where(p => !HierToRemove.Any(e => e == p)).ToList();//removing
                            UpdatedHierList.AddRange(HierNotAddedCurrently);//Addding

                            string HierIds = "";
                            foreach (var item in UpdatedHierList)
                            {
                                HierIds += "''" + item + "'',";
                            }
                            if (HierIds.EndsWith(","))
                            {
                                HierIds = HierIds.Remove(HierIds.Length - 1);
                            }

                            if (DeviceType != "")
                            {
                                FilteredDevices = "d.HierLevel IN (" + HierIds + ")  AND " + DeviceType;
                            }
                            else
                            {
                                FilteredDevices = "d.HierLevel IN (" + HierIds + ")";
                            }

                        }

                    }
                }
                return FilteredDevices.Trim();
            }
            catch (Exception ex)
            {
                errorlog.Error("User: " + UserId + "Error: " + ex);
                return UpdateDevices;
            }

        }

        public static List<SelectListItem> ToListItem(List<string> StList)
        {
            List<SelectListItem> selectItems = new List<SelectListItem>();
            foreach (var item in StList)
            {
                SelectListItem temp = new SelectListItem { Value = item, Text = item, Selected = false };
                selectItems.Add(temp);
            }
            return selectItems;
        }

        public static List<SelectListItem> SelectedItem(List<SelectListItem> StList, string item)
        {
            List<SelectListItem> tempListItem = StList;
            if (!string.IsNullOrEmpty(item))
            {
                foreach (var item1 in tempListItem)
                {
                    if (item1.Text == item)
                    {
                        item1.Selected = true;
                    }
                    else
                    {
                        item1.Selected = false;
                    }
                }
            }
            return tempListItem;
        }

        public static List<Device> GetDevicesFromString(string ATMID)
        {

            List<Device> noData = new List<Device>();
            try
            {
                var obj = new Device();
                List<Device> AllDev = new List<Device>();

                // var ATMID = db.Users.Where(a => a.UserID.Equals(user)).Select(a => a.ATM).First();

                if (ATMID != "" && ATMID != null)
                {
                    using (SurveilAIEntities db_context = new SurveilAIEntities())
                    {
                        if (ATMID.Contains("DeviceID"))
                        {
                            ATMID = ATMID.Remove(0, 13);//removing DeviceID IN
                            AllDev = db_context.Devices.Where(a => ATMID.Contains(a.DeviceID)).ToList();
                        }
                        else if (ATMID.Contains("HierLevel"))
                        {
                            ATMID = ATMID.Remove(0, 14);//removing HierLevel IN
                            AllDev = db_context.Devices.Where(a => ATMID.Contains(a.HierLevel.ToString())).ToList();
                        }
                    }
                    return AllDev;
                }
                else
                {
                    return noData;
                }
            }
            catch (Exception ex)
            {
                var e = "Error: " + ex;
                return noData;
            }
        }

        //public void ConnectDB()
        //{
        //    IISContext db = new IISContext();
        //    IICon
        //}
    }

    public class eventTemp
    {
        public DateTime timestamp { get; set; }
        public string orgmessage { get; set; }
    }
}