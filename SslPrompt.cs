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
    public partial class SslPrompt : Form
    {
        public SslPrompt()
        {
            Icon = System.Drawing.SystemIcons.Warning;
            InitializeComponent();
        }

        public string password = null;

        private void OKBtn_Click(object sender, EventArgs e)
        {
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

        public static string FingerPrint { get; set; }

        private static bool IsHexDigit(char c)
        {
            if (char.IsDigit(c))
            {
                return true;
            }
            switch (c)
            {
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'E':
                case 'e':
                case 'F':
                case 'f':
                    return true;
            }

            return false;
        }
        public static DialogResult ShowFirstContact(string[] msg)
        {
            SslPrompt dlg = new SslPrompt();

            dlg.WarningTB.Text = string.Empty;
            for (int idx = 0; idx < msg.Length; idx++)
            {
                if (string.IsNullOrEmpty(msg[idx]) == false)
                {
                    dlg.WarningTB.Text += msg[idx];

                    if (IsHexDigit(msg[idx][0]) && IsHexDigit(msg[idx][1]))
                    {
                        FingerPrint = msg[idx];
                        break;
                    }

                    dlg.WarningTB.Text += "\r\n";
                }
            }
            dlg.ShowWarningLabel = false;
            dlg.WarningTB.SelectionLength = 0;
            dlg.TrustCB.Focus();

            return dlg.ShowDialog();
        }

        public static DialogResult ShowNewFingerprint(string[] msg)
        {
            SslPrompt dlg = new SslPrompt();

            dlg.WarningTB.Text = string.Empty;
            //skip first line
            for (int idx = 1; idx < msg.Length; idx++)
            {
                if (string.IsNullOrEmpty(msg[idx]) == false)
                {
                    dlg.WarningTB.Text += msg[idx];

                    if (IsHexDigit(msg[idx][0]) && IsHexDigit(msg[idx][1]))
                    {
                        FingerPrint = msg[idx];
                        break;
                    }

                    dlg.WarningTB.Text += "\r\n";
                }
            }
            dlg.ShowWarningLabel = true;
            dlg.WarningTB.SelectionLength = 0;
            dlg.TrustCB.Focus();

            return dlg.ShowDialog();
        }

        public bool ShowWarningLabel
        {
            get { return WarningLbl.Visible; }
            set
            {
                WarningLbl.Visible = value;
            }
        }
        private void TrustCB_CheckedChanged(object sender, EventArgs e)
        {
            ConnectBtn.Enabled = TrustCB.Checked;
        }
     }
}
