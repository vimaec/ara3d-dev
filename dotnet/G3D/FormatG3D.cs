using System;
using System.Threading.Tasks;

namespace Ara3D
{
    public class FormatG3D : IFormatLoader
    {
        private readonly IDataSource _loader;

        public FormatG3D(IDataSource loader)
        {
            _loader = loader;
        }

        // A raw G3D file has no material info
        public Task<string> ResourceMaterialsAsync()
            => Task.FromResult("{}");

        // Our only resource is the file at RootUri
        public Task<byte[]> ResourceGeometryAsync(int index)
            => _loader.ResourceBytesAsync(new Uri("", UriKind.Relative));

        // We return a hard-coded manifest to fake having a scene graph
        public Task<string> ResourceManifestAsync()
            => Task.FromResult(@"[
            {
                ""Transform"": [
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0
                    ],
                ""ElementId"": 0,
                ""GeometryId"": 0,
                ""MaterialId"": 0
            }]");
    }
}
