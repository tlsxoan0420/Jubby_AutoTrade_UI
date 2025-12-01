namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormMenu
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
            this.btnMenu1 = new System.Windows.Forms.Button();
            this.btnMenu2 = new System.Windows.Forms.Button();
            this.btnMenu10 = new System.Windows.Forms.Button();
            this.btnMenu3 = new System.Windows.Forms.Button();
            this.lblProgramName = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labOperationText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnMenu1
            // 
            this.btnMenu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.btnMenu1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMenu1.FlatAppearance.BorderSize = 0;
            this.btnMenu1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMenu1.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMenu1.ForeColor = System.Drawing.Color.Yellow;
            this.btnMenu1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMenu1.Location = new System.Drawing.Point(1777, 5);
            this.btnMenu1.Name = "btnMenu1";
            this.btnMenu1.Size = new System.Drawing.Size(140, 50);
            this.btnMenu1.TabIndex = 8;
            this.btnMenu1.Text = "로그아웃";
            this.btnMenu1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMenu1.UseVisualStyleBackColor = false;
            // 
            // btnMenu2
            // 
            this.btnMenu2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.btnMenu2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMenu2.FlatAppearance.BorderSize = 0;
            this.btnMenu2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMenu2.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMenu2.ForeColor = System.Drawing.Color.Silver;
            this.btnMenu2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMenu2.Location = new System.Drawing.Point(1923, 5);
            this.btnMenu2.Name = "btnMenu2";
            this.btnMenu2.Size = new System.Drawing.Size(140, 50);
            this.btnMenu2.TabIndex = 9;
            this.btnMenu2.Text = "모드 변경";
            this.btnMenu2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMenu2.UseVisualStyleBackColor = false;
            // 
            // btnMenu10
            // 
            this.btnMenu10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.btnMenu10.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMenu10.FlatAppearance.BorderSize = 0;
            this.btnMenu10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMenu10.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMenu10.ForeColor = System.Drawing.Color.Red;
            this.btnMenu10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMenu10.Location = new System.Drawing.Point(2414, 5);
            this.btnMenu10.Name = "btnMenu10";
            this.btnMenu10.Size = new System.Drawing.Size(140, 50);
            this.btnMenu10.TabIndex = 16;
            this.btnMenu10.Text = "종료";
            this.btnMenu10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMenu10.UseVisualStyleBackColor = false;
            // 
            // btnMenu3
            // 
            this.btnMenu3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.btnMenu3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMenu3.FlatAppearance.BorderSize = 0;
            this.btnMenu3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMenu3.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMenu3.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnMenu3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMenu3.Location = new System.Drawing.Point(2069, 5);
            this.btnMenu3.Name = "btnMenu3";
            this.btnMenu3.Size = new System.Drawing.Size(140, 50);
            this.btnMenu3.TabIndex = 10;
            this.btnMenu3.Text = "데이터 베이스";
            this.btnMenu3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMenu3.UseVisualStyleBackColor = false;
            // 
            // lblProgramName
            // 
            this.lblProgramName.AutoSize = true;
            this.lblProgramName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.lblProgramName.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.lblProgramName.ForeColor = System.Drawing.Color.White;
            this.lblProgramName.Location = new System.Drawing.Point(12, 14);
            this.lblProgramName.Name = "lblProgramName";
            this.lblProgramName.Size = new System.Drawing.Size(318, 32);
            this.lblProgramName.TabIndex = 7;
            this.lblProgramName.Text = "Auto Jubby Program";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labOperationText
            // 
            this.labOperationText.AutoSize = true;
            this.labOperationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.labOperationText.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labOperationText.ForeColor = System.Drawing.Color.White;
            this.labOperationText.Location = new System.Drawing.Point(1161, 14);
            this.labOperationText.Name = "labOperationText";
            this.labOperationText.Size = new System.Drawing.Size(121, 32);
            this.labOperationText.TabIndex = 18;
            this.labOperationText.Text = "가동 중";
            this.labOperationText.Visible = false;
            // 
            // FormMenu
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.ClientSize = new System.Drawing.Size(2560, 60);
            this.Controls.Add(this.labOperationText);
            this.Controls.Add(this.lblProgramName);
            this.Controls.Add(this.btnMenu3);
            this.Controls.Add(this.btnMenu10);
            this.Controls.Add(this.btnMenu2);
            this.Controls.Add(this.btnMenu1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMenu";
            this.Text = "FormMenu";
            this.Load += new System.EventHandler(this.FormMenu_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnMenu1;
        private System.Windows.Forms.Button btnMenu2;
        private System.Windows.Forms.Button btnMenu10;
        private System.Windows.Forms.Button btnMenu3;
        private System.Windows.Forms.Label lblProgramName;
        public System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labOperationText;
    }
}