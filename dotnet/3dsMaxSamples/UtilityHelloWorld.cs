
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class SamplePlugin : IUtilityPlugin
    {
        public void Evaluate()
        {
            Core.WriteLine("Hello world!");
        }
    }
}
