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
using System.Diagnostics;

namespace WindowsFormsApp2
{
    public class HotkeyHandlers
    {
        public WindowUtils utils;

        public HotkeyHandlers()
        {
            utils = new WindowUtils();
        }

        public void hook_visualstudio_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler((app) =>
            {
                return app.fileName == "devenv.exe" && 
                // need to do this since Visual Studio has lots of small windows
                // that have no windowText (which we're not interested in)
                app.windowText != null;
            }, "devenv");
        }

        public void hook_gotoCmder_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // not exe to be compatible with normal & 64 bit version
            genericApplicationToggler((app) => {
                return app.fileName != null && app.fileName.Contains("ConEmu");
            }, "ConEmu");
        }

        public void hook_gotoChrome_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler((app) =>
            {
                Debug.WriteLine(app.fileName);
                var hasMatch = app.fileName != null && app.fileName.ToLower() == "chrome.exe";
                return hasMatch;
            }
                , 
                "chrome.exe");
        }

        public void hook_gotoVisualStudioCode_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler((app) => app.fileName == "Code.exe", "code.exe");
        }

        public void hook_gotoExplorer_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler((app) => {
                return app.fileName != null && 
                    app.fileName.ToLower() == "explorer.exe" &&
                    // otherwise jumps to e.g. the startmenu and tray as well
                    // RE ExplorerWClass: https://blogs.msdn.microsoft.com/oldnewthing/20150619-00/?p=45341
                    (app.className == "CabinetWClass" || app.className == "Explorer­WClass");
            }, "explorer");
        }

        public void hook_gotoSpotify_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler((app) => {
                return app.fileName == "Spotify.exe" && app.className == "SpotifyMainWindow";
            }, 
            "Spotify");
        }

        // TODO
        // Problem: if window is minimized, it won't be shown due to find method
        // having to skip windows that are not visible (see issues with Cmder)
        public void genericApplicationToggler(
            Func<WindowInfo, bool> applicationFinder,
            string APP_IDENTIFIER
        )
        {
            utils.saveCurrentState(); // todo: this has to be _before_ getHWNDForApp..
            // why is that?

            var hwnd = utils.GetHwndForApplication(applicationFinder, APP_IDENTIFIER);
            // TODO:
            // start process if not running

            if (hwnd == IntPtr.Zero) return;

            // maximize if minimized

            // TODO: what are the enum values doing here? is there a better choice of method?
            // TODO: This has bugs that make the windows smaller in some cases
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_SHOWNORMAL);
            utils.restoreState(hwnd);

            utils.EnsureCursorWithinWindow(hwnd);

            // get current window
            PInvoke.User32.SetForegroundWindow(hwnd);
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

        public void hook_nextWindow_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            var screens = Screen.AllScreens;
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
                utils.EnsureCursorWithinWindow(hwnd);
            }
        }
    }
}
