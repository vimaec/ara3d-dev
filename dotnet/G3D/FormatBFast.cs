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
        {
            var uri = new Uri("0", UriKind.Relative);
            return _loader.ResourceStringAsync(uri);
        }

        // TODO! Load materials resources from appropriate buffer
        public Task<string> ResourceMaterialsAsync()
            => Task.FromResult("{}");

        public Task<byte[]> ResourceGeometryAsync(int index)
            => _loader.ResourceBytesAsync(new Uri((index + 1).ToString(), UriKind.Relative));
    }
}
