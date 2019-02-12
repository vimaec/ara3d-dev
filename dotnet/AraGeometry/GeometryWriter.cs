using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ara3D
{
    public static class GeometryWriter
    {
        public static void WriteObj(this IGeometry g, string fileName)
        {
            g.WriteObj(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
        }

        public static void WriteObj(this IGeometry g, string folder, string name, bool generateMtl = false)
        {
            Enumerable.Repeat(g, 1).WriteObj(folder, name, generateMtl);
        }

        public static void WriteObj(this IEnumerable<IGeometry> objects, string folder, string name, bool generateMtl = false)
        {
            objects.Select(g => new ObjGeometry(g)).WriteObj(folder, name, generateMtl);
        }

        public static void WriteObj(this IEnumerable<ObjGeometry> objects, string folder, string name, bool generateMtl = true)
        {
            ObjEmitter.Write(objects, folder, name, generateMtl);
        }
    }
}
