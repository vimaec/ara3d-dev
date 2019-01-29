using Autodesk.Max.MaxPlus;
using System.Linq;

namespace Ara3D
{
    public class UtilitySurfaceArea : IUtilityPlugin
    {
        public void Evaluate() 
        {
            var tris = API.AllGeometryOrJustSelected.Triangles();
            var areas = tris.ToEnumerable().Select(t => t.Area);
            var report = areas.StatisticsSummaryReport();
            Core.WriteLine("Triangle areas: " + report);
        }
    }
}
