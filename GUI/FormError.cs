using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormError : Form
    {
        #region ## FormError Define ##
        private Label[] labErrorArray;
        private Button[] btnErrorArray;
        #endregion ## FormError Define ##
        public FormError()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        private void UIOrganize()
        {
            labErrorArray = new Label[]
            {
                labError1, // 0 : 에러 창
                labError2, // 1 : 에러 내용
            };

            btnErrorArray = new Button[]
            {
                btnError1, // 0 : 에러 초기화 버튼
            };
            for (int i = 0; i < btnErrorArray.Length; i++)
            {
                btnErrorArray[i].Click += ButtonClickEvent;
            }
        }
        #endregion ## UI Organize ##

        #region ## FormError Load ##
        private void FormError_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormError Load ##

        #region ## UI Update ##
        private void UIUpdate()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
        }
        #endregion ## UI Update ##

        #region ## UI Event ##
        private void ButtonClickEvent(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int n = int.Parse(button.Name.Replace("btnError", "")) - int.Parse(btnErrorArray[0].Name.Replace("btnError", ""));
            switch (n)
            {
                case 0: // 에러 초기화 버튼

                    if(Flag.Live.ErrorClearSuccess == true)
                    {
                        this.Close();
                    }
                    else
                    {
                        return;
                    }
                    break;
            }

        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        #endregion ## Timer Event ##
    }
}
