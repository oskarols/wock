using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Text;

namespace wock.Controllers
{
    public class Windows
    {
        public Windows()
        {

        }

        public List<Window> windows;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumProc"></param>
        /// <param name="lParam">value to be passed to the callback function</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);        


    }

    /// <summary>
    /// Delegate to filter which windows to include
    /// Not done inline so we can have it be reusable.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="lParam"></param>
    /// <returns>to continue enumeration, return TRUE. to stop enumeration return false.</returns>
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
}