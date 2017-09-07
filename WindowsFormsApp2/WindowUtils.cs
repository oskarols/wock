using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wock.Models;
using System.IO;

namespace WindowsFormsApp2
{
    public struct WindowInfo {
        public string fileName;
        public string windowText;
        public string className;
        public bool isVisible;
    }



    public class WindowUtils {
        Dictionary<IntPtr, Point> windowCursorPositions = new Dictionary<IntPtr, Point>();

        // <filename, hwnd>
        // e.g. <"code.exe", Pointer>
        Dictionary<string, IntPtr> lastActivatedHwndForWindow = new Dictionary<string, IntPtr>();

        public WindowInfo GetWindowInfo(IntPtr hwnd)
        {
            // TODO: This is throwing exceptions like crazy
            try
            {
                var filename = getFilenameForHwnd(hwnd);
                var isVisible = PInvoke.User32.IsWindowVisible(hwnd);
                var className = PInvoke.User32.GetClassName(hwnd);
                var windowText = PInvoke.User32.GetWindowText(hwnd);

                return new WindowInfo
                {
                    fileName = filename,
                    isVisible = isVisible,
                    className = className,
                    windowText = windowText
                };
            }
            catch (Exception e)
            {
                var error = PInvoke.Kernel32.GetLastError();
                return new WindowInfo
                {
                    fileName = "",
                    isVisible = false,
                    className = "",
                    windowText = ""
                };
            }
        }

        /// <summary>
        /// Version of GetWindowInfo where we try to minimize Win32 Exceptions
        /// by skipping PInvoke operations if the hwnd looks sketchy
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public WindowInfo SafeGetWindowInfo(IntPtr hwnd)
        {
            var windowInfo = new WindowInfo();

            windowInfo.isVisible = PInvoke.User32.IsWindowVisible(hwnd);
            // Assume that windows that set isVisible to false are not what
            // we're looking for
            if (windowInfo.isVisible == false) return windowInfo;

            windowInfo.className = PInvoke.User32.GetClassName(hwnd);
            // Assume that without a className its some weird internal
            // window used by the OS
            if (windowInfo.className.Length == 0) return windowInfo;

            windowInfo.fileName = getFilenameForHwnd(hwnd);

            var windowTextLength = PInvoke.User32.GetWindowTextLength(hwnd);
            if (windowTextLength <= 0) return windowInfo;

            // Win32 Exception "Handle is Invalid" when using e.g. "Explorer.EXE"
            windowInfo.windowText = PInvoke.User32.GetWindowText(hwnd);
            return windowInfo;
        }

        // TODO: clean up cycling, split into separate method
        public IntPtr GetHwndForApplication(
            Func<WindowInfo, bool> applicationFinder,
            string APP_IDENTIFIER
        ) {
            var currentlyActiveWindow = PInvoke.User32.GetForegroundWindow();

            // when one toggles between applications too fast sometimes the hwnd
            // sent back will be null, this guards against this
            if (currentlyActiveWindow == IntPtr.Zero) return IntPtr.Zero;

            var currentlyActiveWindowInfo = GetWindowInfo(currentlyActiveWindow);

            // if last pressed was the same "type" as the one being toggled to
            // we should cycle to another window of the same type.
            if (applicationFinder(currentlyActiveWindowInfo))
            {
                // TODO: Break this part out
                this.lastActivatedHwndForWindow[APP_IDENTIFIER] = currentlyActiveWindow;
                var relatedWindows = this.FindWindowsMatch(applicationFinder);

                if (relatedWindows.Count < 1) return IntPtr.Zero;

                var index = relatedWindows.FindIndex((ptr) => ptr == currentlyActiveWindow);
                var isAtEndOfList = index == relatedWindows.Count - 1;
                if (isAtEndOfList)
                {
                    // If at the end, loop around to first item
                    return relatedWindows[0];
                }
                // otherwise just grab the next one
                return relatedWindows[index + 1];
            }
            else
            {
                return FindWindowMatch(applicationFinder);
            }
        }

