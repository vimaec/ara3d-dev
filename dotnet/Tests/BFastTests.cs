using Ara3D;
using NUnit.Framework;
using System.Linq;
using System.Text;

namespace Ara3D.Tests
{
    [TestFixture]
    class BFastTests
    {
        [Test]
        public void Test1()
        {
            var bb = new BFastBuilder();
            var empty = bb.ToBFast();
            Assert.AreEqual(0, empty.Header.NumArrays);
            Assert.IsEmpty(empty.Ranges);
            Assert.AreEqual(0, empty.Count);
            var hello = "Hello";
            bb.Add(hello);
            var oneArray = bb.ToBFast();
            Assert.AreEqual(1, oneArray.Header.NumArrays);
            Assert.AreEqual(1, oneArray.Ranges.Length);
            Assert.AreEqual(1, oneArray.Count);
            Assert.AreEqual(0, empty.Header.NumArrays);
            Assert.IsEmpty(empty.Ranges);
            Assert.AreEqual(0, empty.Count);
            var xs = oneArray[0];
            var s = Encoding.UTF8.GetString(xs);
            Assert.AreEqual(s, hello);
        }
    }
}
