using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D.DotNetUtilities
{
    public interface IResourceLoader
    {
        /// <summary>
        /// Set's a root-level path for subsequent resource loads.
        /// Any relative Uri's will be evaluated relative to this root.
        /// If the root is not set, then requests with local Uri's will throw.
        /// </summary>
        void SetRootUri(Uri root);

        /// <summary>
        /// A special-purpose call to load the manifest makes loading
        /// from BFast a lot easier
        /// </summary>
        Task<bool> ResourceManifest(out string manifest);

        /// <summary>
        /// Attempt to get the byte stream of the selected resource
        /// Returns true if the request is avlud
        /// </summary>
        Task<bool> ResourceBytes(Uri resourceUri, out byte[] data);

        /// <summary>
        /// Attempt to get the string of the selected resource
        /// Returns true if the request is valid, and if the resource
        /// is available the data will be returned in the out data argument
        /// </summary>
        Task<bool> ResourceString(Uri resourceUri, out string data);

        /// <summary>
        /// Get a list of all available resources that are children of the passed Uri
        /// (or of the root, if no Uri is passed)
        /// </summary>
        Task<bool> RequestAvailable(out Uri[] available);
        Task<bool> RequestAvailable(Uri path, out Uri[] available);
    }
}
