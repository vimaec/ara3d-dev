using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    public static class GeometryWriter
    {
        public static void WriteObj(this IGeometry g, string filePath, bool generateMtl = false)
            => Enumerable.Repeat(g, 1).WriteObj(filePath, generateMtl);        

        public static void WriteObj(this IEnumerable<IGeometry> objects, string filePath, bool generateMtl = false)
            => objects.Select(g => new ObjGeometry(g)).WriteObj(filePath, generateMtl);
        
        public static void WriteObj(this IEnumerable<ObjGeometry> objects, string filePath, bool generateMtl = true)
            => new ObjEmitter(objects, filePath, generateMtl);        
    }
}
