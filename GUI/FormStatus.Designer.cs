namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormStatus
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labStatus1 = new System.Windows.Forms.Label();
            this.labStatus2 = new System.Windows.Forms.Label();
            this.labStatus3 = new System.Windows.Forms.Label();
            this.labStatus4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labStatus1
            // 
            this.labStatus1.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.labStatus1.ForeColor = System.Drawing.Color.Silver;
            this.labStatus1.Location = new System.Drawing.Point(10, 5);
            this.labStatus1.Name = "labStatus1";
            this.labStatus1.Size = new System.Drawing.Size(1225, 20);
            this.labStatus1.TabIndex = 2;
            this.labStatus1.Text = "빌드 시간";
            this.labStatus1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labStatus2
            // 
            this.labStatus2.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.labStatus2.ForeColor = System.Drawing.Color.Silver;
            this.labStatus2.Location = new System.Drawing.Point(10, 35);
            this.labStatus2.Name = "labStatus2";
            this.labStatus2.Size = new System.Drawing.Size(1225, 20);
            this.labStatus2.TabIndex = 3;
            this.labStatus2.Text = "권한";
            this.labStatus2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labStatus3
            // 
            this.labStatus3.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.labStatus3.ForeColor = System.Drawing.Color.Silver;
            this.labStatus3.Location = new System.Drawing.Point(1325, 5);
            this.labStatus3.Name = "labStatus3";
            this.labStatus3.Size = new System.Drawing.Size(1225, 20);
            this.labStatus3.TabIndex = 4;
            this.labStatus3.Text = "현재 시간";
            this.labStatus3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labStatus4
            // 
            this.labStatus4.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.labStatus4.ForeColor = System.Drawing.Color.Silver;
            this.labStatus4.Location = new System.Drawing.Point(1325, 35);
            this.labStatus4.Name = "labStatus4";
            this.labStatus4.Size = new System.Drawing.Size(1225, 20);
            this.labStatus4.TabIndex = 5;
            this.labStatus4.Text = "상태 및 구동시간";
            this.labStatus4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormStatus
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(2560, 60);
            this.Controls.Add(this.labStatus4);
            this.Controls.Add(this.labStatus3);
            this.Controls.Add(this.labStatus2);
            this.Controls.Add(this.labStatus1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormStatus";
            this.Text = "FormStatus";
            this.Load += new System.EventHandler(this.FormStatus_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labStatus1;
        private System.Windows.Forms.Label labStatus2;
        private System.Windows.Forms.Label labStatus3;
        private System.Windows.Forms.Label labStatus4;
    }
}