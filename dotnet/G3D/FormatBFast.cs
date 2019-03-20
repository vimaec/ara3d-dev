using System;
using System.Threading.Tasks;

namespace Ara3D
{
    public class FormatBFast : IFormatLoader
    {
        private readonly IDataSource _loader;

        public FormatBFast(IDataSource loader)
        {
            _loader = loader;
        }

        public Task<string> ResourceManifestAsync()
            => _loader.ResourceStringAsync(new Uri("0"));

        // TODO! Load materials resources from appropriate buffer
        public Task<string> ResourceMaterialsAsync()
            => Task.FromResult("{}");

        public Task<byte[]> ResourceGeometryAsync(int index)
            => _loader.ResourceBytesAsync(new Uri((index + 1).ToString()));
    }
}
