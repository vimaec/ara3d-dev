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