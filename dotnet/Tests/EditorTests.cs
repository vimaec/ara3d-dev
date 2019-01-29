using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D.Tests
{
    public static class EditorTests
    {
        public static void LaunchEditorProcess()
        {
            Console.WriteLine("Launching process ...");
            var process = API.ShowEditor();
            Util.OnShutdown(process.SafeClose);
            Console.WriteLine("Process is running ...");

            // TODO: I want to send more messages to the editor 
            // * Open a file 
            // * trigger a compile
            // * create a log 
            // * auto-save 
            // * be notified of changes             
            process.WaitForExit();

            Console.WriteLine("Process is completed ...");
            Console.WriteLine("Press any key to exit ...");
        }
    }
}
