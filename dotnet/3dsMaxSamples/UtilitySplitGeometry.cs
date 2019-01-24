using Autodesk.Max;
using Autodesk.Max.MaxPlus;
using System;

namespace Ara3D
{
    public static class GeometryExtensions
    {
        public static IGeometry CopyFaces(this IGeometry g, Func<int, bool> predicate)
        {
            return new Geometry(g.Arity, g.Vertices, g.Indices, g.Elements.WhereIndices(predicate).ToIArray());
        }
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
            g.CopyFaces(MathOps.Even).ToNode();
            g.CopyFaces(MathOps.Odd).ToNode();
        }
    }
}