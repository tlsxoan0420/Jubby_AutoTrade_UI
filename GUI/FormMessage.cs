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
    public partial class FormMessage : Form
    {
        #region ## FormMessage Define ##
        private Label[] labMessageArray;
        private Button[] btnMessageArray;

        int startTime = 0;
        int nowTime = 0;
        int totalTime = 20000;
        #endregion ## FormMessage Define ##
        public FormMessage()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            labMessageArray = new Label[]
            {
                labMessage1, // 0 : 메세지 창
                labMessage2, // 0 : 메세지 내용
                labMessage3, // 0 : 메세지 종료까지 남은 시간
            };

            btnMessageArray = new Button[]
            {
                btnMessage1, // 0 : 확인 버튼
                btnMessage2, // 1 : 닫기 버튼
            };

            for(int i = 0; i < btnMessageArray.Length; i++)
            {
                btnMessageArray[i].Click += ButtonClickEvent;
            }
        }
        #endregion ## UI Organize ##

        #region ## FormMessage Load ##
        private void FormMessage_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormMessage Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;

            startTime = Share.Ins.GetCurrentTime();
        }
        #endregion ## UI Update ##

        #region ## UI Event ##
        private void ButtonClickEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnMessage", "")) - int.Parse(btnMessageArray[0].Name.Replace("btnMessage", ""));

            switch(n)
            {
                case 0: // 확인 버튼
                    Flag.Live.IsMessageOkClick = true;
                    this.Close();
                    break;

                case 1: // 닫기 버튼
                    Flag.Live.IsMessageOkClick = false;
                    this.Close();
                    break;

                default:
                    break;
            }
        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {
            nowTime = Share.Ins.GetProgressTime(startTime);

            if (Share.Ins.GetTimeOver(startTime, totalTime) == true)
            {
                Flag.Live.IsMessageOkClick = false;
                this.Close();
            }
            else
            {
                labMessageArray[2].Text = string.Format("{0} 초 후에 해당 창은 자동 종료됩니다.", (totalTime - nowTime) / 1000);
            }
        }
        #endregion ## Timer Event ##
    }
}
