using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Perforce.P4;

namespace P4EXP
{
	public partial class NewUserDlg : Form
	{
		bool SetPasswordOnly { get; set; }
		public NewUserDlg(RepoStorage repo)
		{
            Repo = repo;
			InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
		}

		//public P4.User CreatedUser(P4ScmProvider scm)
		//{
			
		//        P4.User newUser = new P4.User();
		//        newUser.Id = userNameTB.Text;
		//        newUser.FullName = fullNameTB.Text;
		//        newUser.Password = password1TB.Text;
		//        newUser.EmailAddress = emailTB.Text;
		//        return newUser;
			
		//}

		public RepoStorage Repo { get; private set; }

		public User Show(RepoStorage repo)
		{
			SetPasswordOnly = false;
            Repo = repo;
            string oldPasswd = null;

			User newUser = new User();

            do
            {
                if (this.ShowDialog() == DialogResult.OK)
                {

					if (!SetPasswordOnly)
					{
						string name = userNameTB.Text;
						if (name.Contains(" "))
						{
							name = Regex.Replace(name, " ", "_");
						}
						Options opts = new Options();
						IList<string> users = new List<string>();
						users.Add(userNameTB.Text);
						if (Repo.rep.GetUsers(users, opts) != null)
						{
							string msg = string.Format(Properties.Resources.NewUserDlg_UserExistsWarning, userNameTB.Text);
							MessageBox.Show(msg, Properties.Resources.P4EXP, MessageBoxButtons.OK, MessageBoxIcon.Error);
							continue;
						}

                        // Set connection options
                        Options options = new Options();
                        options["ProgramName"] = Properties.Resources.P4EXP;
                        options["ProgramVersion"] = P4EXPProgram.ProductVersion;

                        newUser.Id = name;
						newUser.FullName = fullNameTB.Text;
						newUser.EmailAddress = emailTB.Text;
                        Repo.rep.Connection.UserName = newUser.Id;
                        Repo.rep.Connection.Connect(options);

                        //scm.Connection.User = newUser.Id;//.Repository.Connection.UserName = newUser.Id;
                        //scm.Connection.Connect(null);//.Repository.Connection.Connect(null);
                    }
                    if (!string.IsNullOrEmpty(fullNameTB.Text))
					{
						newUser.Password = password1TB.Text;
					}
					try
					{
						if (SetPasswordOnly)
						{
							SetPasswordOnly = false;
                            Repo.rep.Connection.SetPassword(null, password1TB.Text);
						}
						else
						{
							SetPasswordOnly = false;
							newUser = Repo.rep.CreateUser(newUser);
						}
						return newUser;
					}
					catch (P4Exception p4ex)
					{
						// if from Connection.SetPassword(), error has not been shown
						if (P4ClientError.IsBadPasswdError(p4ex.ErrorCode))
						{
							SetPasswordOnly = true;
						}
						if ((p4ex.ErrorCode == P4ClientError.MsgServer_PasswordTooShort) ||
							(p4ex.ErrorCode == P4ClientError.MsgServer_PasswordTooSimple))
						{
							MessageBox.Show(Properties.Resources.NewUserDlg_PasswordTooShortOrSimple, Properties.Resources.P4EXP,
								MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						else
						{
                            MessageBox.Show(p4ex.Message);
							//scm.ShowException(p4ex);
						}
					}

                    P4CommandResult results = Repo.rep.Connection.LastResults;
					oldPasswd = password1TB.Text;

                }
                else
                {
                    return null;
                }
            } while (true);
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (userNameTB.Text.Contains(" "))
			{
				MessageBox.Show(Properties.Resources.NewUserDlg_NameContainsSpacesWarning, Properties.Resources.P4EXP, 
					MessageBoxButtons.OK,MessageBoxIcon.Information);
			}

			if (!(string.IsNullOrEmpty(password1TB.Text))||!(string.IsNullOrEmpty(password2TB.Text)))
			{
				if (password1TB.Text != password2TB.Text)
				{
					MessageBox.Show(Properties.Resources.NewUserDlg_PasswordsDontMatchWarning, Properties.Resources.P4EXP, 
						MessageBoxButtons.OK,MessageBoxIcon.Information);
					return;
				}
			}
			

			Options opts = new Options();
			IList<string> users = new List<string>();
			users.Add(userNameTB.Text);

			if ((SetPasswordOnly == true) || (Repo.rep.GetUsers(users,opts) == null))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				string msg = string.Format(Properties.Resources.NewUserDlg_UserExistsWarning, userNameTB.Text);
				MessageBox.Show(msg, Properties.Resources.P4EXP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void userNameTB_TextChanged(object sender, EventArgs e)
		{
			saveBtn.Enabled = ((userNameTB.Text.Length > 0) && (fullNameTB.Text.Length > 0) && (emailTB.Text.Length > 0));
		}

		private void fullNameTB_TextChanged(object sender, EventArgs e)
		{
			saveBtn.Enabled = ((userNameTB.Text.Length > 0) && (fullNameTB.Text.Length > 0) && (emailTB.Text.Length > 0));
		}

		private void emailTB_TextChanged(object sender, EventArgs e)
		{
			saveBtn.Enabled = ((userNameTB.Text.Length > 0) && (fullNameTB.Text.Length > 0) && (emailTB.Text.Length > 0));
		}

		private void NewUserDlg_Load(object sender, EventArgs e)
		{
			userNameLbl.Enabled = !SetPasswordOnly;
			userNameTB.Enabled = !SetPasswordOnly;
			fullNameTB.Enabled = !SetPasswordOnly;
			password1TB.Enabled = true;
			password2TB.Enabled = true;
			emailTB.Enabled = !SetPasswordOnly;
		}
	}
}
