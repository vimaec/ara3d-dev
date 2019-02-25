using NUnit.Framework;

namespace Ara3D.Tests
{
    [TestFixture]
    public static class WpfTests
    {
        [Test, Explicit]
        public static void ViewTest()
        {
            Helix.ShowModal();
        }
    }
}
