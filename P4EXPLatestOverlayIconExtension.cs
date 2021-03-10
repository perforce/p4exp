using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;
using Perforce.P4;
using System.Drawing;

namespace P4EXP
{
    [ComVisible(true)]

    public class HelixLatestRevOverlayIconExtension : OverlayHandler
    {
        protected override bool CheckAction(FileMetaData fmd)
        {
            return (fmd != null && fmd.HaveRev == fmd.HeadRev && fmd.Action == FileAction.None);
        }

        protected override Icon GetOverlayIcon()
        {
            return Properties.Resources.latest_rev;
        }
    }
}