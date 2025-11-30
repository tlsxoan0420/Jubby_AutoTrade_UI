namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormError
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
            this.btnError1 = new System.Windows.Forms.Button();
            this.labError2 = new System.Windows.Forms.Label();
            this.labError1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnError1
            // 
            this.btnError1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnError1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnError1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnError1.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnError1.ForeColor = System.Drawing.Color.Lime;
            this.btnError1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnError1.Location = new System.Drawing.Point(327, 284);
            this.btnError1.Name = "btnError1";
            this.btnError1.Size = new System.Drawing.Size(140, 50);
            this.btnError1.TabIndex = 27;
            this.btnError1.Text = "에러 초기화";
            this.btnError1.UseVisualStyleBackColor = false;
            // 
            // labError2
            // 
            this.labError2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.labError2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labError2.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labError2.ForeColor = System.Drawing.Color.Red;
            this.labError2.Location = new System.Drawing.Point(0, 41);
            this.labError2.Name = "labError2";
            this.labError2.Size = new System.Drawing.Size(800, 240);
            this.labError2.TabIndex = 25;
            this.labError2.Text = "메세지 창";
            this.labError2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labError1
            // 
            this.labError1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.labError1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labError1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labError1.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labError1.ForeColor = System.Drawing.Color.Yellow;
            this.labError1.Location = new System.Drawing.Point(0, 0);
            this.labError1.Name = "labError1";
            this.labError1.Size = new System.Drawing.Size(800, 40);
            this.labError1.TabIndex = 24;
            this.labError1.Text = "에러 창";
            this.labError1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormError
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(800, 335);
            this.Controls.Add(this.btnError1);
            this.Controls.Add(this.labError2);
            this.Controls.Add(this.labError1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormError";
            this.Text = "FormError";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormError_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnError1;
        public System.Windows.Forms.Label labError2;
        public System.Windows.Forms.Label labError1;
        private System.Windows.Forms.Timer timer1;
    }
}