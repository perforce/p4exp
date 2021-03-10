using System.Windows.Forms;

namespace P4EXP
{
    partial class LaunchingHMFA
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LaunchingHMFA));
            this.closeBtn = new System.Windows.Forms.Button();
            this.launchingGridLbl = new System.Windows.Forms.Label();
            this.expiredGridLbl = new System.Windows.Forms.Label();
            this.downloadBtn = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.Location = new System.Drawing.Point(377, 226);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 6;
            this.closeBtn.Text = Properties.Resources.LaunchingHMFA_CloseBtn;
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // launchingGridLbl
            // 
            this.launchingGridLbl.AutoSize = true;
            this.launchingGridLbl.Location = new System.Drawing.Point(143, 188);
            this.launchingGridLbl.Name = "launchingGridLbl";
            this.launchingGridLbl.Size = new System.Drawing.Size(183, 13);
            this.launchingGridLbl.TabIndex = 5;
            this.launchingGridLbl.Text = Properties.Resources.LaunchingHMFA_LaunchingLabel;
            this.launchingGridLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // expiredGridLbl
            // 
            this.expiredGridLbl.AutoSize = true;
            this.expiredGridLbl.Location = new System.Drawing.Point(13, 13);
            this.expiredGridLbl.MaximumSize = new System.Drawing.Size(440, 0);
            this.expiredGridLbl.Name = "expiredGridLbl";
            this.expiredGridLbl.Size = new System.Drawing.Size(424, 26);
            this.expiredGridLbl.TabIndex = 4;
            this.expiredGridLbl.Text = Properties.Resources.LaunchingHMFA_ExpiredLabel;
            // 
            // downloadBtn
            // 
            this.downloadBtn.Location = new System.Drawing.Point(298, 226);
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(75, 23);
            this.downloadBtn.TabIndex = 7;
            this.downloadBtn.Text = Properties.Resources.LaunchingHMFA_DownloadBtn;
            this.downloadBtn.UseVisualStyleBackColor = true;
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Image = global::P4EXP.Properties.Resources.download_icon;
            this.pictureBox.Location = new System.Drawing.Point(196, 79);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(78, 78);
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            // 
            // LaunchingHMFA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeBtn;
            this.ClientSize = new System.Drawing.Size(464, 261);
            this.Controls.Add(this.downloadBtn);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.launchingGridLbl);
            this.Controls.Add(this.expiredGridLbl);
            this.Controls.Add(this.pictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LaunchingHMFA";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = Properties.Resources.LaunchingHMFA_VerifyTitle;
            this.Shown += new System.EventHandler(this.LaunchingHMFA_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private Label expiredGridLbl;
        private Label launchingGridLbl;
        private Button closeBtn;
        private Button downloadBtn;
    }
}