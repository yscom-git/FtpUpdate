using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections;
using System.Globalization;

namespace FTP_Downloader
{
    public class FtpHelper
    {
        private string m_strServerIP="";
        private string m_strUserName="";
        private string m_strUserPassword="";
        private string m_strFilePath = "";
        private string m_strLocalWorkFolder = "";
        private string m_strBackupPath = "";
        private bool m_bBackupHist = false;

        public bool BackupHist
        {
            get { return m_bBackupHist; }
            set { m_bBackupHist = value; }
        }

        public string BackUpPath
        {
            get { return m_strBackupPath; }
            set 
            { 
                m_strBackupPath = value;
                if (!Directory.Exists(m_strBackupPath))
                {
                    Directory.CreateDirectory(m_strBackupPath);
                }
            }
        }

        public enum directoryEntryTypes
        {
            file = 0,
            directory = 1
        }
        
        public class ftpinfo
        {
            public string filename;
            public string path;
            public directoryEntryTypes fileType;
            public long size;
            public string permission;
            public DateTime fileDateTime;
            public int depth;
        }

        /// <summary>
        /// 동적 FTP정보 부여 Instance - 박용신
        /// </summary>
        /// <param name="strFTP">IP;UID;PWD</param>
        public FtpHelper(string strFTP, string strFilePath, string strLocalWorkFolder)
        {
            if(string.IsNullOrEmpty(strFTP))
            {
                throw new Exception("Wrong FTP information");
            }
            string[] spFTP = strFTP.Split(';');
            if (spFTP.Length >= 3)
            {
                m_strServerIP = spFTP[0];
                m_strUserName = spFTP[1];
                m_strUserPassword = spFTP[2];
                m_strFilePath = strFilePath;
                m_strLocalWorkFolder = strLocalWorkFolder;
            }
            else
            {
                throw new Exception("Wrong FTP information");
            }
        }

