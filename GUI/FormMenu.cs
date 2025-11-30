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
    public partial class FormMenu : Form
    {
        #region ## FormMenu Define ##
        private Button[] btnMenuArray;
        #endregion ## FormMenu Define ##

        public FormMenu()
        {
            InitializeComponent();
            UIOranize();
        }

        #region ## UI Organize ##
        private void UIOranize()
        {
            btnMenuArray = new Button[]
            {
               btnMenu1, // 0. 로그인 버튼
               btnMenu2, // 1. 모드 변경 버튼
               btnMenu3, // 2. 데이터베이스 버튼
               btnMenu10, // 9. 종료 버튼
            };

            for(int i = 0; i < btnMenuArray.Length; i++)
            {
                btnMenuArray[i].Click += ButtonClickEvent;
                btnMenuArray[i].MouseMove += ButtonMouseMoveEvent;
                btnMenuArray[i].MouseLeave += ButtonMouseLeaveEvent;
                btnMenuArray[i].TabStop = false;
                btnMenuArray[i].CausesValidation = false;
            }
        }
        #endregion ## UI Organize ##

        #region ## FormMenu Load ##
        private void FormMenu_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormMenu Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
        }
        #endregion ## UI Update ##

        #region ## UI Event ##

        #region ## Button Click Event ##
        private void ButtonClickEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnMenu", "")) - int.Parse(btnMenuArray[0].Name.Replace("btnMenu", ""));
            this.ActiveControl = null;

            switch (n)
            {
                case 0: // 1. 로그인 버튼
                    if(Flag.Live.IsLogin == true)
                    {
                        Share.Ins.MessageFormOpen("로그아웃을 하시겠습니까 ?");
                        if(Flag.Live.IsMessageOkClick == false)
                        {
                            return;
                        }

                        Flag.Live.IsLogin = false;
                        Share.Ins.ChangeMode(Flag.ModeNumber.Logout);
                    }
                    else
                    {
                        Share.Ins.ChangeMode(Flag.ModeNumber.Logout);
                    }
                    break;

                case 1: // 2. 모드 변경 버튼
                    if (Flag.Live.IsLogin == false)
                    {
                        Share.Ins.MessageFormOpen("로그인이 필요합니다.");
                        return;
                    }
                    if (Flag.Live.Runmode == Flag.ModeNumber.Error)
                    {
                        Share.Ins.MessageFormOpen("현재 에러가 발생해 해당 버튼의 기능을 사용할 수 없습니다.");
                        return;
                    }

                    if (Share.Ins.IsFormOpen(typeof(FormModeMenu)) == false)
                    {
                        FormModeMenu formModeMenu = new FormModeMenu();
                        formModeMenu.Location = new Point(btnMenuArray[1].Location.X, btnMenuArray[1].Location.Y + 50);
                        formModeMenu.Show();
                    }
                    else
                    {
                        Share.Ins.MessageFormOpen("이미 모드 변경 창이 열려 있습니다.");
                        return;
                    }
                    break;

                case 2: // 3. 데이터베이스 버튼
                    if (Flag.Live.IsLogin == false)
                    {
                        Share.Ins.MessageFormOpen("로그인이 필요합니다.");
                        return;
                    }

                    break;

                case 9: // 10. 종료 버튼
                    if (Flag.Live.Runmode == Flag.ModeNumber.Simul || Flag.Live.Runmode == Flag.ModeNumber.Auto)
                    {
                        Share.Ins.MessageFormOpen("모의 주식 중 및 자동 매매 중에는 프로그램을 종료 할 수 없습니다");
                        return;
                    }

                    Share.Ins.MessageFormOpen("프로그램을 종료 하시겠습니까 ?");
                    if(Flag.Live.IsMessageOkClick == true)
                    {
                        Share.Ins.ProgramEnd();
                        Application.ExitThread();
                        Environment.Exit(0);
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion ## Button Click Event ##

        #region ## Button Mouse Move Event ##
        private void ButtonMouseMoveEvent(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnMenu", "")) - int.Parse(btnMenuArray[0].Name.Replace("btnMenu", ""));

            switch (n)
            {
                case 0: // 1. 로그인 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    break;

                case 1: // 2. 모드 변경 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    break;

                case 2: // 3. 데이터베이스 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    break;

                case 9: // 10. 종료 버튼
                    button.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
                    break;

                default:
                    break;
            }
        }
        #endregion ## Button Mouse Move Event ##

        #region ## Button Mouse Leave Event ##
        private void ButtonMouseLeaveEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnMenu", "")) - int.Parse(btnMenuArray[0].Name.Replace("btnMenu", ""));

            switch (n)
            {
                case 0: // 1. 로그인 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    break;

                case 1: // 2. 모드 변경 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    break;

                case 2: // 3. 데이터베이스 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    break;

                case 9: // 10. 종료 버튼
                    button.Font = new Font("HY헤드라인M", 10, FontStyle.Regular);
                    break;

                default:
                    break;
            }
        }
        #endregion ## Button Mouse Leave Event ##

        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Flag.Live.IsLogin == false)
            {
                btnMenuArray[0].Text = "로그아웃";
                btnMenuArray[0].ForeColor = Color.Yellow;
            }
            else
            {
                btnMenuArray[0].Text = "로그인";
                btnMenuArray[0].ForeColor = Color.Lime;
            }

            if (Flag.Live.Runmode <= Flag.ModeNumber.Logout)
            {
                labOperationText.Visible = false;
                labOperationText.ForeColor = Color.Silver;
            }
            else if(Flag.Live.Runmode <= Flag.ModeNumber.Home)
            {
                labOperationText.Visible = true;
                labOperationText.Text = "🔵 대기 모드";
                labOperationText.ForeColor = Color.Silver;
            }
            else if (Flag.Live.Runmode == Flag.ModeNumber.Simul)
            {
                labOperationText.Visible = true;
                labOperationText.Text = "🔵 모의 주식 모드";
                labOperationText.ForeColor = Color.Yellow;
            }
            else if (Flag.Live.Runmode == Flag.ModeNumber.Auto)
            {
                labOperationText.Visible = true;
                labOperationText.ForeColor = Color.Lime;

                labOperationText.Text = "🔴 자동 매매 모드";
            }
            else if( Flag.Live.Runmode == Flag.ModeNumber.Error)
            {
                labOperationText.Visible = true;
                labOperationText.ForeColor = Color.Red;
                labOperationText.Text = "⚠️ 에러 발생 상태";
            }
        }
        #endregion ## Timer Event ##
    }
}
