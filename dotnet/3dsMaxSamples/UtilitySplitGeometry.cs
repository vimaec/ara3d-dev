using Autodesk.Max;
using Autodesk.Max.MaxPlus;
using System;

namespace Ara3D
{
    public static class GeometryExtensions
    {
    }

    public class UtilitySplitGeometry : IUtilityPlugin
    {
        public void Evaluate()
        {
            if (SelectionManager.GetCount() == 0)
                throw new Exception("One node must be selected");
            if (SelectionManager.GetCount() > 1)
                throw new Exception("Only one node must be selected");
            var g = SelectionManager.GetNode(0).ToIGeometry();

            // Create nodes from the even and the odd 
            //g.CopyFaces(MathOps.Even).ToNode();
            //g.CopyFaces(MathOps.Odd).ToNode();

            // Create a new mesh and node for every 50 faces
            var grps = g.CopyFaceGroups(50);
            foreach (var grp in grps.ToEnumerable())
                grp.ToNode();
        }
    }
}