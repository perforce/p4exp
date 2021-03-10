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

    public class HelixAddOverlayIconExtension : OverlayHandler
    {
        protected override bool CheckAction(FileMetaData fmd)
        {
            return fmd != null && fmd.Action == FileAction.Add;
        }
        protected override Icon GetOverlayIcon()
        {
            return Properties.Resources.add;
        }
    }
}