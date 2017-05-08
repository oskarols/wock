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

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        KeyboardHook fullscreenHook = new KeyboardHook();
        KeyboardHook gotoVisualStudio = new KeyboardHook();
        KeyboardHook sendToNextScreenHook = new KeyboardHook();
        KeyboardHook gotoChrome = new KeyboardHook();
        KeyboardHook gotoSpotify = new KeyboardHook();
        KeyboardHook gotoCmder = new KeyboardHook();
        KeyboardHook gotoVisualStudioCode = new KeyboardHook();
        Dictionary<IntPtr, Point> windowCursorPositions = new Dictionary<IntPtr, Point>();
        Dictionary<string, IntPtr> lastActivatedHwndForWindow = new Dictionary<string, IntPtr>();

        public Screen[] screens;
        public Screen primaryScreen;

        public Form1()
        {
            InitializeComponent();

            this.screens = Screen.AllScreens;
            this.primaryScreen = Screen.PrimaryScreen;

            // NEXT SCREEN
            sendToNextScreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_nextWindow_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            sendToNextScreenHook.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.E);

            // CHROME
            gotoChrome.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_gotoChrome_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoChrome.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D1);

            // CMDER
            gotoCmder.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_gotoCmder_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoCmder.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D2);

            // VS
            gotoVisualStudio.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_visualstudio_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudio.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D3);


            /// FULLSCREEN 
            fullscreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_fullscreen_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            fullscreenHook.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.F);
        }


        public void hook_fullscreen_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            var hwnd = PInvoke.User32.GetForegroundWindow();
            if (hwnd == null) { return; }

            var currentScreen = Screen.FromHandle(hwnd);
            // use WorkingArea since it will not include the task bar
            PInvoke.User32.SetWindowPos(
                hwnd, 
                IntPtr.Zero, 
                currentScreen.WorkingArea.X, currentScreen.WorkingArea.Y, 
                currentScreen.WorkingArea.Width, currentScreen.WorkingArea.Height, 
                0
            );
        }

        public void hook_visualstudio_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            this.saveCurrentState();
            var hwnd = FindWindowMatch("Microsoft Visual Studio");
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
            this.restoreState(hwnd);
            // get current window
            PInvoke.User32.SetForegroundWindow(hwnd);
        }

        public void hook_gotoCmder_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            this.saveCurrentState();
            var hwnd = PInvoke.User32.FindWindow(null, "Cmder");
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
            this.restoreState(hwnd);
            // get current window
            PInvoke.User32.SetForegroundWindow(hwnd);
        }

        public void hook_gotoChrome_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            this.saveCurrentState(); // todo: this has to be _before_ getHWNDForApp..
            // why is that?

            var hwnd = getHWNDForApplication("Google Chrome");
            // TODO:
            // start process if not running
            
            // maximize if minimized
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
            this.restoreState(hwnd);

            this.EnsureCursorWithinWindow(hwnd);

            // get current window
            PInvoke.User32.SetForegroundWindow(hwnd);
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

        public void hook_nextWindow_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // bail if only single screen since no point
            if (screens.Length == 1)
            {
                return;
            }

            // get current window
            var hwnd = PInvoke.User32.GetForegroundWindow();
            var currentScreen = Screen.FromHandle(hwnd);
            var index = Array.IndexOf(screens, currentScreen);
            var endOfArray = screens.Length - 1;
            var isAtEndOfArray = index == endOfArray;
            var nextIndex = isAtEndOfArray ? 0 : index + 1;
            var nextWindow = screens[nextIndex];
            
            if (hwnd != null)
            {
                // use WorkingArea since it will not include the task bar
                PInvoke.User32.SetWindowPos(
                    hwnd, 
                    IntPtr.Zero,
                    nextWindow.Bounds.Location.X,
                    nextWindow.Bounds.Location.Y,
                    nextWindow.WorkingArea.Width,
                    nextWindow.WorkingArea.Height, 
                    0
                );
                EnsureCursorWithinWindow(hwnd);
            }
        }


        /// UTILS

        // TODO: clean up cycling, split into separate method
        public IntPtr getHWNDForApplication(string APP_NAME)
        {
            var currenthwnd = PInvoke.User32.GetForegroundWindow();
            var hwndWindowText = PInvoke.User32.GetWindowText(currenthwnd);
            IntPtr hwnd = IntPtr.Zero;

            if (hwndWindowText.ToLower().Contains(APP_NAME.ToLower()))
            {
                this.lastActivatedHwndForWindow[APP_NAME] = currenthwnd;
                var relatedWindows = this.FindWindowsMatch(APP_NAME);
                relatedWindows.Sort((x, y) => x.ToInt32().CompareTo(y.ToInt32()));
                if (relatedWindows.Count > 1)
                {
                    var index = relatedWindows.FindIndex((ptr) => ptr == currenthwnd);
                    var isAtEndOfList = index == relatedWindows.Count - 1;
                    if (isAtEndOfList)
                    {
                        return relatedWindows[0];
                    }
                    else
                    {
                        return relatedWindows[index + 1];
                    }
                }
            }
            else
            {
                return FindWindowMatch(APP_NAME);
            }
            return hwnd;
        }

        public IntPtr FindWindowMatch(string searchStr)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            PInvoke.User32.EnumWindows((IntPtr hwnd, IntPtr param) =>
            {
                var windowText = PInvoke.User32.GetWindowText(hwnd);
                var hasMatch = windowText.ToLower().Contains(searchStr.ToLower());
                if (hasMatch)
                {
                    foundHwnd = hwnd;
                    return false;
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
                var windowText = PInvoke.User32.GetWindowText(hwnd);
                var hasMatch = windowText.ToLower().Contains(searchStr.ToLower());
                if (hasMatch)
                {
                    foundHwnds.Add(hwnd);
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


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
