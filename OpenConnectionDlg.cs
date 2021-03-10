using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Perforce.P4;
using System.Net;
using System.Reflection;

namespace P4EXP
{
    public partial class OpenConnectionDlg : AutoSizeForm
    {
        //MRUList _recentConnections = null;

        public string ServerPort
        {
            get { return ServerTB.Text.Trim(); }
            set { ServerTB.Text = value; }
        }
        public string UserName
        {
            get { return UserTB.Text.Trim(); ; }
            set { UserTB.Text = value; }
        }
        public string Workspace
        {
            get { return WorkspaceTB.Text.Trim(); ; }
            set { WorkspaceTB.Text = value; }
        }
        public string Password { get; private set; }

        //        ThemeManager ThemeMgr = null;

        public OpenConnectionDlg()
        {
            PreferenceKey = "OpenConnectionDlg";

            InitializeComponent();
            //if (!DesignMode)
            //{
            //    ThemeMgr = new ThemeManager(Controls);
            //}
            //else
            //{
            //    ThemeMgr = null;
            //}

            // REPLACE
            //this.Icon = Images.p4exp;
            // Display the file version number.
            VersionLbl.Text = P4EXPProgram.ProductVersion;

            //VersionLbl.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            RecentConnectionsCB.Items.Clear();
            RecentConnectionsCB.Visible = false;
            RecentConnectionsCB.Enabled = false;
            //	if (Preferences.LocalSettings != null)
            //_recentConnections = (MRUList)Preferences.LocalSettings["RecentConnections"];
            //if (_recentConnections != null)
            //{
            //    foreach (ConnectionData con in _recentConnections)
            //    {
            //        if (con != null)
            //        {
            //            RecentConnectionsCB.Items.Add(con.ToString());
            //        }
            //    }
            //    if (RecentConnectionsCB.Items.Count > 0)
            //    {
            //        RecentConnectionsCB.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        RecentConnectionsCB.SelectedIndex = -1;
            //    }
            //    if (_recentConnections[0] != null)
            //    {
            //        ConnectionData cd = _recentConnections[0] as ConnectionData;
            //        if (cd != null)
            //        {
            //            ServerTB.Text = cd.ServerPort;
            //            UserTB.Text = cd.UserName;
            //            WorkspaceTB.Text = cd.Workspace;
            //        }
            //    }
            //}

            string port, user, client = "";
            P4Server.ConnectionInfoFromPath("", out port, out user, out client);
            ServerTB.Text = port.Trim();
            UserTB.Text = user.Trim();
            WorkspaceTB.Text = client.Trim();

            BrowseWorkspaceBtn.Enabled = ServerTB.Text.Length > 0;
            NewWorkspaceBtn.Enabled = ServerTB.Text.Length > 0;
            BrowseUserBtn.Enabled = ServerTB.Text.Length > 0;
            NewUserBtn.Enabled = ServerTB.Text.Length > 0;
            OkBtn.Enabled = (ServerTB.Text.Length > 0) && (UserTB.Text.Length > 0) && (WorkspaceTB.Text.Length > 0);
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    ThemeMgr.Dispose();
        //    base.OnClosing(e);
        //}

        private void RecentConnectionsCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RecentConnectionsCB.SelectedIndex >= 0)
            {
                //ConnectionData cd = _recentConnections[RecentConnectionsCB.SelectedIndex] as ConnectionData;
                //if (cd != null)
                //{
                //	ServerTB.Text = cd.ServerPort;
                //	UserTB.Text = cd.UserName;
                //	WorkspaceTB.Text = cd.Workspace;
                //}
            }
            else
            {
                //ConnectionData cd = _recentConnections[RecentConnectionsCB.SelectedIndex] as ConnectionData;
                ServerTB.Text = string.Empty;
                UserTB.Text = string.Empty;
                WorkspaceTB.Text = string.Empty;
            }
        }


        private void BrowseUserBtn_Click(object sender, EventArgs e)
        {
            this.TopMost = false;

            RepoStorage repo = SetRepoFromDlg();

            if (repo != null && CheckConnection(repo))
            {
                UsersBrowserDlg dlg = new UsersBrowserDlg(repo, null);

                dlg.TopMost = true; ;

                if ((DialogResult.Cancel != dlg.ShowDialog()) && (dlg.SelectedUser.Id != null))
                {
                    UserTB.Text = dlg.SelectedUser.Id;
                }
            }
            //_scm.Dispose();

            this.TopMost = true; ;
        }

        private void NewUserBtn_Click(object sender, EventArgs e)
        {
            this.TopMost = false;

            RepoStorage repo = SetRepoFromDlg();

            if (repo !=null && CheckConnection(repo))
            {
                NewUserDlg dlg = new NewUserDlg(repo);
                User newUser = dlg.Show(repo);
                dlg.TopMost = true;
                if (null != newUser)
                {
                    //_scm.NewUser(newUser);
                    UserTB.Text = newUser.Id;
                }
                //repo.Dispose();
            }
            this.TopMost = true;
        }

