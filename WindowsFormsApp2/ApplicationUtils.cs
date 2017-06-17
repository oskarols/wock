using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    static class ApplicationUtils
    {
        public static int StartApplication(string APP_FILE_PATH)
        {
            var startupInfo = PInvoke.Kernel32.STARTUPINFO.Create();
            var processInfo = new PInvoke.Kernel32.PROCESS_INFORMATION();

            PInvoke.Kernel32.CreateProcess(
                APP_FILE_PATH,
                null,
                null,
                null,
                false, 
                PInvoke.Kernel32.CreateProcessFlags.None, 
                IntPtr.Zero,
                null, 
                ref startupInfo, 
                out processInfo
             );
             
            return 0;
        }

    }
}
