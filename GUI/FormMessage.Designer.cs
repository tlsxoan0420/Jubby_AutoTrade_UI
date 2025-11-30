namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormMessage
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
            this.labMessage1 = new System.Windows.Forms.Label();
            this.labMessage2 = new System.Windows.Forms.Label();
            this.labMessage3 = new System.Windows.Forms.Label();
            this.btnMessage1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnMessage2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labMessage1
            // 
            this.labMessage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.labMessage1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMessage1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labMessage1.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labMessage1.ForeColor = System.Drawing.Color.White;
            this.labMessage1.Location = new System.Drawing.Point(0, 0);
            this.labMessage1.Name = "labMessage1";
            this.labMessage1.Size = new System.Drawing.Size(800, 40);
            this.labMessage1.TabIndex = 19;
            this.labMessage1.Text = "메세지 창";
            this.labMessage1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labMessage2
            // 
            this.labMessage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.labMessage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMessage2.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labMessage2.ForeColor = System.Drawing.Color.Lime;
            this.labMessage2.Location = new System.Drawing.Point(0, 40);
            this.labMessage2.Name = "labMessage2";
            this.labMessage2.Size = new System.Drawing.Size(800, 200);
            this.labMessage2.TabIndex = 20;
            this.labMessage2.Text = "메세지 창";
            this.labMessage2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labMessage3
            // 
            this.labMessage3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.labMessage3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMessage3.Font = new System.Drawing.Font("HY헤드라인M", 24F);
            this.labMessage3.ForeColor = System.Drawing.Color.Yellow;
            this.labMessage3.Location = new System.Drawing.Point(0, 240);
            this.labMessage3.Name = "labMessage3";
            this.labMessage3.Size = new System.Drawing.Size(800, 40);
            this.labMessage3.TabIndex = 21;
            this.labMessage3.Text = "메세지 창";
            this.labMessage3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMessage1
            // 
            this.btnMessage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnMessage1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMessage1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMessage1.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMessage1.ForeColor = System.Drawing.Color.Lime;
            this.btnMessage1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMessage1.Location = new System.Drawing.Point(264, 283);
            this.btnMessage1.Name = "btnMessage1";
            this.btnMessage1.Size = new System.Drawing.Size(140, 50);
            this.btnMessage1.TabIndex = 22;
            this.btnMessage1.Text = "확인";
            this.btnMessage1.UseVisualStyleBackColor = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnMessage2
            // 
            this.btnMessage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.btnMessage2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMessage2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMessage2.Font = new System.Drawing.Font("HY헤드라인M", 10F);
            this.btnMessage2.ForeColor = System.Drawing.Color.Red;
            this.btnMessage2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMessage2.Location = new System.Drawing.Point(410, 283);
            this.btnMessage2.Name = "btnMessage2";
            this.btnMessage2.Size = new System.Drawing.Size(140, 50);
            this.btnMessage2.TabIndex = 23;
            this.btnMessage2.Text = "닫기";
            this.btnMessage2.UseVisualStyleBackColor = false;
            // 
            // FormMessage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(800, 335);
            this.Controls.Add(this.btnMessage2);
            this.Controls.Add(this.btnMessage1);
            this.Controls.Add(this.labMessage3);
            this.Controls.Add(this.labMessage2);
            this.Controls.Add(this.labMessage1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMessage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMessage";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormMessage_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnMessage1;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Label labMessage2;
        public System.Windows.Forms.Label labMessage1;
        public System.Windows.Forms.Label labMessage3;
        private System.Windows.Forms.Button btnMessage2;
    }
}