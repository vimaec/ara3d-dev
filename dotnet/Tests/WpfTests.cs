using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
