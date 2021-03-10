namespace P4EXP
{
    partial class RevertDeleteWarning
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevertDeleteWarning));
            this.YesBtn = new System.Windows.Forms.Button();
            this.NoBtn = new System.Windows.Forms.Button();
            this.warningLbl1 = new System.Windows.Forms.Label();
            this.deleteChkB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // YesBtn
            // 
            this.YesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.YesBtn.Location = new System.Drawing.Point(85, 137);
            this.YesBtn.Name = "YesBtn";
            this.YesBtn.Size = new System.Drawing.Size(75, 23);
            this.YesBtn.TabIndex = 0;
            this.YesBtn.Text = Properties.Resources.RevertDeleteWarningDlg_YesBtn;
            this.YesBtn.UseVisualStyleBackColor = true;
            this.YesBtn.Click += new System.EventHandler(this.YesBtn_Click);
            // 
            // NoBtn
            // 
            this.NoBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NoBtn.Location = new System.Drawing.Point(192, 137);
            this.NoBtn.Name = "NoBtn";
            this.NoBtn.Size = new System.Drawing.Size(75, 23);
            this.NoBtn.TabIndex = 1;
            this.NoBtn.Text = Properties.Resources.RevertDeleteWarningDlg_NoBtn;
            this.NoBtn.UseVisualStyleBackColor = true;
            this.NoBtn.Click += new System.EventHandler(this.NoBtn_Click);
            // 
            // warningLbl1
            // 
            this.warningLbl1.AutoSize = true;
            this.warningLbl1.Location = new System.Drawing.Point(12, 28);
            this.warningLbl1.Name = "warningLbl1";
            this.warningLbl1.Size = new System.Drawing.Size(242, 26);
            this.warningLbl1.TabIndex = 2;
            this.warningLbl1.Text = Properties.Resources.RevertDeleteWarningDlg_RevertWarningLabel;
            // 
            // deleteChkB
            // 
            this.deleteChkB.AutoSize = true;
            this.deleteChkB.Enabled = false;
            this.deleteChkB.Location = new System.Drawing.Point(15, 105);
            this.deleteChkB.Name = "deleteChkB";
            this.deleteChkB.Size = new System.Drawing.Size(184, 17);
            this.deleteChkB.TabIndex = 3;
            this.deleteChkB.Text = Properties.Resources.RevertDeleteWarningDlg_DeleteCB;
            this.deleteChkB.UseVisualStyleBackColor = true;
            this.deleteChkB.Visible = false;
            this.deleteChkB.CheckedChanged += new System.EventHandler(this.deleteChkB_CheckedChanged);
            // 
            // RevertDeleteWarning
            // 
            this.AcceptButton = this.YesBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(284, 174);
            this.Controls.Add(this.deleteChkB);
            this.Controls.Add(this.warningLbl1);
            this.Controls.Add(this.NoBtn);
            this.Controls.Add(this.YesBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RevertDeleteWarning";
            this.Text = Properties.Resources.RevertDeleteWarningDlg_RevertDeleteWarningTitle;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button YesBtn;
        private System.Windows.Forms.Button NoBtn;
        private System.Windows.Forms.Label warningLbl1;
        private System.Windows.Forms.CheckBox deleteChkB;
    }
}