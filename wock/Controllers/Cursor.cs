using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace wock.Controllers
{
    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        /// <summary>
        /// Facilitates conversion to Point
        /// </summary>
        /// <param name="point"></param>
        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public class Cursor
    {
        
        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumProc"></param>
        /// <param name="lParam">value to be passed to the callback function</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);


    }
}