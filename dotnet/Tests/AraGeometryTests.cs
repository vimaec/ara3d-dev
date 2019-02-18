using Ara3D;
using NUnit.Framework;
using System;
using System.IO;
using System.Numerics;

namespace Ara3D.Tests
{
    [TestFixture]
    public static class Tests
    {
        public static byte[] ReadAllBytes(string path)
        {
            var fi = new FileInfo(path);
            if (fi.Length > int.MaxValue)
                throw new Exception("Cannot read files greater than 2GB");
            using (var stream = fi.OpenRead())
            {
                using (var br = new BinaryReader(stream))
                    return br.ReadBytes((int)fi.Length);
            }
        }

        [Test]
        public static void TestReadingBigFile()
        {
            var path = @"C:\tmp\test.obj";
            foreach (var f in Directory.GetFiles(path))
            {
                Console.WriteLine($"Reading file {f}");
                Util.TimeIt(() => ReadAllBytes(f));
            }
        }

        public static bool ArraysEqual(Array a, Array b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; ++i)
                if (!a.GetValue(i).Equals(b.GetValue(i)))
                    return false;
            return true;
        }

        static IGeometry RevolvedVerticalCylinder(float height, float radius, int verticalSegments, int radialSegments)
        {
            return Vector3.UnitZ.ToLine().Interpolate(verticalSegments).Add(-radius.AlongX()).RevolveAroundAxis(Vector3.UnitZ, radialSegments);
        }

        [TestCase]
        public static void TestBinaryConverters()
        {
            var vecs = new[] { new Vector3(0, 1, 2), new Vector3(3, 4, 5) };
            var floats = new[] { 0f, 1f, 2f, 3f, 4f, 5f };

            var bytes1 = vecs.ToBytes();
            var bytes2 = floats.ToBytes();
            Assert.AreEqual(bytes1, bytes2);
            Assert.AreNotEqual(vecs, floats);
            var vecs2 = bytes2.FromBytes<Vector3>();
            Assert.AreEqual(vecs, vecs2);
            var floats2 = bytes1.FromBytes<float>();
            Assert.AreEqual(floats, floats2);

            var vecs3 = floats.ToBytes().FromBytes<Vector3>();
            Assert.AreEqual(vecs, vecs3);

            using (var strm = new MemoryStream(100))
            {
                var bw = new BinaryWriter(strm);
                bw.WriteStructs(vecs);

                strm.Seek(0, SeekOrigin.Begin);
                var br = new BinaryReader(strm);
                var tmp = br.ReadStructs<float>();
                Assert.AreEqual(tmp, floats);

                strm.Seek(0, SeekOrigin.Begin);
                var br2 = new BinaryReader(strm);
                var tmp2 = br2.ReadStructs<Vector3>();
                Assert.AreEqual(tmp2, vecs);
            }
        }
    }
}
