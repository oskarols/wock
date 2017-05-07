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
        Dictionary<string, Point> windowCursorPositions = new Dictionary<string, Point>();

        public Screen[] screens;
        public Screen primaryScreen;

        public Form1()
        {
            InitializeComponent();

            this.screens = Screen.AllScreens;
            this.primaryScreen = Screen.PrimaryScreen;

            // VS
            gotoVisualStudio.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_visualstudio_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            gotoVisualStudio.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.D5);

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


            /// FULLSCREEN 
            fullscreenHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_fullscreen_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            fullscreenHook.RegisterHotKey(
                wock.Models.ModifierKeys.Control | wock.Models.ModifierKeys.Alt,
                Keys.F);
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

        public bool saveCurrentState ()
        {
            var hwnd = PInvoke.User32.GetForegroundWindow();
            var className = PInvoke.User32.GetClassName(hwnd);
            if (className == null) { return false; };

            // TODO: pretty up the transformations
            var rawCursorPoint = PInvoke.User32.GetCursorPos();
            var cursorPoint = new Point(rawCursorPoint.x, rawCursorPoint.y);
            
            this.windowCursorPositions[className] = cursorPoint;

            return true;
        }

        public void restoreState (IntPtr hwnd)
        {
            var className = PInvoke.User32.GetClassName(hwnd);
            if (className == null) { return; };
            if(this.windowCursorPositions.ContainsKey(className))
            {
                var cursorPoint = this.windowCursorPositions[className];
                PInvoke.User32.SetCursorPos(cursorPoint.X, cursorPoint.Y);
            }
        }

        public void hook_fullscreen_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // get current window
            var hwnd = PInvoke.User32.GetForegroundWindow();
            if (hwnd != null)
            {
                // use WorkingArea since it will not include the task bar
                PInvoke.User32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, primaryScreen.WorkingArea.Width, primaryScreen.WorkingArea.Height, 0);
            }
        }

        public void hook_gotoChrome_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // TODO:
            // start process if not running
            
            this.saveCurrentState();
            var hwnd = FindWindowMatch("Google Chrome");
            // maximize if minimized
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
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
            }
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
