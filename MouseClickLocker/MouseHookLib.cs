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
    }
}
