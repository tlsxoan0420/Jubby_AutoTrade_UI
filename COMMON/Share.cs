using Jubby_AutoTrade_UI.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Jubby_AutoTrade_UI.COMMON.Flag;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Jubby_AutoTrade_UI.COMMON
{
    class Share : IDisposable
    {
        #region ## Class Inatance ##
        private static volatile Share instance;
        private static object syncRoot = new Object();
        public static Share Ins
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Share();
                    }
                }
                return instance;
            }
        }
        public Share()
        {
        }
        ~Share()
        {
        }
        public void Dispose()
        {
        }
        /// <summary>
        ///
        /// </summary>
        #endregion ## Class Inatance ##

        #region ## Use Memory ##
        public string UseMemory()
        {
            string strMemory = "";
            string strname = String.Format("{0}", Path.GetFileName(Assembly.GetEntryAssembly().Location));
            strname = strname.Replace(".exe", "");

            Process[] all = Process.GetProcesses();
            foreach (Process thisProc in all.OrderBy(x => x.ProcessName))
            {
                string Name = thisProc.ProcessName;
                if (strname == Name)
                {
                    strMemory = FormatBytes(thisProc.WorkingSet64);
                }
            }
            return strMemory;
        }
        #endregion ## Use Memory ##

        #region ## Format Bytes ##
        public string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                {
                    //return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                    return string.Format("{0:##} {1}", decimal.Divide(bytes, max), order);
                }
                max /= scale;
            }
            return "0 Bytes";
        }
        #endregion ## Format Bytes ##

        #region ## Get Version ##
        public string GetVersion(bool bfull = false)
        {
            string strProgram = null;
            strProgram = String.Format("{0}", Path.GetFileName(Assembly.GetEntryAssembly().Location));
            strProgram = strProgram.Replace(".exe", "");
            return strProgram;
        }
        #endregion ## Get Version ##

        #region ## App Version ##
        static public string AppVersion
        {
            get
            {
                return System.Windows.Forms.Application.ProductVersion;
            }
        }
        #endregion ## App Version ##

        #region ## Build Data ##
        public string BuildDate()
        {
            string buldDate = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            buldDate = string.Format("{0}-{1}-{2} {3}:{4}:{5}", System.IO.File.GetLastWriteTime(assembly.Location).Year,
                                                             System.IO.File.GetLastWriteTime(assembly.Location).Month,
                                                             System.IO.File.GetLastWriteTime(assembly.Location).Day,
                                                             System.IO.File.GetLastWriteTime(assembly.Location).Hour,
                                                             System.IO.File.GetLastWriteTime(assembly.Location).Minute,
                                                             System.IO.File.GetLastWriteTime(assembly.Location).Second);
            return buldDate;
        }
        #endregion ## Build Data ##

        #region ## Get Random ##
        public double GetRandom(int start, int end)
        {
            Random rand = new Random();
            int number = rand.Next(start, end);

            Random rand1 = new Random();
            double number1 = rand.NextDouble();

            number1 = number * number1;


            return number1;
        }
        #endregion ## Get Random ##

        #region ## IsInt ##
        public bool IsInt(string Data)
        {
            bool IsNumbric = false;
            int TestInt = 0;

            IsNumbric = int.TryParse(Data, out TestInt);
            if (IsNumbric != true)
            {
                //Share.Ins.UserMessage(define.Info, "Not a Number.", "Please Enter a Number");
                return false;
            }
            return true;
        }
        #endregion ## IsInt ##

        #region ## isDouble ##
        public bool IsDouble(string Data, bool message = true)
        {
            bool IsNumbric = false;
            float TestFloat = 0;

            IsNumbric = float.TryParse(Data, out TestFloat);
            if (IsNumbric != true)
            {
                if (message == true)
                {
                    //Share.Ins.UserMessage(define.Info, "Not a Number.", "Please Enter a Number");
                }
                return false;
            }
            return true;
        }
        #endregion ## isDouble ##

        #region ## isByte ##
        public bool isByte(string Data, bool message = true)
        {
            bool IsNumbric = false;
            byte Testbyte = 0;

            IsNumbric = byte.TryParse(Data, out Testbyte);
            if (IsNumbric != true)
            {
                if (message == true)
                {
                    //Share.Ins.UserMessage(define.Info, "Not a Number.", "Please Enter a Number");
                }
                return false;
            }
            return true;
        }
        #endregion ## isByte ##

        #region ## GetCureentTime ##
        public int GetCurrentTime()
        {
            return Environment.TickCount;
        }
        #endregion ## GetCureentTime ##

        #region ## GetProgressTime ##
        public int GetProgressTime(int nStartTime)
        {
            return Environment.TickCount - nStartTime;
        }
        #endregion ## GetProgressTime ##

        #region ## GetTimeOver ##
        public bool GetTimeOver(int nStartTime, double dcheckTime)
        {
            int TimeSPan = Environment.TickCount - nStartTime;
            if (TimeSPan > dcheckTime)
                return true;
            return false;
        }
        #endregion ## GetTimeOver ##

        #region ## Get Operation Time ##
        public void GetiReadyTime()
        {
            Flag.Live.iReadyTime = Share.Ins.GetCurrentTime();
            return;
        }

        public void GetiSimulOperationTime()
        {
            Flag.Live.iSimulOperationTime = Share.Ins.GetCurrentTime();
            return;
        }

        public void GetiAutoOperationTime()
        {
            Flag.Live.iAutoOperationTime = Share.Ins.GetCurrentTime();
            return;
        }

        public void GetiErrorTime()
        {
            Flag.Live.iErrorTime = Share.Ins.GetCurrentTime();
            return;
        }

        public string GetsReadyTime(int nStartTime)
        {
            string totalTime = "";

            if(nStartTime <= 0)
            {
                totalTime = "대기 시간 : 00시. 00분. 00초";
                return totalTime;
            }

            int nOpTime = Environment.TickCount - nStartTime;
            int totalSeconds = nOpTime / 1000;
            int totalMinutes = totalSeconds / 60;
            int totalHours = totalMinutes / 60;

            int seconds = totalSeconds % 60;
            int minutes = totalMinutes % 60;
            int hours = totalHours % 24;

            totalTime = string.Format("대기 시간 : {0}시. {1}분. {2}초",
                hours, minutes, seconds);


            return totalTime;
        }
        public string GetsSimulOperationTime(int nStartTime)
        {
            string totalTime = "";

            if (nStartTime <= 0)
            {
                totalTime = "시뮬 가동 시간 : 00시. 00분. 00초";
                return totalTime;
            }

            int nOpTime = Environment.TickCount - nStartTime;
            int totalSeconds = nOpTime / 1000;
            int totalMinutes = totalSeconds / 60;
            int totalHours = totalMinutes / 60;

            int seconds = totalSeconds % 60;
            int minutes = totalMinutes % 60;
            int hours = totalHours % 24;

            totalTime = string.Format("시뮬 가동 시간 : {0}시. {1}분. {2}초",
                hours, minutes, seconds);

            return totalTime;
        }
        public string GetsAutoOperationTime(int nStartTime)
        {
            string totalTime = "";

            if (nStartTime <= 0)
            {
                totalTime = "자동 가동 시간 : 00시. 00분. 00초";
                return totalTime;
            }

            int nOpTime = Environment.TickCount - nStartTime;
            int totalSeconds = nOpTime / 1000;
            int totalMinutes = totalSeconds / 60;
            int totalHours = totalMinutes / 60;

            int seconds = totalSeconds % 60;
            int minutes = totalMinutes % 60;
            int hours = totalHours % 24;

            totalTime = string.Format("자동 가동 시간 : {0}시. {1}분. {2}초",
                hours, minutes, seconds);

            return totalTime;
        }

        public string GetsErrorTime(int nStartTime)
        {
            string totalTime = "";

            if (nStartTime <= 0)
            {
                totalTime = "에러 발생 시간 : 00시. 00분. 00초";
                return totalTime;
            }

            int nOpTime = Environment.TickCount - nStartTime;
            int totalSeconds = nOpTime / 1000;
            int totalMinutes = totalSeconds / 60;
            int totalHours = totalMinutes / 60;

            int seconds = totalSeconds % 60;
            int minutes = totalMinutes % 60;
            int hours = totalHours % 24;

            totalTime = string.Format("에러 발생 시간 : {0}시. {1}분. {2}초",
                hours, minutes, seconds);

            return totalTime;
        }
        #endregion ## Get Operation Time ##

        #region ## SetMode ##
        public void SetMode(int Mode)
        {
            //Live.op.MachineStatus = Mode;

            //if (Live.op.MachineStatus == define.ERROR_Mode)
            //{
            //    string strmsg = string.Format("Error Mode {0:0000} - {1}", Error.Code, Error.Message);
            //    Log.Ins.MachineEventLog(strmsg);
            //    Log.Ins.MachineRunLog("Share", "Set ERROR Mode");
            //}
            //else if (Live.op.MachineStatus == define.MANUAL_Mode)
            //{
            //    Error.ErrorOccur = false;
            //    Log.Ins.MachineEventLog("Manual Mode");
            //    Log.Ins.MachineRunLog("Share", "Set MANUAL Mode");

            //}
            //else if (Live.op.MachineStatus == define.AUTO_Mode)
            //{
            //    Error.ErrorOccur = false;
            //    Log.Ins.MachineEventLog("Auto Mode");
            //    Log.Ins.MachineRunLog("Share", "Set AUTO Mode");
            //}

            //if (Live.op.MachineStatus == define.ERROR_Mode) SetMachine1(define.ERROR_Mode);
            //else if (Live.op.MachineStatus == define.MANUAL_Mode) SetMachine1(define.MANUAL_Mode);
            //else if (Live.op.MachineStatus == define.AUTO_Mode) SetMachine1(define.AUTO_Mode);
        }

        #endregion ## SetMode ##

        #region ## GetMode ##
        public int GetMode()
        {
            return Live.Runmode;
        }
        #endregion ## GetMode ##

        #region ## Set Error ##
        public void SetError(int Code, string site = "", int Step = 0)
        {
            //string strerror = null;
            //strerror = string.Format("Error - {0}, {1}, {2}", Code, site, Step);
            //Log.Ins.MachineRunLog("Share-SetError", strerror);

            //if (Live.op.MachineStatus == define.ERROR_Mode) { return; }
            //if (Error.ErrorOccur == true) { return; }

            //FormError dlgErr = new FormError();
            //Error.ClearCode = 0;
            //Error.ErrorOccur = true;
            //Error.BuzzerOn = true;
            //Error.Code = Code;
            //Error.EventPlace = site;
            //Error.EventStep = Step;
            //Error.EventTime = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

            //Error.EventSite = "H";
            //Disk.Ins.LoadErrorList(Error.Code, out Error.Message, out Error.SubMessage);
            //Share.Ins.DayErrorCount(Error.Code, Error.Message, Error.EventSite);
            //Error.Report = true;
            //Manager.Ins.RunStopFlag(-1);
        }
        #endregion ## Set Error ##

        #region ## Program End ##
        public void ProgramEnd()
        {

        }
        #endregion ## Program End ##

        #region ## ChangeMode ##
        public void ChangeMode(int mode)
        {
            Live.Runmode = mode;
            if (mode == ModeNumber.Home) GetiReadyTime();
            else if (mode == ModeNumber.Simul) GetiSimulOperationTime();
            else if (mode == ModeNumber.Auto) GetiAutoOperationTime();
            else if (mode == ModeNumber.Error) GetiErrorTime();

            Live.FormChange = true;
        }
        #endregion ## ChangeMode ##

        #region ## IsFormOpen ##
        public bool IsFormOpen(Type FormType)
        {
            foreach (Form OpenForm in Application.OpenForms)
            {
                if (OpenForm.GetType() == FormType)
                    return true;
            }
            return false;
        }
        #endregion ## IsFormOpen ##

        #region ## Message Form Open ##
        public void MessageFormOpen(string msg)
        {
            if (IsFormOpen(typeof(FormMessage)) == false)
            {
                FormMessage formMessage = new FormMessage();
                formMessage.labMessage2.Text = string.Format("{0}", msg);
                formMessage.ShowDialog();

                return;
            }
        }
        #endregion ## Message Form Open ##

        #region ## Error Message ##
        public void ErrorMessage(string mes)
        {
            return;
        }
        #endregion ## Error Message ##
    }
}
