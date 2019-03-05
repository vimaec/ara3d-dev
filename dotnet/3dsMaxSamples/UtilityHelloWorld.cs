
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class UtilityLayerPlugin : IUtilityPlugin
    {
        public void Evaluate()
        {
            var obj = Factory.CreateDummyObject();
            var node = Factory.CreateNode(obj, "My node");
            var layer = LayerManager.CreateLayer();
            layer.AddToLayer(node);
        }
    }
}
