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
    public partial class FormLogin : Form
    {

        #region ## FormLogin Define ##
        private Button[] btnLoginArray;
        #endregion ## FormLogin Define ##

        public FormLogin()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            btnLoginArray = new Button[]
            {
                btnLogin1, // 0 : 로그인 버튼
                btnLogin2,  // 1 : 자동 로그인 버튼
                btnLogin3,  // 2 : 회원가입 버튼
                btnLogin4,  // 3 : 종료 버튼
            };

            for(int i =0; i < btnLoginArray.Length; i++)
            {
                btnLoginArray[i].Click += ButtonClickEvent;
                btnLoginArray[i].MouseMove += ButtonMouseMoveEvent;
                btnLoginArray[i].MouseLeave += ButtonMouseLeaveEvent;
            }
        }
        #endregion ## UI Organize ##

        #region ## UI Event ##

        #region ## Button Click Event ##
        private void ButtonClickEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnLogin", "")) - int.Parse(btnLoginArray[0].Name.Replace("btnLogin", ""));

            switch(n)
            {
                case 0: // 로그인 버튼
                    // 일단은 임시 기능으로 로그인 버튼 누루기만 하면 로그인 되도록 설정.
                    Flag.UserStatus.LoginID = "MASTER";
                    Flag.UserStatus.Password = "MASTER";
                    Flag.UserStatus.Name = "신태무";
                    Flag.UserStatus.Level = Flag.UserLevel.MASTER;

                    Flag.Live.IsLogin = true;

                    Share.Ins.ChangeMode(Flag.ModeNumber.Home);

                    this.DialogResult = DialogResult.OK;
                    this.Close();

                    break;

                case 1: // 자동 로그인 버튼

                    break;

                case 2: // 회원가입 버튼

                    break;

                case 3: // 종료 버튼
                    if(Flag.Live.Runmode == Flag.ModeNumber.Simul || Flag.Live.Runmode == Flag.ModeNumber.Auto)
                    {
                        Share.Ins.MessageFormOpen("모의 주식 및 자동 매매 중에는 프로그램을 종료 할 수 없습니다.");
                        break;
                    }

                    this.Close();
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
            int n = int.Parse(button.Name.Replace("btnLogin", "")) - int.Parse(btnLoginArray[0].Name.Replace("btnLogin", ""));
            switch (n)
            {
                case 0: // 로그인 버튼
                    button.ForeColor = Color.Lime;
                    button.Font = new Font("HY헤드라인M", 20, FontStyle.Bold);
                    break;
                case 1: // 자동 로그인 버튼
                    button.ForeColor = Color.Lime;
                    button.Font = new Font("HY헤드라인M", 20, FontStyle.Bold);
                    break;
                case 2: // 회원가입 버튼
                    button.ForeColor = Color.Lime;
                    button.Font = new Font("HY헤드라인M", 20, FontStyle.Bold);
                    break;
                case 3: // 종료 버튼
                    button.ForeColor = Color.Red;
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
            int n = int.Parse(button.Name.Replace("btnLogin", "")) - int.Parse(btnLoginArray[0].Name.Replace("btnLogin", ""));
            switch (n)
            {
                case 0: // 로그인 버튼
                    button.ForeColor = Color.Silver;
                    button.Font = new Font("HY헤드라인M", 18, FontStyle.Regular);
                    break;
                case 1: // 자동 로그인 버튼
                    button.ForeColor = Color.Silver;
                    button.Font = new Font("HY헤드라인M", 18, FontStyle.Regular);
                    break;
                case 2: // 회원가입 버튼
                    button.ForeColor = Color.Silver;
                    button.Font = new Font("HY헤드라인M", 18, FontStyle.Regular);
                    break;
                case 3: // 종료 버튼
                    button.ForeColor = Color.Silver;
                    break;
                default:
                    break;
            }
        }
        #endregion ## Button Mouse Leave Event ##
        #endregion ## UI Event ##
    }
}
