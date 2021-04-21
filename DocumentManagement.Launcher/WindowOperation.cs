using System;
using System.Runtime.InteropServices;

namespace MRS.DocumentManagement.Launcher
{

    public static class WindowOperation
    {
        public static void Hide(IntPtr win)
        {
            ShowWindow(win, 0);
        }

        public static void Show(IntPtr win)
        {
            ShowWindow(win, 1);
        }

        // Link: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
    }
}
