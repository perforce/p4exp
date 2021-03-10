using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Windows.Forms;
using System.Linq;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using Perforce.P4;
using static P4EXP.P4EXPProgram;
using System.Collections.Generic;

namespace P4EXP
{
    /// <summary>
    /// The CountLinesExtensions is an example shell context menu extension,
    /// implemented with SharpShell. It adds the command 'Count Lines' to text
    /// files.
    /// </summary>
    [ComVisible(true)]

    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Directory)]
    public class P4EXPContextMenuExtension : SharpContextMenu
    {
        #region
        ToolStripSeparator mainSeperator1 = new ToolStripSeparator();
        ToolStripMenuItem mainPerforceMenuitem = new ToolStripMenuItem();
        ToolStripSeparator mainSeperator2 = new ToolStripSeparator();
        ToolStripMenuItem outsideRootMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem startP4VtMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem setConnectionMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem connectionInfoMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem preferencesMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem helpMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem getLatestMenuitem = new ToolStripMenuItem();
        ToolStripSeparator subSeperator0 = new ToolStripSeparator();
        ToolStripSeparator subSeperator1 = new ToolStripSeparator();
        ToolStripMenuItem submitMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem checkoutMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem addMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem deleteMenuitem = new ToolStripMenuItem();
        ToolStripSeparator subSeperator2 = new ToolStripSeparator();
        ToolStripMenuItem revertMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem revertUnchangedMenuitem= new ToolStripMenuItem();
        ToolStripSeparator subSeperator3 = new ToolStripSeparator();
        ToolStripMenuItem pendingChangesMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem diffAgainstHaveMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem diffAgainstMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem fileHistoryMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem timeLapseViewMenuitem = new ToolStripMenuItem();
        ToolStripMenuItem propertiesMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem showInP4VMenuitem= new ToolStripMenuItem();
        ToolStripSeparator subSeperator4 = new ToolStripSeparator();
        ToolStripMenuItem removeFromWorkspaceMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem refreshFileStateMenuitem= new ToolStripMenuItem();
        ToolStripSeparator subSeperator5 = new ToolStripSeparator();
        ToolStripMenuItem moreMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem logOffMenuitem= new ToolStripMenuItem();
        ToolStripMenuItem loginMenuitem = new ToolStripMenuItem();
        #endregion

        #region
        bool outsideRootMenuitemVisible = false;
        bool startP4VMenuitemVisible = false;
        bool getLatestMenuitemVisible = false;
        bool subSeperator1Visible = false;
        bool submitMenuitemVisible = false;
        bool checkoutMenuitemVisible = false;
        bool deleteMenuitemVisible = false;
        bool addMenuitemVisible = false;
        bool subSeperator2Visible = false;
        bool revertMenuitemVisible = false;
        bool revertUnchangedMenuitemVisible = false;
        bool subSeperator3Visible = false;
        bool pendingChangesMenuitemVisible = false;
        bool diffAgainstHaveMenuitemVisible = false;
        bool diffAgainstMenuitemVisible = false;
        bool fileHistoryMenuitemVisible = false;
        bool timeLapseViewMenuitemVisible = false;
        bool propertiesMenuitemVisible = false;
        bool showInP4VMenuitemVisible = false;
        bool subSeperator4Visible = false;
        bool removeFromWorkspaceMenuitemVisible = false;
        bool refreshFileStateMenuitemVisible = false;
        bool subSeperator5Visible = false;
        bool moreMenuitemVisible = false;
        bool loginVisible = false;
        #endregion

        IList<FileMetaData> commandFiles = new List<FileMetaData>();

        string outside = Properties.Resources.ContextMenus_OutsideRoot;
        string setConnection = Properties.Resources.ContextMenus_SetConnectionConnectedMenuItem;
        System.Drawing.Bitmap setConnectionImage = Properties.Resources.connected;

        IList<FileMetaData> buildSelectedList()
        {
            List<string> selectedItems = new List<string>();
            commandFiles = new List<FileMetaData>();

            foreach (string path in SelectedItemPaths)
            {
                string pathToAdd = path;
                FileAttributes attr = System.IO.File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    pathToAdd += @"\...";
                    //notindepot = true;
                    //folderselect = true;
                    FileMetaData folder = new FileMetaData();
                    folder.LocalPath = new LocalPath(pathToAdd);
                    commandFiles.Add(folder);
                    pathToAdd = pathToAdd.Replace(@"\...", @"\*");
                    selectedItems.Add(pathToAdd);
                }
                else
                {
                    selectedItems.Add(pathToAdd);
                    FileMetaData file = new FileMetaData();
                    file.LocalPath = new LocalPath(pathToAdd);
                    commandFiles.Add(file);
                }
            }

            return commandFiles;
        }
       /// <summary>
        /// Determines whether this instance can a shell context show menu, given the specified selected file list.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance should show a shell context menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            string[] TargetFiles= SelectedItemPaths.ToArray();

            // start a connection here.
            P4EXPProgram.Connect(TargetFiles[0]);

            RepoStorage store = null;

            try
            {
                store = P4EXPProgram.getRepoStorage(TargetFiles[0]);
            }
            catch (Exception e)
            {
                FileLogger.LogException(Properties.Resources.FileLogger_MenuExtension, e);
                return true;
            }

            lock (store)
            {
                try
                {
                    loginVisible = !store.connected || !store.loggedIn;

                    if (!store.connected)
                    {
                        // notify of disconnected in top menu item
                        setConnection = Properties.Resources.ContextMenus_SetConnectionNotConnectedMenuItem;
                        setConnectionImage = Properties.Resources.disconnected;
                        outsideRootMenuitemVisible = false;
                        startP4VMenuitemVisible = true;
                        return true;
                    }

                    if (store.rep.Connection.Client == null || 
                        (store.rep.Connection.Client.Root == null &&
                        store.roots.Count<1))
                    {
                        // notify of missing P4CLIENT in top menu item
                        setConnection = Properties.Resources.ContextMenus_SetConnectionConnectedMenuItem;
                        setConnectionImage = Properties.Resources.connected_with_issues;
                        outside = Properties.Resources.ContextMenus_NoWorkspaceSet;
                        outsideRootMenuitemVisible = true;
                        startP4VMenuitemVisible = true;
                        return true;
                    }

                    if (!store.IsUnderClientRoot(TargetFiles[0]))
                    {
                        // notify of outside root in top menu item
                        setConnection = Properties.Resources.ContextMenus_SetConnectionConnectedMenuItem;
                        setConnectionImage = Properties.Resources.connected_with_issues;
                        outside = Properties.Resources.ContextMenus_OutsideRoot;
                        outsideRootMenuitemVisible = true;
                        startP4VMenuitemVisible = true;
                        return true;
                    }



                    Repository rep = store.rep;
                    // checking here for connected and directory change

                    bool stale = false;
                    bool checkedout = false;
                    bool indepot = false;
                    bool notindepot = false;
                    bool haverev = false;
                    bool cancheckout = false;
                    bool folderselect = false;

                    //check if each target file is a directory and if so, convert 
                    // convert to =+/...

                    List<string> selectedItems = new List<string>();
                    commandFiles = new List<FileMetaData>();

                    foreach (string path in TargetFiles)
                    {
                        string pathToAdd = path;
                        FileAttributes attr = System.IO.File.GetAttributes(path);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            pathToAdd += @"\...";
                            notindepot = true;
                            folderselect = true;
                            FileMetaData folder = new FileMetaData();
                            folder.LocalPath = new LocalPath(pathToAdd);
                            commandFiles.Add(folder);
                            pathToAdd = pathToAdd.Replace(@"\...", @"\*");
                            selectedItems.Add(pathToAdd);
                        }
                        else
                        {
                            selectedItems.Add(pathToAdd);
                            FileMetaData file = new FileMetaData();
                            file.LocalPath = new LocalPath(pathToAdd);
                            commandFiles.Add(file);
                        }
                    }

                    FileSpec[] fs = new FileSpec[TargetFiles.Length];
                    fs = FileSpec.LocalSpecArray(selectedItems.ToArray());

                    // TODO might need to set an fstat -m limit here
                    if (store.loggedIn)
                    {
                        IList<FileMetaData> fmd = P4EXPProgram.GetFileMetaData(fs);

                        // if no P4Exception is caught by attempting to get
                        // FileMetaData, remove the initial menu items and
                        // add the rest of the menu items and seperators
                        if (fmd != null)
                        {
                            moreMenuitemVisible = true;

                            foreach (FileMetaData d in fmd)
                            {
                                if (stale && checkedout && indepot && notindepot && haverev && cancheckout)
                                {
                                    break;
                                }
                                if (d == null || (d.HaveRev==-1 && d.Action!=FileAction.Add))
                                {
                                    notindepot = true;
                                    break;
                                }
                                if (d.HaveRev < d.HeadRev)
                                {
                                    stale = true;
                                }
                                if (d.Action != FileAction.None)
                                {
                                    checkedout = true;
                                }
                                if (d.IsInDepot)
                                {
                                    indepot = true;
                                }
                                if (d.IsInClient)
                                {
                                    haverev = true;
                                }
                                if ((d.Action == FileAction.None) && d.IsInClient && d.IsInDepot)
                                {
                                    cancheckout = true;
                                }
                            }
                        }
                        else
                        {
                            outsideRootMenuitemVisible = true;
                            startP4VMenuitemVisible = true;
                        }

                        if (stale || folderselect)
                        {
                            getLatestMenuitemVisible = true;
                            subSeperator1Visible = true;
                        }
                        if (checkedout || folderselect)
                        {
                            submitMenuitemVisible = true;
                        }
                        if (cancheckout || folderselect)
                        {
                            checkoutMenuitemVisible = true;
                            subSeperator2Visible = true;
                        }
                        if (notindepot || folderselect)
                        {
                            addMenuitemVisible = true;
                        }
                        if (cancheckout)
                        {
                            deleteMenuitemVisible = true;
                            subSeperator2Visible = true;
                        }
                        if (checkedout || folderselect || notindepot)
                        {
                            subSeperator2Visible = true;
                            if (!notindepot)
                            {
                                if (!folderselect)
                                {
                                    revertMenuitemVisible = true;
                                }
                                revertUnchangedMenuitemVisible = true;
                                subSeperator3Visible = true;
                            }
                        }

                        // only on single selection of file
                        if (TargetFiles.Length == 1)
                        {
                            if (!folderselect && haverev)
                            {
                                diffAgainstHaveMenuitemVisible = true;
                                diffAgainstMenuitemVisible = true;
                                fileHistoryMenuitemVisible = true;
                                timeLapseViewMenuitemVisible = true;
                                propertiesMenuitemVisible = true;
                                showInP4VMenuitemVisible = true;
                                subSeperator4Visible = true;
                            }
                            else
                            {
                                if (!folderselect)
                                {
                                    diffAgainstMenuitemVisible = true;
                                }
                                // don't allow folder history on the
                                // workspace root
                                if (TargetFiles[0]!=store.rep.Connection.Client.Root)
                                {
                                    fileHistoryMenuitemVisible = true;
                                }
                                subSeperator4Visible = true;
                            }
                        }

                        if ((haverev && !(checkedout)) || folderselect)
                        {
                            removeFromWorkspaceMenuitemVisible = true;
                        }
                        if (fmd != null || folderselect || notindepot)
                        {
                            refreshFileStateMenuitemVisible = true;
                            subSeperator5Visible = true;
                            pendingChangesMenuitemVisible = true;
                            moreMenuitemVisible = true;
                        }
                    }
                }
                catch (P4Exception ex)
                {
                    // Leaving this for now. It looks like it is a possible redundant check
                    // of more efficient if (ex.ErrorCode == P4ClientError.MsgDb_NotUnderRoot)
                    // below. It also looks like it would never be hit based on the string
                    // "outside root". Adding a message dialog for the case that this ever gets
                    // hit.
                    if (ex.ErrorLevel == ErrorSeverity.E_FATAL && ex.Message == "outside root")
                    {
                        Message message = new Message("CanShowMenu() P4Exception", ex.Message);
                        message.ShowDialog();
                        setConnection = Properties.Resources.ContextMenus_SetConnectionConnectedMenuItem;
                        setConnectionImage = Properties.Resources.connected_with_issues;
                        outside = Properties.Resources.ContextMenus_OutsideRoot;
                        outsideRootMenuitemVisible = true;
                        startP4VMenuitemVisible = true;
                    }

                    if (P4EXPProgram.IsLoginException(ex.ErrorCode))
                    {
                        P4EXPProgram.Login(TargetFiles[0]);
                        if (store.rep.Connection.Status == ConnectionStatus.Disconnected)
                        {
                            // notify of disconnected in top menu item
                            setConnection = Properties.Resources.ContextMenus_SetConnectionNotConnectedMenuItem;
                            setConnectionImage = Properties.Resources.disconnected;
                            outsideRootMenuitemVisible = false;
                            startP4VMenuitemVisible = true;
                        }
                    }
                    else if (ex.ErrorCode == P4ClientError.MsgServer_PasswordExpired)
                    {
                        store.loggedIn = false;
                        loginVisible = true;
                        string message = ex.Message;
                        Message dlg = new Message(Properties.Resources.MessageDlg_PasswordExpired, message);
                        dlg.ShowDialog();
                    }
                    else if (ex.ErrorCode == P4ClientError.MsgDm_NeedClient)
                    {
                        // second happens when P4CLIENT is not set 841226339
                        setConnection = Properties.Resources.ContextMenus_SetConnectionConnectedMenuItem;
                        setConnectionImage = Properties.Resources.connected_with_issues;
                        outside = Properties.Resources.ContextMenus_NoWorkspaceSet;
                        outsideRootMenuitemVisible = true;
                        startP4VMenuitemVisible = true;
                    }
                    else if (ex.ErrorCode == P4ClientError.MsgDb_NotUnderRoot)
                    {
                        // do nothing, this is a file not under workspace root.
                        // just return, the context menu is already set to show this.
                        outsideRootMenuitemVisible = true;
                        startP4VMenuitemVisible = true;
                    }
                    if (P4EXPProgram.showP4Exception())
                    {
                        string message = ex.Message + "\n" +
                            ex.StackTrace + "\n" +
                                ex.TargetSite.Name;
                        Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException(Properties.Resources.FileLogger_BuildContextMenu, ex);
                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: is this catching a login request?
                    // this does not appear to be something that will be
                    // part of a P4ClientError. Leaving it in for now. Unsure if it
                    // would ever be hit. Adding a message dialog for the case that
                    // this ever gets hit.
                    if (ex.Message.Contains("requires"))
                    {
                        Message message = new Message("CanShowMenu()  Exception", ex.Message);
                        message.ShowDialog();

                        P4EXPProgram.Connect(TargetFiles[0]);
                    }

                    if (store.rep != null && store.rep.Connection.Status == ConnectionStatus.Disconnected)
                    {
                        // notify of disconnected in top menu item
                        setConnection = Properties.Resources.ContextMenus_SetConnectionNotConnectedMenuItem;
                        setConnectionImage = Properties.Resources.disconnected;
                        outsideRootMenuitemVisible = false;
                        startP4VMenuitemVisible = true;
                    }
                    if (P4EXPProgram.showException())
                    {
                        string message = ex.Message + "\n" +
                            ex.StackTrace + "\n" +
                            ex.TargetSite.Name;
                        Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException(Properties.Resources.FileLogger_BuildContextMenu, ex);
                    return true;
                }
            }
            //  We always show the menu for now. This will still return true,
            // but individual menu items may be made invisible 
            return true;
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            //  Create the menu strip.
            var menu = new ContextMenuStrip();

            // Create the top seperator
            menu.Items.Add(mainSeperator1);

            //  Create the main Perforce item.
            mainPerforceMenuitem.Text = Properties.Resources.ContextMenus_MainMenuItem;
            mainPerforceMenuitem.Image = Properties.Resources.p4exp_logo;

            //  Create the Set Connection menu item.
            setConnectionMenuitem.Text = setConnection;
            setConnectionMenuitem.Image = setConnectionImage;
            //  When we click, we'll bring up the Set Connection dialog.
            setConnectionMenuitem.Click += (sender, args) => DoSetConnection();
            ////  Add the item to the context menu.
            mainPerforceMenuitem.DropDownItems.Add(setConnectionMenuitem);

            if (outsideRootMenuitemVisible)
            {
                //  Create the not under root menu item.
                outsideRootMenuitem.Text = outside;
                //  this is always grayed out
                outsideRootMenuitem.Enabled = false;
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(outsideRootMenuitem);
            }

            mainPerforceMenuitem.DropDownItems.Add(subSeperator0);

            if (startP4VMenuitemVisible)
            {
                //  Create the start p4v menu item.
                startP4VtMenuitem.Text = Properties.Resources.ContextMenus_StartP4VMenuItem;
                startP4VtMenuitem.Image = Properties.Resources.p4v_logo;
                //  When we click, we'll launch p4v.
                startP4VtMenuitem.Click += (sender, args) => DoStartP4V();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(startP4VtMenuitem);
            }

            // Always create connection, preferences, and help
            // menu items, it is just a matter of where they are placed

            //  Create the connection info menu item.
            connectionInfoMenuitem.Text = Properties.Resources.ContextMenus_ConnectionInfoMenuItem;
            //  When we click, we'll show the connection info.
            connectionInfoMenuitem.Click += (sender, args) => DoConnectionInfo();
            //  Add the item to the context menu.
            mainPerforceMenuitem.DropDownItems.Add(connectionInfoMenuitem);

            // create a Log in menu item so they can undo a login cancelled/password reset
            if (loginVisible)
            {
                loginMenuitem.Text = Properties.Resources.ContextMenus_LogInMenuItem;
                loginMenuitem.Click += (sender, args) => DoLogin();
                mainPerforceMenuitem.DropDownItems.Add(loginMenuitem);
            }

            //  Create the preferences menu item.
            preferencesMenuitem.Text = Properties.Resources.ContextMenus_PreferencesMenuItem;
            //  When we click, we'll open the preferences dialog.
            preferencesMenuitem.Click += (sender, args) => DoPreferences();
            //  Add the item to the context menu.
            mainPerforceMenuitem.DropDownItems.Add(preferencesMenuitem);

            //  Create the help menu item.
            helpMenuitem.Text = Properties.Resources.ContextMenus_HelpMenuItem;
            //  When we click, we'll open the online help.
            helpMenuitem.Click += (sender, args) => DoHelp();
            //  Add the item to the context menu.
            mainPerforceMenuitem.DropDownItems.Add(helpMenuitem);

            if(getLatestMenuitemVisible)
            {
                //  Create the get latest menu item.
                getLatestMenuitem.Text = Properties.Resources.ContextMenus_GetLatestRevisionMenuItem;
                //  When we click, we'll get the latest revision.
                getLatestMenuitem.Click += (sender, args) => DoGetLateset();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(getLatestMenuitem);
            }

            if(subSeperator1Visible)
            {
                // Create the 1st seperator
                mainPerforceMenuitem.DropDownItems.Add(subSeperator1);
            }

            if (submitMenuitemVisible)
            {
                //  Create the submit menu item.
                submitMenuitem.Text = Properties.Resources.ContextMenus_SubmitMenuItem;
                //  When we click, we'll submit the files.
                submitMenuitem.Click += (sender, args) => DoSubmit();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(submitMenuitem);
            }

            if (checkoutMenuitemVisible)
            {
                //  Create the check out menu item.
                checkoutMenuitem.Text = Properties.Resources.ContextMenus_CheckOutMenuItem;
                //  When we click, we'll check out the files.
                checkoutMenuitem.Click += (sender, args) => DoCheckout();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(checkoutMenuitem);
            }

            if (addMenuitemVisible)
            {
                //  Create the add menu item.
                addMenuitem.Text = Properties.Resources.ContextMenus_AddToSourceControlMenuItem;
                //  When we click, we'll mark the files for add.
                addMenuitem.Click += (sender, args) => DoAdd();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(addMenuitem);
            }

            if (deleteMenuitemVisible)
            {
                //  Create the delete menu item.
                deleteMenuitem.Text = Properties.Resources.ContextMenus_DeleteInHelixServerMenuItem;
                //  When we click, we'll mark the files for delete.
                deleteMenuitem.Click += (sender, args) => DoDelete();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(deleteMenuitem);
            }

            if (subSeperator2Visible)
            {
                // Create the 2nd seperator
                mainPerforceMenuitem.DropDownItems.Add(subSeperator2);
            }

            if(revertMenuitemVisible)
            {
                //  Create the revert menu item.
                revertMenuitem.Text = Properties.Resources.ContextMenus_RevertMenuItem;
                //  When we click, we'll revert the files.
                revertMenuitem.Click += (sender, args) => DoRevert();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(revertMenuitem);
            }

            if(revertUnchangedMenuitemVisible)
            {
                //  Create the revert unchanged menu item.
                revertUnchangedMenuitem.Text = Properties.Resources.ContextMenus_RevertUnchangedMenuItem;
                //  When we click, we'll revert the unchanged files.
                revertUnchangedMenuitem.Click += (sender, args) => DoRevertUnchanged();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(revertUnchangedMenuitem);
            }

            if(subSeperator3Visible)
            {
                // Create the 3rd seperator
                mainPerforceMenuitem.DropDownItems.Add(subSeperator3);
            }

            if (pendingChangesMenuitemVisible)
            {
                //  Create the pending changelists menu item.
                pendingChangesMenuitem.Text = Properties.Resources.ContextMenus_ViewPendingChangelistsMenuItem;
                //  When we click, we'll open the Pending Changelists view.
                pendingChangesMenuitem.Click += (sender, args) => DoPendingChanges();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(pendingChangesMenuitem);
            }

            if (diffAgainstHaveMenuitemVisible)
            {
                //  Create the diff against have menu item.
                diffAgainstHaveMenuitem.Text = Properties.Resources.ContextMenus_DiffAgainstHaveMenuItem;
                //  When we click, we'll diff the file against the have revision.
                diffAgainstHaveMenuitem.Click += (sender, args) => DoDiffAgainstHave();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(diffAgainstHaveMenuitem);
            }

            if(diffAgainstMenuitemVisible)
            {
                //  Create the diff against menu item.
                diffAgainstMenuitem.Text = Properties.Resources.ContextMenus_DiffAgainstMenuItem;
                //  When we click, we'll open the diff against dialog.
                diffAgainstMenuitem.Click += (sender, args) => DoDiffAgainst();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(diffAgainstMenuitem);
            }

            if (fileHistoryMenuitemVisible)
            {
                string selectionType = Properties.Resources.ContextMenus_SelectionTypeFile;
                string[] TargetFiles = SelectedItemPaths.ToArray();
                FileAttributes attr = System.IO.File.GetAttributes(TargetFiles[0]);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    selectionType = Properties.Resources.ContextMenus_SelectionTypeFolder;
                }
                //  Create the file history menu item.
                fileHistoryMenuitem.Text = selectionType + Properties.Resources.ContextMenus_HistoyMenuItem;
                //  When we click, we'll open file history.
                fileHistoryMenuitem.Click += (sender, args) => DoFileHistory();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(fileHistoryMenuitem);
            }

            if (timeLapseViewMenuitemVisible)
            {
                //  Create the Time-lapse View menu item.
                timeLapseViewMenuitem.Text = Properties.Resources.ContextMenus_TimeLapseViewMenuItem;
                //  When we click, we'll open Time-lapse View.
                timeLapseViewMenuitem.Click += (sender, args) => DoTimeLapseView();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(timeLapseViewMenuitem);
            }

            if (propertiesMenuitemVisible)
            {
                //  Create the properties menu item.
                propertiesMenuitem.Text = Properties.Resources.ContextMenus_PropertiesMenuItem;
                //  When we click, we'll open the file properties.
                propertiesMenuitem.Click += (sender, args) => DoShowProperties();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(propertiesMenuitem);
            }

            if (showInP4VMenuitemVisible)
            {
                //  Create the show in p4v menu item.
                showInP4VMenuitem.Text = Properties.Resources.ContextMenus_ShowInP4VMenuItem;
                showInP4VMenuitem.Image = Properties.Resources.p4v_logo;
                //  When we click, we'll show the file in p4v.
                showInP4VMenuitem.Click += (sender, args) => DoShowInP4V();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(showInP4VMenuitem);
            }

            if(subSeperator4Visible)
            {
                // Create the 4th seperator
                mainPerforceMenuitem.DropDownItems.Add(subSeperator4);
            }

            if (removeFromWorkspaceMenuitemVisible)
            {
                //  Create the remove from workspace menu item.
                removeFromWorkspaceMenuitem.Text = Properties.Resources.ContextMenus_RemoveFromWorkspaceMenuItem;
                //  When we click, we'll remove the files from the workspace.
                removeFromWorkspaceMenuitem.Click += (sender, args) => DoRemoveFromWorkspace();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(removeFromWorkspaceMenuitem);
            }

            if (refreshFileStateMenuitemVisible)
            {
                //  Create the refresh menu item.
                refreshFileStateMenuitem.Text = Properties.Resources.ContextMenus_RefreshFileStateMenuItem;
                //  When we click, we'll refresh the files.
                refreshFileStateMenuitem.Click += (sender, args) => DoRefreshFileState();
                //  Add the item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(refreshFileStateMenuitem);
            }

            if(subSeperator5Visible)
            {
                // Create the 5th seperator
                mainPerforceMenuitem.DropDownItems.Add(subSeperator5);
            }

            if(moreMenuitemVisible)
            {
                //  Create the more menu item.
                moreMenuitem.Text = Properties.Resources.ContextMenus_MoreMenuItem;
                //  When we click, we'll open the 5 additional items.

                //  Add the connection info menu item to the more context menu.
                moreMenuitem.DropDownItems.Add(connectionInfoMenuitem);

                //  Add the preferences menu item to the more context menu.
                moreMenuitem.DropDownItems.Add(preferencesMenuitem);

                //  Create the log off menu item.
                logOffMenuitem.Text = Properties.Resources.ContextMenus_LogOffMenuItem;
                //  When we click, we'll log off.
                logOffMenuitem.Click += (sender, args) => DoLogOff();
                //  Add the log off menu item item to the more context menu.
                moreMenuitem.DropDownItems.Add(logOffMenuitem);

                //  Add the help menu item to the more context menu.
                moreMenuitem.DropDownItems.Add(helpMenuitem);

                //  Add the more menu item to the context menu.
                mainPerforceMenuitem.DropDownItems.Add(moreMenuitem);
            }

            //  Add the main Helix item to the context menu.
            menu.Items.Add(mainPerforceMenuitem);

            // create the bottom seperator
            menu.Items.Add(mainSeperator2);

            //  Return the menu.
            return menu;
        }

        /// <summary>
        /// Shows the connetion info dialog.
        /// </summary>
        private void DoConnectionInfo()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.ConnectionInfo(path);
        }

        private void DoStartP4V()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.StartP4V(path);
        }

        private void DoPreferences()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.Preferences();
        }

        private void DoHelp()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.Help();
        }

        // 14 actions
        private void DoGetLateset()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.GetLatest(commandFiles);
        }

        private void DoSubmit()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.Submit(commandFiles);
        }

        private void DoCheckout()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.Checkout(commandFiles);
        }

        private void DoAdd()
        {
            string[] path = SelectedItemPaths.ToArray();
            P4EXPProgram.Add(path);
        }

        private void DoDelete()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.Delete(commandFiles);
        }
        private void DoRevert()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.Revert(commandFiles);
        }

        private void DoRevertUnchanged()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.RevertUnchanged(commandFiles);
        }

        private void DoPendingChanges()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.PendingChangelists(path);
        }

        private void DoDiffAgainstHave()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.DiffHave(commandFiles);
        }

        private void DoDiffAgainst()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.DiffAgainst(commandFiles);
        }

        private void DoFileHistory()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.FileHistory(commandFiles);
        }

        private void DoTimeLapseView()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.TimeLapseView(commandFiles);
        }

        private void DoShowProperties()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.ShowProperties(commandFiles);
        }

        private void DoShowInP4V()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.OpenInP4V(buildSelectedList());
        }

        private void DoRemoveFromWorkspace()
        {
            commandFiles = buildSelectedList();
            P4EXPProgram.RemoveFromWorkspace(commandFiles);
        }

        private void DoRefreshFileState()
        {
            // need to handle login exceptions
            string[] path = SelectedItemPaths.ToArray();
            try
            {
                P4EXPProgram.RefreshFileState(path);
            }
            catch (P4Exception e)
            {
                FileLogger.LogException(Properties.Resources.FileLogger_RefreshFileState, e);
                if (P4EXPProgram.IsLoginException(e.ErrorCode))
                {
                    P4EXPProgram.Login(path[0]);
                    P4EXPProgram.RefreshFileState(path);
                }
            }
            catch (Exception e)
            {
                FileLogger.LogException(Properties.Resources.FileLogger_RefreshFileState, e);
            }
        }

        private void DoLogin()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            RepoStorage store = P4EXPProgram.getRepoStorage(path);

            lock (store)
            {
                // force the login dialog
                store.ClearReconnect();
                P4EXPProgram.Connect(path, true);
            }
        }

        private void DoLogOff()
        {
            string path = SelectedItemPaths.FirstOrDefault();
            P4EXPProgram.LogOff(path);
        }

        private void DoSetConnection()
        {
            P4EXPProgram.SetConnection();
        }
    }
}