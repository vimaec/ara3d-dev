using g3;
using System.Collections.Generic;

namespace Ara3D
{
    public static class G3SharpIO
    {
        public static List<DMesh3> ReadGeometry(string path)
        {
            var builder = new DMesh3Builder();
            var reader = new StandardMeshReader { MeshBuilder = builder };
            var result = reader.Read(path, ReadOptions.Defaults);
            if (result.code == IOCode.Ok)
                return builder.Meshes;
            return null;
        }

        public static bool WriteGeometry(string path, DMesh3 mesh)
        {
            var writer = new StandardMeshWriter();
            var m = new WriteMesh(mesh);
            var opts = new WriteOptions();
            // NOTE: Some fun options to play with in opts
            var result = writer.Write(path, new List<WriteMesh> { m }, opts);
            return result.Equals(IOWriteResult.Ok);
        }
    }
}
