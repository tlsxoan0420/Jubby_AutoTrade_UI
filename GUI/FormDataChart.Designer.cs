namespace Jubby_AutoTrade_UI.GUI
{
    partial class FormDataChart
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
            this.dgvChart1 = new System.Windows.Forms.DataGridView();
            this.dgvChart2 = new System.Windows.Forms.DataGridView();
            this.dgvChart3 = new System.Windows.Forms.DataGridView();
            this.dgvChart4 = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart4)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvChart1
            // 
            this.dgvChart1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.dgvChart1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChart1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgvChart1.Location = new System.Drawing.Point(7, 7);
            this.dgvChart1.Name = "dgvChart1";
            this.dgvChart1.RowTemplate.Height = 23;
            this.dgvChart1.Size = new System.Drawing.Size(1270, 290);
            this.dgvChart1.TabIndex = 0;
            this.dgvChart1.DoubleClick += new System.EventHandler(this.dgvChart1_DoubleClick);
            // 
            // dgvChart2
            // 
            this.dgvChart2.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.dgvChart2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChart2.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgvChart2.Location = new System.Drawing.Point(1283, 7);
            this.dgvChart2.Name = "dgvChart2";
            this.dgvChart2.RowTemplate.Height = 23;
            this.dgvChart2.Size = new System.Drawing.Size(1270, 290);
            this.dgvChart2.TabIndex = 1;
            // 
            // dgvChart3
            // 
            this.dgvChart3.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.dgvChart3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChart3.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgvChart3.Location = new System.Drawing.Point(7, 303);
            this.dgvChart3.Name = "dgvChart3";
            this.dgvChart3.RowTemplate.Height = 23;
            this.dgvChart3.Size = new System.Drawing.Size(1270, 290);
            this.dgvChart3.TabIndex = 2;
            // 
            // dgvChart4
            // 
            this.dgvChart4.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.dgvChart4.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChart4.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgvChart4.Location = new System.Drawing.Point(1283, 303);
            this.dgvChart4.Name = "dgvChart4";
            this.dgvChart4.RowTemplate.Height = 23;
            this.dgvChart4.Size = new System.Drawing.Size(1270, 290);
            this.dgvChart4.TabIndex = 3;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormDataChart
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.ClientSize = new System.Drawing.Size(2560, 600);
            this.Controls.Add(this.dgvChart4);
            this.Controls.Add(this.dgvChart3);
            this.Controls.Add(this.dgvChart2);
            this.Controls.Add(this.dgvChart1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormDataChart";
            this.Text = "FormDataChart";
            this.Load += new System.EventHandler(this.FormDataChart_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChart4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvChart1;
        private System.Windows.Forms.DataGridView dgvChart2;
        private System.Windows.Forms.DataGridView dgvChart3;
        private System.Windows.Forms.DataGridView dgvChart4;
        public System.Windows.Forms.Timer timer1;
    }
}