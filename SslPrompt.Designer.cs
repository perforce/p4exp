namespace P4EXP
{
    partial class SslPrompt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SslPrompt));
            this.WarningLbl = new System.Windows.Forms.Label();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.WarningTB = new System.Windows.Forms.TextBox();
            this.TrustCB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // WarningLbl
            // 
            this.WarningLbl.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.WarningLbl.AutoSize = true;
            this.WarningLbl.ForeColor = System.Drawing.Color.Red;
            this.WarningLbl.Location = new System.Drawing.Point(30, 9);
            this.WarningLbl.Name = "WarningLbl";
            this.WarningLbl.Size = new System.Drawing.Size(340, 13);
            this.WarningLbl.TabIndex = 0;
            this.WarningLbl.Text = Properties.Resources.SslPromptDlg_WarningLabel;
            this.WarningLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ConnectBtn.Location = new System.Drawing.Point(317, 149);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(75, 23);
            this.ConnectBtn.TabIndex = 3;
            this.ConnectBtn.Text = Properties.Resources.SslPromptDlg_ConnectBtn;
            this.ConnectBtn.UseVisualStyleBackColor = true;
            this.ConnectBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(234, 149);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = Properties.Resources.SslPromptDlg_CancelBtn;
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // WarningTB
            // 
            this.WarningTB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WarningTB.BackColor = System.Drawing.SystemColors.Control;
            this.WarningTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WarningTB.Location = new System.Drawing.Point(41, 25);
            this.WarningTB.Multiline = true;
            this.WarningTB.Name = "WarningTB";
            this.WarningTB.ReadOnly = true;
            this.WarningTB.Size = new System.Drawing.Size(348, 97);
            this.WarningTB.TabIndex = 0;
            this.WarningTB.TabStop = false;
            this.WarningTB.Text = Properties.Resources.SslPromptDlg_WarningText;
            // 
            // TrustCB
            // 
            this.TrustCB.AutoSize = true;
            this.TrustCB.Location = new System.Drawing.Point(45, 126);
            this.TrustCB.Name = "TrustCB";
            this.TrustCB.Size = new System.Drawing.Size(230, 17);
            this.TrustCB.TabIndex = 5;
            this.TrustCB.Text = Properties.Resources.SslPromptDlg_TrustCB;
            this.TrustCB.UseVisualStyleBackColor = true;
            // 
            // SslPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(401, 185);
            this.Controls.Add(this.TrustCB);
            this.Controls.Add(this.WarningTB);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.ConnectBtn);
            this.Controls.Add(this.WarningLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SslPrompt";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = Properties.Resources.SslPromptDlg_SslTitle;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label WarningLbl;
        private System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.TextBox WarningTB;
        private System.Windows.Forms.CheckBox TrustCB;
    }
}