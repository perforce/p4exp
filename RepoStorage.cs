using Perforce.P4;
using System;
using System.Collections.Generic;
using System.IO;

namespace P4EXP
{
    public class RepoStorage
    {
        public Repository rep { get; set; }
        public bool connected { get; set; }
        public bool loggedIn { get; set; }
        public String clientName;
        public List<String> roots = new List<String>();
        DateTime clientExpire;
        DateTime clientFailed;
        DateTime reconnect;
        public bool loginCancelled { get; set; }
        public DateTime loginRetry { get; set; }

        // TODO: make a filecache per path, to make it
        // easier to 
        // (a) clear when refreshing
        // (b) clear when expiring to free memory
        internal class PathCache
        {
            private FileCache fileCache = new FileCache();
            public String path { get; }
            private DateTime expire;

            public PathCache(String path, DateTime expire)
            {
                this.path = path;
                this.expire = expire;
            }

            public FileMetaData GetMetaData(String filename)
            {
                return fileCache.Get(filename);
            }

            public bool AddFiles(IList<FileMetaData> fmds)
            {
                foreach (FileMetaData fmd in fmds)
                {
                    fileCache.Add(fmd);
                }
                return true;
            }

            public bool AddFile(FileMetaData fmd)
            {
                fileCache.Add(fmd);
                return false;
            }

            public bool IsExpired()
            {
                return DateTime.Now.CompareTo(expire) > 0;
            }
        }

        // track two things
        // a list of PathCache references in expiration order
        // a map a path -> pathcache for speedy lookup
        private Dictionary<String, PathCache> pathCache = new Dictionary<string, PathCache>();
        private List<PathCache> expirationList = new List<PathCache>();

        public RepoStorage(Repository _rep)
        {
            rep = _rep;
            connected = false;
            loggedIn = false;
            loginCancelled = false;
        }

        public bool IsConnected()
        {
            lock (this)
            {
                return rep.Connection.connectionEstablished() && connected;
            }
        }

        internal String cleanRoot(String root)
        {
            if (string.IsNullOrEmpty(root))
            { return root; }
            if (root[root.Length - 1] != '\\')
                root += '\\';
            return root.ToLower().Replace("/", "\\");
        }

        private void RefreshClientData(bool force = false)
        {
            lock (this)
            {
                if (!connected)
                    return;

                DateTime now = DateTime.Now;
                if (now.CompareTo(clientFailed) < 0)
                    return; // give up for a bit, there was a client error

                if (roots.Count > 0 && !force && now.CompareTo(clientExpire) < 0)
                    return; // no need to refresh
                FileLogger.LogMessage(3, Properties.Resources.FileLogger_RepoStorage,
                    String.Format(Properties.Resources.FileLogger_RefreshingClient, clientName));

                // re-initializing this RepoStorage
                roots.Clear();

                // set the client again, forcing a refresh
                try
                {
                    // Check for valid Client
                    IList<Client> clients = rep.GetClients(new ClientsCmdOptions(ClientsCmdFlags.None,
                            null, clientName, 1, null));
                    if (clients == null || clients.Count <= 0)
                    {
                        // If P4CLIENT is non-existent or a client that does not exist on the
                        // server, make sure the Client itself is set to null.
                        rep.Connection.Client = null;
                    }
                    else
                    {
                        rep.Connection.Client = clients[0];
                    }
                }
                catch (P4Exception e)
                {
                    if (e.ErrorCode == P4ClientError.MsgServer_LoginExpired ||
                        e.ErrorCode == P4ClientError.MsgServer_BadPassword ||
                        e.ErrorCode == P4ClientError.MsgServer_PasswordExpired)
                    {
                        clientFailed = now.AddSeconds(15);
                        throw;  // throw it out, someone should handle with a Login call
                    }
                }
                catch (Exception e)
                {
                    FileLogger.LogException(Properties.Resources.FileLogger_RefreshClientData, e);
                    rep.Connection.Client = null;
                }

                // we failed for some reason, record the failure so that we
                // don't launch a retry attack
                if (rep.Connection.Client == null || rep.Connection.Client.Root == null)
                {
                    FileLogger.LogMessage(1, Properties.Resources.FileLogger_RefreshClientData, Properties.Resources.FileLogger_FailedToSetClientDelay15);
                    clientFailed = now.AddSeconds(15);
                    return;
                }

                // now read the roots into our root array
                // convert to lower case, reverse the path indicators
                roots.Add(cleanRoot(rep.Connection.Client.Root));
                if (rep.Connection.Client.AltRoots != null)
                {
                    foreach (String root in rep.Connection.Client.AltRoots)
                    {
                        roots.Add(cleanRoot(root));
                    }
                }

                // TODO: make this configurable
                clientExpire = now.AddMinutes(5);
            }
        }

