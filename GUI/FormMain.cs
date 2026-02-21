
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormMain : Form
    {
        #region ## FormMain Define ##
        private FormMenu formMenu = new FormMenu();
        private FormStatus formStatus = new FormStatus();

        private Panel[] PalMainArray;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private bool mouseDown = false;
        private Point lastLocation;
        private bool WindowFull = false;
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

            EnableDrag(PalMainArray[0]);
            EnableDrag(formMenu);   // ★ 추가: child form도 드래그 가능
            EnableDrag(Auto.Ins.formGraphic);
            EnableDrag(Auto.Ins.formDataChart);
            EnableDrag(formStatus);
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

        #region ## UI Event ##

        #region ## Panel Move Down Event ###
        private void PanelMoveDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // ★ 더블클릭이면 창 토글만 하고 드래그는 안 함
                if (e.Clicks == 2)
                {
                    FrameControl();
                    return;
                }

                // 여기부터는 "싱글클릭 → 드래그" 동작
                mouseDown = true;
                lastLocation = e.Location;

                // 윈도우 기본 드래그 스타일 활성화 (제목바 드래그처럼)
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        #endregion ## Panel Move Down Event ###

        #region ## Panel Mouse Move Event ###
        private void PanelMouseMoveEvent(object sender, MouseEventArgs e)
        {

        }
        #endregion ## Panel Mouse Move Event ###

        #endregion ## UI Event ##

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

        #region ## FrameControl ##
        private void FrameControl()
        {
            WindowFull = !WindowFull;

            if (WindowFull == true)
            {
                Flag.Live.Runmode = Flag.ModeNumber.Hide;
                Flag.Live.FormChange = true;
            }
            else
            {
                Flag.Live.Runmode = Flag.Live.OldFormMode;
                Flag.Live.FormChange = true;
            }
        }
        #endregion ## FrameControl ##

        #region ## EnableDrag ##
        private void EnableDrag(Control parent)
        {
            // [수정] 차트(FormsPlot) 영역에서는 창 드래그 기능이 안 먹히게 예외 처리 추가!
            if (parent is Button ||
                parent is TextBox ||
                parent is ComboBox ||
                parent is CheckBox ||
                parent is RadioButton ||
                parent is ListView ||
                parent is DataGridView ||
                parent.GetType().Name == "FormsPlot") // <--- 이 줄을 꼭 추가하세요!
            {
                return;
            }

            parent.MouseDown += PanelMoveDownEvent;
            parent.MouseMove += PanelMouseMoveEvent;

            foreach (Control c in parent.Controls)
                EnableDrag(c);

            if (parent is Form f)
            {
                foreach (Control c in f.Controls)
                    EnableDrag(c);
            }
        }
        #endregion ## EnableDrag ##
        #region ## Form Change ##
        public void FormChange(int frame)
        {
            if (frame == Flag.ModeNumber.Hide)
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
                return;
            }

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

                Auto.Ins.formGraphic.Location = new Point(0, 0);
                Auto.Ins.formGraphic.Size = new Size(PalMainArray[1].Width, PalMainArray[1].Height);
                Auto.Ins.formGraphic.TopLevel = false;
                Auto.Ins.formGraphic.TopMost = false;

                Auto.Ins.formDataChart.Location = new Point(0, 0);
                Auto.Ins.formDataChart.Size = new Size(PalMainArray[2].Width, PalMainArray[2].Height);
                Auto.Ins.formDataChart.TopLevel = false;
                Auto.Ins.formDataChart.TopMost = false;

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
                PalMainArray[1].Controls.Add(Auto.Ins.formGraphic);

                PalMainArray[2].Visible = true;
                PalMainArray[2].Location = new Point(0, 780);
                PalMainArray[2].Size = new Size(2560, 600);
                PalMainArray[2].Controls.Add(Auto.Ins.formDataChart);

                PalMainArray[3].Visible = true;
                PalMainArray[3].Location = new Point(0, 1380);
                PalMainArray[3].Size = new Size(2560, 60);
                PalMainArray[3].Controls.Add(formStatus);
                PalMainArray[3].Dock = DockStyle.Bottom;
            }

            Auto.Ins.formGraphic.Hide();
            Auto.Ins.formDataChart.Hide();
            formStatus.Hide();
            Flag.Live.OldFormMode = frame;

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
                    Auto.Ins.formGraphic.Show();
                    Auto.Ins.formDataChart.Show();
                    formStatus.Show();
                    break;

                case Flag.ModeNumber.Simul:
                    Auto.Ins.formGraphic.Show();
                    Auto.Ins.formDataChart.Show();
                    formStatus.Show();
                    break;

                case Flag.ModeNumber.Auto:
                    Auto.Ins.formGraphic.Show();
                    Auto.Ins.formDataChart.Show();
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
    }
}
