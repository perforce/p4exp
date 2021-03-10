using System;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using LogicNP.EZShellExtensions;
using Perforce.P4;
namespace P4EXP
{
    [Guid("F511DE12-0209-44E8-87BF-7BA899198356"), ComVisible(true)]
    [TargetExtension(".txt", true)]
    //[TargetExtension(".bmp", true)]
    //[TargetExtension(".dll", true)]
    //[TargetExtension(".exe", true)]
    //[TargetExtension(".vsdx", true)]
    //[TargetExtension(".docx", true)]
    //[TargetExtension(".png", true)]
    //[TargetExtension(".jpg", true)]
    //[TargetExtension(".sln", true)]
    //[TargetExtension(".rtf", true)]


    public class P4EXPColumnExtension : PropertyHandler
    {
        public P4EXPColumnExtension()
        {
        }

        public static bool checkServer()
        {
            Properties.Settings mySettings = new Properties.Settings();
            return mySettings.CheckServer;
        }

        protected override Property[] GetProperties()
        {
            // Defines 3 properties for a file under Perforce control:
            // File State: shows whether a file is checked out and its current status.
            // Action: action taking place on that file.
            //         For instance, edit, add, branch, etc.
            // Rev: revision that is currently in your workspace. 

            UserDefinedProperty prop = new UserDefinedProperty("P4File.State");
            prop.Description = "shows whether a file is checked out";
            prop.LabelText = "File State";

            UserDefinedProperty prop1 = new UserDefinedProperty("P4File.Action");
            prop1.Description = "shows action taking place on that file";
            prop1.LabelText = "Action";

            UserDefinedProperty prop2 = new UserDefinedProperty("P4File.Rev");
            prop2.Description = "shows revision that is currently in the workspace";
            prop2.LabelText = "Rev";

            return new Property[] { prop, prop1, prop2 };
        }

        void ProcessFile()
        {
            try
            {
                StreamReader tr = System.IO.File.OpenText(TargetFile);
                numLines = 0;
                numWords = 0;
                string line = null;
                while ((line = tr.ReadLine()) != null)
                {
                    numLines++;
                    string[] words = line.Split(' ');
                    numWords += words.Length;
                }

                // IMPORTANT : Close stream
                tr.Close();
            }
            catch (System.Exception e)
            {
                numLines = 0;
                numWords = 0;
            }
        }

        int numLines = -1;
        int numWords = -1;

        protected override object GetPropertyValue(Property property)
        {
            if (!(checkServer()))
            {
                return null;
            }
            if (!P4EXPProgram.connected)
            {
                P4EXPProgram.Connect();
            }

            if (TargetFile != null)
            {
                try
                {
                    IList<FileMetaData> fmd =
                    P4EXPProgram.rep.GetFileMetaData(null,
                    new FileSpec(null, null, new LocalPath(TargetFile), null));

                    if (property.CanonicalName == "P4File.State")
                    {
                        if (fmd != null && fmd[0] != null)
                        {
                            if (fmd[0].Action != FileAction.None &&
                                fmd[0].HaveRev == fmd[0].HeadRev)
                            {
                                if (fmd[0].Action == FileAction.Add)
                                {
                                    return "Marked for add";
                                }
                                else
                                {
                                    return "Checked out";
                                }
                            }
                            else if (fmd[0].HaveRev == fmd[0].HeadRev)
                            {
                                return "Up-to-date";
                            }
                            else
                            {
                                return "Needs Update";
                            }

                        }
                        return "Not on Server";
                    }

                    if (property.CanonicalName == "P4File.Action")
                    {
                        if (fmd != null && fmd[0] != null && fmd[0].Action != FileAction.None)
                        {
                            return fmd[0].Action.ToString();
                        }
                        return null;
                    }

                    if (property.CanonicalName == "P4File.Rev")
                    {
                        if (fmd != null && fmd[0] != null)
                        {
                            if (fmd[0].HaveRev != -1 && fmd[0].HeadRev != -1)
                                return "#" + fmd[0].HaveRev + "/" + fmd[0].HeadRev;
                        }
                        return null;
                    }
                }
                catch (P4Exception ex)
                {
                    if (P4EXPProgram.showP4Exception())
                    {
                        string message = ex.Message + "\n" +
                            ex.StackTrace + "\n" +
                             ex.TargetSite.Name;
                        Message dlg = new Message("P4Exception", message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException("Get column value", ex);
                }
                catch (Exception ex)
                {
                    if (P4EXPProgram.showException())
                    {
                        string message = ex.Message + "\n" +
                            ex.StackTrace + "\n" +
                            ex.TargetSite.Name;
                        Message dlg = new Message("Exception", message);
                        dlg.ShowDialog();
                    }
                    FileLogger.LogException("Get column value", ex);
                }
            }
            

            return null;
        }

        [ComRegisterFunction]
        public static void Register(System.Type t)
        {
            PropertyHandler.RegisterExtension(typeof(P4EXPColumnExtension));
        }

        [ComUnregisterFunction]
        public static void UnRegister(System.Type t)
        {
            PropertyHandler.UnRegisterExtension(typeof(P4EXPColumnExtension));
        }

    }
}