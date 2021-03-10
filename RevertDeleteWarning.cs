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
    public partial class RevertDeleteWarning : Form
    {
        Properties.Settings mySettings = new Properties.Settings();
        public RevertDeleteWarning(bool delete)
        {
            InitializeComponent();
            Text = Properties.Resources.RevertDeleteWarningDlg_RevertWarningTitle;
            if (delete)
            {
                Text = Properties.Resources.RevertDeleteWarningDlg_DeleteWarningTitle;
                deleteChkB.Enabled = true;
                deleteChkB.Visible = true;
                deleteChkB.Checked = !mySettings.warnDelete;
                warningLbl1.Text = Properties.Resources.RevertDeleteWarningDlg_DeleteWarningLabel;
            }
        }

        private void YesBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult=DialogResult.OK;
            mySettings.Save();
            Dispose();
        }

        private void NoBtn_Click(object sender, EventArgs e)
        {
            mySettings.Save();

            Dispose();
        }

        private void deleteChkB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.warnDelete = !deleteChkB.Checked;
        }
    }
}
