namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormMain
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
            this.palMain1 = new System.Windows.Forms.Panel();
            this.palMain4 = new System.Windows.Forms.Panel();
            this.palMain2 = new System.Windows.Forms.Panel();
            this.palMain3 = new System.Windows.Forms.Panel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // palMain1
            // 
            this.palMain1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.palMain1.Location = new System.Drawing.Point(0, 0);
            this.palMain1.Name = "palMain1";
            this.palMain1.Size = new System.Drawing.Size(2560, 60);
            this.palMain1.TabIndex = 0;
            // 
            // palMain4
            // 
            this.palMain4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.palMain4.Location = new System.Drawing.Point(0, 1380);
            this.palMain4.Name = "palMain4";
            this.palMain4.Size = new System.Drawing.Size(2560, 60);
            this.palMain4.TabIndex = 3;
            // 
            // palMain2
            // 
            this.palMain2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.palMain2.Location = new System.Drawing.Point(0, 60);
            this.palMain2.Name = "palMain2";
            this.palMain2.Size = new System.Drawing.Size(2560, 720);
            this.palMain2.TabIndex = 1;
            // 
            // palMain3
            // 
            this.palMain3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.palMain3.Location = new System.Drawing.Point(0, 780);
            this.palMain3.Name = "palMain3";
            this.palMain3.Size = new System.Drawing.Size(2560, 600);
            this.palMain3.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.ClientSize = new System.Drawing.Size(2560, 1440);
            this.Controls.Add(this.palMain2);
            this.Controls.Add(this.palMain3);
            this.Controls.Add(this.palMain4);
            this.Controls.Add(this.palMain1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "FormMain";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel palMain1;
        private System.Windows.Forms.Panel palMain4;
        private System.Windows.Forms.Panel palMain2;
        private System.Windows.Forms.Panel palMain3;
        private System.Windows.Forms.Timer timer1;
    }
}