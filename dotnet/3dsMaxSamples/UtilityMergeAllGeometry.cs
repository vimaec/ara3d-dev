using Autodesk.Max;
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class UtilityMergeAllGeometry : IUtilityPlugin
    {
        public void Evaluate()
        {
            API.AllGeometry.ToNode();
        }
    }
}