
using Autodesk.Max;
using Autodesk.Max.MaxPlus;
using System.Text;

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
