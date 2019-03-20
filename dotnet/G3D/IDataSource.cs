using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{
    /// <summary>
    /// This interface is to be implemented to provide resource loading from arbitrary sources
    /// It follows the basic TAP structure.  All functions should be treated as asynchronous,
    /// with no guarantees on when/which order they complete.  All functions will return either
    /// the data requested if possible or null on failure.
    /// NOTE - Currently this class eats all implementation-specific exceptions (eg access error,
    /// path not found etc).  This is by design, as the exceptions thrown are implementation specific,
    /// and should not be propagated to client code which can only treat this class in a generic way.
    /// It is intended that the implementation manage a logging/reporting mechanism and handle exceptions
    /// gracefully.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Get's a root-level path for resource loads.
        /// Any relative Uri's will be evaluated relative to this root.
        /// If the root is not set, then requests with local Uri's will throw.
        /// NOTE - it is expected this value is set in the implementing constructor
        /// </summary>
        //Uri RootUri { get; }

        /// <summary>
        /// Attempt to get the byte stream of the selected resource
        /// Returns true if the request is valid
        /// </summary>
        Task<byte[]> ResourceBytesAsync(Uri resourceUri);

        /// <summary>
        /// Attempt to get the string of the selected resource
        /// Returns true if the request is valid, and if the resource
        /// is available the data will be returned in the out data argument
        /// </summary>
        Task<string> ResourceStringAsync(Uri resourceUri);

        /// <summary>
        /// Get a list of all available resources that are children of the passed Uri
        /// (or of the root, if no Uri is passed)
        /// (NOT YET USED: May be useful for scene iteration/loading from server etc.)
        /// </summary>
        Task<Uri[]> RequestAvailableAsync();
        Task<Uri[]> RequestAvailableAsync(Uri path);
    }
}
