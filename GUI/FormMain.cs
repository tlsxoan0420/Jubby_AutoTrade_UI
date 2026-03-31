
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
        private FormDataBase formDataBase = new FormDataBase();

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

        private Size originalSize;
        private FormBorderStyle originalBorderStyle;
        private bool isMiniMode = false;
        private int lastTradeCount = -1;
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
            EnableDrag(formDataBase);
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
            formDataBase.Hide();
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

                case Flag.ModeNumber.Database:
                    formDataBase.Show();
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

        #region ## FormMain Key Event ##
        // =========================================================================
        // ⌨️ [단축키 감지] 폼에서 키보드가 눌렸을 때 실행되는 이벤트
        // 폼의 KeyDown 이벤트에 이 함수를 연결해 주세요!
        // =========================================================================
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl 키와 F7 키가 동시에 눌렸는지 확인!
            if (e.Control && e.KeyCode == Keys.F7)
            {
                ToggleMiniMode(); // 미니 모드 변신 스위치 작동!
            }
        }

        // =========================================================================
        // 📱 [미니 모드] 항상 위 & 귀여운 사이즈로 변신하는 PIP 기능
        // =========================================================================
        private void ToggleMiniMode()
        {
            isMiniMode = !isMiniMode;

            if (isMiniMode)
            {
                // 🟢 미니 모드 ON
                originalSize = this.Size;                    // 현재 폼 크기 기억하기
                originalBorderStyle = this.FormBorderStyle;  // 테두리 스타일 기억하기

                this.FormBorderStyle = FormBorderStyle.None; // 테두리 다 날려버리기
                this.Size = new Size(350, 150);              // 화면 구석에 둘 작고 귀여운 사이즈로 축소
                this.TopMost = true;                         // 웹서핑/게임 중에도 항상 화면 맨 위에 띄우기!

                // (선택) 미니 모드로 변신할 때 알림 띄우기
                ShowToast("미니 모드 작동", "주삐가 미니 모드로 전환되었습니다.");

                // 💡 팁: 여기서 큰 차트(dgvChart)나 거추장스러운 패널들을 숨기고(Visible = false),
                // 총자산/수익률만 보여주는 핵심 라벨만 남기면 완벽합니다.
            }
            else
            {
                // 🔴 미니 모드 OFF (원래대로 복구)
                this.FormBorderStyle = originalBorderStyle;
                this.Size = originalSize;
                this.TopMost = false;

                ShowToast("미니 모드 해제", "주삐가 원래 화면으로 복구되었습니다.");

                // 💡 팁: 숨겼던 차트와 패널들을 다시 보이게(Visible = true) 원상복구 해주세요!
            }
        }

        // =========================================================================
        // 🔔 카카오톡처럼 윈도우 우측 하단에 알림(Toast) 띄우는 만능 함수
        // =========================================================================
        public void ShowToast(string title, string message)
        {
            // 트레이 아이콘이 없으면 알림을 띄울 수 없으므로 무조건 활성화
            notifyIconMain.Visible = true;

            // 알림창 세팅
            notifyIconMain.BalloonTipTitle = title;         // 알림 제목
            notifyIconMain.BalloonTipText = message;        // 알림 내용
            notifyIconMain.BalloonTipIcon = ToolTipIcon.Info; // 아이콘 모양 (Info, Error, Warning 등)

            // 3초(3000ms) 동안 화면 우측 하단에 띄웁니다!
            notifyIconMain.ShowBalloonTip(3000);
        }
        #endregion
    }
}
