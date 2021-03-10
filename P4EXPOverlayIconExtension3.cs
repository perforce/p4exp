using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using LogicNP.EZShellExtensions;
using Perforce.P4;

namespace P4EXP
{
    [Guid("A687EFDD-5C65-442C-B3E2-9841BB7C63E0"), ComVisible(true)]
    public class P4EXPOverlayIconExtension3 : OverlayHandler
    {
        public P4EXPOverlayIconExtension3()
        {
            m_icon = P4EXPProgram.AssemblyDirectory + @"\stale_rev.ico";
        }

        protected override bool CheckAction(FileMetaData fmd)
        {
            return fmd != null && fmd.HaveRev != fmd.HeadRev && fmd.Action == FileAction.None;
        }

        [ComRegisterFunction]
        public static void Register(System.Type t)
        {
            OverlayIconExtension.RegisterExtension(typeof(P4EXPOverlayIconExtension3));
        }

        [ComUnregisterFunction]
        public static void UnRegister(System.Type t)
        {
            OverlayIconExtension.UnRegisterExtension(typeof(P4EXPOverlayIconExtension3));
        }
    }
}