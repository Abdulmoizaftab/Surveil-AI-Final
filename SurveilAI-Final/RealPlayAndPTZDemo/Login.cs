using Dapper;
//using MySql.Data.MySqlClient.Memcached;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using Telerik.WinControls.Commands;
using static System.Net.WebRequestMethods;

namespace RealPlayAndPTZDemo
{
  
  
    public partial class Login : Form
    {
        RealPlayAndPTZDemo real = new RealPlayAndPTZDemo();
        public static dynamic getvalue;
        SqlConnection con = new SqlConnection("Data Source=192.168.1.100; Initial Catalog = SurveilAI; User ID = sa; Password=Dev@2022");
        public Login()
        {
            InitializeComponent();
            comboBox1.SelectedIndex=0;
            comboBox1.Items.Add("Static");
            comboBox1.Items.Add("Dynamic");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
       

        private async void login_button_Click(object sender, EventArgs e)
        {
            MapLogin res = new MapLogin();
            HttpClient client = new HttpClient();
           
            res.user = textBox1.Text;
            res.password = textBox2.Text;
            string atms = "";
            var url = "http://localhost:38735/api/DesktopApi/Login";

            var serialized = JsonConvert.SerializeObject(res);
            var stringContent = new StringContent(serialized, System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url,stringContent).ConfigureAwait(true);
            //string responseBody = await result.Content.ReadAsStringAsync();
            string result = resp.Content.ReadAsStringAsync().Result;
            var ress = JsonConvert.DeserializeObject<Root>(result);

            //api

            //MessageBox.Show(ress.Status.ToString()+ atms+ ress.Data.UserID + ress.Data.Message);
            if (ress.Status == 200)
            {
                if (ress.Data.HierAssignLevl != null)
                {
                    //foreach (var item in ress.Data.HierAssignLevl)
                    //{
                    //    atms+= item.ToString();
                    //}
                }
                else
                {
                    foreach (var item in ress.Data.ATM)
                    {
                        atms += item.ToString();
                    }
                }
                GetData.Status = ress.Status;
                GetData.Data = ress.Data;
                getvalue = comboBox1.SelectedItem;
                if (getvalue != "--Select View--")
                {
                    Settings f2 = new Settings();
                    f2.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Kindly, Select View");
                }
                
            }


        }




        public List<LoginClass> GetLogin(string email,string password)
        {
            try
            {
                var query = @"select UserID,Password from Users where UserID='"+email+"' and Password='"+password+"'";
                con.Open();
                var rs = con.Query<LoginClass>(query);
                con.Close();
                return rs.ToList();
            }

            catch (Exception ex)
            {
                con.Close();
                MessageBox.Show(ex.ToString());
                throw ex;
            }

        }

        private void Login_Load(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void radMultiColumnComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    public class MapLogin
    {
        public string user { get; set; }
        public string password { get; set; }
    }
    public class Data
    {
        public string UserID { get; set; }
        public List<string> ATM { get; set; }
        public string AccountType { get; set; }
        public string Message { get; set; }
        public List<string> Devices { get; set; }
        public List<string> DeviceType { get; set; }
        public List<string> HierName { get; set; }
        public List<HierAssignLevel> HierAssignLevl { get; set; }

    }
    public class HierAssignLevel
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

    public class Root
    {
        public int Status { get; set; }
        public Data Data { get; set; }
    }
    public static class GetData
    {
        public static int Status { get; set; }
        public static Data Data { get; set; }


    }
}
