using Perforce.P4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace P4EXP
{
    class FileCache
    {
        private Dictionary<String, FileMetaData> pathToStatMap =
            new Dictionary<string, FileMetaData>(StringComparer.InvariantCultureIgnoreCase);

        // maintain a cache of file status data
        public FileCache()
        {
        }

        public FileMetaData Get(String path)
        {
            if (pathToStatMap.ContainsKey(path))
                return pathToStatMap[path];
            return null;
        }

        public bool Add(IList<FileMetaData> mdl)
        {
            if (mdl == null)
                return false;

            foreach (FileMetaData md in mdl)
            {
                // FileLogger.LogMessage(0, "FileCache", String.Format("{0} added", md.LocalPath.Path));
                pathToStatMap[md.LocalPath.Path] = md;
            }
            return true;
        }

        public bool Add(FileMetaData fileStatus)
        {
            // FileLogger.LogMessage(0, "FileCache", String.Format("{0} added", fileStatus.LocalPath.Path));
            pathToStatMap[fileStatus.LocalPath.Path] = fileStatus;

            return true;
        }

        public bool Remove(String path)
        {
            lock (pathToStatMap)
            {
                return pathToStatMap.Remove(path);
            }
        }

        public void Clear()
        {
            pathToStatMap.Clear();
        }
    }
}
