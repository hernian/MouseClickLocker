using System;
using System.Runtime.InteropServices;

namespace MouseClickLocker
{
    internal static class MouseHookLib
    {
        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void Initialize();

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void EnableClickLock(bool enable);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void SetClickLockDelayMS(int delayMs);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void SetMarkerOffset(int xOffset, int yOffset);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void SetMarkerPreview(bool markerPreview);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void SetMouseHook(IntPtr hWndMarker);

        [DllImport("MouseHookLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern void UnsetMouseHook();

        public const int WM_NOTIFY_LOCK_STATE = Win32Api.WM_USER + 1;
        public const int LOCKTYPE_LEFT = 1;
        public const int LOCKTYPE_RIGHT = 2;
        public enum LockState
        {
            DEACTIVATED = 0,
            ACTIVATED = 1,
            SUPPRESSED = 2
        }
    }
}
