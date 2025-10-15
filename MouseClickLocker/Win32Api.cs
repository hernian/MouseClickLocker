using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MouseClickLocker
{
    internal class Win32Api
    {
        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);

        [DllImport("shcore.dll")]
        public static extern int GetProcessDpiAwareness(IntPtr hprocess, out int awareness);
        public enum ProcessDpiAwareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);

        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(
            IntPtr hmonitor,
            int dpiType,
            out uint dpiX,
            out uint dpiY);

        public const int MDT_EFFECTIVE_DPI = 0;
        public const uint MONITOR_DEFAULTTONEAREST = 2;

        public const int WM_USER = 0x0400;

        public const int WM_NCHITTEST = 0x0084;
        public const int HTCAPTION = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

    }
}
