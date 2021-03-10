using Perforce.P4;
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
	public partial class WorkspaceBrowserDlg : AutoSizeForm
	{
		private System.Windows.Forms.ImageList imageList1;

		public WorkspaceBrowserDlg(RepoStorage repo, string sender)
		{
			PreferenceKey = "WorkspaceBrowserDlg";

            Repo = repo;
			InitializeComponent();

            if (components == null)
            {
                components = new Container();
            }
            //this.Icon = Images.icon_p4vs_16px;

			imageList1 = new System.Windows.Forms.ImageList(components);

			// 
			// imageList1
			// 
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.Add("workspace_icon.png", Images.workspace_icon);

			this.listView1.LargeImageList = this.imageList1;
			this.listView1.SmallImageList = this.imageList1;

            if (Repo != null)
			{
                ClientsCmdOptions opts = new ClientsCmdOptions(ClientsCmdFlags.None, null,
                    null, 0, null);
                try
                {
                    IList<Client> workspaces = Repo.rep.GetClients(opts);
                    foreach (Client workspace in workspaces)
                    {
                        string id = workspace.Name;

                        DateTime localAccess = workspace.Accessed;

                        // we need a pref for local time, until then, don't do this:
                        //DateTime localAccess = TimeZone.CurrentTimeZone.ToLocalTime(user.Accessed);
                        string access = "";
                        //if (Preferences.LocalSettings.GetBool("P4Date_format", true))
                        //{
                        //    access = localAccess.ToString("yyyy/MM/dd HH:mm:ss");
                        //}
                        //else
                        //{
                        access = string.Format("{0} {1}", localAccess.ToShortDateString(),
                                                            localAccess.ToShortTimeString());
                        //}

                        string lastAccessed = access;
                        string email = workspace.OwnerName;
                        string name = workspace.Description;

                        string[] theUser = new string[] { id, email, lastAccessed, name };
                        ListViewItem lvi = new ListViewItem(theUser);
                        lvi.Tag = workspace;
                        lvi.ImageIndex = 0;
                        listView1.Items.Add(lvi);
                    }
                }
                catch(Exception ex)
                {
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, ex.Message);
                    dlgCancelled = true;
                    dlg.ShowDialog();
                }
				

			}
			ClosedByDoubleClick = false;
		}

        public bool dlgCancelled = false;
		private WorkspaceBrowserDlg()
		{
			PreferenceKey = "WorkspaceBrowserDlg";

			InitializeComponent();
            //this.Icon = Images.icon_p4vs_16px;
			imageList1 = new System.Windows.Forms.ImageList(components);

			// 
			// imageList1
			// 
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.Add("workspace_icon.png", Images.workspace_icon);

			this.listView1.LargeImageList = this.imageList1;
			this.listView1.SmallImageList = this.imageList1;
        }

        public Client SelectedWorkspace
		{
			get
			{
				if (this.listView1.SelectedItems.Count > 0)
		{
		 return (Client)this.listView1.SelectedItems[0].Tag;
		}
				return null;
			}
			//return this.listView1.SelectedItems[0]; }
			//private set; 
		}

		public RepoStorage Repo { get; private set; }

		public Client Show(RepoStorage repo)
		{
			if (this.DialogResult == DialogResult.OK)
			{
				return SelectedWorkspace;
			}
			return null;
		}

		private void OKBtn_Click(object sender, EventArgs e)
		{
			if (SelectedWorkspace == null)
			{
				this.DialogResult=DialogResult.Cancel;
			}
		}

		public bool ClosedByDoubleClick { get; private set; }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedWorkspace != null)
            {
				ClosedByDoubleClick = true;
                this.DialogResult = DialogResult.OK;
            }
        }
	}
}
