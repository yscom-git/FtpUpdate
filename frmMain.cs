using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Net.NetworkInformation;


namespace FTP_Downloader
{
    public partial class frmMain : Form
    {
        private int iFile = 1;
        private int iMax = 0;
        private int iCount = 0;
        private bool m_bExit = false;
        public frmMain()
        {
            InitializeComponent();
        }
        private DateTime m_DTStart = DateTime.Now;
        public ProcessInfo m_sProcessInfo;
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (!getLoadIniData())
                {
                    Application.Exit();
                }
                else
                {
                   
                    this.Text = m_sProcessInfo.strCompanyName + " FTP Downlader   [Ver:" + Assembly.GetExecutingAssembly().GetName().Version.ToString() +"]";
                    Lbl_Logo.Text = m_sProcessInfo.strCompanyName + "-" + m_sProcessInfo.strPlantName;
                    timer1.Enabled = true;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private bool getLoadIniData()
        {
            //실행 파일 폴더 가져오기
            string strFileName = @".\FTP_Downloader.ini";

            //FTP
            m_sProcessInfo.strServer_ip=IniControl.ReadString(strFileName, "FTP_SERVER", "SERVER_IP", "10.10.32.1");
            m_sProcessInfo.strUser_id=IniControl.ReadString(strFileName, "FTP_SERVER", "USER_ID", "mes");
            m_sProcessInfo.strPassword=IniControl.ReadString(strFileName, "FTP_SERVER", "PASSWORD", "mes");
            m_sProcessInfo.strDownloadPath=IniControl.ReadString(strFileName, "FTP_SERVER", "DOWNLOAD_PATH", "MES_Process");
            m_sProcessInfo.strProgram_name=IniControl.ReadString(strFileName, "FTP_SERVER", "PROGRAM_NAME", "LF_FPROOF.exe");
            m_sProcessInfo.strWorkDir = IniControl.ReadString(strFileName, "FTP_SERVER", "WORK_PATH", @".\");
            m_sProcessInfo.strBackUpPath = IniControl.ReadString(strFileName, "FTP_SERVER", "BACKUP_PATH", @".\BACKUP");
            m_sProcessInfo.bBackupHist = IniControl.ReadBool(strFileName, "FTP_SERVER", "BACKUP_HISTORY");

            //GENERAL
            m_sProcessInfo.strBizcd = IniControl.ReadString(strFileName, "GENERAL", "BIZCD", "1002");
            m_sProcessInfo.strCorcd = IniControl.ReadString(strFileName, "GENERAL", "CORCD", "1000");
            m_sProcessInfo.strCompanyName = IniControl.ReadString(strFileName, "GENERAL", "COMPANY_NAME", "HanilEH");
            m_sProcessInfo.strPlantName = IniControl.ReadString(strFileName, "GENERAL", "PLANT_NAME", "Plant");
            return true;

        }
        
        private void setDownLoad()
        {
            try
            {
                FtpHelper helper = new FtpHelper
                (
                m_sProcessInfo.strServer_ip + ";" + m_sProcessInfo.strUser_id + ";" + m_sProcessInfo.strPassword
                , m_sProcessInfo.strDownloadPath
                , m_sProcessInfo.strWorkDir
                );
                helper.BackUpPath = m_sProcessInfo.strBackUpPath;
                List<FtpHelper.ftpinfo> listFullSub = helper.GetFullFilesList();

                

                string strErr = "";
                iMax = listFullSub.Count;
                iCount = Convert.ToInt16(Math.Round(Convert.ToDouble(100 / listFullSub.Count)));

                if (progressBar1.InvokeRequired)
                {
                    object obj = new object();
                    MethodInvoker del = delegate { setProgressBarSet(); };
                    this.Invoke(del);
                }
                else
                {
                    progressBar1.Maximum = iCount * iMax;
                }

                foreach (FtpHelper.ftpinfo name in listFullSub)
                {

                    if (label3.InvokeRequired)
                    {
                        object obj = new object();
                        MethodInvoker del = delegate { setFileNameSet(name.filename); };
                        this.Invoke(del);
                    }
                    else
                    {
                        label3.Text = name.filename;
                    }

                    if (progressBar1.InvokeRequired)
                    {
                        object obj = new object();
                        MethodInvoker del = delegate { setProgressBarValue(); };
                        this.Invoke(del);
                    }
                    else
                    {
                        setProgressBarValue();
                    }

                    if (label2.InvokeRequired)
                    {
                        object obj = new object();
                        MethodInvoker del = delegate { setFileView(); };
                        this.Invoke(del);
                    }
                    else
                    {
                        label2.Text = iFile + "/" + iMax;
                    }
                    helper.FileDownLoad(name, ref strErr);
                    iFile++;
                }
                if (!m_bExit)
                {
                    Process pros = new Process();
                    string sPath = Application.StartupPath + "\\" + m_sProcessInfo.strProgram_name;
                    pros.StartInfo.CreateNoWindow = true;
                    pros.StartInfo.Arguments = GetArgstr();
                    pros.StartInfo.FileName = sPath;
                    pros.Start();
                   

                    
                }

                Application.Exit(new CancelEventArgs(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }          
        }
        private string GetArgstr()
        {
            string[] args = Environment.GetCommandLineArgs();
            string argSTR = "";

            if (args.Length >= 2)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        argSTR += args[i];
                        if (i < args.Length - 1)
                        {
                            argSTR += " ";
                        }
                    }
                    
                }
            }
            return argSTR;
        }
        private void setFileNameSet(string name)
        {
            label3.Text = name;
        }
        private void setProgressBarValue()
        {
            progressBar1.Value += iCount;
        }
        private void setProgressBarSet()
        {
            progressBar1.Maximum = iCount * iMax;
        }

        private void setFileView()
        {
            label2.Text = iFile + "/" + iMax;
        }

        private void setStart()
        {
            
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(setDownLoad));
            th.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_bExit = true;
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            TimeSpan span = DateTime.Now - m_DTStart;

            if (PingTest(m_sProcessInfo.strServer_ip))
            {
                Lbl_Network.BackColor = Color.Lime;
                Lbl_Network.Text = "Network-OK";
                setStart();
                return;
            }
            else
            {
                Lbl_Network.BackColor = Color.Red;
                Lbl_Network.Text = "Network-NG";
            }
            if (span.TotalSeconds > 30)
            {
                this.Hide();
                if (MessageBox.Show("Server Connection is waiting..\nDo you want to keep trying?", "Question", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Show();
                    m_DTStart = DateTime.Now;
                }
                else
                {
                    Process pros = new Process();
                    string sPath = Application.StartupPath + "\\" + m_sProcessInfo.strProgram_name;
                    if (String.IsNullOrEmpty(m_sProcessInfo.strProgram_name) == false)
                    {
                        pros.StartInfo.CreateNoWindow = true;
                        pros.StartInfo.FileName = sPath;
                        pros.StartInfo.Arguments = GetArgstr();
                        pros.Start();
                    }
                    Application.Exit();
                }
            }

            timer1.Enabled = true;
        }

        private bool PingTest(string serverIP)
        {
            bool pingable = false;
            Ping pinger = new Ping();

            try
            {
                PingReply reply = pinger.Send(serverIP);

                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;

        }
    }
}
