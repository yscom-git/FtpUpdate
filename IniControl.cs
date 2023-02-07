//===============================================================================================
//
//  프로그램명 : Ini 관련 - 공용 모듈 정의
//
//      파일명 : IniControl.cs
//
//    모듈설명 : ini 데이터 Read/Write
//
//      작성자 : (주)맥스 조성제
//
//-----------------------------------------------------------------------------------------------
// 변경이력
//-----------------------------------------------------------------------------------------------
// 2010.06.01 - 조성제 - 최초작성
//===============================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FTP_Downloader
{
    //ini 관리 클래스
    public class IniControl
    {

        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpstrFileName);

        [DllImport("kernel32")]
        public static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpstrFileName);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpstrFileName);

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 정수 읽기
        // Argument : strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //            iDefault      - Default 정수
        //   Return : int           - ini에서 읽은 정수
        //            iDefault      - Default 정수
        //-------------------------------------------------------------------------------
        public static int ReadInteger(string strFileName, string strAppName, string strKeyName, int iDefault)
        {
            try{
                StringBuilder result = new StringBuilder(255);
                IniControl.GetPrivateProfileString(strAppName, strKeyName, "", result, 255, strFileName);

                if (result.ToString() == ""){
                    return iDefault;
                }
                else{
                    return Convert.ToInt32(result.ToString());
                }
            }
            catch{
                return iDefault;
            }
        }

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 Boolean 읽기
        // Argument : strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //   Return : Boolean       - ini에서 읽은 Boolean
        //-------------------------------------------------------------------------------
        public static Boolean ReadBool(string strFileName, string strAppName, string strKeyName)
        {
            try{
                StringBuilder result = new StringBuilder(255);

                IniControl.GetPrivateProfileString(strAppName, strKeyName, "", result, 255, strFileName);

                if (result.ToString().ToUpper() == "TRUE" || result.ToString().ToUpper() == "Y" || result.ToString() == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch{
                return false;
            }
        }

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 문자열 읽기
        // Argument : strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //            strDefault    - Default 문자열
        //   Return : result        - ini에서 읽은 문자
        //            strDefault    - Defalut 문자열
        //-------------------------------------------------------------------------------
        public static string ReadString(string strFileName, string strAppName, string strKeyName, string strDefault)
        {
            try{
                StringBuilder result = new StringBuilder(255);

                IniControl.GetPrivateProfileString(strAppName, strKeyName, "", result, 255, strFileName);

                if (result.ToString() == "")
                {
                    return strDefault;
                }
                else
                {
                    return result.ToString();
                }
            }
            catch
            {
                return strDefault;
            }
        }

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 문자열 읽기
        // Argument : strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //   Return : result        - ini에서 읽은 문자열
        //-------------------------------------------------------------------------------
        public static string GetIni(string strFileName, string strAppName, string strKeyName)
        {
            try{
                StringBuilder result = new StringBuilder(255);

                IniControl.GetPrivateProfileString(strAppName, strKeyName, "", result, 255, strFileName);

                return result.ToString();
            }
            catch
            {
                return "";
            }
        }

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 문자열 쓰기
        // Argument : strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //            IpValue       - ini에 쓸 문자
        //   Return : true          - 성공
        //            false         - 실패
        //-------------------------------------------------------------------------------
        public static Boolean SetIni(string strFileName, string strAppName, string strKeyName, string IpValue)
        {
            try
            {
                string inifile = strFileName;    //Path + File
                IniControl.WritePrivateProfileString(strAppName, strKeyName, IpValue, strFileName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        //-------------------------------------------------------------------------------
        // 함수설명 : INI - 해당 문자열 쓰기
        // Argument : filePath      - Path
        //            strFileName   - INI 파일명
        //            strAppName    - AppName
        //            strKeyName    - KeyName
        //            IpValue       - ini에 쓸 문자
        //   Return : true          - 성공
        //            false         - 실패
        //-------------------------------------------------------------------------------
        public static Boolean SetIni(string filePath, string strFileName, string strAppName, string strKeyName, string IpValue)
        {
            try
            {
                IniControl.WritePrivateProfileString(strAppName, strKeyName, IpValue, strFileName);

                return true;
            }
            catch
            {
                return false;
            }
        }

    } //Class
} //Name Space
