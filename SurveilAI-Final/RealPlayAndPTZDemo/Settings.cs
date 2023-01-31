using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RealPlayAndPTZDemo
{

    public partial class Settings : Form
    {
        public static List<ChannelCameras> ChannelCamerass = new List<ChannelCameras>();
        public static string DeviceId = "";
        public static string DeviceTypeCredCam = "";

        public static List<NDVRChannel> ndvrchannel = new List<NDVRChannel>();
        SqlConnection con = new SqlConnection("Data Source=192.168.1.100; Initial Catalog = SurveilAI; User ID = sa; Password=Dev@2022");
        RealPlayAndPTZDemo real = new RealPlayAndPTZDemo();
        public string AllDevices;
        public string AllHierLevels;
        List<MappedDevices> res2 = new List<MappedDevices>();



        public Settings()
        {
            InitializeComponent();
            if (Login.getvalue == "Static")
            {
                if (GetData.Data.HierAssignLevl != null)
                {
                    for (var i = 0; i < GetData.Data.HierAssignLevl.Count; i++)
                    {
                        AllDevices += "'" + GetData.Data.HierAssignLevl[i].Devices + "'" + ",";
                        AllHierLevels += "'" + GetData.Data.HierAssignLevl[i].Hierlevel + "'" + ",";
                    }
                    comboBox1.Hide();

                    GetDevicesFromHierarchy();

                }
                else
                {
                    for (var i = 0; i < GetData.Data.Devices.Count; i++)
                    {
                        AllDevices += "'" + GetData.Data.Devices[i] + "'" + ",";
                        AllHierLevels += "'" + GetData.Data.HierAssignLevl[i].Hierlevel + "'" + ",";
                    }
                    radTreeView1.Hide();
                    GetDevices();

                }
            }
            else
            {
                GetDynamicViewType();
                if (GetData.Data.HierAssignLevl != null)
                {
                    for (var i = 0; i < GetData.Data.HierAssignLevl.Count; i++)
                    {
                        AllDevices += "'" + GetData.Data.HierAssignLevl[i].Devices + "'" + ",";
                        AllHierLevels += "'" + GetData.Data.HierAssignLevl[i].Hierlevel + "'" + ",";
                    }

                }
                else
                {
                    for (var i = 0; i < GetData.Data.Devices.Count; i++)
                    {
                        AllDevices += "'" + GetData.Data.Devices[i] + "'" + ",";
                    }

                }
            }
           
            textBox1.Text = "WELCOME  " + GetData.Data.UserID.ToUpper();


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          


        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        public void GetDevicesFromHierarchy()
        {
            List<MappedDevices> res = new List<MappedDevices>();



            res.Add(new MappedDevices { DeviceID = "--Select All--", DeviceType = "--Select All--" });

            var query = @"Select d.DeviceID,d.DeviceType,d.BranchName,a.Cam1,a.Cam2,a.Cam3,a.Cam4,
                        a.Cam5,a.Cam6,a.Cam7,a.Cam8,a.Cam9 from Device as d
                        inner join AddCameras as a on a.DeviceID=d.DeviceID
                        where d.DeviceID
                        IN (" + AllDevices.Substring(0, AllDevices.Length - 1) + ")";
            con.Open();
            var ress = con.Query<MappedDevices>(query);
            int i = -1;

            foreach (var item in ress)
            {
                if (item.DeviceType == "NVR" || item.DeviceType == "DVR")
                {
                    res.Add(new MappedDevices { DeviceID = item.DeviceID, DeviceType = item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() });
                    res2.Add(new MappedDevices { DeviceID = item.DeviceID, DeviceType = item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() });
                    radTreeView1.Nodes.Add(item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper());
                 
                    i++;
                    if (item.Cam1 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam1 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam1 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam1 });

                    }
                    if (item.Cam2 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam2 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam2 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam2 });

                    }
                    if (item.Cam3 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam3 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam3 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam3 });

                    }
                    if (item.Cam4 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam4 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam4 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam4 });

                    }
                    if (item.Cam5 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam5 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam5 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam5 });

                    }
                    if (item.Cam6 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam6 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam6 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam6 });

                    }
                    if (item.Cam7 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam7 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam7 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam7 });

                    }
                    if (item.Cam8 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam8 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam8 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam8 });

                    }
                    if (item.Cam9 != "NULL")
                    {
                        radTreeView1.Nodes[i].Nodes.Add(item.Cam9 + " Device(" + item.DeviceID + ")");
                        res.Add(new MappedDevices { DeviceType = "   " + item.Cam9 });
                        res2.Add(new MappedDevices { DeviceType = "   " + item.Cam9 });

                    }
                }
                else
                {
                    radTreeView1.Nodes.Add(item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() + " Device(" + item.DeviceID + ")");
                    res2.Add(new MappedDevices { DeviceID = item.DeviceID, DeviceType = item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() });
                    i++;
                    res.Add(new MappedDevices { DeviceID = item.DeviceID, DeviceType = item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() });
                }
               
            }
     

            con.Close();
        }
        public void GetDynamicViewType()
        {
            try
            {
                var query = @"select CTID,CamType as CamTypee from CameraTypes";
                con.Open();
                var rs=con.Query<CamType>(query);
                con.Close();
                foreach(var item in rs)
                {
                    radTreeView1.Nodes.Add(item.CamTypee.ToString());
                }
            }
            catch(Exception ex) 
            {
            }
        }

        public void GetDevices()
        {
            List<MappedDevices> res = new List<MappedDevices>();
            res.Add(new MappedDevices { DeviceID = "--Select Device--", DeviceType = "--Select Device--" });
            var query = @"Select DeviceID,DeviceType,BranchName from Device where DeviceID IN (" + AllDevices.Substring(0, AllDevices.Length - 1) + ")";
            con.Open();
            var ress = con.Query<MappedDevices>(query);
            foreach (var item in ress)
            {
                res.Add(new MappedDevices { DeviceID = item.DeviceID, DeviceType = item.DeviceType + "-" + item.BranchName.Replace(" ", "").ToUpper() });
            }
            comboBox1.DataSource = res;
            comboBox1.ValueMember = "DeviceID";
            comboBox1.DisplayMember = "DeviceType";
            con.Close();
        }
        public List<MappedDevices> GetCredentials(string DeviceId)
        {
            var query = @"select d.DeviceType as DeviceType, d.DeviceID AS DeviceId, n.NDip as IP,n.NDName as Name,n.NDusername as Username,
            NDpassword as Password from Device as d inner join NDVR as n on d.IP=n.NDip where d.DeviceID IN(" + DeviceId + ")";
            con.Open();
            var rs = con.Query<MappedDevices>(query);
            con.Close();
            return rs.ToList();
        }
        public List<MappedDevices> GetIP()
        {
            var query = @"select DeviceID,IP from Device";
            con.Open();
            var rs = con.Query<MappedDevices>(query);
            con.Close();
            return rs.ToList();
        }
       
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
          
        }



        private void button1_Click(object sender, EventArgs e)
        {
            string NDVRIP = "";
            
            int count = 1;
            string[] channels = new string[16];
            MappedDevices MapDev = new MappedDevices();
            List<MapDeviceCred> DeviceCred = new List<MapDeviceCred>();

            List<MappedDevices> DevicesHier = new List<MappedDevices>();
            if (Login.getvalue == "Static")
            {
                if (GetData.Data.HierAssignLevl != null)
                {
                  
                    var checkedNodes = radTreeView1.CheckedNodes;

                    foreach (var indexChecked in checkedNodes)
                    {
                        DevicesHier.Add(new MappedDevices { DeviceType = indexChecked.ToString() });
                    }
              
                    foreach (var item in DevicesHier)
                    {
                        if (item.DeviceType.Contains("Camera"))
                        {
                            var check = item.DeviceType.Split()[2];
                            var againcheck = check.Split()[0];
                            string DevId = item.DeviceType.Substring(item.DeviceType.Length - 11);
                            var PureDeviceId = DevId.Remove(DevId.Length - 1, 1);
                            DeviceId += "'" + DevId.Remove(DevId.Length - 1, 1) + "'" + ",";

                        }
                        else
                        {
                            var check = item.DeviceType.Split('.')[1];
                            var againcheck = check.Split()[0];
                            string DevId = item.DeviceType.Substring(item.DeviceType.Length - 11);
                            var PureDeviceId = DevId.Remove(DevId.Length - 1, 1);
                            DeviceId += "'" + DevId.Remove(DevId.Length - 1, 1) + "'" + ",";
                            foreach (var item1 in GetIP())
                            {
                                if (PureDeviceId == item1.DeviceID)
                                {
                                    NDVRIP = item1.IP;
                                }
                            }
                            if (againcheck == "0")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam1 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "1")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam2 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "2")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam3 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "3")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam4 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "4")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam5 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "5")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam6 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "6")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam7 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "7")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam8 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }
                            if (againcheck == "8")
                            {
                                ndvrchannel.Add(new NDVRChannel
                                {
                                    Cam9 = int.Parse(againcheck),
                                    IP = NDVRIP
                                });
                            }

                        }
                    }
                    var b = GetData.Data.HierAssignLevl.ToArray();
                    foreach (var Cred in GetCredentials(DeviceId.TrimEnd(',')))
                    {
                        DeviceCred.Add(new MapDeviceCred { Name = Cred.Name, IP = Cred.IP, Username = Cred.Username, Password = Cred.Password, DeviceId = Cred.DeviceID });
                    }
                    for (int i = 0; i < DeviceCred.Count; i++)
                    {
                        if (DeviceCred[i].Name != "Camera")
                        {
                            int j = 0;
                            if (DeviceCred[i].DeviceId != null)
                            {
                                foreach (var x in b)
                                {
                                    if (x.Devices == DeviceCred[i].DeviceId)
                                    {
                                        ChannelCamerass.Add(new ChannelCameras
                                        {
                                            IP = DeviceCred[i].IP,
                                            channel1 = x.Cam1,
                                            channel2 = x.Cam2,
                                            channel3 = x.Cam3,
                                            channel4 = x.Cam4,
                                            channel5 = x.Cam5,
                                            channel6 = x.Cam6,
                                            channel7 = x.Cam7,
                                            channel8 = x.Cam8,
                                            channel9 = x.Cam9
                                        });
                                    }
                                } 
                            }
                        }

                        else
                        {
                            if (DeviceCred[i].DeviceId != null && DeviceCred[i].DeviceId != "--Select Device--")
                            {
                                foreach (var x in b)
                                {
                                    if (x.Devices == DeviceCred[i].DeviceId)
                                    {
                                      
                                        Channels.CameraChannel++;
                                    }
                                }
                            }
                        }
                        if (checkBox1.Checked)
                        {
                            FRMapped.Check = 1;
                        }
                        else
                        {
                            FRMapped.Check = 0;
                        }
                        if (checkBox3.Checked)
                        {
                            FRMapped.RRCheck = 1;
                        }
                        else
                        {
                            FRMapped.RRCheck = 0;
                        }
                    }
                }
                else
                {
                    var device = comboBox1.SelectedValue;
                    var b = GetData.Data.ATM.ToArray();
                    foreach (var Cred in GetCredentials(device.ToString()))
                    {
                        Channels.IP = Cred.IP;
                        Channels.Username = Cred.Username;
                        Channels.Password = Cred.Password;
                        Channels.Name = Cred.Name;
                    }
                    if (Channels.Name != "Camera")
                    {
                        var i = 0;
                        if (device != null && device.ToString() != "--Select Device--")
                        {
                            foreach (var x in b)
                            {
                                if (x.StartsWith(device.ToString()))
                                {
                                    channels[i++] += x;
                                }
                            }
                            if (channels[0] != null)
                            {
                                Channels.channel1 = 0;
                            }
                            else
                            {
                                Channels.channel1 = -1;
                            }
                            if (channels[1] != null)
                            {
                                Channels.channel2 = 1;
                            }
                            else
                            {
                                Channels.channel2 = -1;
                            }
                            if (channels[2] != null)
                            {
                                Channels.channel3 = 2;
                            }
                            else
                            {
                                Channels.channel3 = -1;
                            }
                            if (channels[3] != null)
                            {
                                Channels.channel4 = 3;
                            }
                            else
                            {
                                Channels.channel4 = -1;
                            }
                            if (channels[4] != null)
                            {
                                Channels.channel5 = 4;
                            }
                            else
                            {
                                Channels.channel5 = -1;
                            }
                            if (channels[5] != null)
                            {
                                Channels.channel6 = 5;
                            }
                            else
                            {
                                Channels.channel6 = -1;
                            }
                            if (channels[6] != null)
                            {
                                Channels.channel7 = 6;
                            }
                            else
                            {
                                Channels.channel7 = -1;
                            }
                            if (channels[7] != null)
                            {
                                Channels.channel8 = 7;
                            }
                            else
                            {
                                Channels.channel8 = -1;
                            }
                            if (channels[8] != null)
                            {
                                Channels.channel9 = 8;
                            }
                            else
                            {
                                Channels.channel9 = -1;
                            }
                            if (channels[9] != null)
                            {
                                Channels.channel10 = 9;
                            }
                            else
                            {
                                Channels.channel10 = -1;
                            }
                            if (channels[10] != null)
                            {
                                Channels.channel11 = 10;
                            }
                            else
                            {
                                Channels.channel11 = -1;
                            }
                            if (channels[11] != null)
                            {
                                Channels.channel12 = 11;
                            }
                            else
                            {
                                Channels.channel12 = -1;
                            }
                            if (channels[12] != null)
                            {
                                Channels.channel13 = 12;
                            }
                            else
                            {
                                Channels.channel13 = -1;
                            }
                            if (channels[13] != null)
                            {
                                Channels.channel14 = 13;
                            }
                            else
                            {
                                Channels.channel14 = -1;
                            }
                            if (channels[14] != null)
                            {
                                Channels.channel15 = 14;
                            }
                            else
                            {
                                Channels.channel15 = -1;
                            }
                            if (channels[15] != null)
                            {
                                Channels.channel16 = 15;
                            }
                            else
                            {
                                Channels.channel16 = -1;
                            }
                        }
                    }
                    if (checkBox1.Checked)
                    {
                        FRMapped.Check = 1;

                    }
                    else
                    {
                        FRMapped.Check = 0;
                    }

                }
            }
            else
            {
                var checkedNodes = radTreeView1.CheckedNodes;
                string Types = "";
                string GetType = "";

                foreach (var indexChecked in checkedNodes)
                {
                    GetType = indexChecked.ToString().Split()[1];
                    Types += "'"+GetType+"',";
                }
                //var query = @"select DeviceID,CamName,CamType from Cameras where CamType IN("+Types.Substring(0,Types.Length-1)+") and DeviceID in("+ AllDevices.Substring(0, AllDevices.Length - 1)+")";
                var query = @"select distinct c.DeviceID,CamName,CamType,d.ip as IP,d.DeviceType from Cameras as c
                            inner join Device as d on d.DeviceID=c.DeviceID
                            inner join NDVR AS nd on c.DeviceID=d.DeviceID
                            where c.CamType IN(" + Types.Substring(0, Types.Length - 1) + ") and c.DeviceID in(" + AllDevices.Substring(0, AllDevices.Length - 1) + ") and c.CHierLevel in(" + AllHierLevels.Substring(0, AllHierLevels.Length - 1) + ")";
                con.Open();
                var rs = con.Query<MappedTypes>(query);
                con.Close();
                foreach(var item in rs)
                {
                    int checkcam = int.Parse(item.CamName.Split('.')[1]);
                    DeviceId += "'"+item.DeviceID+"',";
                    if (item.DeviceType == "NVR")
                    {
                        if (checkcam == 0)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam1 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 1)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam2 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 2)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam3 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 3)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam4 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 4)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam5 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 5)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam6 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 6)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam7 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 7)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam8 = checkcam, IP = item.IP });
                        }
                        else if (checkcam == 8)
                        {
                            ndvrchannel.Add(new NDVRChannel { Cam9 = checkcam, IP = item.IP });
                        }
                    }
                    else if(item.DeviceType=="Camera")
                    {
                        Channels.CameraChannel++;
                    }
                }

                var b = GetData.Data.HierAssignLevl.ToArray();
                foreach (var Cred in GetCredentials(DeviceId.Substring(0, DeviceId.Length-1)))
                {
                    DeviceCred.Add(new MapDeviceCred { Name = Cred.Name, IP = Cred.IP, Username = Cred.Username, Password = Cred.Password, DeviceId = Cred.DeviceID });
                    
                }

            }
            this.Hide();
            real.Show();
        }

       

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void radTreeView1_SelectedNodeChanged(object sender, RadTreeViewEventArgs e)
        {

        }
    }
    public class MappedDevices
    {
        public string DeviceID { get; set; }
        public string DeviceType { get; set; }
        public string BranchName { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

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
    public static class Channels
    {
        public static int channel1 { get; set; }
        public static int channel2 { get; set; }
        public static int channel3 { get; set; }
        public static int channel4 { get; set; }
        public static int channel5 { get; set; }
        public static int channel6 { get; set; }
        public static int channel7 { get; set; }
        public static int channel8 { get; set; }
        public static int channel9 { get; set; }
        public static int channel10 { get; set; }
        public static int channel11 { get; set; }
        public static int channel12 { get; set; }
        public static int channel13 { get; set; }
        public static int channel14 { get; set; }
        public static int channel15 { get; set; }
        public static int channel16 { get; set; }
        public static string IP { get; set; }
        public static string Name { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static int CameraChannel { get; set; }
    }
    public class MappedTypes
    {
        public string DeviceID { get; set;}
        public string CamName { get; set; }
        public string CamType { get; set; }
        public string IP { get; set; }
        public string DeviceType { get; set; }



    }

}