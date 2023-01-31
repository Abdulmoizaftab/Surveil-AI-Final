using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetSDKCS;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using Dapper;
using Timer = System.Windows.Forms.Timer;
using static RealPlayAndPTZDemo.Settings;

namespace RealPlayAndPTZDemo
{

    public partial class RealPlayAndPTZDemo : Form
    {
        string getframe;
        List<LoginMapped> LM = new List<LoginMapped>();
        List<DetectionClass> DC = new List<DetectionClass>();
        FR FaceRecog = new FR();
        GetCam cam = new GetCam();
        Timer timer = new Timer();
        int count = 0;
        int[] GetCamId = new int[8];
        int ChannelCount1 = 0;
        int ChannelCount2 = 0;
        int ChannelCount3 = 0;
        int ChannelCount4 = 0;
        int ChannelCount5 = 0;
        int ChannelCount6 = 0;
        int ChannelCount7 = 0;
        int ChannelCount8 = 0;
        int ChannelCount9 = 0;
        int FaceTimer = 0;
        int LoginCount=0;
       
        #region Field 字段
        private const int m_WaitTime = 5000;
        private const int SyncFileSize = 5 * 1024 * 1204;
        private static fDisConnectCallBack m_DisConnectCallBack;
        private static fHaveReConnectCallBack m_ReConnectCallBack;
        private static fRealDataCallBackEx2 m_RealDataCallBackEx2;
        private static fSnapRevCallBack m_SnapRevCallBack;

        private IntPtr m_LoginID = IntPtr.Zero;
        private NET_DEVICEINFO_Ex m_DeviceInfo;
        private IntPtr m_RealPlayID = IntPtr.Zero;
        private IntPtr m_RealPlayID1 = IntPtr.Zero;
        private IntPtr m_RealPlayID2 = IntPtr.Zero;
        private IntPtr m_RealPlayID3 = IntPtr.Zero;
        private IntPtr m_RealPlayID4 = IntPtr.Zero;
        private IntPtr m_RealPlayID5 = IntPtr.Zero;
        private IntPtr m_RealPlayID6 = IntPtr.Zero;
        private IntPtr m_RealPlayID7 = IntPtr.Zero;
        private IntPtr m_RealPlayID8 = IntPtr.Zero;
        private uint m_SnapSerialNum = 1;
        private bool m_IsInSave = false;
        private int SpeedValue = 4;
        private const int MaxSpeed = 8;
        private const int MinSpeed = 1;
        #endregion


        public RealPlayAndPTZDemo()
        {
            InitializeComponent();
            label2.ForeColor = Color.Red;
            label2.Left = 10;
            label3.ForeColor = Color.Red;
            Channels.channel1 = -1;
            Channels.channel2 = -1;
            Channels.channel3 = -1;
            Channels.channel4 = -1;
            Channels.channel5 = -1;
            Channels.channel6 = -1;
            Channels.channel7 = -1;
            Channels.channel8 = -1;
            Channels.channel9 = -1;
            this.Load += new EventHandler(RealPlayAndPTZDemo_Load);

        }

        private void RealPlayAndPTZDemo_Load(object sender, EventArgs e)
        {
            m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
            m_ReConnectCallBack = new fHaveReConnectCallBack(ReConnectCallBack);
            m_RealDataCallBackEx2 = new fRealDataCallBackEx2(RealDataCallBackEx);
            m_SnapRevCallBack = new fSnapRevCallBack(SnapRevCallBack);

            try
            {
                NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
                NETClient.SetAutoReconnect(m_ReConnectCallBack, IntPtr.Zero);
                NETClient.SetSnapRevCallBack(m_SnapRevCallBack, IntPtr.Zero);
                InitOrLogoutUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
            //Build a list

            var dataSource = new List<GetRefreshRate>();
            int i = 5;
            while (i <= 500)
            {
                dataSource.Add(new GetRefreshRate() { Name = i.ToString(), Value = i });
                i = i + 5;
            }

            //Setup data binding
            this.comboBox1.DataSource = dataSource;
            this.comboBox1.DisplayMember = "Name";
            this.comboBox1.ValueMember = "Value";

            // make it readonly
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        public List<MapDeviceCred> GetMultiCred()
        {
            Settings set = new Settings();
            List<MapDeviceCred> DeviceCred = new List<MapDeviceCred>();
            foreach (var Cred in set.GetCredentials(DeviceId.TrimEnd(',')))
            {
                DeviceCred.Add(new MapDeviceCred { Name = Cred.Name, IP = Cred.IP, Username = Cred.Username, Password = Cred.Password,DeviceType=Cred.DeviceType });
            }
            return DeviceCred;
        }
        public void AgainIn()
        {
            
            Channels.channel1 = -1;
            Channels.channel2 = -1;
            Channels.channel3 = -1;
            Channels.channel4 = -1;
            Channels.channel5 = -1;
            Channels.channel6 = -1;
            Channels.channel7 = -1;
            Channels.channel8 = -1;
            Channels.channel9 = -1;
            ChannelCount1 = 0;
            ChannelCount2 = 0;
            ChannelCount3 = 0;
            ChannelCount4 = 0;
            ChannelCount5 = 0;
            ChannelCount6 = 0;
            ChannelCount7 = 0;
            ChannelCount8 = 0;
            ChannelCount9 = 0;
            //if(LogoutCount==5)
            //{
            //    LogOutAll();
            //}
            if (FRMapped.RRCheck == 1)
            {
                var GetCred = GetMultiCred();
                foreach (var item in cam.GetRefreshRate())
                {
                    for (int i = 0; i < GetCred.Count; i++)
                    {
                        if (GetCred[i].IP == item.IP)
                        {
                            MFR(item.Did, item.Score, item.IP, item.Username, item.Password, item.DeviceType);
                        }
                    }
                }
                LoginCount++;
            }
            //Face Recog
            if (FRMapped.Check == 1)
            {
                MFR();
            }

        }
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                AgainIn();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        #region CallBack 回调
        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            this.BeginInvoke((Action)UpdateDisConnectUI);
        }

        private void UpdateDisConnectUI()
        {
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Offline(离线)";
        }

        private void ReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            this.BeginInvoke((Action)UpdateReConnectUI);
        }
        private void UpdateReConnectUI()
        {
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Online(在线)";
        }

        private void RealDataCallBackEx(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr param, IntPtr dwUser)
        {
            //do something such as save data,send data,change to YUV. 比如保存数据，发送数据，转成YUV等.
        }

