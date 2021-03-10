namespace P4EXP
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.msgLbl = new System.Windows.Forms.Label();
            this.promptLbl = new System.Windows.Forms.Label();
            this.passwordTB = new System.Windows.Forms.TextBox();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // msgLbl
            // 
            this.msgLbl.AutoSize = true;
            this.msgLbl.Location = new System.Drawing.Point(12, 9);
            this.msgLbl.Name = "msgLbl";
            this.msgLbl.Size = new System.Drawing.Size(243, 13);
            this.msgLbl.TabIndex = 0;
            this.msgLbl.Text = Properties.Resources.LoginDlg_MessageLabel;
            // 
            // promptLbl
            // 
            this.promptLbl.AutoSize = true;
            this.promptLbl.Location = new System.Drawing.Point(12, 42);
            this.promptLbl.Name = "promptLbl";
            this.promptLbl.Size = new System.Drawing.Size(135, 13);
            this.promptLbl.TabIndex = 1;
            this.promptLbl.Text = Properties.Resources.LoginDlg_PromptLabel;
            // 
            // passwordTB
            // 
            this.passwordTB.Location = new System.Drawing.Point(15, 70);
            this.passwordTB.Name = "passwordTB";
            this.passwordTB.PasswordChar = '*';
            this.passwordTB.Size = new System.Drawing.Size(274, 20);
            this.passwordTB.TabIndex = 2;
            this.passwordTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.passwordTB_KeyDown);
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(130, 110);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = Properties.Resources.LoginDlg_OKBtn;
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(211, 110);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = Properties.Resources.LoginDlg_CancelBtn;
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(303, 148);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.passwordTB);
            this.Controls.Add(this.promptLbl);
            this.Controls.Add(this.msgLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.Text = Properties.Resources.LoginDlg_LoginTitle;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label msgLbl;
        private System.Windows.Forms.Label promptLbl;
        private System.Windows.Forms.TextBox passwordTB;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}