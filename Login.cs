using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace P4EXP
{
    public partial class Login : Form
    {
        public Login(string user, string port)
        {
            InitializeComponent();

            this.msgLbl.Text =
                string.Format(Properties.Resources.Login_PasswordRequired,
                user, port);
        }

        public string password = null;

        private void OKBtn_Click(object sender, EventArgs e)
        {
            password = passwordTB.Text;
            DialogResult = DialogResult.OK;
        }

        private void passwordTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (sender != null)
                    OKBtn_Click(null, null);
                e.Handled = true;
            }
        }

     }
}
