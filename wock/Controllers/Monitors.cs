using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace wock.Controllers
{
    public class Monitors
    {
        public struct RECT
        {
            public long left;
            public long top;
            public long right;
            public long bottm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumProc"></param>
        /// <param name="lParam">value to be passed to the callback function</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, RECT lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);


    }
}