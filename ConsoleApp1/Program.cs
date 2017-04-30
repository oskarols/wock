using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wock.Controllers;
using System.Drawing;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var point = Cursor.GetCursorPosition();
            Console.WriteLine("X: {0}, Y: {1}", point.X, point.Y);

            Cursor.SetCursorPos(200, 200);

            // var window = new wock.Controllers.Windows();
            wock.Controllers.Windows.EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                var size = Window.GetWindowTextLength(wnd);
                
                if (size > 0)
                {
                    var builder = new StringBuilder(size + 1); // why + 1?
                    Window.GetWindowText(wnd, builder, builder.Capacity); // wut?
                    var isVisible = Window.IsWindowVisible(wnd);
                    if (isVisible)
                    {
                        Console.WriteLine(String.Format("Window size: {0}", size));
                        Console.WriteLine(builder.ToString());
                    }
                }
                return true;
                
            }, IntPtr.Zero); // wtf does this do?

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