        private void BrowseWorkspaceBtn_Click(object sender, EventArgs e)
        {
            this.TopMost = false;

            RepoStorage repo = SetRepoFromDlg();

            if (repo != null && CheckConnection(repo) && CheckLogin(repo))
            {
                WorkspaceBrowserDlg dlg = new WorkspaceBrowserDlg(repo, null);
                dlg.TopMost = true;

                if (!dlg.dlgCancelled)
                {
                    if (DialogResult.Cancel != dlg.ShowDialog())
                    {
                        if ((dlg.SelectedWorkspace != null) && (dlg.SelectedWorkspace.Name != null))
                            WorkspaceTB.Text = dlg.SelectedWorkspace.Name.ToString();
                    }
                }
                //_scm.Dispose();
            }
            this.TopMost = true;
        }

        private void NewWorkspaceBtn_Click(object sender, EventArgs e)
        {
            this.TopMost = false;

            RepoStorage repo = SetRepoFromDlg();

            if (repo!=null && CheckConnection(repo) && CheckLogin(repo))
            {
                Client workspace = null;
                Client clientInfo = new Client();
                string newName = GetStringDlg.Show(Properties.Resources.OpenConnectionDlg_NewWorkspaceDlgTitle,
                    Properties.Resources.OpenConnectionDlg_NewWorkspaceDlgPrompt, null);
                if ((newName != null) && (newName != string.Empty))
                {
                    if (newName.Contains(" "))
                    {
                        MessageBox.Show(Properties.Resources.OpenConnectionDlg_NameContainsSpacesWarning, Properties.Resources.P4EXP,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    ClientsCmdOptions opts = new ClientsCmdOptions(ClientsCmdFlags.None, null, newName, 1, null);
                    //IList<Client> checkExisting = _scm.getClients(ClientsCmdFlags.None, null, newName, 1, null);
                    IList<Client> checkExisting = repo.rep.GetClients(opts);

                    if (checkExisting == null)
                    {
                        clientInfo = repo.rep.GetClient(newName);
                        if (clientInfo != null)
                        {
                            // adjust root here based on users dir
                            string root = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            int idx = root.LastIndexOf(@"\");
                            root = root.Remove(idx + 1);
                            root += newName;
                            clientInfo.Root = root;
                            workspace = DlgEditWorkspace.EditWorkspace(repo, clientInfo);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format(Properties.Resources.OpenConnectionDlg_WorkspaceExistsWarning, newName),
                            Properties.Resources.P4EXP, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        NewWorkspaceBtn_Click(null, null);
                    }

                }
                else
                {
                    if (newName == string.Empty)
                    {
                        MessageBox.Show(Properties.Resources.OpenConnectionDlg_EmptyWorkspaceNameWarning,
                            Properties.Resources.P4EXP, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        NewWorkspaceBtn_Click(null, null);
                    }
                }
                if (workspace != null)
                {
                    WorkspaceTB.Text = workspace.Name;
                }
            }
            this.TopMost = true;
        }

        private bool CheckConnection(RepoStorage repo)
        {
            if (repo.rep.Connection.Status==ConnectionStatus.Connected)
            { return true; }
            else
            {
                string message = Properties.Resources.OpenConnectionDlg_CheckConnectionError;
                Message dlg = new Message(Properties.Resources.MessageDlg_UnableToCompleteAction, message);
                dlg.ShowDialog();
                return false;
            }
        }

        private bool CheckLogin(RepoStorage repo)
        {
            if (repo.rep.Connection.LastResults.ErrorList!=null)
            {
                foreach (P4ClientError err in repo.rep.Connection.LastResults.ErrorList)
                {
                    if (err.ErrorCode==807672853)
                    {
                        P4EXPProgram.Login(repo,true);
                        if (repo.loggedIn)
                        { return true; }
                        string message = Properties.Resources.OpenConnectionDlg_CheckLoginError;
                        Message dlg = new Message(Properties.Resources.MessageDlg_UnableToCompleteAction, message);
                        dlg.ShowDialog();
                        return false;
                    }
                }
            }
            return true;
        }
        private RepoStorage SetRepoFromDlg()
        {
            RepoStorage repo = P4EXPProgram.getRepoStorage("");
            repo.rep = new Repository(new Server(new ServerAddress(ServerTB.Text.Trim())));
            repo.rep.Connection.UserName = UserTB.Text.Trim();
            try
            {
                repo.rep.Connection.Connect(null);
            }
            catch (Exception)
            {
                string message = Properties.Resources.OpenConnectionDlg_SetRepoFromDlgError;
                Message dlg = new Message(Properties.Resources.MessageDlg_UnableToCompleteAction, message);
                dlg.ShowDialog();
                return null;
            }
            return repo;
        }
        //public ConnectionData ConnectionInfo { get; private set; }

        private void OkBtn_Click(object sender, EventArgs e)
		{
            RepoStorage repo = P4EXPProgram.getRepoStorage("");
            repo.rep = new Repository(new Server(new ServerAddress(ServerTB.Text.Trim())));
            repo.rep.Connection.UserName = UserTB.Text.Trim();
            try
            {
                repo.rep.Connection.Connect(null);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                Message dlg = new Message(Properties.Resources.MessageDlg_ConnectionError, message);
                dlg.ShowDialog();
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!P4EXPProgram.IsHAS(""))
            { 
                if (repo.rep.Connection.LastResults != null &&
                    repo.rep.Connection.LastResults.ErrorList != null)
                {
                    string message = repo.rep.Connection.LastResults.ErrorList[0].ErrorMessage;
                    Message dlg = new Message(Properties.Resources.MessageDlg_ConnectionError, message);
                    dlg.ShowDialog();
                    this.DialogResult = DialogResult.None;
                    repo.rep.Connection.Disconnect();
                    return;
                }
            }
            repo.rep.Connection.Disconnect();
            this.DialogResult = DialogResult.OK;

            //ConnectionData cd = new ConnectionData();
            //cd.ServerPort = ServerTB.Text;
            //cd.UserName = UserTB.Text;
            //cd.Workspace = WorkspaceTB.Text;

            //ConnectionInfo = cd;

            // moved this to P4ScmProvider so we can decide whether to save the most
            // recent connection after we know it was successful.

            //if (_recentConnections == null)
            //{
            //    _recentConnections = new MRUList(5);
            //}
            //_recentConnections.Add(cd);

            //Preferences.LocalSettings["RecentConnections"] = _recentConnections;
        }

		private void HelpBtn_Click(object sender, EventArgs e)
		{
			this.TopMost = false;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion; // 2011.1.0.0 is the format
            version = version.Remove(0, 2);
            string[] versionSplit = version.Split('.');
            string relDir = "r" + versionSplit[0] + "." + versionSplit[1];
            try
            {
                bool pageExists = false;
                string helpPath = Properties.Resources.HelpLink_Connection;
                string versionPath = helpPath.Replace("doc.current", relDir);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(versionPath);
                request.Method = WebRequestMethods.Http.Head;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    pageExists = response.StatusCode == HttpStatusCode.OK;
                }
                catch
                {
                    // likely got a 404 here, page does not exist
                    // this will happen if the html help has not been
                    // pushed to web for the assembly's version. For
                    // example, builds from main.
                }
                if (pageExists)
                {
                    Help.ShowHelp(null, versionPath, null);
                }
                else
                {
                    Help.ShowHelp(null, helpPath, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.TopMost = true;
        }

        private void ServerTB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
				OkBtn.PerformClick();
		}

		private void UserTB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
				OkBtn.PerformClick();
		}

		private void WorkspaceTB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
				OkBtn.PerformClick();
		}

		private void EnableButtons()
		{
			BrowseWorkspaceBtn.Enabled = ServerTB.Text.Length > 0;
			NewWorkspaceBtn.Enabled = ServerTB.Text.Length > 0;
			BrowseUserBtn.Enabled = ServerTB.Text.Length > 0;
			NewUserBtn.Enabled = ServerTB.Text.Length > 0;

			OkBtn.Enabled = (ServerTB.Text.Length > 0) && (UserTB.Text.Length > 0) && (WorkspaceTB.Text.Length > 0);
		}
		private void ServerTB_TextChanged(object sender, EventArgs e)
		{
			EnableButtons();
		}

		private void UserTB_TextChanged(object sender, EventArgs e)
		{
			EnableButtons();
		}

		private void WorkspaceTB_TextChanged(object sender, EventArgs e)
		{
			EnableButtons();
		}

        private void RecentConnectionsCB_DropDown(object sender, EventArgs e)
        {
            int widest = RecentConnectionsCB.DropDownWidth;
            if (RecentConnectionsCB.Items.Count>0)
            {
                foreach(object item in RecentConnectionsCB.Items)
                {
                    Image fakeImage = new Bitmap(1, 1);
                    Graphics graphics = Graphics.FromImage(fakeImage);
                    SizeF measure = graphics.MeasureString(item.ToString(), Font);
                    if (measure.Width > widest)
                    {
                        widest = Convert.ToInt32(measure.Width);
                    }
                }
                RecentConnectionsCB.DropDownWidth = widest;
            }
        }

        private void OpenConnectionDlg_Load(object sender, EventArgs e)
        {
            if (StartPosition == FormStartPosition.CenterParent)
            {
                // if opening in the default location, move it uup the screen so it can't
                // end up behind the VS initializing progress box, which is also always on top.
                StartPosition = FormStartPosition.Manual;
                Top -= Top / 2;
            }
        }
    }
}
