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

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(ref CURSORINFO pci);

        [DllImport("user32.dll")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        public static Bitmap? GetCursorBitmap()
        {
            var ci = new CURSORINFO();
            ci.cbSize = Marshal.SizeOf(ci);
            if (!GetCursorInfo(ref ci))
            {
                return null;
            }
            // hCursorをIconに変換
            using var icon = Icon.FromHandle(ci.hCursor);
            // IconからBitmapを取得
            var bitmap = icon.ToBitmap();
            // リソース解放
            DestroyIcon(icon.Handle);
            return bitmap;
        }
    }
}
