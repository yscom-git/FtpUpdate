using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FTP_Downloader
{
    public struct ProcessInfo
    {
        public string strServer_ip;   
        public string strUser_id;     
        public string strPassword;    
        public string strDownloadPath;
        public string strProgram_name;
        public string strWorkDir;
        public string strCorcd;
        public string strBizcd;
        public string strCompanyName;
        public string strPlantName;
        public string strBackUpPath;
        public bool bBackupHist;
    }
    

    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
