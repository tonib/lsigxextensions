using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{
    /// <summary>
    /// Tool to enumerate window handlers from a native window
    /// </summary>
    /// <remarks>
    /// See:
    /// http://gabay.myds.me/svn/filedetails.php?repname=Niv&path=%2FTestApi%2Fuser32.cs
    /// http://bytes.com/topic/net/answers/855274-getting-child-windows-using-process-getprocess
    /// </remarks>
    public class NativeWindowEnumerator
    {
        // Windows styles
        private const int WS_EX_MDICHILD = 0x00000040;
        private const int WS_EX_WINDOWEDGE = 0x00000100;
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        private const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        private const int WS_EX_APPWINDOW = 0x00040000;

        private const int GWL_EXSTYLE = (-20);

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("USER32.DLL")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int nIndex);


        private static int GetWindowStyleEx(IntPtr handle)
        {
            return GetWindowLong(handle, GWL_EXSTYLE);
        } 

        /// <summary>
        /// Process to inspect
        /// </summary>
        private Process Process;

        private Dictionary<IntPtr, string> DictWindows;

        private IntPtr HShellWindow;

        public NativeWindowEnumerator(Process process)
        {
            Process = process;
            HShellWindow = GetShellWindow();
        }

        private bool IsInterestingWindow(IntPtr hWnd)
        {
            int style = GetWindowStyleEx(hWnd);
            if ((style & WS_EX_APPWINDOW) == WS_EX_APPWINDOW)
                return true;
            if ((style & WS_EX_MDICHILD) == WS_EX_MDICHILD)
                return true;
            return false;
        }

        private bool CheckHWnd(IntPtr hWnd)
        {
            if (hWnd == HShellWindow)
                return false;
            if (!IsWindowVisible(hWnd))
                return false;

            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return false;

            uint windowPid;
            GetWindowThreadProcessId(hWnd, out windowPid);
            if (windowPid != Process.Id)
                return false;

            // Check style:
            if (!IsInterestingWindow(hWnd))
                return false;

            StringBuilder stringBuilder = new StringBuilder(length);
            GetWindowText(hWnd, stringBuilder, length + 1);
            if (!DictWindows.ContainsKey(hWnd))
            {
                DictWindows.Add(hWnd, stringBuilder.ToString());
                return true;
            }
            return false;
        }

        private bool CheckWindow(IntPtr hWnd, int lParam)
        {
            if( CheckHWnd(hWnd) )
                // Do a recursive search
                EnumChildWindows(hWnd, CheckWindow, 0);
            return true;
        }

        private bool CheckTopWindow(IntPtr hWnd, int lParam)
        {
            if( CheckHWnd(hWnd) )
                EnumChildWindows(hWnd, CheckWindow, 0);
            return true;
        }

        public Dictionary<IntPtr, string> GetProcessWindows()
        {
            DictWindows = new Dictionary<IntPtr, string>();
            EnumWindows(CheckTopWindow, 0);
            return DictWindows;
        }
    }
}