        private void SnapRevCallBack(IntPtr lLoginID, IntPtr pBuf, uint RevLen, uint EncodeType, uint CmdSerial, IntPtr dwUser)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "capture";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (EncodeType == 10) //.jpg
            {
                DateTime now = DateTime.Now;
                string fileName = "async" + CmdSerial.ToString() + ".jpg";
                string filePath = path + "\\" + fileName;
                byte[] data = new byte[RevLen];
                Marshal.Copy(pBuf, data, 0, (int)RevLen);
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    stream.Write(data, 0, (int)RevLen);
                    stream.Flush();
                    stream.Dispose();
                }
            }
        }
        #endregion
        private void port_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public void LogOutAll()
        {
            bool result = NETClient.Logout(m_LoginID);
            m_LoginID = IntPtr.Zero;
            if (result)
            {
                bool ret = NETClient.StopRealPlay(m_RealPlayID);
                bool ret1 = NETClient.StopRealPlay(m_RealPlayID1);
                bool ret2 = NETClient.StopRealPlay(m_RealPlayID2);
                bool ret3 = NETClient.StopRealPlay(m_RealPlayID3);
                bool ret4 = NETClient.StopRealPlay(m_RealPlayID4);
                bool ret5 = NETClient.StopRealPlay(m_RealPlayID5);
                bool ret6 = NETClient.StopRealPlay(m_RealPlayID6);
                bool ret7 = NETClient.StopRealPlay(m_RealPlayID7);
                bool ret8 = NETClient.StopRealPlay(m_RealPlayID8);
                //if (!ret && !ret1 && !ret2 && !ret3 && !ret4 && !ret5 && !ret6 && !ret7 && !ret8)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                comboBox1.Enabled = true;
                button1.Text = "Start";
                m_RealPlayID = IntPtr.Zero;
                m_RealPlayID1 = IntPtr.Zero;
                m_RealPlayID2 = IntPtr.Zero;
                m_RealPlayID3 = IntPtr.Zero;
                m_RealPlayID4 = IntPtr.Zero;
                m_RealPlayID5 = IntPtr.Zero;
                m_RealPlayID6 = IntPtr.Zero;
                m_RealPlayID7 = IntPtr.Zero;
                m_RealPlayID8 = IntPtr.Zero;
                realplay_pictureBox.Refresh();
                realplay_pictureBox1.Refresh();
                realplay_pictureBox2.Refresh();
                realplay_pictureBox3.Refresh();
                realplay_pictureBox4.Refresh();
                realplay_pictureBox5.Refresh();
                realplay_pictureBox6.Refresh();
                realplay_pictureBox7.Refresh();
                realplay_pictureBox8.Refresh();
                label2.Text = "";
                label3.Text = "";
                label4.Text = "";
                label5.Text = "";
                label6.Text = "";
                label7.Text = "";
                label8.Text = "";
                label9.Text = "";
                label10.Text = "";
                Channels.channel1 = -1;
                Channels.channel2 = -1;
                Channels.channel3 = -1;
                Channels.channel4 = -1;
                Channels.channel5 = -1;
                Channels.channel6 = -1;
                Channels.channel7 = -1;
                Channels.channel8 = -1;
                Channels.channel9 = -1;
                ChannelCount1 = 0;
                ChannelCount2 = 0;
                ChannelCount3 = 0;
                ChannelCount4 = 0;
                ChannelCount5 = 0;
                ChannelCount6 = 0;
                ChannelCount7 = 0;
                ChannelCount8 = 0;
                ChannelCount9 = 0;
            }
        }
        private void login_button_Click(object sender, EventArgs e)
        {

            LogOutAll();
           // timer.Stop();
            this.Hide();
            Login lg = new Login();
            lg.Show();



        }

        //private void start_realplay_button_Click(object sender, EventArgs e)
        //{
        //    if (IntPtr.Zero == m_RealPlayID)
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        if(streamtype_comboBox.SelectedIndex == 0)
        //        {
        //            type = EM_RealPlayType.Realplay;
        //        }
        //        else
        //        {
        //            type = EM_RealPlayType.Realplay_1;
        //        }
        //        m_RealPlayID = NETClient.RealPlay(m_LoginID, channel_comboBox.SelectedIndex, realplay_pictureBox.Handle, type);
        //        realplay_pictureBox.Refresh();
        //        if (IntPtr.Zero == m_RealPlayID)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        start_realplay_button.Text = "StopReal(停止监视)";
        //        channel_comboBox.Enabled = false;
        //        streamtype_comboBox.Enabled = false;
        //        save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID = IntPtr.Zero;
        //        start_realplay_button.Text = "StartReal(开始监视)";
        //        realplay_pictureBox.Refresh();
        //        channel_comboBox.Enabled = true;
        //        streamtype_comboBox.Enabled = true;
        //        save_button.Enabled = false;
        //        if (m_IsInSave)
        //        {
        //            m_IsInSave = false;
        //            save_button.Text = "StartSave(开始保存)";
        //        }
        //    }
        //}

        //private void capture_button_Click(object sender, EventArgs e)
        //{
        //    #region remote async snapshot 远程异步抓图
        //    NET_SNAP_PARAMS asyncSnap = new NET_SNAP_PARAMS();
        //    asyncSnap.Channel = (uint)channel_comboBox.SelectedIndex;
        //    asyncSnap.Quality = 6;
        //    asyncSnap.ImageSize = 2;
        //    asyncSnap.mode = 0;
        //    asyncSnap.InterSnap = 0;
        //    asyncSnap.CmdSerial = m_SnapSerialNum;
        //    bool ret = NETClient.SnapPictureEx(m_LoginID, asyncSnap, IntPtr.Zero);
        //    if (!ret)
        //    {
        //        MessageBox.Show(this, NETClient.GetLastError());
        //        return;
        //    }
        //    m_SnapSerialNum++;
        //    #endregion

        //    #region client capture 本地抓图
        //    //if (IntPtr.Zero == m_RealPlayID)
        //    //{
        //    //    MessageBox.Show(this, "Please realplay first(请先打开监视)!");
        //    //    return;
        //    //}
        //    //string path = AppDomain.CurrentDomain.BaseDirectory + "capture";
        //    //if (!Directory.Exists(path))
        //    //{
        //    //    Directory.CreateDirectory(path);
        //    //}
        //    //string filePath = path + "\\" + "client" + m_SnapSerialNum.ToString() + ".jpg";
        //    //bool result = NETClient.CapturePicture(m_RealPlayID, filePath, EM_NET_CAPTURE_FORMATS.JPEG);
        //    //if (!result)
        //    //{
        //    //    MessageBox.Show(this, NETClient.GetLastError());
        //    //    return;
        //    //}
        //    //MessageBox.Show(this, "client capture success(本地抓图成功)!");
        //    #endregion
        //}

        //private void save_button_Click(object sender, EventArgs e)
        //{
        //    if (IntPtr.Zero == m_RealPlayID)
        //    {
        //        MessageBox.Show(this, "Please realplay first(请先打开监视)!");
        //        return;
        //    }
        //    if (m_IsInSave)
        //    {
        //        bool ret = NETClient.StopSaveRealData(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_IsInSave = false;
        //        //save_button.Text = "StartSave(开始保存)";
        //    }
        //    else
        //    {
        //        SaveFileDialog saveFileDialog = new SaveFileDialog();
        //        saveFileDialog.FileName = "data";
        //        saveFileDialog.Filter = "|*.dav";
        //        string path = AppDomain.CurrentDomain.BaseDirectory + "savedata";
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        saveFileDialog.InitialDirectory = path;
        //        var res = saveFileDialog.ShowDialog();
        //        if (res == System.Windows.Forms.DialogResult.OK)
        //        {
        //            m_IsInSave = NETClient.SaveRealData(m_RealPlayID, saveFileDialog.FileName); //call saverealdata function.
        //            if (!m_IsInSave)
        //            {
        //                saveFileDialog.Dispose();
        //                MessageBox.Show(this, NETClient.GetLastError());
        //                return;
        //            }
        //            //save_button.Text = "StopSave(停止保存)";
        //        }
        //        saveFileDialog.Dispose();
        //    }
        //}

        //#region PTZ Control 云台控制

        //private void step_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    SpeedValue = step_comboBox.SelectedIndex + 1;
        //}

        //private void PTZControl(EM_EXTPTZ_ControlType type, int param1, int param2, bool isStop)
        //{
        //    bool ret = NETClient.PTZControl(m_LoginID, channel_comboBox.SelectedIndex, type, param1, param2, 0, isStop, IntPtr.Zero);
        //    if (!ret)
        //    {
        //        MessageBox.Show(this, NETClient.GetLastError());
        //    }
        //}

        //private void topleft_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTTOP, SpeedValue, SpeedValue, false);
        //}

        //private void topleft_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTTOP, SpeedValue, SpeedValue, true);
        //}

        //private void top_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.UP_CONTROL, 0, SpeedValue, false);
        //}

        //private void top_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.UP_CONTROL, 0, SpeedValue, true);
        //}

        //private void topright_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTTOP, SpeedValue, SpeedValue, false);
        //}

        //private void topright_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTTOP, SpeedValue, SpeedValue, true);
        //}

        //private void left_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFT_CONTROL, 0, SpeedValue, false);
        //}

        //private void left_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFT_CONTROL, 0, SpeedValue, true);
        //}

        //private void right_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHT_CONTROL, 0, SpeedValue, false);
        //}

        //private void right_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHT_CONTROL, 0, SpeedValue, true);
        //}

        //private void bottomleft_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTDOWN, SpeedValue, SpeedValue, false);
        //}

        //private void bottomleft_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTDOWN, SpeedValue, SpeedValue, true);
        //}

        //private void bottom_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.DOWN_CONTROL, 0, SpeedValue, false);
        //}

        //private void bottom_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.DOWN_CONTROL, 0, SpeedValue, true);
        //}

        //private void bottomright_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTDOWN, SpeedValue, SpeedValue, false);
        //}

        //private void bottomright_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTDOWN, SpeedValue, SpeedValue, true);
        //}

        //private void zoomadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void zoomadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void zoomdec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void zoomdec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_DEC_CONTROL, 0, SpeedValue, true);
        //}

        //private void focusadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void focusadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void focusdec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void focusdec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_DEC_CONTROL, 0, SpeedValue, true);
        //}

        //private void apertureadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void apertureadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void aperturedec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void aperturedec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_DEC_CONTROL, 0, SpeedValue, true);
        //}
        //#endregion

        #region Update UI 更新UI
        private void InitOrLogoutUI()
        {
            //step_comboBox.Enabled = false;
            //step_comboBox.Items.Clear();
            //login_button.Text = "Login(登录)";
            //channel_comboBox.Items.Clear();
            //channel_comboBox.Enabled = false;
            //streamtype_comboBox.Items.Clear();
            //streamtype_comboBox.Enabled = false;
            //start_realplay_button.Enabled = false;
            //capture_button.Enabled = false;
            //save_button.Enabled = false;
            //topleft_button.Enabled = false;
            //topright_button.Enabled = false;
            //top_button.Enabled = false;
            //left_button.Enabled = false;
            //right_button.Enabled = false;
            //bottom_button.Enabled = false;
            //bottomleft_button.Enabled = false;
            //bottomright_button.Enabled = false;
            //zoomadd_button.Enabled = false;
            //zoomdec_button.Enabled = false;
            //focusadd_button.Enabled = false;
            //focusdec_button.Enabled = false;
            //apertureadd_button.Enabled = false;
            //aperturedec_button.Enabled = false;
            m_RealPlayID = IntPtr.Zero;
            //start_realplay_button.Text = "StartReal(开始监视)";
            realplay_pictureBox.Refresh();
            realplay_pictureBox1.Refresh();
            realplay_pictureBox2.Refresh();
            //save_button.Text = "StartSave(开始保存)";
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo)";
        }
        private void LoginUI()
        {
            //step_comboBox.Enabled = true;
            //for (int i = MinSpeed; i <= MaxSpeed; i++)
            //{
            //    step_comboBox.Items.Add(i);
            //}
            //step_comboBox.SelectedIndex = SpeedValue - 1;
            login_button.Text = "Logout(登出)";
            //channel_comboBox.Enabled = true;
            //streamtype_comboBox.Enabled = true;
            //start_realplay_button.Enabled = true;
            //capture_button.Enabled = true;
            //topleft_button.Enabled = true;
            //topright_button.Enabled = true;
            //top_button.Enabled = true;
            //left_button.Enabled = true;
            //right_button.Enabled = true;
            //bottom_button.Enabled = true;
            //bottomleft_button.Enabled = true;
            //bottomright_button.Enabled = true;
            //zoomadd_button.Enabled = true;
            //zoomdec_button.Enabled = true;
            //focusadd_button.Enabled = true;
            //focusdec_button.Enabled = true;
            //apertureadd_button.Enabled = true;
            //aperturedec_button.Enabled = true;
            //for (int i = 1; i <= m_DeviceInfo.nChanNum; i++)
            //{
            //    channel_comboBox.Items.Add(i);
            //}
            //streamtype_comboBox.Items.Add("Main Stream(主码流)");
            //streamtype_comboBox.Items.Add("Extra Stream(辅码流)");
            //channel_comboBox.SelectedIndex = 0;
            //streamtype_comboBox.SelectedIndex = 0;
            //this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Online(在线)";
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NETClient.Cleanup();
        }

        public void Stream(int CameraId)
        {
            //if(CameraId==0)
            //{
            //    CameraId = 1;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }

                    NETClient.StopRealPlay(m_RealPlayID);
                    m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox.Handle, type);


                    //if (IntPtr.Zero == m_RealPlayID)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream(int CameraId, string ip, IntPtr loginid)
        {
            //if(CameraId==0)
            //{
            //    CameraId = 1;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                   
                        NETClient.StopRealPlay(m_RealPlayID);
                        m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox.Handle, type);

                    //if (IntPtr.Zero == m_RealPlayID)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream1(int CameraId)
        {
            //if (CameraId == 0)
            //{
            //    CameraId = 2;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                    NETClient.StopRealPlay(m_RealPlayID1);
                    m_RealPlayID1 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox1.Handle, type);

                    //if (IntPtr.Zero == m_RealPlayID1)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID1, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID1);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID1 = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox1.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream1(int CameraId, string ip, IntPtr loginid)
        {
            //if (CameraId == 0)
            //{
            //    CameraId = 2;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                    if (LoginCount == 0)
                    {
                        NETClient.StopRealPlay(m_RealPlayID1);
                        m_RealPlayID1 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox1.Handle, type);
                    }
                  
                    //if (IntPtr.Zero == m_RealPlayID1)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID1, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID1);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID1 = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox1.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream2(int CameraId)
        {

            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                    NETClient.StopRealPlay(m_RealPlayID2);
                    m_RealPlayID2 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox2.Handle, type);

                    //if (IntPtr.Zero == m_RealPlayID2)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID2, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID2);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID2 = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox2.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream2(int CameraId, string ip, IntPtr loginid)
        {

            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                        NETClient.StopRealPlay(m_RealPlayID2);
                        m_RealPlayID2 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox2.Handle, type);
                    
                    
                    //if (IntPtr.Zero == m_RealPlayID2)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    NETClient.SetRealDataCallBack(m_RealPlayID2, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID2);
                    //if (!ret)
                    //{
                    //    MessageBox.Show(this, NETClient.GetLastError());
                    //    return;
                    //}
                    m_RealPlayID2 = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox2.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Stream3(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID3);
                m_RealPlayID3 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox3.Handle, type);

                //if (IntPtr.Zero == m_RealPlayID3)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID3, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID3);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID3 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox3.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream3(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                    NETClient.StopRealPlay(m_RealPlayID3);
                    m_RealPlayID3 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox3.Handle, type);
                 
                //if (IntPtr.Zero == m_RealPlayID3)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID3, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID3);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID3 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox3.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream4(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID4);
                m_RealPlayID4 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox4.Handle, type);
                //if (IntPtr.Zero == m_RealPlayID4)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID4, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID4);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID4 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox4.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream4(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                    NETClient.StopRealPlay(m_RealPlayID4);
                    m_RealPlayID4 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox4.Handle, type);
            
                //if (IntPtr.Zero == m_RealPlayID4)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID4, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID4);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID4 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox4.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream5(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID5);

                m_RealPlayID5 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox5.Handle, type);

                //if (IntPtr.Zero == m_RealPlayID5)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID5, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID5);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID5 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox5.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream5(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                    NETClient.StopRealPlay(m_RealPlayID5);
                    m_RealPlayID5 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox5.Handle, type);
                
         
                //if (IntPtr.Zero == m_RealPlayID5)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID5, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID5);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID5 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox5.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream6(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID6);

                m_RealPlayID6 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox6.Handle, type);

                //if (IntPtr.Zero == m_RealPlayID6)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID6, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID6);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID6 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox6.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream6(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                    
                    NETClient.StopRealPlay(m_RealPlayID6);
                    m_RealPlayID6 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox6.Handle, type);
                
               
                //if (IntPtr.Zero == m_RealPlayID6)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID6, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID6);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID6 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox6.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream7(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID7);

                m_RealPlayID7 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox7.Handle, type);

                //if (IntPtr.Zero == m_RealPlayID7)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID7, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID7);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID7 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox7.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream7(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }

                    NETClient.StopRealPlay(m_RealPlayID7);
                    m_RealPlayID7 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox7.Handle, type);
               
                //if (IntPtr.Zero == m_RealPlayID7)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID7, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID7);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID7 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox7.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream8(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                NETClient.StopRealPlay(m_RealPlayID8);

                m_RealPlayID8 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox8.Handle, type);

                //if (IntPtr.Zero == m_RealPlayID8)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID8, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID8);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID8 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox8.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream8(int CameraId, string ip, IntPtr loginid)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
             
                    NETClient.StopRealPlay(m_RealPlayID8);
                    m_RealPlayID8 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox8.Handle, type);
         
                //if (IntPtr.Zero == m_RealPlayID8)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                NETClient.SetRealDataCallBack(m_RealPlayID8, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID8);
                //if (!ret)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                m_RealPlayID8 = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox8.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }

        private void realplay_pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            

            var GetAllChannels=cam.AllCredentials();
            if(button1.Text == "Stop")
            {
                LogOutAll();
                goto End;
              }

            MFR();
            if (FRMapped.RRCheck == 1)
            {
                timer.Interval = (10 * 1000);
                timer.Tick += new EventHandler(timer_Tick);
                comboBox1.Enabled = false;
                timer.Start();
            }
            //fOR fACE Recog
            if (FRMapped.Check == 1)
            {
                timer.Interval = (5 * 1000);
                timer.Tick += new EventHandler(timer_Tick);
                comboBox1.Enabled = false;
                timer.Start();
                FaceTimer++;
            }
            //For Refresh Rate
           
            button1.Text = "Stop";
        End:
            if(button1.Text=="Start")
            {
                button1.Text = "Start";
            }
        }
         
        
        private void realplay_pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void MFR()
        {
            var GetCred = GetMultiCred();
            for (int i = 0; i < GetCred.Count; i++)
            {

                getframe = comboBox1.SelectedValue.ToString();
                ushort port = 0;
                try
                {
                    port = Convert.ToUInt16(37777);
                }
                catch
                {
                    MessageBox.Show("Input port error(输入端口错误)!");
                    //return;
                }
                try
                {
                    if (FaceTimer != 1)
                    {
                        m_DeviceInfo = new NET_DEVICEINFO_Ex();
                        m_LoginID = NETClient.Login(GetCred[i].IP, port, GetCred[i].Username, GetCred[i].Password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                        if (m_DeviceInfo.nDVRType == 0)
                        {
                            //Logic will become.
                        }
                        if (IntPtr.Zero == m_LoginID)
                        {
                            MessageBox.Show(this, NETClient.GetLastError());
                            return;
                        }
                    }
                    //Face Recgnition
                    if (FRMapped.Check == 1)
                    {
                        cam.FaceRecog();
                        if (TakeCam.cam.Count != 0)
                        {
                            FRMapped.Person_Name = TakeCam.cam[0].Person_Name;
                            FRMapped.Probability = TakeCam.cam[0].Probability;
                            FRMapped.Did = TakeCam.cam[0].Did;
                            FRMapped.IP = TakeCam.cam[0].IP;
                        }
                        if (TakeCam.cam1.Count != 0)
                        {
                            FRMapped.Person_Name1 = TakeCam.cam1[0].Person_Name1;
                            FRMapped.Probability1 = TakeCam.cam1[0].Probability1;
                            FRMapped.Did1 = TakeCam.cam1[0].Did1;
                            FRMapped.IP1 = TakeCam.cam1[0].IP1;
                        }
                        if (TakeCam.cam2.Count != 0)
                        {
                            FRMapped.Person_Name2 = TakeCam.cam2[0].Person_Name2;
                            FRMapped.Probability2 = TakeCam.cam2[0].Probability2;
                            FRMapped.Did2 = TakeCam.cam2[0].Did2;
                            FRMapped.IP2 = TakeCam.cam2[0].IP2;

                        }
                        if (TakeCam.cam3.Count != 0)
                        {
                            FRMapped.Person_Name3 = TakeCam.cam3[0].Person_Name3;
                            FRMapped.Probability3 = TakeCam.cam3[0].Probability3;
                            FRMapped.Did3 = TakeCam.cam3[0].Did3;
                            FRMapped.IP3 = TakeCam.cam3[0].IP3;

                        }
                        if (TakeCam.cam4.Count != 0)
                        {
                            FRMapped.Person_Name4 = TakeCam.cam4[0].Person_Name4;
                            FRMapped.Probability4 = TakeCam.cam4[0].Probability4;
                            FRMapped.Did4 = TakeCam.cam4[0].Did4;
                            FRMapped.IP4 = TakeCam.cam4[0].IP4;

                        }
                        if (TakeCam.cam5.Count != 0)
                        {
                            FRMapped.Person_Name5 = TakeCam.cam5[0].Person_Name5;
                            FRMapped.Probability5 = TakeCam.cam5[0].Probability5;
                            FRMapped.Did5 = TakeCam.cam5[0].Did5;
                            FRMapped.IP5 = TakeCam.cam5[0].IP5;

                        }
                        if (TakeCam.cam6.Count != 0)
                        {
                            FRMapped.Person_Name6 = TakeCam.cam6[0].Person_Name6;
                            FRMapped.Probability6 = TakeCam.cam6[0].Probability6;
                            FRMapped.Did6 = TakeCam.cam6[0].Did6;
                            FRMapped.IP6 = TakeCam.cam6[0].IP6;

                        }
                        if (TakeCam.cam7.Count != 0)
                        {
                            FRMapped.Person_Name7 = TakeCam.cam7[0].Person_Name7;
                            FRMapped.Probability7 = TakeCam.cam7[0].Probability7;
                            FRMapped.Did7 = TakeCam.cam7[0].Did7;
                            FRMapped.IP7 = TakeCam.cam7[0].IP7;

                        }
                        if (TakeCam.cam8.Count != 0)
                        {
                            FRMapped.Person_Name8 = TakeCam.cam8[0].Person_Name8;
                            FRMapped.Probability8 = TakeCam.cam8[0].Probability8;
                            FRMapped.Did8 = TakeCam.cam8[0].Did8;
                            FRMapped.IP8 = TakeCam.cam8[0].IP8;

                        }
                    }

                        foreach (var GetCamera in ndvrchannel)
                        {
                            if (GetCred[i].IP == GetCamera.IP)
                            {
                                Channels.channel1 = GetCamera.Cam1 != -1 && Channels.channel1 == -1 ? GetCamera.Cam1 : Channels.channel1;
                                Channels.channel2 = GetCamera.Cam2 != -1 && Channels.channel2 == -1 ? GetCamera.Cam2 : Channels.channel2;
                                Channels.channel3 = GetCamera.Cam3 != -1 && Channels.channel3 == -1 ? GetCamera.Cam3 : Channels.channel3;
                                Channels.channel4 = GetCamera.Cam4 != -1 && Channels.channel4 == -1 ? GetCamera.Cam4 : Channels.channel4;
                                Channels.channel5 = GetCamera.Cam5 != -1 && Channels.channel5 == -1 ? GetCamera.Cam5 : Channels.channel5;
                                Channels.channel6 = GetCamera.Cam6 != -1 && Channels.channel6 == -1 ? GetCamera.Cam6 : Channels.channel6;
                                Channels.channel7 = GetCamera.Cam7 != -1 && Channels.channel7 == -1 ? GetCamera.Cam7 : Channels.channel7;
                                Channels.channel8 = GetCamera.Cam8 != -1 && Channels.channel8 == -1 ? GetCamera.Cam8 : Channels.channel8;
                                Channels.channel9 = GetCamera.Cam9 != -1 && Channels.channel9 == -1 ? GetCamera.Cam9 : Channels.channel9;
                            }
                        }   //For Camera Stream
                        if (GetCred[i].DeviceType == "Camera")
                        {
                            Channels.channel1 = Channels.channel1 == -1 ? 0 : -1;
                            if (Channels.channel1 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream(Channels.channel1);
                                }
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label2.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label2.Text = "";
                                }
                                ChannelCount1++;
                                goto NDVR;
                            }
                            Channels.channel2 = Channels.channel2 == -1 ? 0 : -1;
                            if (Channels.channel2 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream1(Channels.channel2);
                                }
                               
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label3.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                ChannelCount2++;
                                goto NDVR;
                            }
                            Channels.channel3 = Channels.channel3 == -1 ? 0 : -1;
                            if (Channels.channel3 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream2(Channels.channel3);
                                }
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label4.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label4.Text = "";
                                }
                                ChannelCount3++;
                                goto NDVR;
                            }
                            Channels.channel4 = Channels.channel4 == -1 ? 0 : -1;
                            if (Channels.channel4 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream3(Channels.channel4);
                                }
                                
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label5.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label5.Text = "";
                                }
                                ChannelCount4++;
                                goto NDVR;
                            }
                            Channels.channel5 = Channels.channel5 == -1 ? 0 : -1;
                            if (Channels.channel5 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream4(Channels.channel5);
                                }
                                
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label6.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label6.Text = "";
                                }
                                ChannelCount5++;
                                goto NDVR;
                            }
                            Channels.channel6 = Channels.channel6 == -1 ? 0 : -1;
                            if (Channels.channel6 != -1)
                            {

                               
                                if (FaceTimer != 1)
                                {
                                    Stream5(Channels.channel6);
                                }
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label7.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label7.Text = "";
                                }
                                ChannelCount6++;
                                goto NDVR;
                            }
                            Channels.channel7 = Channels.channel7 == -1 ? 0 : -1;
                            if (Channels.channel7 != -1)
                            {

                                
                                if (FaceTimer != 1)
                                {
                                    Stream6(Channels.channel7);
                                }
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label8.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label8.Text = "";
                                }
                                ChannelCount7++;
                                goto NDVR;
                            }
                            Channels.channel8 = Channels.channel8 == -1 ? 0 : -1;
                            if (Channels.channel8 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream7(Channels.channel8);
                                }
                               
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label9.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label9.Text = "";
                                }
                                ChannelCount8++;
                                goto NDVR;
                            }
                            Channels.channel9 = Channels.channel9 == -1 ? 0 : -1;
                            if (Channels.channel9 != -1)
                            {
                                if (FaceTimer != 1)
                                {
                                    Stream8(Channels.channel9);
                                }
                               
                                if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                }
                                else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                {
                                    label10.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                }
                                else
                                {
                                    label10.Text = "";
                                }
                                ChannelCount9++;
                                goto NDVR;
                            }
                        }
                    //For NVR/DVR Stream
                    NDVR:
                        if (GetCred[i].DeviceType == "NVR" || GetCred[i].DeviceType == "DVR")
                        {

                            if (Channels.channel1 != -1)
                            {
                                if (ChannelCount1 == 1)
                                {
                                    Channels.channel2 = Channels.channel2 == -1 ? Channels.channel1 : Channels.channel2;
                                    if (Channels.channel1 == Channels.channel2)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel3 = Channels.channel3 == -1 ? Channels.channel1 : Channels.channel3;
                                    if (Channels.channel1 == Channels.channel3)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel4 = Channels.channel4 == -1 ? Channels.channel1 : Channels.channel4;
                                    if (Channels.channel1 == Channels.channel4)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel5 = Channels.channel5 == -1 ? Channels.channel1 : Channels.channel5;
                                    if (Channels.channel1 == Channels.channel5)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel6 = Channels.channel6 == -1 ? Channels.channel1 : Channels.channel6;
                                    if (Channels.channel1 == Channels.channel2)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel1 : Channels.channel7;
                                    if (Channels.channel1 == Channels.channel7)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel1 : Channels.channel8;
                                    if (Channels.channel1 == Channels.channel8)
                                    {
                                        goto Forward2;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel1 : Channels.channel9;
                                    if (Channels.channel1 == Channels.channel9)
                                    {
                                        goto Forward2;
                                    }
                                }
                                if (ChannelCount1 == 0)
                                {
                                    if (FaceTimer != 1)
                                    {
                                        Stream(Channels.channel1);
                                    }

                                ChannelCount1++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel1 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel1 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel1 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel1 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel1 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel1 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel1 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel1 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel1 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label2.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                }
                            }
                        Forward2:
                            if (Channels.channel2 != -1)
                            {
                                if (ChannelCount2 == 1)
                                {
                                    Channels.channel3 = Channels.channel3 == -1 ? Channels.channel2 : Channels.channel3;
                                    if (Channels.channel2 == Channels.channel3)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel4 = Channels.channel4 == -1 ? Channels.channel2 : Channels.channel4;
                                    if (Channels.channel2 == Channels.channel4)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel5 = Channels.channel5 == -1 ? Channels.channel2 : Channels.channel5;
                                    if (Channels.channel2 == Channels.channel5)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel6 = Channels.channel6 == -1 ? Channels.channel2 : Channels.channel6;
                                    if (Channels.channel2 == Channels.channel2)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel2 : Channels.channel7;
                                    if (Channels.channel2 == Channels.channel7)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel2 : Channels.channel8;
                                    if (Channels.channel2 == Channels.channel8)
                                    {
                                        goto Forward3;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel2 : Channels.channel9;
                                    if (Channels.channel2 == Channels.channel9)
                                    {
                                        goto Forward3;
                                    }
                                }
                                if (ChannelCount2 == 0)
                                {
                                    
                                    if (FaceTimer != 1)
                                    {
                                        Stream1(Channels.channel2);
                                    }
                                    ChannelCount2++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel2 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel2 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel2 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel2 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel2 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel2 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel2 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel2 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel2 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label3.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                }
                            }

                        Forward3:
                            if (Channels.channel3 != -1)
                            {
                                if (ChannelCount3 == 1)
                                {
                                    Channels.channel4 = Channels.channel4 == -1 ? Channels.channel3 : Channels.channel4;
                                    if (Channels.channel3 == Channels.channel4)
                                    {
                                        goto Forward4;
                                    }
                                    Channels.channel5 = Channels.channel5 == -1 ? Channels.channel3 : Channels.channel5;
                                    if (Channels.channel3 == Channels.channel5)
                                    {
                                        goto Forward4;
                                    }
                                    Channels.channel6 = Channels.channel6 == -1 ? Channels.channel3 : Channels.channel6;
                                    if (Channels.channel3 == Channels.channel6)
                                    {
                                        goto Forward4;
                                    }
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel3 : Channels.channel7;
                                    if (Channels.channel3 == Channels.channel7)
                                    {
                                        goto Forward4;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel3 : Channels.channel8;
                                    if (Channels.channel3 == Channels.channel8)
                                    {
                                        goto Forward4;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel3 : Channels.channel9;
                                    if (Channels.channel3 == Channels.channel9)
                                    {
                                        goto Forward4;
                                    }
                                }
                                if (ChannelCount3 == 0)
                                {
                                    if (FaceTimer != 1)
                                    {
                                        Stream2(Channels.channel3);
                                    }
                                    
                                    ChannelCount3++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel3 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel3 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel3 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel3 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel3 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel3 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel3 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel3 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel3 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label4.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label4.Text = "";
                                    }

                                }
                            }
                        Forward4:
                            if (Channels.channel4 != -1)
                            {
                                if (ChannelCount4 == 1)
                                {
                                    Channels.channel5 = Channels.channel5 == -1 ? Channels.channel4 : Channels.channel5;
                                    if (Channels.channel4 == Channels.channel5)
                                    {
                                        goto Forward5;
                                    }
                                    Channels.channel6 = Channels.channel6 == -1 ? Channels.channel4 : Channels.channel6;
                                    if (Channels.channel4 == Channels.channel2)
                                    {
                                        goto Forward5;
                                    }
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel4 : Channels.channel7;
                                    if (Channels.channel4 == Channels.channel7)
                                    {
                                        goto Forward5;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel4 : Channels.channel8;
                                    if (Channels.channel4 == Channels.channel8)
                                    {
                                        goto Forward5;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel4 : Channels.channel9;
                                    if (Channels.channel4 == Channels.channel9)
                                    {
                                        goto Forward5;
                                    }
                                }
                                if (ChannelCount4 == 0)
                                {
                                   
                                    if (FaceTimer != 1)
                                    {
                                        Stream3(Channels.channel4);
                                    }
                                    ChannelCount4++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel4 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel2 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel2 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel2 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel2 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel2 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel2 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel2 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel2 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label5.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label5.Text = "";
                                    }
                                }
                            }
                        Forward5:
                            if (Channels.channel5 != -1)
                            {
                                if (ChannelCount5 == 1)
                                {
                                    Channels.channel6 = Channels.channel6 == -1 ? Channels.channel5 : Channels.channel6;
                                    if (Channels.channel5 == Channels.channel2)
                                    {
                                        goto Forward6;
                                    }
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel5 : Channels.channel7;
                                    if (Channels.channel5 == Channels.channel7)
                                    {
                                        goto Forward6;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel5 : Channels.channel8;
                                    if (Channels.channel5 == Channels.channel8)
                                    {
                                        goto Forward6;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel5 : Channels.channel9;
                                    if (Channels.channel5 == Channels.channel9)
                                    {
                                        goto Forward6;
                                    }
                                }
                                if (ChannelCount5 == 0)
                                {
                                    
                                    if (FaceTimer != 1)
                                    {
                                        Stream4(Channels.channel5);
                                    }
                                    ChannelCount5++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel5 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel5 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel5 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel5 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel5 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel5 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel5 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel5 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel5 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label6.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label6.Text = "";
                                    }
                                }
                            }
                        Forward6:
                            if (Channels.channel6 != -1)
                            {
                                if (ChannelCount6 == 1)
                                {
                                    Channels.channel7 = Channels.channel7 == -1 ? Channels.channel6 : Channels.channel7;
                                    if (Channels.channel6 == Channels.channel7)
                                    {
                                        goto Forward7;
                                    }
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel6 : Channels.channel8;
                                    if (Channels.channel6 == Channels.channel8)
                                    {
                                        goto Forward7;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel6 : Channels.channel9;
                                    if (Channels.channel6 == Channels.channel9)
                                    {
                                        goto Forward7;
                                    }
                                }
                                if (ChannelCount6 == 0)
                                {
                                  
                                    if (FaceTimer != 1)
                                    {
                                        Stream5(Channels.channel6);
                                    }
                                    ChannelCount6++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel6 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel6 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel6 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel6 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel6 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel6 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel6 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel6 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel6 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label7.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label7.Text = "";
                                    }
                                }
                            }
                        Forward7:
                            if (Channels.channel7 != -1)
                            {
                                if (ChannelCount7 == 1)
                                {
                                    Channels.channel8 = Channels.channel8 == -1 ? Channels.channel7 : Channels.channel8;
                                    if (Channels.channel7 == Channels.channel8)
                                    {
                                        goto Forward8;
                                    }
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel7 : Channels.channel9;
                                    if (Channels.channel7 == Channels.channel9)
                                    {
                                        goto Forward8;
                                    }
                                }
                                if (ChannelCount7 == 0)
                                {
                                   
                                    if (FaceTimer != 1)
                                    {
                                        Stream6(Channels.channel7);
                                    }
                                    ChannelCount7++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel7 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel7 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel7 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel7 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel7 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel7 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel7 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel7 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel7 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label8.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label8.Text = "";
                                    }
                                }
                            }
                        Forward8:
                            if (Channels.channel8 != -1)
                            {
                                if (ChannelCount8 == 1)
                                {
                                    Channels.channel9 = Channels.channel9 == -1 ? Channels.channel8 : Channels.channel9;
                                    if (Channels.channel8 == Channels.channel9)
                                    {
                                        goto Forward9;
                                    }
                                }
                                if (ChannelCount8 == 0)
                                {
                                    
                                    if (FaceTimer != 1)
                                    {
                                        Stream7(Channels.channel8);
                                    }
                                    ChannelCount8++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == Channels.channel8 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == Channels.channel8 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == Channels.channel8 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == Channels.channel8 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == Channels.channel8 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == Channels.channel8 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == Channels.channel8 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == Channels.channel8 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == Channels.channel8 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label9.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label9.Text = "";
                                    }
                                }
                            }
                        Forward9:
                            if (Channels.channel9 != -1)
                            {
                                if (ChannelCount9 == 0)
                                {
                                   

                                    if (FaceTimer != 1)
                                    {
                                        Stream8(Channels.channel9);
                                    }
                                    ChannelCount9++;
                                    if (FRMapped.Check == 1 && FRMapped.Did == 0 && FRMapped.IP == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name + " " + FRMapped.Probability + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did1 == 1 && FRMapped.IP1 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name1 + " " + FRMapped.Probability1 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did2 == 2 && FRMapped.IP2 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name2 + " " + FRMapped.Probability2 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did3 == 3 && FRMapped.IP3 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name3 + " " + FRMapped.Probability3 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did4 == 4 && FRMapped.IP4 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name4 + " " + FRMapped.Probability4 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did5 == 5 && FRMapped.IP5 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name5 + " " + FRMapped.Probability5 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did6 == 6 && FRMapped.IP6 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name6 + " " + FRMapped.Probability6 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did7 == 7 && FRMapped.IP7 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name7 + " " + FRMapped.Probability7 + GetCred[i].IP;
                                    }
                                    else if (FRMapped.Check == 1 && FRMapped.Did8 == 8 && FRMapped.IP8 == GetCred[i].IP)
                                    {
                                        label10.Text = FRMapped.Person_Name8 + " " + FRMapped.Probability8 + GetCred[i].IP;
                                    }
                                    else
                                    {
                                        label10.Text = "";
                                    }
                                }
                            }
                        }

                       
                      
                        

                    }
                
                catch (Exception ex)
                {
                    throw ex;
                }


            }
        }
        public void MFR(int Did, int Score, string IP, string Username, string Password, string DeviceType)
        {
                getframe = comboBox1.SelectedValue.ToString();
                ushort port = 0;
                try
                {
                    port = Convert.ToUInt16(37777);
                }
                catch
                {
                    MessageBox.Show("Input port error(输入端口错误)!");
                    return;
                }
                try
                {
                if (FaceTimer != 1)
                {

                        m_DeviceInfo = new NET_DEVICEINFO_Ex();
                        m_LoginID = NETClient.Login(IP, port, Username, Password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                        if (m_DeviceInfo.nDVRType == 0)
                        {
                            //Logic will become.
                        }
                        if (IntPtr.Zero == m_LoginID)
                        {
                            MessageBox.Show(this, NETClient.GetLastError());
                            return;
                        }
                    
                }
                    if (FRMapped.Check == 1)
                    {
                        //Face Recgnition
                        cam.FaceRecog();
                        if (TakeCam.cam.Count != 0)
                        {
                            FRMapped.Person_Name = TakeCam.cam[0].Person_Name;
                            FRMapped.Probability = TakeCam.cam[0].Probability;
                            FRMapped.Did = TakeCam.cam[0].Did;
                            FRMapped.IP = TakeCam.cam[0].IP;
                        }
                        if (TakeCam.cam1.Count != 0)
                        {
                            FRMapped.Person_Name1 = TakeCam.cam1[0].Person_Name1;
                            FRMapped.Probability1 = TakeCam.cam1[0].Probability1;
                            FRMapped.Did1 = TakeCam.cam1[0].Did1;
                            FRMapped.IP1 = TakeCam.cam1[0].IP1;
                        }
                        if (TakeCam.cam2.Count != 0)
                        {
                            FRMapped.Person_Name2 = TakeCam.cam2[0].Person_Name2;
                            FRMapped.Probability2 = TakeCam.cam2[0].Probability2;
                            FRMapped.Did2 = TakeCam.cam2[0].Did2;
                            FRMapped.IP2 = TakeCam.cam2[0].IP2;

                        }
                        if (TakeCam.cam3.Count != 0)
                        {
                            FRMapped.Person_Name3 = TakeCam.cam3[0].Person_Name3;
                            FRMapped.Probability3 = TakeCam.cam3[0].Probability3;
                            FRMapped.Did3 = TakeCam.cam3[0].Did3;
                            FRMapped.IP3 = TakeCam.cam3[0].IP3;

                        }
                        if (TakeCam.cam4.Count != 0)
                        {
                            FRMapped.Person_Name4 = TakeCam.cam4[0].Person_Name4;
                            FRMapped.Probability4 = TakeCam.cam4[0].Probability4;
                            FRMapped.Did4 = TakeCam.cam4[0].Did4;
                            FRMapped.IP4 = TakeCam.cam4[0].IP4;

                        }
                        if (TakeCam.cam5.Count != 0)
                        {
                            FRMapped.Person_Name5 = TakeCam.cam5[0].Person_Name5;
                            FRMapped.Probability5 = TakeCam.cam5[0].Probability5;
                            FRMapped.Did5 = TakeCam.cam5[0].Did5;
                            FRMapped.IP5 = TakeCam.cam5[0].IP5;

                        }
                        if (TakeCam.cam6.Count != 0)
                        {
                            FRMapped.Person_Name6 = TakeCam.cam6[0].Person_Name6;
                            FRMapped.Probability6 = TakeCam.cam6[0].Probability6;
                            FRMapped.Did6 = TakeCam.cam6[0].Did6;
                            FRMapped.IP6 = TakeCam.cam6[0].IP6;

                        }
                        if (TakeCam.cam7.Count != 0)
                        {
                            FRMapped.Person_Name7 = TakeCam.cam7[0].Person_Name7;
                            FRMapped.Probability7 = TakeCam.cam7[0].Probability7;
                            FRMapped.Did7 = TakeCam.cam7[0].Did7;
                            FRMapped.IP7 = TakeCam.cam7[0].IP7;

                        }
                        if (TakeCam.cam8.Count != 0)
                        {
                            FRMapped.Person_Name8 = TakeCam.cam8[0].Person_Name8;
                            FRMapped.Probability8 = TakeCam.cam8[0].Probability8;
                            FRMapped.Did8 = TakeCam.cam8[0].Did8;
                            FRMapped.IP8 = TakeCam.cam8[0].IP8;

                        }
                    }

                dynamic LoginID = from aluu in LM.Where(x => x.IP == IP && x.Did == Did)
                          select aluu.LoginId;
                //IntPtr logID= (IntPtr)0x0000024f783a6350;
                IntPtr logID=IntPtr.Zero;
                foreach (var items in LoginID)
                {
                    logID = items;
                }
                if (ChannelCount1 != 1)
                {
                    if (Channels.channel1 == -1)
                    {
                        Stream(Did);
                        ChannelCount1++;
                        label11.Text = IP +" "+ DeviceType;
                        
                    }
                }
                else if (ChannelCount2 != 1)
                {
                if (Channels.channel2 == -1)
                    {
                        Stream1(Did);
                        ChannelCount2++;
                        label12.Text = IP + " " + DeviceType;
                    }
                }
               else if (ChannelCount3 != 1)
                {
                if (Channels.channel3 == -1)
                    {
                        Stream2(Did);
                        ChannelCount3++;
                        label13.Text = IP + " " + DeviceType;

                    }
                }
               else if (ChannelCount4 != 1)
                {
                 if (Channels.channel4 == -1)
                    {
                        Stream3(Did);
                        ChannelCount4++;
                        label14.Text = IP + " " + DeviceType;

                    }
                }
                else if (ChannelCount5 != 1)
                {
                if (Channels.channel5 == -1)
                    {
                        Stream4(Did);
                        ChannelCount5++;
                        label15.Text = IP + " " + DeviceType;

                    }
                }
                else if (ChannelCount6 != 1)
                {
                    if (Channels.channel6 == -1)
                    {
                        Stream5(Did);
                        ChannelCount6++;
                        label16.Text = IP + " " + DeviceType;

                    }
                }
                else if (ChannelCount7 != 1)
                {
                    if (Channels.channel7 == -1)
                    {
                        Stream6(Did);
                        ChannelCount7++;
                        label17.Text = IP + " " + DeviceType;

                    }
                }
                else if (ChannelCount8 != 1)
                {
                    if (Channels.channel8 == -1)
                    {
                        Stream7(Did);
                        ChannelCount8++;
                        label18.Text = IP + " " + DeviceType;

                    }
                }
               else if (ChannelCount9 != 1)
                {
                    if (Channels.channel9 == -1)
                    {
                        Stream8(Did);
                        ChannelCount9++;
                        label19.Text = IP + " " + DeviceType;

                    }
                }
               }
                catch (Exception ex)
                {
                    throw ex;
                }
            
        }
    }
    public static class TakeCam
    {
        public static dynamic cam;
        public static dynamic cam1;
        public static dynamic cam2;
        public static dynamic cam3;
        public static dynamic cam4;
        public static dynamic cam5;
        public static dynamic cam6;
        public static dynamic cam7;
        public static dynamic cam8;
    }
    public class GetCam
    {

        SqlConnection con = new SqlConnection("Data Source=192.168.1.100; Initial Catalog = RTODE; User ID = sa; Password=Dev@2022");
        SqlConnection con1 = new SqlConnection("Data Source=192.168.1.100; Initial Catalog = SurveilAI; User ID = sa; Password=Dev@2022");
        public List<DetectionClass> GetRefreshRate()
        {
            try
            {
                //frametime = "-" + frametime;
                //var query = @"Select Did,sum(Score) as Score from Object_Detection where DateTimee>DATEADD(SECOND," + int.Parse(frametime) + ",CURRENT_TIMESTAMP)  group by Did order by Score desc";
                var query = @"Select r.Did,r.IP,sum(r.Score) as Score,s.NDusername as Username,s.NDpassword as Password,d.DeviceType from [RTODE].[dbo].Object_Detection as r
                              inner join [SurveilAI].[dbo].NDVR as s on r.IP=s.NDip
                              inner join [SurveilAI].[dbo].Device as d on d.IP=r.IP
                              where DateTimee>DATEADD(SECOND,-1000,CURRENT_TIMESTAMP)  
                              group by Did,r.IP,s.NDusername,NDpassword,d.DeviceType order by Score desc";
                con.Open();
                var rs = con.Query<DetectionClass>(query);
                con.Close();
                return rs.ToList();
            }

            catch (Exception ex)
            {
                con.Close();
                throw ex;
            }
        }
       

        public void FaceRecog()
        {
            try
            {
                var query = @"select Top 1 DeviceId as Did,Person_Name as Person_Name,Probability as Probability,IP from FR_Logs5 where DeviceId=0 order by Log_id desc
                            select Top 1 DeviceId as Did1,Person_Name as Person_Name1,Probability as Probability1,IP as IP1 from FR_Logs5 where DeviceId=1 order by Log_id desc
                            select Top 1 DeviceId as Did2,Person_Name as Person_Name2,Probability as Probability2,IP as IP2 from FR_Logs5 where DeviceId=2 order by Log_id desc
                            select Top 1 DeviceId as Did3,Person_Name as Person_Name3,Probability as Probability3,IP as IP3 from FR_Logs5 where DeviceId=3 order by Log_id desc
                            select Top 1 DeviceId as Did4,Person_Name as Person_Name4,Probability as Probability4,IP as IP4 from FR_Logs5 where DeviceId=4 order by Log_id desc
                            select Top 1 DeviceId as Did5,Person_Name as Person_Name5,Probability as Probability5,IP as IP5 from FR_Logs5 where DeviceId=5 order by Log_id desc
                            select Top 1 DeviceId as Did6,Person_Name as Person_Name6,Probability as Probability6,IP as IP6 from FR_Logs5 where DeviceId=6 order by Log_id desc
                            select Top 1 DeviceId as Did7,Person_Name as Person_Name7,Probability as Probability7,IP as IP7 from FR_Logs5 where DeviceId=7 order by Log_id desc
                            select Top 1 DeviceId as Did8,Person_Name as Person_Name8,Probability as Probability8,IP as IP8 from FR_Logs5 where DeviceId=8 order by Log_id desc";
                con.Open();
                var rs = con.QueryMultiple(query);
                TakeCam.cam = rs.Read<FR>();
                TakeCam.cam1 = rs.Read<FR1>();
                TakeCam.cam2 = rs.Read<FR2>();
                TakeCam.cam3 = rs.Read<FR3>();
                TakeCam.cam4 = rs.Read<FR4>();
                TakeCam.cam5 = rs.Read<FR5>();
                TakeCam.cam6 = rs.Read<FR6>();
                TakeCam.cam7 = rs.Read<FR7>();
                TakeCam.cam8 = rs.Read<FR8>();
                con.Close();
            }
            catch (Exception ex)
            {
                con.Close();
                throw ex;
            }
        }

        public List<MapDeviceCred> AllCredentials()
        {
            try
            {
                var query = @"select NDip AS IP,NDusername as Username,NDpassword as Password from NDVR";
                con1.Open();
                var rs = con1.Query<MapDeviceCred>(query);
                con1.Close();
                return rs.ToList();
            }
            catch (Exception ex)
            {
                con1.Close();
                throw ex;
            }

        }
    }

}
