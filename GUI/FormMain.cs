
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormMain : Form
    {
        #region ## FormMain Define ##
        private Panel[] PalMainArray;

        private FormDataChart formnDataChart = new FormDataChart();
        private FormGraphic formGraphic = new FormGraphic();
        private FormMenu formMenu = new FormMenu();
        private FormStatus formStatus = new FormStatus();
        #endregion ## FormMain Define ##

        public FormMain()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            PalMainArray = new Panel[]
            {
                palMain1, // 0 : 메뉴 창
                palMain2, // 1 : 차트 창
                palMain3, // 2 : 데이터 창
                palMain4, // 3 : 상태 창
            };
        }
        #endregion ## UI Organize ##

        #region ## FormMain Load ##
        private void FormMain_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormMain Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;

            this.Location = new Point(0, 0);
            this.Size = new Size(2560, 60);

            PalMainArray[0].Visible = true;
            PalMainArray[1].Visible = false;
            PalMainArray[2].Visible = false;
            PalMainArray[3].Visible = false;
            PalMainArray[3].Dock = DockStyle.None;

            formMenu.Location = new Point(0, 0);
            formMenu.Size = new Size(PalMainArray[0].Width, PalMainArray[0].Height);
            formMenu.TopLevel = false;
            formMenu.TopMost = false;

            PalMainArray[0].Visible = true;
            PalMainArray[0].Location = new Point(0, 0);
            PalMainArray[0].Size = new Size(2560, 60);
            PalMainArray[0].Controls.Add(formMenu);
            PalMainArray[0].Dock = DockStyle.Top;

            formMenu.Show();
        }
        #endregion ## UI Update ##

        #region ## Form Change ##
        public void FormChange(int frame)
        {
            if (frame == Flag.ModeNumber.Logout)
            {
                this.Location = new Point(0, 0);
                this.Size = new Size(2560, 60);

                PalMainArray[0].Visible = true;
                PalMainArray[1].Visible = false;
                PalMainArray[2].Visible = false;
                PalMainArray[3].Visible = false;
                PalMainArray[3].Dock = DockStyle.None;

                formMenu.Location = new Point(0, 0);
                formMenu.Size = new Size(PalMainArray[0].Width, PalMainArray[0].Height);
                formMenu.TopLevel = false;
                formMenu.TopMost = false;

                PalMainArray[0].Visible = true;
                PalMainArray[0].Location = new Point(0, 0);
                PalMainArray[0].Size = new Size(2560, 60);
                PalMainArray[0].Controls.Add(formMenu);
                PalMainArray[0].Dock = DockStyle.Top;

                formMenu.Show();
            }
            else
            {

                this.Location = new Point(0, 0);
                this.Size = new Size(2560, 1440);

                formMenu.Location = new Point(0, 0);
                formMenu.Size = new Size(PalMainArray[0].Width, PalMainArray[0].Height);
                formMenu.TopLevel = false;
                formMenu.TopMost = false;

                formGraphic.Location = new Point(0, 0);
                formGraphic.Size = new Size(PalMainArray[1].Width, PalMainArray[1].Height);
                formGraphic.TopLevel = false;
                formGraphic.TopMost = false;

                formnDataChart.Location = new Point(0, 0);
                formnDataChart.Size = new Size(PalMainArray[2].Width, PalMainArray[2].Height);
                formnDataChart.TopLevel = false;
                formnDataChart.TopMost = false;

                formStatus.Location = new Point(0, 0);
                formStatus.Size = new Size(PalMainArray[3].Width, PalMainArray[3].Height);
                formStatus.TopLevel = false;
                formStatus.TopMost = false;

                PalMainArray[0].Visible = true;
                PalMainArray[0].Location = new Point(0, 0);
                PalMainArray[0].Size = new Size(2560, 60);
                PalMainArray[0].Controls.Add(formMenu);
                PalMainArray[0].Dock = DockStyle.Top;

                PalMainArray[1].Visible = true;
                PalMainArray[1].Location = new Point(0, 60);
                PalMainArray[1].Size = new Size(2560, 720);
                PalMainArray[1].Controls.Add(formGraphic);

                PalMainArray[2].Visible = true;
                PalMainArray[2].Location = new Point(0, 780);
                PalMainArray[2].Size = new Size(2560, 600);
                PalMainArray[2].Controls.Add(formnDataChart);

                PalMainArray[3].Visible = true;
                PalMainArray[3].Location = new Point(0, 1380);
                PalMainArray[3].Size = new Size(2560, 60);
                PalMainArray[3].Controls.Add(formStatus);
                PalMainArray[3].Dock = DockStyle.Bottom;
            }

            formGraphic.Hide();
            formnDataChart.Hide();
            formStatus.Hide();

            switch (frame)
            {
                case Flag.ModeNumber.Logout:

                    if (Share.Ins.IsFormOpen(typeof(FormLogin)) == false)
                    {
                        FormLogin formLogin = new FormLogin();

                        formLogin.Show();
                    }
                    else
                    {
                        Share.Ins.MessageFormOpen("이미 로그인 창이 열려있습니다.");
                        return;
                    }
                    break;

                case Flag.ModeNumber.Home:
                    formGraphic.Show();
                    formnDataChart.Show();
                    formStatus.Show();
                    break;

                case Flag.ModeNumber.Simul:
                    formGraphic.Show();
                    formnDataChart.Show();
                    formStatus.Show();
                    break;

                case Flag.ModeNumber.Auto:
                    formGraphic.Show();
                    formnDataChart.Show();
                    formStatus.Show();
                    break;

                case Flag.ModeNumber.Error:
                    break;

                default:

                    break;
            }

            Flag.Live.Runmode = frame;
        }
        #endregion ## Form Change ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(Flag.Live.FormChange == true)
            {
                FormChange(Flag.Live.Runmode);
                Flag.Live.FormChange = false;
            }
        }
        #endregion ## Timer Event ##
    }
}
