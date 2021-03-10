namespace P4EXP
{
    partial class Preferences
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Preferences));
            this.iconsCB = new System.Windows.Forms.CheckBox();
            this.messagesCB = new System.Windows.Forms.CheckBox();
            this.OKBtn = new System.Windows.Forms.Button();
            this.ExceptionCB = new System.Windows.Forms.CheckBox();
            this.P4ExceptionCB = new System.Windows.Forms.CheckBox();
            this.logToFileCB = new System.Windows.Forms.CheckBox();
            this.pathLbl = new System.Windows.Forms.Label();
            this.sizeLbl = new System.Windows.Forms.Label();
            this.sizeTB = new System.Windows.Forms.TextBox();
            this.nameTB = new System.Windows.Forms.TextBox();
            this.selectBtn = new System.Windows.Forms.Button();
            this.diagLoggingCB = new System.Windows.Forms.CheckBox();
            this.deletePromptChkB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // iconsCB
            // 
            this.iconsCB.AutoSize = true;
            this.iconsCB.Location = new System.Drawing.Point(12, 11);
            this.iconsCB.Name = "iconsCB";
            this.iconsCB.Size = new System.Drawing.Size(406, 17);
            this.iconsCB.TabIndex = 0;
            this.iconsCB.Text = Properties.Resources.PreferencesDlg_IconsCB;
            this.iconsCB.UseVisualStyleBackColor = true;
            this.iconsCB.CheckedChanged += new System.EventHandler(this.iconsCB_CheckedChanged);
            // 
            // messagesCB
            // 
            this.messagesCB.AutoSize = true;
            this.messagesCB.Location = new System.Drawing.Point(12, 71);
            this.messagesCB.Name = "messagesCB";
            this.messagesCB.Size = new System.Drawing.Size(149, 17);
            this.messagesCB.TabIndex = 1;
            this.messagesCB.Text = Properties.Resources.PreferencesDlg_MessagesCB;
            this.messagesCB.UseVisualStyleBackColor = true;
            this.messagesCB.CheckedChanged += new System.EventHandler(this.messagesCB_CheckedChanged);
            // 
            // OKBtn
            // 
            this.OKBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKBtn.Location = new System.Drawing.Point(335, 216);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 7;
            this.OKBtn.Text = Properties.Resources.PreferencesDlg_OKBtn;
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // ExceptionCB
            // 
            this.ExceptionCB.AutoSize = true;
            this.ExceptionCB.Enabled = false;
            this.ExceptionCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExceptionCB.ForeColor = System.Drawing.Color.Red;
            this.ExceptionCB.Location = new System.Drawing.Point(12, 217);
            this.ExceptionCB.Name = "ExceptionCB";
            this.ExceptionCB.Size = new System.Drawing.Size(168, 17);
            this.ExceptionCB.TabIndex = 22;
            this.ExceptionCB.TabStop = false;
            this.ExceptionCB.Text = Properties.Resources.PreferencesDlg_ExceptionsCB;
            this.ExceptionCB.UseVisualStyleBackColor = true;
            this.ExceptionCB.Visible = false;
            this.ExceptionCB.CheckedChanged += new System.EventHandler(this.ExceptionCB_CheckedChanged);
            // 
            // P4ExceptionCB
            // 
            this.P4ExceptionCB.AutoSize = true;
            this.P4ExceptionCB.Enabled = false;
            this.P4ExceptionCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.P4ExceptionCB.ForeColor = System.Drawing.Color.Red;
            this.P4ExceptionCB.Location = new System.Drawing.Point(12, 248);
            this.P4ExceptionCB.Name = "P4ExceptionCB";
            this.P4ExceptionCB.Size = new System.Drawing.Size(183, 17);
            this.P4ExceptionCB.TabIndex = 22;
            this.P4ExceptionCB.TabStop = false;
            this.P4ExceptionCB.Text = Properties.Resources.PreferencesDlg_P4ExceptionsCB;
            this.P4ExceptionCB.UseVisualStyleBackColor = true;
            this.P4ExceptionCB.Visible = false;
            this.P4ExceptionCB.CheckedChanged += new System.EventHandler(this.P4ExceptionCB_CheckedChanged);
            // 
            // logToFileCB
            // 
            this.logToFileCB.AutoSize = true;
            this.logToFileCB.Location = new System.Drawing.Point(12, 101);
            this.logToFileCB.Name = "logToFileCB";
            this.logToFileCB.Size = new System.Drawing.Size(124, 17);
            this.logToFileCB.TabIndex = 2;
            this.logToFileCB.Text = Properties.Resources.PreferencesDlg_LoggingCB;
            this.logToFileCB.UseVisualStyleBackColor = true;
            this.logToFileCB.CheckedChanged += new System.EventHandler(this.logToFileCB_CheckedChanged);
            // 
            // pathLbl
            // 
            this.pathLbl.AutoSize = true;
            this.pathLbl.Location = new System.Drawing.Point(34, 132);
            this.pathLbl.Name = "pathLbl";
            this.pathLbl.Size = new System.Drawing.Size(38, 13);
            this.pathLbl.TabIndex = 22;
            this.pathLbl.Text = Properties.Resources.PreferencesDlg_LoggingPathLabel;
            // 
            // sizeLbl
            // 
            this.sizeLbl.AutoSize = true;
            this.sizeLbl.Location = new System.Drawing.Point(34, 159);
            this.sizeLbl.Name = "sizeLbl";
            this.sizeLbl.Size = new System.Drawing.Size(51, 13);
            this.sizeLbl.TabIndex = 22;
            this.sizeLbl.Text = Properties.Resources.PreferencesDlg_LoggingSizeLabel;
            // 
            // sizeTB
            // 
            this.sizeTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeTB.Location = new System.Drawing.Point(95, 152);
            this.sizeTB.Name = "sizeTB";
            this.sizeTB.Size = new System.Drawing.Size(234, 20);
            this.sizeTB.TabIndex = 5;
            this.sizeTB.Text = Properties.Resources.PreferencesDlg_LoggingSizeTB;
            this.sizeTB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.sizeTB_KeyPress);
            // 
            // nameTB
            // 
            this.nameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameTB.Location = new System.Drawing.Point(95, 125);
            this.nameTB.Name = "nameTB";
            this.nameTB.Size = new System.Drawing.Size(234, 20);
            this.nameTB.TabIndex = 3;
            this.nameTB.Text = Properties.Resources.PreferencesDlg_Logging_PathTB;
            // 
            // selectBtn
            // 
            this.selectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectBtn.Location = new System.Drawing.Point(335, 123);
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.Size = new System.Drawing.Size(75, 23);
            this.selectBtn.TabIndex = 4;
            this.selectBtn.Text = Properties.Resources.PreferencesDlg_SelectBtn;
            this.selectBtn.UseVisualStyleBackColor = true;
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            // 
            // diagLoggingCB
            // 
            this.diagLoggingCB.AutoSize = true;
            this.diagLoggingCB.Location = new System.Drawing.Point(12, 186);
            this.diagLoggingCB.Name = "diagLoggingCB";
            this.diagLoggingCB.Size = new System.Drawing.Size(147, 17);
            this.diagLoggingCB.TabIndex = 6;
            this.diagLoggingCB.Text = Properties.Resources.PreferencesDlg_DiagLoggingCB;
            this.diagLoggingCB.UseVisualStyleBackColor = true;
            this.diagLoggingCB.CheckedChanged += new System.EventHandler(this.diagLoggingCB_CheckedChanged);
            // 
            // deletePromptChkB
            // 
            this.deletePromptChkB.AutoSize = true;
            this.deletePromptChkB.Checked = true;
            this.deletePromptChkB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deletePromptChkB.Location = new System.Drawing.Point(12, 41);
            this.deletePromptChkB.Name = "deletePromptChkB";
            this.deletePromptChkB.Size = new System.Drawing.Size(247, 17);
            this.deletePromptChkB.TabIndex = 23;
            this.deletePromptChkB.Text = Properties.Resources.PreferencesDlg_DeletePromptCB;
            this.deletePromptChkB.UseVisualStyleBackColor = true;
            this.deletePromptChkB.CheckedChanged += new System.EventHandler(this.deletePromptChkB_CheckedChanged);
            // 
            // Preferences
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(460, 281);
            this.Controls.Add(this.deletePromptChkB);
            this.Controls.Add(this.diagLoggingCB);
            this.Controls.Add(this.selectBtn);
            this.Controls.Add(this.nameTB);
            this.Controls.Add(this.sizeTB);
            this.Controls.Add(this.sizeLbl);
            this.Controls.Add(this.pathLbl);
            this.Controls.Add(this.logToFileCB);
            this.Controls.Add(this.P4ExceptionCB);
            this.Controls.Add(this.ExceptionCB);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.messagesCB);
            this.Controls.Add(this.iconsCB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = Properties.Resources.PreferencesDlg_PreferencesTitle;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox iconsCB;
        private System.Windows.Forms.CheckBox messagesCB;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.CheckBox ExceptionCB;
        private System.Windows.Forms.CheckBox P4ExceptionCB;
        private System.Windows.Forms.CheckBox logToFileCB;
        private System.Windows.Forms.Label pathLbl;
        private System.Windows.Forms.Label sizeLbl;
        private System.Windows.Forms.TextBox sizeTB;
        private System.Windows.Forms.TextBox nameTB;
        private System.Windows.Forms.Button selectBtn;
        private System.Windows.Forms.CheckBox diagLoggingCB;
        private System.Windows.Forms.CheckBox deletePromptChkB;
    }
}