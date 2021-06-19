using System;
using System.Runtime.InteropServices;

namespace Pinpoint.Win.Native
{
    public class User32
    {
        [DllImport("user32")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    }
}