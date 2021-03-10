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
    [Guid("AFEF2F61-DBD3-42F1-944C-0E79A3A07355"), ComVisible(true)]
    public class P4EXPOverlayIconExtension4 : OverlayHandler
    {
        public P4EXPOverlayIconExtension4()
        {
            m_icon = P4EXPProgram.AssemblyDirectory + @"\add.ico";
        }

        protected override bool CheckAction(FileMetaData fmd)
        {
            return fmd != null && fmd.Action == FileAction.Add;
        }

        [ComRegisterFunction]
        public static void Register(System.Type t)
        {
            OverlayIconExtension.RegisterExtension(typeof(P4EXPOverlayIconExtension4));
        }

        [ComUnregisterFunction]
        public static void UnRegister(System.Type t)
        {
            OverlayIconExtension.UnRegisterExtension(typeof(P4EXPOverlayIconExtension4));
        }
    }
}