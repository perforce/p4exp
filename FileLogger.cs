using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Perforce.P4;

namespace P4EXP
{
	class FileLogger
	{
        private static string LogFilePath { get; set; }
		static string RolloverFilePath { get; set; }
		static int LogFileMaxSize { get; set; }

		static bool _intialized = false;

        internal static void Init()
		{
            Properties.Settings mySettings = new Properties.Settings();

			string logFileDirectory;
            LogFilePath = mySettings.LogFilePath;
                //Preferences.LocalSettings.GetString("Log_path", null);

			if (LogFilePath != null)
			{
				logFileDirectory = Path.GetDirectoryName(LogFilePath);
				if (Directory.Exists(logFileDirectory) == false)
				{
					LogFilePath = null;
				}
			}
			if (LogFilePath == null)
			{
				string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				appPath = Path.GetDirectoryName(appPath);

				LogFilePath = Path.Combine(appPath, "P4EXP_Log.txt");

				mySettings.LogFilePath = LogFilePath;
			}
			LogFileMaxSize = mySettings.LogFileSize;

            mySettings.Save();

			string ext = Path.GetExtension(LogFilePath);
			string baseName = Path.GetFileNameWithoutExtension(LogFilePath);
			logFileDirectory = Path.GetDirectoryName(LogFilePath);

			RolloverFilePath = string.Format("{0}_prev{1}", baseName, ext);
			RolloverFilePath = Path.Combine(logFileDirectory, RolloverFilePath);

			_intialized = true;
		}

		public static void LogMessage(int log_level,
										String source,
										String message)
		{
            Properties.Settings mySettings = new Properties.Settings();
            LogFilePath = mySettings.LogFilePath;
			if (mySettings.LogToFile)
			{
				if (_intialized == false)
				{
					Init();
				}
				string LogLevelStr = null;
				switch (log_level)
				{
					case 0:
						LogLevelStr = Properties.Resources.FileLogger_LogLevelFatal;
						break;
					case 1:
						LogLevelStr = Properties.Resources.FileLogger_LogLevelError;
                        break;
					case 2:
						LogLevelStr = Properties.Resources.FileLogger_LogLevelWarning;
						break;
					case 3:
						LogLevelStr = Properties.Resources.FileLogger_LogLevelInfo;
						break;
					default:
						LogLevelStr = string.Format(Properties.Resources.FileLogger_Debug, log_level);
						break;
				}
				DateTime now = DateTime.Now;
				String msg = String.Format("[{0}: {1}] {2} : {3}\r\n",
					LogLevelStr, source, now.ToString("MMMM dd, yyyy HH:mm:ss.ffff"), message);
                lock(LogFilePath)
                {
                    try
                    {
                        if (System.IO.File.Exists(LogFilePath))
                        {
                            FileInfo fi = new FileInfo(LogFilePath);

                            if ((fi.Length > 1024) && ((fi.Length / 1024) > (LogFileMaxSize / 2)))
                            {
                                // File has grown to half the allotted size, delete the existing rollover log
                                // (if any), move the current log to the rollover, and start a new log

                                if (System.IO.File.Exists(RolloverFilePath))
                                {
                                    System.IO.File.Delete(RolloverFilePath);
                                }
                                System.IO.File.Move(LogFilePath, RolloverFilePath);
                            }
                        }
                        using (StreamWriter sr = new StreamWriter(LogFilePath, true))
                        {
                            sr.Write(msg);
                        }
                    }
                    catch { } // never fail because of an error writing a log message
                }
				// TODO: Implement an internal logging function
			}
		}

		public static void LogException(String source, Exception ex)
		{
            Properties.Settings mySettings = new Properties.Settings();

            if (mySettings.LogToFile)
			{
				try
				{

					String msg = String.Format("{0}:{1}\r\n{2}",
						ex.GetType().ToString(),
						ex.Message,
						ex.StackTrace);
					LogMessage(1, source, msg);

					if (ex.InnerException != null)
						LogException(Properties.Resources.FileLogger_InnerException, ex);
				}
				catch { } // never fail because of an error writing a log message
			}
		}

        private static P4CallBacks.LogMessageDelegate logDelegate = null;
        private static int[] debugObjectCount;

        private static void LogFunction(int log_level, string file, int line, string message)
        {
            if (!diagnosticLogging())
            {
                logDelegate = null;
                return;
            }
            LogMessage(log_level, file + " (" + line + ") ", message);
        }
        public static bool diagnosticLogging()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.DiagnosticLogging;
        }
        public static void startDebugLogging(string action)
        {
            if (diagnosticLogging())
            {
                LogMessage(4, action, Properties.Resources.FileLogger_BeginDiagLogging);
                logDelegate = new P4CallBacks.LogMessageDelegate(LogFunction);

                P4Debugging.SetBridgeLogFunction(logDelegate);
                debugObjectCount = new int[P4Debugging.GetAllocObjectCount()];

                for (int i = 0; i < debugObjectCount.Length; i++)
                {
                    debugObjectCount[i] = P4Debugging.GetAllocObject(i);
                }
            }
        }
        public static void stopDebugLogging(string action)
        {
            logDelegate = null;
            P4Debugging.SetBridgeLogFunction(logDelegate);
            if (diagnosticLogging())
            {
                LogMessage(4, action, Properties.Resources.FileLogger_EndDiagLogging);
            }
        }
    }
}
