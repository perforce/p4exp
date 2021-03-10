using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Perforce.P4;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Deployment.WindowsInstaller;

namespace P4EXP
{
    public class P4EXPProgram
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string _p4VPath = null;
        private static string _p4VCPath = null;
        private static string _p4MergePath = null;
        internal static bool connectionSuccess = true;

        /// <summary>
        /// find P4Merge.exe
        /// </summary>
        /// <returns></returns>
        private static string P4MergePath()
        {
            Properties.Settings mySettings = new Properties.Settings();

            if (_p4MergePath != null)
            {
                return _p4MergePath;
            }

            string prefString = mySettings.P4MergePath;

            if ((prefString != null) && (prefString.EndsWith("P4Merge.exe")))
            {
                _p4MergePath = prefString;
            }
            else
            {
                string installLocation = P4InstallLocation();
                if (System.IO.File.Exists(installLocation + "P4Merge.exe"))
                {
                    mySettings.P4MergePath = installLocation + "P4Merge.exe";
                    _p4MergePath = installLocation + "P4Merge.exe";
                    mySettings.Save();
                    return _p4MergePath;
                }
            }
            mySettings.Save();
            return _p4MergePath;
        }

        internal static bool IsLoginException(int errorCode)
        {
            return (errorCode == P4ClientError.MsgServer_LoginExpired ||
                     errorCode == P4ClientError.MsgServer_LoggedOut ||
                     errorCode == P4ClientError.MsgServer_BadPassword ||
                     errorCode == P4ClientError.MsgServer_BadPassword0 ||
                     errorCode == P4ClientError.MsgServer_BadPassword1);
        }

        /// <summary>
        /// find P4V.exe
        /// </summary>
        /// <returns></returns>
        private static string P4VPath()
        {
            Properties.Settings mySettings = new Properties.Settings();

            if (_p4VPath != null)
            {
                return _p4VPath;
            }

            string prefString = mySettings.P4VPath;

            if ((prefString != null) && (prefString.EndsWith("P4V.exe"))
                && System.IO.File.Exists(prefString))
            {
                _p4VPath = prefString;
            }
            else
            {
                string installLocation = P4InstallLocation();
                if (System.IO.File.Exists(installLocation + "P4V.exe"))
                {
                    mySettings.P4VPath = installLocation + "P4V.exe";
                    _p4VPath = installLocation + "P4V.exe";
                    mySettings.Save();
                    return _p4VPath;
                }
            }
            mySettings.Save();
            return _p4VPath;
        }

