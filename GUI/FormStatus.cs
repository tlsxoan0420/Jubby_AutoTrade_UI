using Jubby_AutoTrade_UI.COMMON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormStatus : Form
    {
        #region ## FormStatus Define ##
        private Label[] labStatusArray;
        #endregion ## FormStatus Define ##
        public FormStatus()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            labStatusArray = new Label[]
            {
                labStatus1, // 0 : 빌드 날짜 및 제작자
                labStatus2, // 1 : 사용자 레벨 및 사용자 명
                labStatus3, // 2 : 현재 시간
                labStatus4, // 3 : 현재 가동중인 상태 및 가동시간
            };
        }
        #endregion ## UI Organize ##

        #region ## FormStatus Load ##
        private void FormStatus_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormStatus Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 10;
            timer1.Enabled = true;
        }
        #endregion ## UI Update ##

        private void timer1_Tick(object sender, EventArgs e)
        {
            labStatusArray[0].Text = string.Format("사용 버전 : {0}, 빌드 날짜 : {1}, 제작자 : 중국쿠팡",
                Share.Ins.GetVersion(),Share.Ins.BuildDate());

            if(Flag.UserStatus.Level == Flag.UserLevel.GUEST)
            {
                labStatusArray[1].Text = string.Format("사용자 명 : {0}, 사용자 권한 : 👤 일반 사용자",
                    Flag.UserStatus.Name);
            }
            else if(Flag.UserStatus.Level == Flag.UserLevel.ADMIN)
            {
                labStatusArray[1].Text = string.Format("사용자 명 : {0}, 사용자 권한 : 🛠️ 관리자",
                    Flag.UserStatus.Name);
            }
            else if(Flag.UserStatus.Level == Flag.UserLevel.MASTER)
            {
                labStatusArray[1].Text = string.Format("사용자 명 : {0}, 사용자 권한 : 👑 최고 관리자",
                    Flag.UserStatus.Name);
            }

            DateTime NowTime = DateTime.Now;
            labStatusArray[2].Text = string.Format("현재시간 : {0}년. {1}월. {2}일. {3}시. {4}분. {5}초",
                NowTime.Year, NowTime.Month, NowTime.Day, NowTime.Hour, NowTime.Minute, NowTime.Second);

            if(Flag.Live.Runmode == Flag.ModeNumber.Home)
            {
                labStatusArray[3].Text = string.Format("현재 모드 : 🔄 정지 상태, {0}",
                    Share.Ins.GetsReadyTime(Flag.Live.iReadyTime));
                labStatusArray[3].ForeColor = Color.Silver;
            }
            else if (Flag.Live.Runmode == Flag.ModeNumber.Simul)
            {
                labStatusArray[3].Text = string.Format("현재 모드 : 💻 모의 주식 거래 중, {0}",
                   Share.Ins.GetsSimulOperationTime(Flag.Live.iSimulOperationTime));
                labStatusArray[3].ForeColor = Color.Yellow;
            }
            else if (Flag.Live.Runmode == Flag.ModeNumber.Auto)
            {
                labStatusArray[3].Text = string.Format("현재 모드 : 💻 자동 매매 중, {0}",
                    Share.Ins.GetsAutoOperationTime(Flag.Live.iAutoOperationTime));
                labStatusArray[3].ForeColor = Color.Lime;
            }
            else if (Flag.Live.Runmode == Flag.ModeNumber.Error)
            {
                labStatusArray[3].Text = string.Format("현재 모드 : ❌ 에러 발생, {0}",
                    Share.Ins.GetsErrorTime(Flag.Live.iErrorTime));
                labStatusArray[3].ForeColor = Color.Red;
            }
        }
    }
}
