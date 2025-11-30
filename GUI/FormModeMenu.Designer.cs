namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormModeMenu
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
            this.btnModeMenu1 = new System.Windows.Forms.Button();
            this.btnModeMenu2 = new System.Windows.Forms.Button();
            this.btnModeMenu3 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnModeMenu1
            // 
            this.btnModeMenu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnModeMenu1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnModeMenu1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModeMenu1.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnModeMenu1.ForeColor = System.Drawing.Color.Silver;
            this.btnModeMenu1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnModeMenu1.Location = new System.Drawing.Point(5, 5);
            this.btnModeMenu1.Name = "btnModeMenu1";
            this.btnModeMenu1.Size = new System.Drawing.Size(140, 50);
            this.btnModeMenu1.TabIndex = 10;
            this.btnModeMenu1.Text = "대기 모드";
            this.btnModeMenu1.UseVisualStyleBackColor = false;
            // 
            // btnModeMenu2
            // 
            this.btnModeMenu2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnModeMenu2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnModeMenu2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModeMenu2.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnModeMenu2.ForeColor = System.Drawing.Color.Silver;
            this.btnModeMenu2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnModeMenu2.Location = new System.Drawing.Point(5, 61);
            this.btnModeMenu2.Name = "btnModeMenu2";
            this.btnModeMenu2.Size = new System.Drawing.Size(140, 50);
            this.btnModeMenu2.TabIndex = 11;
            this.btnModeMenu2.Text = "모의 주식 모드";
            this.btnModeMenu2.UseVisualStyleBackColor = false;
            // 
            // btnModeMenu3
            // 
            this.btnModeMenu3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnModeMenu3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnModeMenu3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModeMenu3.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnModeMenu3.ForeColor = System.Drawing.Color.Silver;
            this.btnModeMenu3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnModeMenu3.Location = new System.Drawing.Point(5, 117);
            this.btnModeMenu3.Name = "btnModeMenu3";
            this.btnModeMenu3.Size = new System.Drawing.Size(140, 50);
            this.btnModeMenu3.TabIndex = 12;
            this.btnModeMenu3.Text = "자동 매매 모드";
            this.btnModeMenu3.UseVisualStyleBackColor = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormModeMenu
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(150, 172);
            this.Controls.Add(this.btnModeMenu3);
            this.Controls.Add(this.btnModeMenu2);
            this.Controls.Add(this.btnModeMenu1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormModeMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormModeMenu";
            this.Load += new System.EventHandler(this.FormModeMenu_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnModeMenu1;
        private System.Windows.Forms.Button btnModeMenu2;
        private System.Windows.Forms.Button btnModeMenu3;
        private System.Windows.Forms.Timer timer1;
    }
}