        /// <summary>
        /// find P4VC.exe
        /// </summary>
        /// <returns></returns>
        private static string P4VCPath()
        {
            Properties.Settings mySettings = new Properties.Settings();

            if (_p4VCPath != null)
            {
                return _p4VCPath;
            }

            string prefString = mySettings.P4VCPath;

            if ((prefString != null) && (prefString.EndsWith("p4vc.exe")))
            {
                _p4VCPath = prefString;
            }
            else
            {
                string installLocation = P4InstallLocation();
                if (System.IO.File.Exists(installLocation + "p4vc.exe"))
                {
                    mySettings.P4VCPath = installLocation + "p4vc.exe";
                    _p4VCPath = installLocation + "p4vc.exe";
                    mySettings.Save();
                    return _p4VCPath;
                }
                else
                {
                    string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                    Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                    dlg.ShowDialog();
                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_LocateP4VC, message);
                }
            }
            return _p4VCPath;
        }

        public static bool errorsOnly()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.ShowOnlyErrors;
        }

        public static bool showP4Exception()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.ShowP4Exception;
        }

        public static bool showException()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.ShowException;
        }

        public static IList<FileSpec> fmd2fs(IList<FileMetaData> fmd, VersionSpec ver)
        {
            IList<FileSpec> fileSpecs = new List<FileSpec>();
            foreach (FileMetaData f in fmd)
            {
                FileSpec fs = new FileSpec(f);
                fs.Version = ver;
                fileSpecs.Add(fs);
            }
            return fileSpecs;
        }

        private static Dictionary<String, RepoStorage> repoStorageMap = new Dictionary<String, RepoStorage>();

        public static FileMetaData GetFileMetaData(String path)
        {
            RepoStorage store = getRepoStorage(path);

            lock (store)
            {
                return store.GetFileMetaData(path);
            }
        }

        public static IList<FileMetaData> GetFileMetaData(FileSpec[] paths)
        {
            RepoStorage store = getRepoStorage(paths[0].LocalPath.Path);

            lock (store)
            {
                return store.GetFileMetaData(paths);
            }
        }

        public static RepoStorage getRepoStorage(IList<FileMetaData> files)
        {
            // grab a repo based on the first thing's path
            return getRepoStorage(files.First().LocalPath.Path);
        }

        internal static String HashConnection(String port, String user, String client)
        {
            return port + "+" + user + "+" + client;
        }

        static bool firstRepoCall = true;

        public static RepoStorage getRepoStorage(String path)
        {
            // we're relying on some implicit behavior here: that the underlying client api behaves the same all the time
            //   if you ask "for this path what is the client" you might get a different answer if you have initialized 
            //   the network stack or not.  Initialize it here in a roundabout way, then never do that again
            if (firstRepoCall)
            {
                firstRepoCall = false;
                try
                {
                    System.Net.Sockets.TcpClient con = new System.Net.Sockets.TcpClient("0.0.0.0", 1);
                }
                catch (Exception)
                {
                    // throw it away
                }
            }
            StackTrace st = new StackTrace();

            // based on path (may be a file path or a directory), figure out the connection
            // info and look in our map for an existing repo.
            String dir = null;
            if (System.IO.File.Exists(path))
            {
                dir = Path.GetDirectoryName(path);
            }
            if (dir == null)
                dir = path;

            // convert this to a port/user/client
            String port, user, client;
            P4Server.ConnectionInfoFromPath(dir, out port, out user, out client);
            // no port? forget it
            if (port.Length == 0 || user.Length == 0 || client.Length == 0)
                return null;
            String hash = HashConnection(port, user, client);

            lock (repoStorageMap)
            {
                // TODO: remove this
                bool resetMap = false;
                if (resetMap)
                {
                    repoStorageMap.Clear();
                }

                // No repo?  Make a new one!
                if (repoStorageMap.ContainsKey(hash))
                {
                    return repoStorageMap[hash];
                }
                else
                {
                    // explicitly *do not* use a multithreaded repository
                    // requires that we lock something (the storage object)
                    // when using the repo or the repo results
                    FileLogger.LogMessage(3, Properties.Resources.FileLogger_GetRepository,
                        String.Format(Properties.Resources.FileLogger_CreatingNewRepository, port, user, client));
                    Repository rep = new Repository(new Server(new ServerAddress(port)), false);
                    rep.Connection.CurrentWorkingDirectory = dir;
                    rep.Connection.UserName = user;

                    RepoStorage repoStorage = new RepoStorage(rep);
                    // record the client name
                    repoStorage.clientName = client;

                    // don't attempt to connect, that's a little complicated for this request
                    repoStorageMap[hash] = repoStorage;
                    return repoStorage;
                }
            }
        }

        public static void Login(string path, bool force = false)
        {
            RepoStorage store = getRepoStorage(path);
            if (store != null)
                Login(store, force);
        }

        public static void Login(RepoStorage store, bool force = false)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Login);
            lock (store)
            {
                // when you cancel a login, you have to go back and force a login in the context menu
                if (!store.RetryLogin() && !force)
                {
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Login);
                    return;
                }

                store.loginCancelled = false;
                Repository rep = store.rep;
                string password = string.Empty;

                // Check for P4LOGINSSO
                string ssoVar = rep.Connection.GetP4EnvironmentVar("P4LOGINSSO");
                if (ssoVar != null)
                {
                    password = "UsingSSO"; // dummy value server will not prompt for a password when using SSO
                    FileLogger.LogMessage(3, Properties.Resources.FileLogger_UsingSSO, ssoVar);
                    try
                    {
                        rep.Connection.Login(password, null);
                        // SSO agent login succeeded
                        if (rep.Connection.LastResults.ErrorList == null)
                        {
                            store.loggedIn = true;
                            return;
                        }
                        // SSO agent login canceled
                        if (rep.Connection.LastResults.ErrorList != null)
                        {
                            // display trigger error
                            if (rep.Connection.LastResults.ErrorList[0].ErrorCode ==
                                P4ClientError.MsgServer_TriggerFailed)
                            {
                                Message dlg = new Message(Properties.Resources.MessageDlg_LoginFailed,
                                    rep.Connection.LastResults.ErrorList[0].ErrorMessage);
                                dlg.ShowDialog();
                                store.loginCancelled = true;
                                LogOff(store);
                                return;
                            }
                            // otherwise, continue and bring up PW prompt
                        }
                    }
                    catch (P4Exception ex)
                    {
                        FileLogger.LogException(Properties.Resources.FileLogger_Login, ex);
                        FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Login);
                        return;
                    }
                }

                // Check for HAS
                HASCheckDlg hASCheckDlg = new HASCheckDlg(rep);

                if (hASCheckDlg.ShowDialog() == DialogResult.OK)
                {
                    // OK is returned if the dialog has been launched,
                    // which also means HandleUrl was called.
                    if (hASCheckDlg.ExternalAuth)
                    {
                        return;
                    }
                    else
                    {
                        Message dlg = new Message(Properties.Resources.P4EXP,
                            string.Format(Properties.Resources.HAS_Auth_Fail,
                rep.Connection.UserName, rep.Connection.Server.Address.Uri));
                        dlg.ShowDialog();
                        store.loginCancelled = true;
                        LogOff(store);
                        return;
                    }
                }

                Login enterPW = new Login(rep.Connection.UserName,
                                                            rep.Connection.Server.Address.Uri);
                enterPW.ShowDialog();
                // TODO: make this configurable?
                store.loginRetry = DateTime.Now.AddSeconds(15);

                if (enterPW.DialogResult == DialogResult.Cancel)
                {
                    store.loginCancelled = true;
                    LogOff(store);
                }
                if (enterPW.DialogResult == DialogResult.OK)
                {
                    password = enterPW.password;
                }

                try
                {
                    rep.Connection.Login(password, null);
                    store.loggedIn = true;
                }
                catch (P4Exception ex)
                {
                    // if (ex.ErrorCode == 805445297)
                    {
                        rep.Connection.Disconnect();

                        string message = ex.Message;
                        Message dlg = new Message(Properties.Resources.MessageDlg_LoginFailed, message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException(Properties.Resources.FileLogger_Login, ex);
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Login);
                    return;
                }
                catch (Exception)
                {
                    // catch potential null exception here.
                }
            }
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Login);
        }

        private static string _productVersion = null;
        /// <summary>
        /// Get the file version for the executing assembly.
        /// </summary>
        public static string ProductVersion
        {
            get
            {
                if (_productVersion == null)
                {
                    // Get the file version for the executing assembly.
                    string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                    if (assemblyPath.StartsWith("file:"))
                    {
                        assemblyPath = assemblyPath.Substring(5).TrimStart('\\', '/');
                    }
                    FileVersionInfo.GetVersionInfo(assemblyPath);
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

                    // return the file version number.
                    _productVersion = myFileVersionInfo.ProductVersion;
                }
                return _productVersion;
            }
        }

        public static void Disconnect(string path)
        {
            RepoStorage store = getRepoStorage(path);
            lock (store)
            {
                Connection con = store.rep.Connection;
                con.Disconnect();
            }
        }

        public static void Connect(String path, bool force = false, bool loginRequired = true)
        {
            try
            {
                RepoStorage store = getRepoStorage(path);
                lock (store)
                {
                    if (!force && store.IsConnected() && store.loggedIn)
                    {
                        return;
                    }

                    if (!force && (!store.ReconnectTimeout() || !store.RetryLogin()))
                    {
                        return;
                    }

                    store.ClearReconnect();

                    FileLogger.LogMessage(3, Properties.Resources.FileLogger_Connect,
                        String.Format(Properties.Resources.FileLogger_RetryingConnection, path));
                    Connection con = store.rep.Connection;

                    Options options = new Options();
                    options["ProgramName"] = Properties.Resources.P4EXP;
                    options["ProgramVersion"] = ProductVersion;
                    try
                    {
                        store.loggedIn = false;
                        store.connected = con.Connect(options);
                        // Connect() does not throw auth errors (intentionally), so try and 
                        // set the client at this point to exersize the connection
                        Client cli = new Client();
                        cli.Name = store.clientName;
                        // this will force a server call and trigger auth problems if they are present
                        con.Client = cli;
                        store.loggedIn = true;
                    }
                    catch (P4Exception ex)
                    {
                        if (ex.ErrorCode == P4ClientError.MsgRpc_HostKeyMismatch)
                        {
                            string exception = ex.Message.Replace("Perforce", "Helix Core server");
                            string[] sslMsg = exception.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (SslPrompt.ShowNewFingerprint(sslMsg) == DialogResult.Cancel)
                            {
                                connectionSuccess = false;
                                throw new P4TrustException(ErrorSeverity.E_FAILED, exception);
                            }
                            string fingerprint = SslPrompt.FingerPrint;
                            con.TrustAndConnect(null, "-i", fingerprint);
                            connectionSuccess = true;
                        }
                        else if (ex.ErrorCode == P4ClientError.MsgRpc_HostKeyUnknown)
                        {
                            string exception = ex.Message.Replace("Perforce", "Helix Core server");
                            string[] sslMsg = exception.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (SslPrompt.ShowFirstContact(sslMsg) == DialogResult.Cancel)
                            {
                                connectionSuccess = false;
                                throw new P4TrustException(ErrorSeverity.E_FAILED, exception);
                            }
                            string fingerprint = SslPrompt.FingerPrint;
                            con.TrustAndConnect(null, "-i", fingerprint);
                            connectionSuccess = true;
                        }
                        else if (ex.ErrorCode == P4ClientError.MsgServer_Login2Required)
                        {
                            if (LaunchHelixMFA(con.UserName, con.Server.Address.Uri) == 0)
                            {
                                Connect(path, true, loginRequired);
                            }
                        }
                        else if (IsLoginException(ex.ErrorCode))
                        {
                            if (loginRequired)
                            {
                                Login(store, force);

                                if (!con.LastResults.Success && con.LastResults.ErrorList != null)
                                {
                                    if (con.LastResults.ErrorList[0].ErrorCode == P4ClientError.MsgServer_Login2Required)
                                    {
                                        if (LaunchHelixMFA(con.UserName, con.Server.Address.Uri) == 0)
                                        {
                                            Connect(path, true, loginRequired);
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            // probably a connection error, so set a timeout
                            store.loggedIn = false;
                            store.SetReconnect(DateTime.Now.AddSeconds(15));
                            throw;
                        }
                    }

                    con.CommandTimeout = TimeSpan.FromSeconds(5);
                }
            }
            catch (P4Exception ex)
            {
                if (ex.ErrorCode == P4ClientError.MsgServer_PasswordExpired)
                {
                    string message = ex.Message;
                    Message dlg = new Message(Properties.Resources.MessageDlg_PasswordExpired, message);
                    dlg.ShowDialog();
                }
                else if (P4EXPProgram.showP4Exception())
                {
                    string message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Connect, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    string message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Connect, ex);
            }
        }
        public static void ConnectionInfo(String path)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_ConnectionInfo);
            // force the connection retry, don't require login
            Connect(path, true, false);
            RepoStorage store = getRepoStorage(path);

            lock (store)
            {
                // Check if the connection worked, and also confirm that the server is online.
                if (!store.IsConnected() || store.rep.Server.State==ServerState.Offline)
                {
                    Message badConnection = new Message(Properties.Resources.MessageDlg_NoConnection,
                        String.Format(Properties.Resources.MessageDlg_NoConnectionForPort, store.rep.Server.Address));
                    badConnection.ShowDialog();
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_ConnectionInfo);
                    return;
                }
                Repository rep = store.rep;

                string currentYear = DateTime.Now.Year.ToString();
                string p4info = Properties.Resources.ConnectionInfo_HelixPlugin + "\r\n" +
               Properties.Resources.ConnectionInfo_Copyright + currentYear +
               Properties.Resources.ConnectionInfo_PerforceSoftware + "\r\n" +
               "\r\n";
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.ProductVersion;
                PortableExecutableKinds p;
                ImageFileMachine machineInfo;
                assembly.ManifestModule.GetPEKind(out p, out machineInfo);
                string platform = Properties.Resources.ConnectionInfo_Platformx64;
                if (machineInfo.ToString().EndsWith("86"))
                { platform = Properties.Resources.ConnectionInfo_Platformx86; }

                p4info += Properties.Resources.ConnectionInfo_ClientVersion + version + " (" + platform + ")" + "\r\n";
                P4Command cmd = new P4Command(rep.Connection, "info", false, null);
                P4CommandResult results = cmd.Run();
                if ((results.Success) && (results.InfoOutput != null))
                {
                    foreach (P4ClientInfoMessage infoLine in results.InfoOutput)
                        if (infoLine.Message.StartsWith("Current directory"))
                        {
                            if (System.IO.File.Exists(path))
                            {
                                p4info += Properties.Resources.ConnectionInfo_CurrentDirectory + Path.GetDirectoryName(path) + "\r\n";
                            }
                            else
                            {
                                p4info += Properties.Resources.ConnectionInfo_CurrentDirectory + path + "\r\n";
                            }
                        }
                        else
                        {
                            p4info += infoLine.Message.ToString() + "\r\n";
                        }
                }
                else
                {
                    p4info += results.ErrorList;
                }

                Message dlg = new Message(Properties.Resources.MessageDlg_ConnectionInfo, p4info);
                dlg.ShowDialog();
                FileLogger.LogMessage(3, Properties.Resources.FileLogger_ConnectionInfo, p4info);
            }
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_ConnectionInfo);
        }

        public static bool IsHAS(String path)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_ConnectionInfo);
            // force the connection retry, don't require login
            Connect(path, true, false);
            RepoStorage store = getRepoStorage(path);

            lock (store)
            {
                if (!store.IsConnected() || store.rep.Server.State == ServerState.Offline)
                {
                    Message badConnection = new Message(Properties.Resources.MessageDlg_NoConnection,
                        String.Format(Properties.Resources.MessageDlg_NoConnectionForPort, store.rep.Server.Address));
                    badConnection.ShowDialog();
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_ConnectionInfo);
                    return false;
                }

                Repository rep = store.rep;
                P4Command cmd = new P4Command(rep.Connection, "info", true, null);
                P4CommandResult results = cmd.Run();
                if ((results.Success) && (results.TaggedOutput != null))
                {
                    foreach (TaggedObject result in results.TaggedOutput)
                    {
                        if (result.ContainsKey("ssoAuth"))
                        {
                            if ((result["ssoAuth"].Contains("optional") || result["ssoAuth"].Contains("required")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;

        }

        public static void Preferences()
        {
            Preferences dlg = new Preferences();
            dlg.ShowDialog();
            FileLogger.LogMessage(3, Properties.Resources.FileLogger_Preferences,
                Properties.Resources.FileLogger_LaunchingPreferences);
        }

        public static void Help()
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Help);
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion; // 2011.1.0.0 is the format
            version = version.Remove(0, 2);
            string[] versionSplit = version.Split('.');
            string relDir = "r" + versionSplit[0] + "." + versionSplit[1];
            try
            {
                bool pageExists = false;
                string helpPath = Properties.Resources.HelpLink_Index;
                string versionPath = helpPath.Replace("doc.current", relDir);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(versionPath);
                request.Method = WebRequestMethods.Http.Head;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    pageExists = response.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    // likely got a 404 here, page does not exist
                    // this will happen if the html help has not been
                    // pushed to web for the assembly's version. For
                    // example, builds from main.
                }
                if (pageExists)
                {
                    System.Windows.Forms.Help.ShowHelp(null, versionPath, null);
                }
                else
                {
                    System.Windows.Forms.Help.ShowHelp(null, helpPath, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Help);
        }


        public static void GetLatest(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_GetLatestRevision);
            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, null);
            string title = Properties.Resources.MessageDlg_GetLatestRevision;
            string message = string.Empty;

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);
                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    IList<FileSpec> filesSynced =
                        rep.Connection.Client.SyncFiles(fileSpecs, null);
                    if (filesSynced != null)
                    {
                        if (filesSynced.Count > 1)
                        {
                            message = filesSynced.Count + Properties.Resources.MessageDlg_FilesUpdated;
                        }
                        else
                        {
                            message = filesSynced.Count + Properties.Resources.MessageDlg_FileUpdated;
                        }
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesUpdated;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_GetLatest, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_GetLatest, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_GetLatest, ex);
            }

            RefreshFileState(fileSpecs);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_GetLatest);
        }

        public static void Submit(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Submit);
            string p4vcPath = null;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                // use rich client framework
                p4vcPath = P4VCPath();

                if (string.IsNullOrEmpty(p4vcPath) || !System.IO.File.Exists(p4vcPath))
                {
                    string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                    Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                    dlg.ShowDialog();
                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_Submit, message);
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Submit);
                    return;
                }
                Process startP4VC = new Process();
                startP4VC.StartInfo.FileName = p4vcPath;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }

                string filesToSubmit = string.Empty;
                foreach (FileMetaData file in commandFiles)
                {
                    filesToSubmit += file.LocalPath.Path.TrimEnd('\\', '.', '.' ,'.') + "<";
                }

                filesToSubmit = filesToSubmit.Remove(filesToSubmit.Length - 1);
                startP4VC.StartInfo.Arguments =
                    " -p " + rep.Connection.Server.Address.Uri
                    + " -u " + rep.Connection.UserName
                    + " -c " + rep.Connection.Client.Name
                    + charset
                    + " submit " + "\"" + filesToSubmit + "\"";

                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.StartInfo.UseShellExecute = false;
                try
                {
                    startP4VC.Start();

                    if (startP4VC.WaitForExit(200) == true)
                    {
                        // P4VC exited to quickly
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message + "\n" +
                           ex.StackTrace + "\n" +
                           ex.TargetSite.Name;
                    // TODO: find an error code for this
                    if (ex.Message.Contains("too long"))
                    {
                        message = commandFiles.Count +
                            Properties.Resources.MessageDlg_MaximumFileLimitExceeded;
                        Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                        dlg.ShowDialog();
                        FileLogger.LogException(Properties.Resources.FileLogger_Submit, ex);
                        FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Submit);
                        return;
                    }
                    if (P4EXPProgram.showException())
                    {
                        Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                        dlg.ShowDialog();
                        FileLogger.LogException(Properties.Resources.FileLogger_Submit, ex);
                    }
                }
                startP4VC.WaitForExit();
                RefreshFileState(commandFiles);
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Submit);
                return;
            }
        }

        public static void Checkout(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_CheckOut);
            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, null);
            string title = Properties.Resources.MessageDlg_CheckOut;
            string message = string.Empty;

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);

                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    IList<FileSpec> filesCheckedOut = rep.Connection.Client.EditFiles(fileSpecs, null);
                    if (filesCheckedOut != null)
                    {
                        if (filesCheckedOut.Count > 1)
                        {
                            message = filesCheckedOut.Count + Properties.Resources.MessageDlg_FilesCheckedOut;
                        }
                        else
                        {
                            message = filesCheckedOut.Count + Properties.Resources.MessageDlg_FileCheckedOut;
                        }
                        if(rep.Connection.LastResults.InfoOutput!=null &&
                           rep.Connection.LastResults.InfoOutput.Count>0)
                        {
                            message += Properties.Resources.MessageDlg_CheckoutWarningsAndErrors;
                            foreach (P4ClientInfoMessage info in rep.Connection.LastResults.InfoOutput)
                            {
                                message+= info.Message +"\r\n";
                            }
                        }
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesCheckedOut;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_CheckOut, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_CheckOut, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_CheckOut, ex);
            }

            RefreshFileState(fileSpecs);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_CheckOut);
        }

        public static void Add(string[] targetFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Add);
            IList<FileSpec> fileSpecs = new List<FileSpec>();
            foreach (string path in targetFiles)
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    string[] filePaths = Directory.GetFiles(path, "*",
                                         SearchOption.AllDirectories);
                    FileSpec[] subFiles = new FileSpec[filePaths.Length];
                    subFiles = FileSpec.LocalSpecArray(filePaths);

                    foreach (FileSpec subFile in subFiles)
                    {
                        fileSpecs.Add(subFile);
                    }

                }
                else
                {
                    FileSpec f =
                        new FileSpec(null, null, new LocalPath(path), null);
                    fileSpecs.Add(f);
                }
            }
            string title = Properties.Resources.MessageDlg_AddFiles;
            string message = string.Empty;
            try
            {
                RepoStorage store = getRepoStorage(targetFiles[0]);

                lock (store)
                {
                    // a quick check for a file potentially re-added
                    // by creating a new file with the same name as
                    // a file deleted in the depot and currently synced
                    // to that deleted head revision
                    // this is to avoid an error on submit. P4V will not
                    // submit C:\workspace\re-add.txt#<deleted_rev>
                    // limit this check to 10 files for performance
                    // considerations on add of a directory. There is a
                    // chance they could encounter the error in that case.
                    IList<FileSpec> syncKFiles = new List<FileSpec>();
                    if (fileSpecs.Count<=10)
                    {
                        IList<FileMetaData> checkForDelete = GetFileMetaData(fileSpecs.ToArray());
                        foreach (FileMetaData fmd in checkForDelete)
                        {
                            if (fmd != null &&  fmd.HaveRev<=0 && fmd.HeadAction==FileAction.Delete)
                            {
                                FileSpec fs = new FileSpec();
                                fs.LocalPath = fmd.LocalPath;
                                fs.Version = new Revision(fmd.HeadRev - 1);
                                syncKFiles.Add(fs);
                            }
                        }
                    }
                    Repository rep = store.rep;

                    // if there were re-adds via creating a new file with the
                    // same name, do a sync -k
                    if (syncKFiles.Count>0)
                    {
                        rep.Connection.Client.SyncFiles(syncKFiles,
    new SyncFilesCmdOptions(SyncFilesCmdFlags.ServerOnly));
                    }
                    AddFilesCmdOptions options =
                        new AddFilesCmdOptions(AddFilesCmdFlags.KeepWildcards, -1, null);
                    IList<FileSpec> filesAdded = rep.Connection.Client.AddFiles(fileSpecs, options);
                    if (filesAdded != null)
                    {
                        if (filesAdded.Count > 1)
                        {
                            message = filesAdded.Count + Properties.Resources.MessageDlg_FilesAdded;
                        }
                        else
                        {
                            message = filesAdded.Count + Properties.Resources.MessageDlg_FileAdded;
                        }
                        RefreshFileState(fileSpecs);
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesAdded;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_Add, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Add, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Add, ex);
            }

            RefreshFileState(targetFiles);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Add);
        }

        public static void Delete(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Delete);
            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, null);
            string title = Properties.Resources.MessageDlg_Delete;
            string message = string.Empty;

            // check prefs here for warning
            Properties.Settings mySettings = new Properties.Settings();
            if(mySettings.warnDelete)
            {
                RevertDeleteWarning warn = new RevertDeleteWarning(true);

                if ((warn.ShowDialog() == DialogResult.Cancel))
                {
                    Message dlg = new Message(Properties.Resources.MessageDlg_Delete, Properties.Resources.MessageDlg_NoFilesMarkedForDelete);
                    dlg.ShowDialog();
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Delete);
                    return;
                }
            }
            

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);

                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    IList<FileSpec> filesCheckedOut = rep.Connection.Client.DeleteFiles(fileSpecs, null);
                    if (filesCheckedOut != null)
                    {
                        if (filesCheckedOut.Count > 1)
                        {
                            message = filesCheckedOut.Count + Properties.Resources.MessageDlg_FilesMarkedForDelete;
                        }
                        else
                        {
                            message = filesCheckedOut.Count + Properties.Resources.MessageDlg_FileMarkedForDelete;
                        }
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesMarkedForDelete;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_Delete, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Delete, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Delete, ex);
            }

            RefreshFileState(fileSpecs);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Delete);
        }
        public static void Revert(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Revert);
            // warn? (curent p4exp gives a very general
            // warning whether or not the file has changed
            // likely need a pref for that too, if implemented
            RevertDeleteWarning warn = new RevertDeleteWarning(false);
            if ((warn.ShowDialog() == DialogResult.Cancel))
            {
                Message dlg = new Message(Properties.Resources.MessageDlg_Revert,
                    Properties.Resources.MessageDlg_NoFilesReverted);
                dlg.ShowDialog();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Revert);
                return;
            }

            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, null);
            string title = Properties.Resources.MessageDlg_Revert;
            string message = string.Empty;

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);
                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    IList<FileSpec> filesReverted = rep.Connection.Client.RevertFiles(fileSpecs, null);
                    if (filesReverted != null)
                    {
                        if (filesReverted.Count > 1)
                        {
                            message = filesReverted.Count + Properties.Resources.MessageDlg_FilesReverted;
                        }
                        else
                        {
                            message = filesReverted.Count + Properties.Resources.MessageDlg_FileReverted;
                        }
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_Revert, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Revert, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_Revert, ex);
            }
            RefreshFileState(fileSpecs);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Revert);
        }

        public static void RevertUnchanged(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_RevertUnchanged);
            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, null);
            string title = Properties.Resources.MessageDlg_RevertUnchanged;
            string message = string.Empty;

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);

                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    RevertCmdOptions options = new RevertCmdOptions(RevertFilesCmdFlags.UnchangedOnly, -1);
                    IList<FileSpec> filesReverted = rep.Connection.Client.RevertFiles(fileSpecs, options);
                    if (filesReverted != null)
                    {
                        if (filesReverted.Count > 1)
                        {
                            message = filesReverted.Count + Properties.Resources.MessageDlg_FilesReverted;
                        }
                        else
                        {
                            message = filesReverted.Count + Properties.Resources.MessageDlg_FileReverted;
                        }
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesReverted;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_RevertUnchanged, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_RevertUnchanged, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_RevertUnchanged, ex);
            }

            RefreshFileState(commandFiles);
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_RevertUnchanged);
        }

        public static void PendingChangelists(string path)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_PendingChangelists);

            // try to use rich client framework
            string p4vcPath = P4VCPath();

            if (string.IsNullOrEmpty(p4vcPath))
            {
                // just return, app is not in prefs and could not be
                // found installed. User has already seen missing app
                // message
                return;
            }
            if (!System.IO.File.Exists(p4vcPath))
            {
                // this is for the case of the app being installed previously
                // and saved in the prefs, but somehow now missing
                {
                    string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                    Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                    dlg.ShowDialog();
                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_LocateP4VC, message);
                }
                return;
            }

            Process startP4VC = new Process();
            startP4VC.StartInfo.FileName = p4vcPath;
            RepoStorage store = getRepoStorage(path);

            lock (store)
            {
                Repository rep = store.rep;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }
                startP4VC.StartInfo.Arguments = " -p " + rep.Connection.Server.Address.Uri +
                        " -u " + rep.Connection.UserName + " -c " + rep.Connection.Client.Name +
                        charset + " pendingchanges";

                startP4VC.StartInfo.UseShellExecute = false;
                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.Start();

                if (startP4VC.WaitForExit(200) == true)
                {
                    // P4VC exited to quickly
                }
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_PendingChangelists);
                return;
            }
        }

        public static void DiffHave(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_DiffVSHave);
            Options options = new Options();
            options["-f"] = null;
            options["-Od"] = null;

            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                rep.Connection.Connect(null);

                P4Command cmd = new P4Command(rep.Connection, "diff", true, commandFiles[0].LocalPath.Path);
                P4CommandResult results = cmd.Run(options);

                if (results.ErrorList != null)
                {
                    Message dlg = new Message(Properties.Resources.MessageDlg_Error, results.ErrorList[0].ErrorMessage);
                    dlg.ShowDialog();

                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_DiffAgainstHave,
                        results.ErrorList[0].ErrorMessage);
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_DiffVSHave);
                    return;
                }
                if (string.IsNullOrEmpty(results.TextOutput))
                {
                    string message = Properties.Resources.MessageDlg_NoDiffsFromHaveRev;
                    Message dlg = new Message(Properties.Resources.MessageDlg_DiffAgainstHave, message);
                    dlg.ShowDialog();
                    FileLogger.LogMessage(3, Properties.Resources.FileLogger_DiffAgainstHave, message);
                    FileLogger.stopDebugLogging(Properties.Resources.FileLogger_DiffVSHave);
                    return;
                }

                ParameterizedThreadStart pts = new ParameterizedThreadStart(DiffFileThreadProc);

                Thread diffThread = new Thread(pts);
                diffThread.IsBackground = true;
                diffThread.Start(commandFiles[0]);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_DiffVSHave);
                return;
            }
        }

        public static void DiffAgainst(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_DiffAgainst);
            string p4vcPath = null;

            // use rich client framework
            p4vcPath = P4VCPath();

            if (string.IsNullOrEmpty(p4vcPath) || !System.IO.File.Exists(p4vcPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4Merge;
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_DiffAgainst, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_DiffAgainst);
                return;
            }
            Process startP4VC = new Process();
            startP4VC.StartInfo.FileName = p4vcPath;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                rep.Connection.Connect(null);

                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }

                startP4VC.StartInfo.Arguments = " -p " + rep.Connection.Server.Address.Uri +
                    " -u " + rep.Connection.UserName + " -c " + rep.Connection.Client.Name +
                    charset + " diff " + "\"" + commandFiles[0].LocalPath.Path + "\"" + " "
                    + "\"" + commandFiles[0].LocalPath.Path + "\"";

                startP4VC.StartInfo.UseShellExecute = false;
                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.Start();

                if (startP4VC.WaitForExit(200) == true)
                {
                    // P4VC exited to quickly
                }
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_DiffAgainst);
                return;
            }
        }

        public static void FileHistory(IList<FileMetaData> commandFiles)
        {
            // First confirm if this is a file or folder.
            // This command should only be launched with
            // a list of one file

            string historyPath = commandFiles[0].LocalPath.Path;
            if (historyPath.EndsWith("\\..."))
            {
                historyPath = historyPath.Remove(historyPath.LastIndexOf("\\..."));
            }

            FileLogger.startDebugLogging(Properties.Resources.FileLogger_FileHistory);
            string p4vcPath = null;

            // use rich client framework
            p4vcPath = P4VCPath();

            if (string.IsNullOrEmpty(p4vcPath) || !System.IO.File.Exists(p4vcPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                dlg.ShowDialog();
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_FileHistory, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_FileHistory);
                return;
            }
            Process startP4VC = new Process();
            startP4VC.StartInfo.FileName = p4vcPath;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }

                startP4VC.StartInfo.Arguments =
                    " -p " + rep.Connection.Server.Address.Uri
                    + " -u " + rep.Connection.UserName
                    + " -c " + rep.Connection.Client.Name
                    + charset
                    + " history " + "\"" + historyPath + "\"";

                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.StartInfo.UseShellExecute = false;
                startP4VC.Start();

                if (startP4VC.WaitForExit(200) == true)
                {
                    // P4VC exited to quickly
                }
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_FileHistory);
                return;
            }
        }

        public static void TimeLapseView(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_TimeLapseView);
            string p4vcPath = null;

            // use rich client framework
            p4vcPath = P4VCPath();

            if (string.IsNullOrEmpty(p4vcPath) || !System.IO.File.Exists(p4vcPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_TimeLapseView, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_TimeLapseView);
                return;
            }
            Process startP4VC = new Process();
            startP4VC.StartInfo.FileName = p4vcPath;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }

                startP4VC.StartInfo.Arguments = " -p " + rep.Connection.Server.Address.Uri +
                    " -u " + rep.Connection.UserName + " -c " + rep.Connection.Client.Name +
                    charset + " timelapse " + "\"" + commandFiles[0].LocalPath.Path + "\"";

                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.StartInfo.UseShellExecute = false;
                startP4VC.Start();

                if (startP4VC.WaitForExit(200) == true)
                {
                    // P4VC exited too quickly
                }
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_TimeLapseView);
                return;
            }
        }

        public static void ShowProperties(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_FileProperties);
            string p4vcPath = null;

            // use rich client framework
            p4vcPath = P4VCPath();

            if (string.IsNullOrEmpty(p4vcPath) || !System.IO.File.Exists(p4vcPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                dlg.ShowDialog();
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_ShowProperties, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_FileProperties);
                return;
            }
            Process startP4VC = new Process();
            startP4VC.StartInfo.FileName = p4vcPath;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }
                startP4VC.StartInfo.Arguments =
                    " -p " + rep.Connection.Server.Address.Uri
                    + " -u " + rep.Connection.UserName
                    + " -c " + rep.Connection.Client.Name
                    + charset
                    + " properties " + "\"" + commandFiles[0].LocalPath.Path + "\"";

                startP4VC.StartInfo.CreateNoWindow = true;
                startP4VC.StartInfo.UseShellExecute = false;
                startP4VC.Start();

                if (startP4VC.WaitForExit(200) == true)
                {
                    // P4VC exited to quickly
                }
                startP4VC.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_FileProperties);
                return;
            }
        }

        public static void StartP4V(String path)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_StartP4V);
            string p4vPath = null;

            // use configured application
            p4vPath = P4VPath();
            if (string.IsNullOrEmpty(p4vPath) || !System.IO.File.Exists(p4vPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4V; 
                Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                dlg.ShowDialog();
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_StartP4V, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_StartP4V);
                return;
            }
            Process startP4V = new Process();
            startP4V.StartInfo.FileName = p4vPath;
            string port = "";
            string user = "";
            string client = "";
            string charset = "";
            RepoStorage store = getRepoStorage(path);

            lock (store)
            {
                Repository rep = store.rep;
                try
                {
                    port = " -p " + rep.Connection.Server.Address.Uri;
                    user = " -u " + rep.Connection.UserName;
                    client = " -c " + rep.Connection.Client.Name;
                    charset = " -C " + rep.Connection.CharacterSetName;
                }
                catch
                {
                    // just a catch on null connection,
                    // leave all vars as ""
                }
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }

                if (port == " -p ")
                {
                    port = "";
                }
                if (user == " -u ")
                {
                    user = "";
                }
                if (client == " -c ")
                {
                    client = "";
                }
                startP4V.StartInfo.Arguments =
                   port + user + client + charset;

                startP4V.StartInfo.CreateNoWindow = true;
                startP4V.Start();

                if (startP4V.WaitForExit(200) == true)
                {
                    // P4V exited to quickly
                }
                startP4V.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_StartP4V);
                return;
            }
        }

        public static void OpenInP4V(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_Open_in_P4V);
            string p4vPath = null;

            // use configured application
            p4vPath = P4VPath();
            if (string.IsNullOrEmpty(p4vPath) || !System.IO.File.Exists(p4vPath))
            {
                string message = Properties.Resources.MessageDlg_CannotLocateP4VC;
                Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                dlg.ShowDialog();
                FileLogger.LogMessage(1, Properties.Resources.FileLogger_Open_in_P4V, message);
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Open_in_P4V);
                return;
            }
            Process startP4V = new Process();
            startP4V.StartInfo.FileName = p4vPath;
            RepoStorage store = getRepoStorage(commandFiles);

            lock (store)
            {
                Repository rep = store.rep;
                string charset = rep.Connection.CharacterSetName;
                if (charset == null || charset == "")
                {
                    charset = string.Empty;
                }
                else
                {
                    charset = " -C " + charset;
                }
                startP4V.StartInfo.Arguments =
                    " -p " + rep.Connection.Server.Address.Uri
                    + " -u " + rep.Connection.UserName
                    + " -c " + rep.Connection.Client.Name
                    + charset
                    + " -s \"" + commandFiles[0].LocalPath.Path + "\"";

                startP4V.StartInfo.CreateNoWindow = true;
                startP4V.Start();

                if (startP4V.WaitForExit(200) == true)
                {
                    // P4V exited to quickly
                }
                startP4V.Dispose();
                FileLogger.stopDebugLogging(Properties.Resources.FileLogger_Open_in_P4V);
                return;
            }
        }

        /// <summary>
        /// find install location of Perforce applications
        /// </summary>
        /// <returns>install path</returns>
        private static string P4InstallLocation()
        {
            string location = "";
            foreach (ProductInstallation product in
                ProductInstallation.GetRelatedProducts("{70A9FDC7-885B-4D6D-BAFD-CB2D27AB2963}"))
            {
                if (product.InstallLocation != null)
                {
                    location = product.InstallLocation;
                    return location;
                }
            }
            return null;
        }

        private static string _helixMFAPath = null;
        /// <summary>
		/// find HelixMFA.exe
		/// </summary>
		/// <returns></returns>
        private static string HelixMFAPath()
        {
            if (_helixMFAPath != null)
            {
                if (_helixMFAPath.EndsWith("HelixMFA.exe") && System.IO.File.Exists(_helixMFAPath))
                {
                    return _helixMFAPath;
                }
            }
            Properties.Settings mySettings = new Properties.Settings();
            string prefString = mySettings.HMFAPath;

            if ((prefString != null) && (prefString.EndsWith("HelixMFA.exe"))
                && System.IO.File.Exists(prefString))
            {
                _helixMFAPath = prefString;
            }
            else
            {
                // check for standalone install of HMFA
                foreach (ProductInstallation product in
                    ProductInstallation.GetRelatedProducts("{771150B5-B57D-47EC-B4E4-299711C020C4}"))
                {
                    if (product.InstallLocation != null)
                    {
                        _helixMFAPath = product.InstallLocation + "HelixMFA.exe";
                        if (System.IO.File.Exists(_helixMFAPath))
                        {
                            mySettings.HMFAPath = _helixMFAPath;
                            mySettings.Save();
                            return _helixMFAPath;
                        }
                    }
                }
                // check if HMFA was installed with P4V
                string installRoot = P4InstallLocation();
                if (installRoot != null && System.IO.File.Exists(installRoot + "HelixMFA.exe"))
                {
                    mySettings.HMFAPath = installRoot + "HelixMFA.exe";
                    mySettings.Save();
                    _helixMFAPath = installRoot + "HelixMFA.exe";
                    return _helixMFAPath;
                }
            }
            return _helixMFAPath;
        }
        public static int LaunchHelixMFA(string user, string port)
        {
            string helixMFAPath = HelixMFAPath();
            LaunchingHMFA launchDlg;

            if (string.IsNullOrEmpty(helixMFAPath) || !System.IO.File.Exists(helixMFAPath))
            {
                launchDlg = new LaunchingHMFA(false,
                user, port, helixMFAPath);
            }
            else
            {
                launchDlg = new LaunchingHMFA(true,
                user, port, helixMFAPath);
            }

            launchDlg.ShowDialog();
            return launchDlg.exitCode;
        }
        public static void RemoveFromWorkspace(IList<FileMetaData> commandFiles)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_RemoveFromWorkspace);
            IList<FileSpec> fileSpecs = fmd2fs(commandFiles, new Revision(0));
            string title = Properties.Resources.MessageDlg_RemoveFromWorkspace;
            string message = string.Empty;

            try
            {
                RepoStorage store = getRepoStorage(commandFiles);

                lock (store)
                {
                    Repository rep = store.rep;
                    rep.Connection.Connect(null);

                    IList<FileSpec> filesRemoved = rep.Connection.Client.SyncFiles(fileSpecs, null);
                    if (filesRemoved != null)
                    {
                        if (filesRemoved.Count > 1)
                        {
                            message = filesRemoved.Count + Properties.Resources.MessageDlg_FilesRemovedFromWorkspace;
                        }
                        else
                        {
                            message = filesRemoved.Count + Properties.Resources.MessageDlg_FileRemovedFromWorkspace;
                        }
                    }
                    else
                    {
                        message = Properties.Resources.MessageDlg_NoFilesRemovedFromWorkspace;
                    }
                    if (message != null && message != string.Empty)
                    {
                        if (!(errorsOnly()))
                        {

                            Message dlg = new Message(title, message);
                            dlg.ShowDialog();
                        }
                        FileLogger.LogMessage(3, Properties.Resources.FileLogger_RemoveFromWorkspace, message);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                    FileLogger.LogException(Properties.Resources.FileLogger_RemoveFromWorkspace, ex);
                }
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                    FileLogger.LogException(Properties.Resources.FileLogger_RemoveFromWorkspace, ex);
                }
            }
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_RemoveFromWorkspace);
        }

        public static void RefreshFileState(IList<FileSpec> files)
        {
            // force a refresh of the metadata for all files
            HashSet<String> uniquePaths = new HashSet<String>();
            foreach (FileSpec fs in files)
            {
                uniquePaths.Add(Path.GetDirectoryName(fs.LocalPath.Path));
                RefreshIcon(fs.LocalPath.Path);
            }

            RefreshFileState(uniquePaths);
        }

        public static void RefreshFileState(IList<FileMetaData> files)
        {
            // force a refresh of the metadata for all files
            HashSet<String> uniquePaths = new HashSet<String>();
            foreach (FileMetaData fmd in files)
            {
                uniquePaths.Add(Path.GetDirectoryName(fmd.LocalPath.Path));
                RefreshIcon(fmd.LocalPath.Path);
            }

            RefreshFileState(uniquePaths);
        }

        public static void RefreshFileState(string[] files)
        {
            // force a refresh of the metadata for all files
            HashSet<String> uniquePaths = new HashSet<String>();
            foreach (String p in files)
            {
                uniquePaths.Add(Path.GetDirectoryName(p));
                RefreshIcon(p);
            }

            RefreshFileState(uniquePaths);
        }

        private static void RefreshFileState(HashSet<String> uniquePaths)
        {
            foreach (String path in uniquePaths)
            {
                RepoStorage store = getRepoStorage(path);
                lock (store)
                {
                    store.RefreshPath(path, true);
                }
            }

            // now tell explorer to update
            RefreshWindowsExplorer();
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        const int SHCNF_PATH = 0x0005;
        const int SHCNE_ATTRIBUTES = 0x00000800;
        const int SHCNE_ASSOCCHANGED = 0x08000000;
        const int SHCNF_FLUSH = 0x1000;

        public static void RefreshIcon(string file)
        {
            IntPtr path = Marshal.StringToHGlobalAuto(file);
            SHChangeNotify(SHCNE_ATTRIBUTES, SHCNF_PATH | SHCNF_FLUSH, path, IntPtr.Zero);
        }

        public static void LogOff(String path)
        {
            RepoStorage store = getRepoStorage(path);
            LogOff(store);
        }

        public static void LogOff(RepoStorage store)
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_LogOff);
            lock (store)
            {
                Repository rep = store.rep;

                rep.Connection.Logout(null);
                rep.Connection.Disconnect();
                store.loggedIn = false;
                // TODO: not sure what this is supposed to do, clear the status icons?
                //   if so, clear the store and refresh
                store.ClearAll();
                RefreshWindowsExplorer();
            }
            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_LogOff);
        }

        public static void SetConnection()
        {
            FileLogger.startDebugLogging(Properties.Resources.FileLogger_SetConnection);

            OpenConnectionDlg dlg = new OpenConnectionDlg();
            dlg.ShowDialog();
            if (dlg.DialogResult == DialogResult.OK)
            {
                // set the env vars
                P4Server.Set("P4PORT", dlg.ServerPort.Trim());
                P4Server.Set("P4USER", dlg.UserName.Trim());
                P4Server.Set("P4CLIENT", dlg.Workspace.Trim());
            }


            FileLogger.stopDebugLogging(Properties.Resources.FileLogger_SetConnection);
        }

        public static void RefreshWindowsExplorer()
        {

            // Refresh the desktop
            //SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);

            // Refresh any open explorer windows
            // based on http://stackoverflow.com/questions/2488727/refresh-windows-explorer-in-win7
            Guid CLSID_ShellApplication = new Guid("13709620-C279-11CE-A49E-444553540000");
            Type shellApplicationType = Type.GetTypeFromCLSID(CLSID_ShellApplication, true);

            try
            {
                object shellApplication = Activator.CreateInstance(shellApplicationType);
                object windows = shellApplicationType.InvokeMember("Windows", System.Reflection.BindingFlags.InvokeMethod, null, shellApplication, new object[] { });

                Type windowsType = windows.GetType();
                object count = windowsType.InvokeMember("Count", System.Reflection.BindingFlags.GetProperty, null, windows, null);
                for (int i = 0; i < (int)count; i++)
                {
                    object item = windowsType.InvokeMember("Item", System.Reflection.BindingFlags.InvokeMethod, null, windows, new object[] { i });
                    if (item == null)
                        continue;

                    Type itemType = item.GetType();

                    // Only refresh Windows Explorer, without checking for the name this could refresh open IE windows
                    string itemName = (string)itemType.InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, item, null);
                    if (itemName == "Windows Explorer" || itemName == "File Explorer")
                    {
                        itemType.InvokeMember("Refresh", System.Reflection.BindingFlags.InvokeMethod, null, item, null);
                    }
                }
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.showP4Exception())
                {
                    string message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_RefreshExplorer, ex);
            }
            catch (Exception ex)
            {
                if (P4EXPProgram.showException())
                {
                    string message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                        ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                    dlg.ShowDialog();
                }
                FileLogger.LogException(Properties.Resources.FileLogger_RefreshExplorer, ex);
            }
        }

        /// <summary>
        /// Run the diff from a background thread so it can delete the temp file 
        /// after the diff program is closed
        /// </summary>
        /// <param name="param"></param>
        public static void DiffFileThreadProc(object param)
        {
            Repository rep = null;
            try
            {
                string diffPath = P4MergePath();
                if (string.IsNullOrEmpty(diffPath) || !System.IO.File.Exists(diffPath))
                {
                    string message = Properties.Resources.MessageDlg_CannotLocateP4Merge;
                    Message dlg = new Message(Properties.Resources.MessageDlg_MissingApplication, message);
                    dlg.ShowDialog();
                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_LocateP4Merge, message);
                    return;
                }

                if (diffPath != null)
                {
                    FileMetaData fmd = param as FileMetaData;
                    RepoStorage store = getRepoStorage(fmd.LocalPath.Path);

                    lock (store)
                    {
                        rep = store.rep;
                        // param may not be full fileMetaData, so fetch again
                        IList<FileMetaData> fmdlist = rep.GetFileMetaData(null, new FileSpec(null, null,
                            new LocalPath(fmd.LocalPath.Path), null));
                        fmd = fmdlist[0];

                        string localFile = fmd.LocalPath.Path;

                        if (localFile == null)
                        {
                            return;
                        }

                        using (TempFile tempPath = new TempFile())
                        {
                            FileSpec fs = new FileSpec(null, null, new LocalPath(localFile),
                            (Revision.Have));
                            GetFileContentsCmdOptions options =
                                new GetFileContentsCmdOptions(GetFileContentsCmdFlags.Suppress, tempPath._path);

                            try
                            {
                                IList<string> tempContents = rep.GetFileContents(options, fs);
                            }
                            catch (Exception ex)
                            {
                                FileLogger.LogException(Properties.Resources.FileLogger_GetFileContents, ex);
                            }
                            string args = string.Empty;

                            args = string.Format("-nl \"{0} (depot file)\" -nr \"{2} (local file)\" \"{1}\" \"{2}\"",
                                                 fmd.DepotPath.Path + "#have", tempPath._path, localFile);

                            Process launchP4Merge = new Process();

                            launchP4Merge.StartInfo.FileName = diffPath;
                            launchP4Merge.StartInfo.Arguments = args;
                            launchP4Merge.StartInfo.CreateNoWindow = true;
                            launchP4Merge.StartInfo.UseShellExecute = false;
                            launchP4Merge.Start();

                            launchP4Merge.WaitForExit();

                            launchP4Merge.Dispose();

                            tempPath.Dispose();
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
                // ignore thread aborts
                return;
            }
            catch (Exception ex)
            {
                // If the error is because the repository is now null, it means
                // the connection was closed in the middle of a command, so ignore it.

                // NOTE: do not use rep without locking the store
                if (rep != null)
                {
                    if (P4EXPProgram.showException())
                    {
                        string message = ex.Message + "\n" +
                            ex.StackTrace + "\n" +
                            ex.TargetSite.Name;
                        Message dlg = new Message(Properties.Resources.MessageDlg_Exception, message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException(Properties.Resources.FileLogger_DiffFile, ex);
                }
                return;
            }
        }

        public class TempFile : IDisposable
        {
            internal string _path;

            public TempFile()
            {
                string tempPath = System.IO.Path.GetTempPath();

                int n = 1;

                string tempFileName = string.Format("exptmp({0}).tmp", n);

                _path = System.IO.Path.Combine(tempPath, tempFileName);

                while (System.IO.File.Exists(_path))
                {
                    tempFileName = string.Format("exptmp({0}).tmp", ++n);

                    _path = System.IO.Path.Combine(tempPath, tempFileName);
                }

                System.IO.File.Create(_path).Close();
            }

            public void Dispose()
            {
                if (System.IO.File.Exists(_path))
                {
                    System.IO.File.SetAttributes(_path, System.IO.FileAttributes.Archive);
                    System.IO.File.Delete(_path);
                }
                if (System.IO.File.Exists("P4EXP.P4EXPProgram+TempFile"))
                {
                    System.IO.File.SetAttributes("P4EXP.P4EXPProgram+TempFile",
                        System.IO.FileAttributes.Archive);
                    System.IO.File.Delete("P4EXP.P4EXPProgram+TempFile");
                }
            }
        }
    }
}
