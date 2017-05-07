using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wock.Controllers;
using System.Drawing;
using System.ServiceModel;
using wock.Models;
using System.Runtime.InteropServices;
using PInvoke;
using System.Windows.Forms;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var point = wock.Controllers.Cursor.GetCursorPosition();
            Console.WriteLine("X: {0}, Y: {1}", point.X, point.Y);

            wock.Controllers.Cursor.SetCursorPos(200, 200);

            // var window = new wock.Controllers.Windows();
            EnumWindows();

            /*
            var monEx = new User32.MONITORINFOEX();
            monEx.cbSize = Marshal.SizeOf(monEx);
            //User32.GetMonitorInfoEx();

            
            unsafe { 
                User32.EnumDisplayMonitors(IntPtr.Zero, null, (IntPtr hMonitor, IntPtr hdcMonitor, RECT* lprcMonitor, void* dwData) => {
                    var moninfo = new Monitors32.MONITORINFO();
                    moninfo.cbSize = Marshal.SizeOf(moninfo); // wut?
                    Monitors32.GetMonitorInfo(hMonitor, ref moninfo);
                    return true;
                }, null);
            }
            */
            var screens = System.Windows.Forms.Screen.AllScreens;
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            var spotifyHWND = PInvoke.User32.FindWindow(null, "Spotify");
            if (spotifyHWND != IntPtr.Zero)
            {
                //User32.SetWindowPos()
                PInvoke.User32.MoveWindow(spotifyHWND, 0, 0, primaryScreen.Bounds.Width / 2, primaryScreen.Bounds.Height, false);
            }


            Monitors32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Monitors32.RECT lprcMonitor, IntPtr dwData) =>
            {
                var moninfo = new Monitors32.MONITORINFO();
                moninfo.cbSize = Marshal.SizeOf(moninfo); // wut?
                Monitors32.GetMonitorInfo(hMonitor, ref moninfo);

                return true;
            }, IntPtr.Zero);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static public void ResizeSpotify()
        {
            var spotifyHWND = PInvoke.User32.FindWindow(null, "Spotify");
            if (spotifyHWND != IntPtr.Zero)
            {
                PInvoke.User32.MoveWindow(spotifyHWND, 200, 200, 200, 200, false);
            }
        }

        static public void EnumWindows()
        {
            Windows32.EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                var size = Window32.GetWindowTextLength(wnd);

                if (size > 0)
                {
                    var builder = new StringBuilder(size + 1); // why + 1?
                    Window32.GetWindowText(wnd, builder, builder.Capacity); // wut?
                    var isVisible = Window32.IsWindowVisible(wnd);
                    if (isVisible)
                    {
                        var sb = new StringBuilder(256); // max class length
                        var className = PInvoke.User32.GetClassName(wnd);
                        Console.WriteLine(builder.ToString());
                        Console.WriteLine(String.Format("ClassName: {0}\n\n", className));
                        //PInvoke.User32.MoveWindow(wnd, -100, 100, 300, 300, false);
                    }
                }
                return true;

            }, IntPtr.Zero); // wtf does this do?
        }



        [OperationContract(IsOneWay = true)]
        void NormalFunction() {
        }

    }
}
