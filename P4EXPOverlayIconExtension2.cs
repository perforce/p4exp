using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Perforce.P4;

namespace P4EXP
{
    [Guid("754FFD0B-2D90-4B4A-AEA7-F2069E1DE5BE"), ComVisible(true)]
    public class P4EXPOverlayIconExtension2 : OverlayHandler
    {
        public P4EXPOverlayIconExtension2()
        {
            m_icon = P4EXPProgram.AssemblyDirectory + @"\checked-out.ico";
        }

        protected override bool CheckAction(FileMetaData fmd)
        {
            return (fmd.Action != FileAction.None && fmd.Action != FileAction.Add);
        }

        [ComRegisterFunction]
        public static void Register(System.Type t)
        {
            OverlayHandler.RegisterExtension(typeof(P4EXPOverlayIconExtension2));
        }

        [ComUnregisterFunction]
        public static void UnRegister(System.Type t)
        {
            OverlayHandler.UnRegisterExtension(typeof(P4EXPOverlayIconExtension2));
        }
    }
}