
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class UtilityLayerPlugin : IUtilityPlugin
    {
        public void Evaluate()
        {
            Core.WriteLine("Hello world");
        }
    }
}
