using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.DATABASE;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataBase : Form
    {
        private DB_Manager dbManager;

        // 동적으로 생성할 UI 컨트롤들
        private ComboBox cmbTables;
        private Button btnLoad;
        private Button btnSave;
        private DataGridView dgvData;

        // 현재 화면에 띄워진 테이블의 데이터
        private DataTable currentDataTable;

        public FormDataBase()
        {
            InitializeComponent();
            dbManager = new DB_Manager();
        }

        private void FormDataBase_Load(object sender, EventArgs e)
        {
            // 1. UI 컨트롤들을 Panel1 내부에 동적으로 생성 및 배치
            InitializeUIControls();

            // 2. DB에서 테이블 목록을 불러와서 콤보박스에 세팅
            LoadTableNames();
        }

        private void InitializeUIControls()
        {
            // --- 상단 컨트롤 배치용 패널 (콤보박스, 버튼 등) ---
            Panel topPanel = new Panel();
            topPanel.Height = 50;
            topPanel.Dock = DockStyle.Top;
            topPanel.BackColor = Color.FromArgb(15, 15, 25); // 다크 테마 포인트 컬러
            this.panel1.Controls.Add(topPanel);

            // 테이블 선택 콤보박스
            Label lblTable = new Label() { Text = "테이블 선택 :", ForeColor = Color.White, Location = new Point(20, 15), AutoSize = true, Font = new Font("맑은 고딕", 11, FontStyle.Bold) };
            cmbTables = new ComboBox() { Location = new Point(130, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("맑은 고딕", 11) };
            cmbTables.SelectedIndexChanged += CmbTables_SelectedIndexChanged; // 콤보박스 변경 시 자동 로드 이벤트 연결

            // 불러오기 버튼 (새로고침용)
            btnLoad = new Button() { Text = "새로고침", Location = new Point(390, 10), Width = 100, Height = 30, ForeColor = Color.White, BackColor = Color.FromArgb(60, 60, 80), FlatStyle = FlatStyle.Flat };
            btnLoad.Click += BtnLoad_Click;

            // 저장하기 버튼
            btnSave = new Button() { Text = "💾 변경사항 DB에 저장", Location = new Point(500, 10), Width = 180, Height = 30, ForeColor = Color.White, BackColor = Color.DarkGreen, FlatStyle = FlatStyle.Flat, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;

            topPanel.Controls.Add(lblTable);
            topPanel.Controls.Add(cmbTables);
            topPanel.Controls.Add(btnLoad);
            topPanel.Controls.Add(btnSave);

            // --- 데이터베이스 표 (DataGridView) 세팅 ---
            dgvData = new DataGridView();
            dgvData.Dock = DockStyle.Fill;
            dgvData.BackgroundColor = Color.FromArgb(20, 20, 30); // 다크 테마 배경
            dgvData.ForeColor = Color.Black;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.AllowUserToAddRows = true; // 새로운 행 추가 허용 (INSERT)
            dgvData.AllowUserToDeleteRows = true; // 행 삭제 허용 (DELETE)

            this.panel1.Controls.Add(dgvData);

            // DataGridView를 topPanel 아래로 보내서 화면에 꽉 차게 설정
            dgvData.BringToFront();
        }

        private void LoadTableNames()
        {
            List<string> tables = dbManager.GetAllTableNames();
            cmbTables.Items.Clear();
            foreach (string table in tables)
            {
                cmbTables.Items.Add(table);
            }

            if (cmbTables.Items.Count > 0)
                cmbTables.SelectedIndex = 0; // 첫 번째 테이블 자동 선택
        }

        // 콤보박스에서 다른 테이블을 선택했을 때
        private void CmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTableData();
        }

        // 새로고침 버튼 눌렀을 때
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            LoadTableData();
        }

        // 선택한 테이블의 데이터를 DataGridView에 바인딩
        private void LoadTableData()
        {
            if (cmbTables.SelectedItem == null) return;
            string selectedTable = cmbTables.SelectedItem.ToString();

            currentDataTable = dbManager.GetRawTableData(selectedTable);
            dgvData.DataSource = currentDataTable;
        }

        // 저장 버튼 눌렀을 때 (UPDATE, INSERT, DELETE 실행)
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem == null || currentDataTable == null) return;

            // 현재 타이핑 중인 셀의 데이터까지 확실히 DataTable에 적용하기 위한 마감 처리
            dgvData.EndEdit();

            string selectedTable = cmbTables.SelectedItem.ToString();

            // 변경된 내역을 DB_Manager를 통해 SQLite에 덮어쓰기
            bool success = dbManager.UpdateTableData(selectedTable, currentDataTable);

            if (success)
            {
                MessageBox.Show($"{selectedTable} 테이블의 변경사항이 성공적으로 저장되었습니다!", "저장 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTableData(); // 저장 후 깔끔하게 다시 불러오기
            }
            else
            {
                MessageBox.Show("DB 저장 중 오류가 발생했습니다.\n파이썬 프로그램이 DB를 사용 중인지 확인해주세요.", "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}