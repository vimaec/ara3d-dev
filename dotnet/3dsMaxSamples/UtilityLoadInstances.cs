using System;

namespace Ara3D
{
    public class UtilityLoadInstances : IUtilityPlugin
    {
        public void Evaluate()
        {
            //var folder = @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-10_12-52-27-rac_basic_sample_project";
            var folder = @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-10_23-42-47-main";
            API.ConsoleToMAXScriptListener();
            var logger = new StdLogger();
            var document = API.LoadDocument(folder, logger);

            API.LoadScene(document);
        }
    }
}