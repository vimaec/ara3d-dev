using System.Threading.Tasks;

namespace Ara3D
{
    public interface IFormatLoader
    {
        /// <summary>
        /// A special-purpose call to load the manifest makes loading
        /// from BFast a lot easier
        /// </summary>
        Task<string> ResourceManifestAsync();

        /// <summary>
        /// Fetch of materials resource.  May return NULL
        /// </summary>
        Task<string> ResourceMaterialsAsync();

        /// <summary>
        /// Fetch of geometry by index.  May return NULL
        /// </summary>
        Task<byte[]> ResourceGeometryAsync(int index);
    }
}