using System;
using System.Runtime.InteropServices;

namespace MouseClickLocker
{
    internal static class MouseHookLib
    {
        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern bool SetMouseHook(IntPtr hWnd, IntPtr hWndMarker);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void UnsetMouseHook();

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void ActivateClickLock(bool clickLock);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetLastMousePos();

        public const int WM_NOTIFY_LOCK_STATE = Win32Api.WM_USER + 1;
        public const int LOCK_STATE_LEFT = 1;
        public const int LOCK_STATE_RIGHT = 2;
    }
}
