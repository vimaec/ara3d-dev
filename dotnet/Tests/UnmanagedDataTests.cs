using System;
using NUnit.Framework;
using System.Numerics;

namespace Ara3D.Tests
{
    [TestFixture]
    class UnmanagedDataTests
    {
        [Test]
        public static void TestBuffers()
        {
            var xs = new[]
            {
                new Vector3(0, 1, 2),
                new Vector3(3, 4, 5),
                new Vector3(6, 7, 8),
                new Vector3(9, 10, 11),
                new Vector3(12, 13, 14)
            };

            using (var pin = xs.Pin())
            {
                var bytes = pin.ToBytes();
                Assert.AreEqual(60, bytes.Length);

                using (var buffer = new UnmanagedBuffer(bytes.Length))
                {
                    pin.CopyTo(buffer);

                    Assert.IsTrue(pin.SequenceEqual(buffer));

                    var ys = new Vector3[5];

                    using (var pin2 = ys.Pin())
                        pin.CopyTo(pin2);

                    Assert.AreEqual(xs, ys);

                    var zs = new float[15];
                    using (var pin3 = zs.Pin())
                        pin.CopyTo(pin3);

                    Assert.AreEqual(4.0, zs[4], Double.Epsilon);
                    Assert.AreEqual(11.0, zs[11], Double.Epsilon);
                }
            }
        }

        [Test]
        public static void TypeSizeChecks()
        {
            Assert.AreEqual(4, typeof(int).SizeOf());
            Assert.AreEqual(4, typeof(float).SizeOf());
            Assert.AreEqual(4, typeof(uint).SizeOf());

            Assert.AreEqual(8, typeof(long).SizeOf());
            Assert.AreEqual(8, typeof(double).SizeOf());
            Assert.AreEqual(8, typeof(ulong).SizeOf());

            Assert.AreEqual(2, typeof(short).SizeOf());
            Assert.AreEqual(2, typeof(ushort).SizeOf());
            Assert.AreEqual(2, typeof(char).SizeOf());

            Assert.AreEqual(1, typeof(byte).SizeOf());

            Assert.AreEqual(8, typeof(Vector2).SizeOf());
            Assert.AreEqual(12, typeof(Vector3).SizeOf());
            Assert.AreEqual(16, typeof(Vector4).SizeOf());
            Assert.AreEqual(16, typeof(Quaternion).SizeOf());

            Assert.AreEqual(FileHeader.Size, typeof(FileHeader).SizeOf());

            Assert.AreEqual(64, typeof(Matrix4x4).SizeOf());
        }

    }
}
