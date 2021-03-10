using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Perforce.P4;
using System.Configuration;
using Microsoft.Win32;
using System.Reflection;

namespace P4EXP
{
    public partial class Preferences : Form
    {
        Properties.Settings mySettings = new Properties.Settings();

        public Preferences()
        {
            InitializeComponent();

            iconsCB.Checked = mySettings.ShowIcons;
            deletePromptChkB.Checked = mySettings.warnDelete;
            messagesCB.Checked = mySettings.ShowOnlyErrors;
            P4ExceptionCB.Checked = mySettings.ShowP4Exception;
            ExceptionCB.Checked = mySettings.ShowException;
            logToFileCB.Checked = mySettings.LogToFile;
            nameTB.Enabled = logToFileCB.Checked;
            sizeTB.Enabled = logToFileCB.Checked;
            nameTB.Text = mySettings.LogFilePath;
            if (String.IsNullOrEmpty(nameTB.Text))
            {
                nameTB.Text = Properties.Resources.PreferencesDlg_Logging_PathTB;
            }
            if(mySettings.LogFileSize == 0)
            {
                sizeTB.Text = Properties.Resources.PreferencesDlg_LoggingSizeTB;
            }
            else
            {
                sizeTB.Text = mySettings.LogFileSize.ToString();
            }
            diagLoggingCB.Enabled= logToFileCB.Checked;
            diagLoggingCB.Checked = mySettings.DiagnosticLogging;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            int size = 500;
            int.TryParse(sizeTB.Text, out size);
            mySettings.LogFileSize = size;
            mySettings.LogFilePath = nameTB.Text;
            mySettings.Save();
            SetRegistryConfigLocation();
            Dispose();
        }

        private int GetPlatform()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds p;
            ImageFileMachine machineInfo;
            assembly.ManifestModule.GetPEKind(out p, out machineInfo);
            int platform = 64;
            if (machineInfo.ToString().EndsWith("86"))
            { platform = 86; }
            return platform;
        }
        private void SetRegistryConfigLocation()
        {
            int platform = GetPlatform();

            string userRoot = "HKEY_CURRENT_USER";
            string subkey = "Software\\Perforce\\P4EXP";
            if (platform==86 && Environment.Is64BitOperatingSystem)
            {
                subkey = "SOFTWARE\\WOW6432Node\\Perforce\\P4EXP";
            }
            string keyName = userRoot + "\\" + subkey;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Registry.SetValue(keyName, "UserConfigLocation", config.FilePath);
        }
        private void iconsCB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.ShowIcons = iconsCB.Checked;
        }

        private void messagesCB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.ShowOnlyErrors = messagesCB.Checked;
        }

        private void P4ExceptionCB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.ShowP4Exception = P4ExceptionCB.Checked;
        }

        private void ExceptionCB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.ShowException = ExceptionCB.Checked;
        }

        private void selectBtn_Click(object sender, EventArgs e)
        {
            System.IO.Stream logSelectStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = Properties.Resources.SaveFileDlg_Filter;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = Properties.Resources.SaveFileDlg_Title;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((logSelectStream = saveFileDialog.OpenFile()) != null)
                {
                    nameTB.Text = saveFileDialog.FileName;
                    logSelectStream.Close();
                }
            }

        }

        private void sizeTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
            {
                e.Handled = true;
            }

            if ((sizeTB.Text.Length > 4) && (!(char.IsControl(e.KeyChar))))
            {
                e.Handled = true;
            }
        }

        private void logToFileCB_CheckedChanged(object sender, EventArgs e)
        {
            nameTB.Enabled = logToFileCB.Checked;
            sizeTB.Enabled = logToFileCB.Checked;
            diagLoggingCB.Enabled = logToFileCB.Checked;
            mySettings.LogToFile = logToFileCB.Checked;
        }
        private void diagLoggingCB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.DiagnosticLogging = diagLoggingCB.Checked;
        }

        private void deletePromptChkB_CheckedChanged(object sender, EventArgs e)
        {
            mySettings.warnDelete = deletePromptChkB.Checked;
        }
    }
}
