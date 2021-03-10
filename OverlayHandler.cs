using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perforce.P4;
using System.IO;
using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;
using System.Runtime.InteropServices;

namespace P4EXP
{
    public abstract class OverlayHandler : SharpIconOverlayHandler
    {
        protected override int GetPriority()
        {
            return 0;
        }

        public static bool showOverlay()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.ShowIcons;
        }

        protected override bool CanShowOverlay(string targetFile, FILE_ATTRIBUTE attributes)
        {
            if (!showOverlay() || !P4EXPProgram.connectionSuccess)
            {
                return false;
            }

            try
            {
                // TODO: need to check archive too?
                if (attributes.HasFlag(FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY))
                {
                    // ignoring directories for now (like current p4exp)
                    return false;
                }
            }
            catch (Exception)
            {
                // likely a dvd drive that causes the failure, so just return
                //FileLogger.LogException("Check for display of checked-out icon", ex);
                return false;
            }

            FileMetaData fmd = null;

            try
            {
                P4EXPProgram.Connect(targetFile);
                fmd = P4EXPProgram.GetFileMetaData(targetFile);
            }
            catch (P4Exception ex)
            {
                if (P4EXPProgram.IsLoginException(ex.ErrorCode))
                {
                    // login and try again
                    P4EXPProgram.Login(targetFile);
                    fmd = P4EXPProgram.GetFileMetaData(targetFile);
                }
                else if (P4EXPProgram.showP4Exception())
                {
                    string message = ex.Message + "\n" +
                        ex.StackTrace + "\n" +
                         ex.TargetSite.Name;
                    Message dlg = new Message(Properties.Resources.MessageDlg_P4Exception, message);
                    dlg.ShowDialog();
                }
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
                //FileLogger.LogException("Check for display of checked-out icon", ex);
            }

            if (fmd != null)
            {
                return CheckAction(fmd);
            }
            return false;
        }

        protected abstract bool CheckAction(FileMetaData fmd);
    }
}
