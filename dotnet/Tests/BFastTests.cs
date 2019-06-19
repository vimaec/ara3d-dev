using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    [TestFixture]
    public static class BFastTests
    {
        public static byte[] emptyBuffer = new byte[0];
        public static byte[] smallBuffer = new byte[] { 1, 2, 3 };
        public static byte[] bigBuffer = Enumerable.Range(0, 1000000).Select(x => (byte)x).ToArray();

        public static List<byte[]> noBuffers = new List<byte[]>();
        public static List<byte[]> oneEmptyBuffer = new List<byte[]> { emptyBuffer };
        public static List<byte[]> tenEmptyBuffers = emptyBuffer.Repeat(10).ToList();
        public static List<byte[]> twoSmallBuffers = smallBuffer.Repeat(2).ToList();
        public static List<byte[]> fiveBigBuffers = bigBuffer.Repeat(5).ToList();

        public static void TestBFast(IEnumerable<byte[]> buffers)
        {
            var total = buffers.Sum(b => b.Length);
            var count = buffers.Count();
            Console.WriteLine($"Testing BFAST with {count} buffers and a total of {total} bytes");
            var bfastBytes = buffers.ToBFastBytes();
            Console.WriteLine($"Generated a BFAST with {bfastBytes.Length} bytes");
            var buffers2 = bfastBytes.ToBFastBuffers();
            Assert.IsTrue(bfastBytes.Length > total);
            Assert.AreEqual(count, buffers2.Count);
            var i = 0;
            foreach (var buffer in buffers)
            {
                var buffer2 = buffers2[i++].ToBytes();
                Assert.AreEqual(buffer, buffer2);
            }
            var bfastBytes2 = buffers.ToBFastBytes();
            Assert.AreEqual(bfastBytes.Length, bfastBytes2.Length);
            Assert.AreEqual(bfastBytes, bfastBytes2);
        }

        [Test]
        public static void BasicBFastTests()
        {
            TestBFast(noBuffers);
            TestBFast(oneEmptyBuffer);
            TestBFast(tenEmptyBuffers);
            TestBFast(twoSmallBuffers);
            TestBFast(fiveBigBuffers);
        }

        public static void TestGenerateBuffers()
        {
            for (var i = 0; i < 1000; ++i)
            {
                var bytes = fiveBigBuffers.ToBFastBytes();
                Assert.IsTrue(bytes.Length > 5000000);
            }
        }

        [Test]
        public static void BFastTimingTest()
        {
            Util.TimeIt(TestGenerateBuffers);
        }

        [Test]
        public static void BFastMemoryTest()
        {
            TestGenerateBuffers();
            var memConsumption = Util.GetMemoryConsumption(TestGenerateBuffers);
            Assert.AreEqual(0, memConsumption);
        }
    }
}
