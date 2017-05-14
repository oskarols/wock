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
    public class HotkeyHandlers
    {
        public WindowUtils utils;

        public HotkeyHandlers()
        {
            utils = new WindowUtils();
        }

        public void hook_visualstudio_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler("Microsoft Visual Studio");
        }

        public void hook_gotoCmder_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler("cmd");
        }

        public void hook_gotoChrome_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler("Google Chrome");
        }

        public void hook_gotoVisualStudioCode_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            genericApplicationToggler("Visual Studio Code");
        }

        public void genericApplicationToggler(string APP_NAME)
        {
            utils.saveCurrentState(); // todo: this has to be _before_ getHWNDForApp..
            // why is that?

            var hwnd = utils.GetHwndForApplication(APP_NAME);
            // TODO:
            // start process if not running

            // maximize if minimized
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
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
