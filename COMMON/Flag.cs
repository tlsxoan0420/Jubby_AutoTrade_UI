using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jubby_AutoTrade_UI.GUI;

namespace Jubby_AutoTrade_UI.COMMON
{
    class Flag
    {
        #region ## Define ##
        public class Define
        {
            public const string APP_NAME = "Jubby AutoTrade UI";
            public const string APP_VERSION = "v1.0.0";

            public bool IsSimulation = false;
            public bool IsAuto = false;
        }
        #endregion ## Define ##

        #region ## Live ##
        public class Live
        {
            public static int Runmode = 0;

            public static int iReadyTime = 0;
            public static int iReadyPuaseTime = 0;
            public static int iSimulOperationTime = 0;
            public static int iSimulOperationPuaseTime = 0;
            public static int iAutoOperationTime = 0;
            public static int iAutoOperationPuaseTime = 0;
            public static int iErrorTime = 0;

            public static bool IsLogin = false;
            public static bool FormChange = false;
            public static bool IsMessageOkClick = false;
            public static bool ErrorClearSuccess = false;
        }
        #endregion ## Live ##

        #region ## User Status##
        public class UserStatus
        {
            static public int Level;

            static public string Password;
            static public string Name;
            static public string LoginID;
        }
        #endregion ## User Status##

        #region ## Trade Data ##
        public class TradeMarketData
        {

        }

        public class TradeAccountData
        {

        }

        public class TradeOrderData
        {

        }

        public class TradeStrategyData
        {

        }
        #endregion ## Trade Data ##

        #region ## Mode Number ##
        public class ModeNumber
        {
            public const int Logout = 0;
            public const int Home = 1;
            public const int Simul = 2;
            public const int Auto = 3;
            public const int Error = 4;
        }
        #endregion ## Mode Number ##

        #region ## User Level ##
        public class UserLevel
        {
            public const int GUEST = 0;
            public const int ADMIN = 1;
            public const int MASTER = 2;
        }
        #endregion ## User Level ##

        #region ## Page Number ##
        public class PageNumber
        {
            public const int FORM_LOGOUT = 0;
            public const int FORM_HOME = 1;
            public const int FORM_AUTO = 2;
            public const int FORM_ERROR = 3;
        }
        #endregion ## Page Number ##
    }
}
