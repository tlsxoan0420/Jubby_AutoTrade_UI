namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormGraphic
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
            this.palGrapic1 = new System.Windows.Forms.Panel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // palGrapic1
            // 
            this.palGrapic1.Location = new System.Drawing.Point(0, 0);
            this.palGrapic1.Name = "palGrapic1";
            this.palGrapic1.Size = new System.Drawing.Size(2560, 720);
            this.palGrapic1.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormGraphic
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.ClientSize = new System.Drawing.Size(2560, 720);
            this.Controls.Add(this.palGrapic1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormGraphic";
            this.Text = "FormGraphic";
            this.Load += new System.EventHandler(this.FormGraphic_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel palGrapic1;
        public System.Windows.Forms.Timer timer1;
    }
}