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
    public class WindowUtils
    {

        Dictionary<IntPtr, Point> windowCursorPositions = new Dictionary<IntPtr, Point>();

        // <filename, hwnd>
        // e.g. <"code.exe", Pointer>
        Dictionary<string, IntPtr> lastActivatedHwndForWindow = new Dictionary<string, IntPtr>();

        // TODO: clean up cycling, split into separate method
        public IntPtr GetHwndForApplication(string APP_FILE_NAME)
        {
            var currenthwnd = PInvoke.User32.GetForegroundWindow();
            var hwndWindowText = PInvoke.User32.GetWindowText(currenthwnd);
            var currentlyActiveApplicationFilename = this.getFilenameForHwnd(currenthwnd);
            IntPtr hwnd = IntPtr.Zero;

            // if last pressed was the same "type" as the one being toggled to
            // we should cycle to another window of the same type.
            if (currentlyActiveApplicationFilename.ToLower().Contains(APP_FILE_NAME.ToLower()))
            {
                this.lastActivatedHwndForWindow[APP_FILE_NAME] = currenthwnd;
                var relatedWindows = this.FindWindowsMatch(APP_FILE_NAME);
                relatedWindows.Sort((x, y) => x.ToInt32().CompareTo(y.ToInt32()));
                if (relatedWindows.Count > 1)
                {
                    var index = relatedWindows.FindIndex((ptr) => ptr == currenthwnd);
                    var isAtEndOfList = index == relatedWindows.Count - 1;
                    if (isAtEndOfList)
                    {
                        return relatedWindows[0];
                    }
                    return relatedWindows[index + 1];
                }
            }
            else
            {
                return FindWindowMatch(APP_FILE_NAME);
            }
            return hwnd;
        }

        public string getFilenameForHwnd(IntPtr hwnd)
        {
            int processID;
            PInvoke.User32.GetWindowThreadProcessId(hwnd, out processID);
            var process = System.Diagnostics.Process.GetProcessById(processID);
            var filename = process.MainModule.FileName;
            if (filename == null) return null;
            return Path.GetFileName(filename);
        }

        public IntPtr FindWindowMatch(string searchStr)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            PInvoke.User32.EnumWindows((IntPtr hwnd, IntPtr param) =>
            {
                var filename = this.getFilenameForHwnd(hwnd);
                var windowTextLength = PInvoke.User32.GetWindowTextLength(hwnd);
                var hasWindowText = windowTextLength > 0; 
                var isVisible = PInvoke.User32.IsWindowVisible(hwnd);
                if (filename != null && filename.Length > 0 && isVisible && hasWindowText)
                {
                    //var windowText = PInvoke.User32.GetWindowText(hwnd);
                    var hasMatch = filename.ToLower().Contains(searchStr.ToLower());
                    if (hasMatch)
                    {
                        foundHwnd = hwnd;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;
        }

        public List<IntPtr> FindWindowsMatch(string searchStr)
        {
            var foundHwnds = new List<IntPtr>();
            PInvoke.User32.EnumWindows((IntPtr hwnd, IntPtr param) =>
            {
                var windowTextLength = PInvoke.User32.GetWindowTextLength(hwnd);
                var hasWindowText = windowTextLength > 0;
                var filename = this.getFilenameForHwnd(hwnd);
                var isVisible = PInvoke.User32.IsWindowVisible(hwnd);
                if (filename != null && filename.Length > 0 && hasWindowText && isVisible)
                {
                    //var windowText = PInvoke.User32.GetWindowText(hwnd);
                    var hasMatch = filename.ToLower().Contains(searchStr.ToLower());
                    if (hasMatch)
                    {
                        foundHwnds.Add(hwnd);
                    }
                }

                return true;
            }, IntPtr.Zero);

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