        public string getFilenameForHwnd(IntPtr hwnd)
        {
            int processID;
            PInvoke.User32.GetWindowThreadProcessId(hwnd, out processID);
            var process = System.Diagnostics.Process.GetProcessById(processID);
            string filename = null;

            // Win32Exception Access Denied
            try
            {
                filename = process.MainModule.FileName;
            }
            catch (Exception e)
            {
                return null;
            }

            if (filename == null) return null;
            return Path.GetFileName(filename);
        }

        public IntPtr FindWindowMatch(Func<WindowInfo, bool> applicationFinder)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            PInvoke.User32.EnumWindows((IntPtr hwnd, IntPtr param) =>
            {
                var windowInfo = SafeGetWindowInfo(hwnd);

                if (applicationFinder(windowInfo))
                {
                    foundHwnd = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;
        }

        public List<IntPtr> FindWindowsMatch(Func<WindowInfo, bool> applicationFinder)
        {
            var foundHwnds = new List<IntPtr>();
            PInvoke.User32.EnumWindows((IntPtr hwnd, IntPtr param) =>
            {
                var windowInfo = SafeGetWindowInfo(hwnd);

                if (applicationFinder(windowInfo))
                {
                    foundHwnds.Add(hwnd);
                }
                return true;
            }, IntPtr.Zero);

            // since otherwise the order is quite inconsistent
            // and we won't be able to rely on order
            foundHwnds.Sort((x, y) => x.ToInt32().CompareTo(y.ToInt32()));

            return foundHwnds;
        }

        public bool saveCurrentState()
        {
            var hwnd = PInvoke.User32.GetForegroundWindow();
            //var className = PInvoke.User32.GetClassName(hwnd);
            //if (className == null) { return false; };

            // TODO: pretty up the transformations
            var rawCursorPoint = PInvoke.User32.GetCursorPos();
            var cursorPoint = new Point(rawCursorPoint.x, rawCursorPoint.y);

            this.windowCursorPositions[hwnd] = cursorPoint;

            return true;
        }

        public void restoreState(IntPtr hwnd)
        {
            //var className = PInvoke.User32.GetClassName(hwnd);
            //if (className == null) { return; };
            if (this.windowCursorPositions.ContainsKey(hwnd))
            {
                var cursorPoint = this.windowCursorPositions[hwnd];
                PInvoke.User32.SetCursorPos(cursorPoint.X, cursorPoint.Y);
            }
        }

        public void EnsureCursorWithinWindow(IntPtr hwnd)
        {
            var currentCursorPos = PInvoke.User32.GetCursorPos();
            var windowRect = new PInvoke.RECT();
            PInvoke.User32.GetWindowRect(hwnd, out windowRect);
            System.Diagnostics.Debug.WriteLine(String.Format("x: {0}, y: {1}", currentCursorPos.x, currentCursorPos.y));
            System.Diagnostics.Debug.WriteLine(String.Format("t: {0}, l: {1}, b: {2}, r: {3}", windowRect.top, windowRect.left, windowRect.bottom, windowRect.right));
            System.Diagnostics.Debug.WriteLine(IsPointWithinWindow(currentCursorPos, windowRect).ToString());
            System.Diagnostics.Debug.WriteLine("\n");
            if (!IsPointWithinWindow(currentCursorPos, windowRect))
            {
                CenterCursorOn(hwnd);
            }

        }

        public bool IsPointWithinWindow(PInvoke.POINT point, PInvoke.RECT rect)
        {
            return point.x > rect.left && point.x < rect.right &&
                   point.y < rect.bottom && point.y > rect.top;
        }

        public void CenterCursorOn(IntPtr hwnd)
        {
            var windowRect = new PInvoke.RECT();
            PInvoke.User32.GetWindowRect(hwnd, out windowRect);
            var height = windowRect.bottom - windowRect.top;
            var width = windowRect.right - windowRect.left;

            PInvoke.User32.SetCursorPos(
                windowRect.left + (width / 2),
                windowRect.top + (height / 2)
            );
        }


    }
}