        private Match GetMatchingRegex(string line)
        {
            string[] formats = { 
	                    @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)" ,
	                    @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)" ,
	                    @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)" ,
	                    @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)" ,
	                    @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})(\s+)(?<size>(\d+))(\s+)(?<ctbit>(\w+\s\w+))(\s+)(?<size2>(\d+))\s+(?<timestamp>\w+\s+\d+\s+\d{2}:\d{2})\s+(?<name>.+)" ,
	                    @"(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)"};
            Regex rx;
            Match m;
            for (int i = 0; i < formats.Length; i++)  //As Integer = 0 To formats.Length - 1
            {
                rx = new Regex(formats[i]);
                m = rx.Match(line);
                if (m.Success)
                {
                    return m;
                }
            }
            return null;
        }

        private long getLocalFileSize(string strFile)
        {
            long lFilesize = 0;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(m_strLocalWorkFolder);
                ArrayList list = new ArrayList();

                list.Add(dir.GetFiles());

                for (int i = 0; i < list.Count; i++)
                {
                    FileInfo[] fa = (FileInfo[])list[i];
                    foreach (FileInfo f in fa)
                    {
                        if (f.Name == strFile)
                        {
                            lFilesize = f.Length;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lFilesize;
        }
        private bool CanDownLoad_LocalAndFTP(string localPath, ftpinfo ftpFile)
        {
            FileInfo localFile = new FileInfo(localPath);
            if (ftpFile.size == localFile.Length)
            {
                if (
                    localFile.LastWriteTime.Year == ftpFile.fileDateTime.Year
                    && localFile.LastWriteTime.Month == ftpFile.fileDateTime.Month
                    && localFile.LastWriteTime.Day == ftpFile.fileDateTime.Day
                    && localFile.LastWriteTime.Hour == ftpFile.fileDateTime.Hour
                    && localFile.LastWriteTime.Minute == ftpFile.fileDateTime.Minute
                    )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
        /// <summary>
        /// Ftp 파일 확인하여 다운로드
        /// </summary>
        public bool FileDownLoad(ftpinfo ftpFile, ref string strErr)
        {
            
            try
            {
                if (!string.IsNullOrEmpty(ftpFile.filename))
                {
                    string localPath = ParseFTPPath2LocalPath(m_strLocalWorkFolder, ftpFile.path + ftpFile.filename);
                    if (File.Exists(localPath))
                    {   //파일이 있으면 비교
                        if (!CanDownLoad_LocalAndFTP(localPath, ftpFile))
                        {
                            return false;
                        }
                    }
                    if (File.Exists(localPath))
                    {
                        if (!string.IsNullOrEmpty(m_strBackupPath))
                        {
                            string destPath = ParseFTPPath2LocalPath(m_strBackupPath, ftpFile.path);
                            if (!Directory.Exists(destPath))
                            {
                                Directory.CreateDirectory(destPath);
                            }
                            if (BackupHist)
                            {
                                if (File.Exists(destPath + ftpFile.filename))
                                {
                                    string[] spFileName = SplitFileName(ftpFile.filename);
                                    string yyyymmdd = DateTime.Now.ToString("yyMMddHHmmss");
                                    if (spFileName.Length > 1)
                                    {   //확장자 있을때
                                        File.Copy(destPath + ftpFile.filename, destPath + spFileName[0] + "_" + yyyymmdd + "." + spFileName[1], true);
                                    }
                                    else
                                    {   //확장자 없을때
                                        File.Copy(destPath + ftpFile.filename, destPath + ftpFile.filename + "_" + yyyymmdd, true);
                                    }

                                }
                            }
                            File.Copy(localPath, destPath + ftpFile.filename, true);
                        }
                    }
                    Download(ftpFile);
                    return true;
                }
                else
                {
                    return false;
                }
                
                
            }
            catch (Exception ex)
            {
                strErr = ex.Message;
                return false;
            }
            
        }
        private string[] SplitFileName(string fileName)
        {
            string[] strRet = fileName.Split('.');
            return strRet;
        }


        private void Download(ftpinfo ftpFile)
        {
            string filePath = m_strLocalWorkFolder;
            FtpWebRequest reqFTP;
            try
            {
                string localFullPath =ParseFTPPath2LocalPath(m_strLocalWorkFolder,ftpFile.path) + ftpFile.filename;
                FileStream outputStream = new FileStream(localFullPath, FileMode.Create,FileAccess.ReadWrite);
 

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpFile.path + ftpFile.filename));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(m_strUserName, m_strUserPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                File.SetCreationTime(localFullPath, ftpFile.fileDateTime);
                File.SetLastWriteTime(localFullPath, ftpFile.fileDateTime);
                //                Program.g_sProcessInfo.bVideoFlag = "N";
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// FTP기준폴더를 로컬폴더로 변환
        /// </summary>
        /// <param name="localStartPath">기준 로컬폴더</param>
        /// <param name="ftpDir">변환할 FTP폴더</param>
        /// <returns></returns>
        public string ParseFTPPath2LocalPath(string localStartPath, string ftpDir)
        {
            string parsePath = "";
            parsePath = ftpDir.Replace("ftp://", "").Replace("FTP://", "").Replace("Ftp://", "");

            parsePath = parsePath.Replace(m_strServerIP + "//", "");
            parsePath = parsePath.Replace(m_strServerIP + "/", "");
            parsePath = parsePath.Replace(m_strFilePath + "//", "");
            parsePath = parsePath.Replace(m_strFilePath + "/", "");
            parsePath = parsePath.Replace("//", "\\");
            parsePath = parsePath.Replace("/", "\\");
            if (localStartPath.Substring(localStartPath.Length - 1, 1) != "\\")
            {
                localStartPath = localStartPath + "\\";
            }
            return localStartPath + parsePath;
        }
        /// <summary>
        /// 서브File List호출
        /// </summary>
        /// <param name="depth">레벨</param>
        /// <param name="files">해당파일</param>
        /// <param name="subFolder">서브폴더</param>
        /// <returns></returns>
        private List<ftpinfo> GetFullFilesListSub(int depth, List<ftpinfo> files, string subFolder)
        {
            string path = "";
            path = subFolder + "/";
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            

            request.Credentials = new NetworkCredential(m_strUserName, m_strUserPassword);
            Stream rs = (Stream)request.GetResponse().GetResponseStream();


            StreamReader sr = new StreamReader(rs, Encoding.Default);
            string strList = sr.ReadToEnd();
            string[] lines = null;

            if (strList.Contains("\r\n"))
            {
                lines = strList.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            else if (strList.Contains("\n"))
            {
                lines = strList.Split(new string[] { "\n" }, StringSplitOptions.None);
            }


            if (lines == null || lines.Length == 0)
                return null;

            foreach (string line in lines)
            {
                if (line.Length == 0)
                    continue;
                //parse line
                Match m = GetMatchingRegex(line);
                if (m == null)
                {
                    //failed
                    throw new ApplicationException("Unable to parse line: " + line);
                }
                
                ftpinfo item = new ftpinfo();
                item.filename = m.Groups["name"].Value.Trim('\r');
                item.path = path;
                item.depth = depth+1;
                if (line.Contains("<DIR>") || line.Contains("<dir>"))
                {
                    item.size = 0;
                }
                else
                {
                    item.size = Convert.ToInt64(m.Groups["size"].Value);
                }
                item.permission = m.Groups["permission"].Value;
                string _dir = m.Groups["dir"].Value;
                if (_dir.Length > 0 && _dir != "-")
                {
                    item.fileType = directoryEntryTypes.directory;
                }
                else
                {
                    item.fileType = directoryEntryTypes.file;
                }

                try
                {
                    item.fileDateTime = DateTime.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    item.fileDateTime = DateTime.MinValue; //null;
                }

                files.Add(item);
                if (item.fileType == directoryEntryTypes.directory)
                {
                    files = GetFullFilesListSub(item.depth, files, path + item.filename);
                }
            }

            return files;
        }
        /// <summary>
        /// 기본 Root기준 FullList가져오기 + SubList호출
        /// </summary>
        /// <returns></returns>
        public List<ftpinfo> GetFullFilesList()
        {
            try
            {


                string path = "";
                if (string.IsNullOrEmpty(m_strFilePath))
                {
                    path = "ftp://" + m_strServerIP + "/";
                }
                else
                {
                    path = "ftp://" + m_strServerIP + "/" + m_strFilePath + "/";
                }
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                List<ftpinfo> files = new List<ftpinfo>();

                request.Credentials = new NetworkCredential(m_strUserName, m_strUserPassword);
                Stream rs = (Stream)request.GetResponse().GetResponseStream();


                StreamReader sr = new StreamReader(rs, Encoding.Default);
                string strList = sr.ReadToEnd();
                string[] lines = null;

                if (strList.Contains("\r\n"))
                {
                    lines = strList.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                }
                else if (strList.Contains("\n"))
                {
                    lines = strList.Split(new string[] { "\n" }, StringSplitOptions.None);
                }


                if (lines == null || lines.Length == 0)
                    return null;

                foreach (string line in lines)
                {
                    if (line.Length == 0)
                        continue;
                    //parse line
                    Match m = GetMatchingRegex(line);
                    if (m == null)
                    {
                        //failed
                        throw new ApplicationException("Unable to parse line: " + line);
                    }

                    ftpinfo item = new ftpinfo();
                    item.depth = 0;
                    item.filename = m.Groups["name"].Value.Trim('\r');
                    item.path = path;
                    if (line.Contains("<DIR>") || line.Contains("<dir>"))
                    {
                        item.size = 0;
                    }
                    else
                    {
                        item.size = Convert.ToInt64(m.Groups["size"].Value);
                    }
                    item.permission = m.Groups["permission"].Value;
                    string _dir = m.Groups["dir"].Value;
                    if (_dir.Length > 0 && _dir != "-")
                    {
                        item.fileType = directoryEntryTypes.directory;
                    }
                    else
                    {
                        item.fileType = directoryEntryTypes.file;
                    }

                    try
                    {

                        item.fileDateTime = DateTime.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        item.fileDateTime = DateTime.MinValue; //null;
                    }

                    files.Add(item);
                    List<ftpinfo> subFiles = new List<ftpinfo>();
                    if (item.fileType == directoryEntryTypes.directory)
                    {
                        subFiles = GetFullFilesListSub(item.depth, files, path + item.filename);
                        if (subFiles != null)
                        {
                            files = subFiles;
                        }
                    }
                }
                List<ftpinfo> lstRet = new List<ftpinfo>();

                foreach (ftpinfo ftpFile in files)
                {
                    if (ftpFile.fileType == FtpHelper.directoryEntryTypes.directory)
                    {
                        string folder = ParseFTPPath2LocalPath(m_strLocalWorkFolder, ftpFile.path + ftpFile.filename);
                        if (!System.IO.Directory.Exists(folder))
                        {

                            Directory.CreateDirectory(folder);
                        }

                    }
                    else
                    {
                        lstRet.Add(ftpFile);
                    }
                }
                return lstRet;
            }
            catch(Exception eLog)
            {
                
            }

            return null;
        }
    }
}
