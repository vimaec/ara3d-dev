using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ara3D
{
    public static class GeometryReader
    {
        public static IGeometry ReadG3D(string filePath)
            => G3DExtensions.ReadFromFile(filePath).ToIGeometry();

        public static List<IGeometry> LoadGeometries(this IList<Memory<byte>> buffers)
        {
            // Need to remove Parallel Linq to work in il2cpp
            //=> bfast.Buffers
            //    .AsParallel().AsOrdered()
            //    .Select(b => G3D.Create(b).ToIGeometry())
            //    .ToList();
            var geometries = new IGeometry[buffers.Count];
            Parallel.For(0, buffers.Count, i =>
                {
                    geometries[i] = G3D.Create(buffers[i]).ToIGeometry();
                });
            return geometries.ToList();
        }

        public static List<IGeometry> LoadGeometries(this Stream stream)
            => stream.ReadAllBytes().ToBFastBuffers().LoadGeometries();

        public static List<IGeometry> LoadGeometries(string filePath)
            => File.OpenRead(filePath).LoadGeometries();
    }
}
