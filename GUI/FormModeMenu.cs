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
    public partial class FormModeMenu : Form
    {
        #region ## FormModeMenu Define ##
        private Button[] btnModeMenuArray;
        #endregion ## FormModeMenu Define ##

        public FormModeMenu()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            btnModeMenuArray = new Button[]
            {
               btnModeMenu1, // 1. 대기 모드 버튼
               btnModeMenu2, // 2. 모의 주식 모드 버튼
               btnModeMenu3, // 3. 자동 매매 모드 버튼
            };
            for (int i = 0; i < btnModeMenuArray.Length; i++)
            {
                btnModeMenuArray[i].Click += ButtonClickEvent;
                btnModeMenuArray[i].MouseMove += ButtonMouseMoveEvent;
                btnModeMenuArray[i].MouseLeave += ButtonMouseLeaveEvent;
            }
        }
        #endregion ## UI Organize ##

        #region ## FormModeMenu Load ##
        private void FormModeMenu_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormModeMenu Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
        }
        #endregion ## UI Update ##

        #region ## UI Event ##
        private void ButtonClickEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnModeMenu", "")) - int.Parse(btnModeMenuArray[0].Name.Replace("btnModeMenu", ""));

            switch (n)
            {
                case 0: // 1. 대기 모드 버튼
                    Share.Ins.MessageFormOpen("대기 모드로 변경 하시겠습니까 ?");
                    if (Flag.Live.IsMessageOkClick == false)
                    {
                        return;
                    }

                    Share.Ins.ChangeMode(Flag.ModeNumber.Home);
                    Share.Ins.GetiReadyTime();

                    this.Close();
                    break;

                case 1: // 2. 모의 주식 모드 버튼
                    Share.Ins.MessageFormOpen("모의 주식 모드로 변경 하시겠습니까 ?");
                    if (Flag.Live.IsMessageOkClick == false)
                    {
                        return;
                    }

                    Share.Ins.ChangeMode(Flag.ModeNumber.Simul);
                    Share.Ins.GetiSimulOperationTime();

                    this.Close();
                    break;

                case 2: // 3. 자동 매매 모드 버튼
                    Share.Ins.MessageFormOpen("자동 매매 모드로 변경 하시겠습니까 ?");
                    if (Flag.Live.IsMessageOkClick == false)
                    {
                        return;
                    }

                    Share.Ins.ChangeMode(Flag.ModeNumber.Auto);
                    Share.Ins.GetiAutoOperationTime();

                    this.Close();
                    break;

                default:
                    break;
            }
        }
        private void ButtonMouseMoveEvent(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnModeMenu", "")) - int.Parse(btnModeMenuArray[0].Name.Replace("btnModeMenu", ""));

            switch (n)
            {
                case 0: // 1. 대기 모드 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    button.ForeColor = Color.Lime;
                    break;

                case 1: // 2. 모의 주식 모드 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    button.ForeColor = Color.Lime;
                    break;

                case 2: // 3. 자동 매매 모드 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    button.ForeColor = Color.Lime;
                    break;

                default:
                    break;
            }
        }
        private void ButtonMouseLeaveEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnModeMenu", "")) - int.Parse(btnModeMenuArray[0].Name.Replace("btnModeMenu", ""));

            switch (n)
            {
                case 0: // 1. 대기 모드 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    button.ForeColor = Color.Silver;
                    break;

                case 1: // 2. 모의 주식 모드 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    button.ForeColor = Color.Silver;
                    break;

                case 2: // 3. 자동 매매 모드 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    button.ForeColor = Color.Silver;
                    break;

                default:
                    break;
            }
        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        #endregion ## Timer Event ##
    }
}