        // the assumption is that each of the FileSpecs are in the same directory
        public IList<FileMetaData> GetFileMetaData(FileSpec[] paths)
        {
            String basePath = Path.GetDirectoryName(paths[0].LocalPath.Path);
            if (!IsUnderClientRoot(basePath))
                return null;
            RefreshPath(basePath, false);
            PathCache pc = null;
            if (pathCache.ContainsKey(basePath))
            {
                pc = pathCache[basePath];
                // this seems expensive if the list is long
                expirationList.Remove(pc);
                expirationList.Add(pc);
            }
            else
            {
                // TODO: configurable expiration time
                pc = new PathCache(basePath, DateTime.Now.AddMinutes(10));
                pathCache[basePath] = pc;
                expirationList.Add(pc);
            }

            List<FileMetaData> list = new List<FileMetaData>();
            foreach (FileSpec fs in paths)
            {
                list.Add(pc.GetMetaData(fs.LocalPath.Path));
            }

            return list;
        }

        public bool IsUnderClientRoot(String basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            { return false; }
            basePath = cleanRoot(basePath);
            RefreshClientData();

            // assume client roots have already been lower cased
            foreach (String root in roots)
            {
                if (basePath.StartsWith(root) && (basePath.Length == root.Length ||
                basePath.Substring(root.Length-1,1)=="\\"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RefreshPath(String basePath, bool force)
        {
            // if the list of paths is large (configurable?), try to expire some paths
            if (expirationList.Count > 100)
            {
                ClearExpiredPaths();
            }

            // first check if basePath is under the client's root.  if not, don't bother
            if (!connected || !IsUnderClientRoot(basePath))
                return false;

            PathCache pc = (pathCache.ContainsKey(basePath) ? pathCache[basePath] : null);
            bool useCache = (pc != null);

            if (pc != null)
                useCache = !pc.IsExpired();

            if (force)
                useCache = false;

            if (useCache)
                return true;

            FileLogger.LogMessage(3, Properties.Resources.FileLogger_RepoStorage, string.Format(Properties.Resources.FileLogger_RefreshingPath, basePath));

            if (pc != null)
            {
                // remove from the lists
                pathCache.Remove(basePath);
                expirationList.Remove(pc);
                pc = null;
            }

            // TODO: configurable expiration
            pc = new PathCache(basePath, DateTime.Now.AddMinutes(5));

            // is it better to do a local dir call to build a list of fstat targets, or to 
            // do a fstat path/* ?
            // the answer from the performance lab is that passing the wildcard to the server should be more performant
            FileSpec[] specs = new FileSpec[] { new FileSpec(null, null, new LocalPath(Path.Combine(basePath, "*")), null) };
            IList<FileMetaData> fds = null;

            try
            {
                fds = rep.GetFileMetaData(null, specs);
            }
            catch (P4Exception e)
            {
                // add it anyway or we get lots of retries
                if (fds != null)
                    pc.AddFiles(fds);

                pathCache[basePath] = pc;
                expirationList.Add(pc);

                // alaways throw here?  display an error message?
                if (P4EXPProgram.IsLoginException(e.ErrorCode))
                {
                    throw;
                }
                FileLogger.LogException(Properties.Resources.FileLogger_P4EXPProgram, e);
            }
            catch (Exception e)
            {
                FileLogger.LogException(Properties.Resources.FileLogger_P4EXPProgram, e);
            }

            // add it anyway
            if (fds != null)
                pc.AddFiles(fds);

            pathCache[basePath] = pc;
            expirationList.Add(pc);

            return true;
        }

        public FileMetaData GetFileMetaData(String targetFile)
        {
            String basePath = Path.GetDirectoryName(targetFile);
            if (string.IsNullOrEmpty(basePath) || !IsUnderClientRoot(basePath))
                return null;
            RefreshPath(basePath, false);
            PathCache pc = pathCache[basePath];
            if (pc == null)
                return null;    // this seems bad
            return pc.GetMetaData(targetFile);
        }

        public int ClearExpiredPaths()
        {
            int expired = 0;
            while (expirationList.Count > 0)
            {
                PathCache pc = expirationList[0];
                if (!pc.IsExpired())
                    break;

                expired++;
                expirationList.Remove(pc);
                pathCache.Remove(pc.path);
            }

            return expired;
        }

        internal void ClearAll()
        {
            expirationList.Clear();
            pathCache.Clear();
            connected = false;
            loggedIn = false;
        }
        internal void SetReconnect(DateTime dateTime)
        {
            reconnect = dateTime;
        }

        internal void ClearReconnect()
        {
            reconnect = new DateTime();
        }

        internal bool ReconnectTimeout()
        {
            // return true if sufficient time has passed to attempt a reconnect
            return (DateTime.Now > reconnect);
        }

        public bool RetryLogin()
        {
            return (!loginCancelled && loginRetry < DateTime.Now);
        }
    }
}