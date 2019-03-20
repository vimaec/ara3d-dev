using System;
using System.IO;
using System.Threading.Tasks;

namespace Ara3D
{
    /// <summary>
    /// Sample implementation of IResourceLoader that loads directly from
    /// the file system.
    /// Note the methods are actually synchronous despite returning Task
    /// </summary>
    public class DataSourceFileSystem : IDataSource
    {
        public Uri RootUri { get; }
        
        public DataSourceFileSystem(string rootPath)
        {
            RootUri = new Uri(rootPath, UriKind.Absolute);
        }

        private Uri GetUri(Uri localUri)
            => localUri != null ? new Uri(RootUri, localUri) : RootUri;

        private string GetPath(Uri localUri)
            => GetUri(localUri).LocalPath;

        public Task<byte[]> ResourceBytesAsync(Uri resourceUri)
        {
            var path = GetPath(resourceUri);
            try
            {
                if (File.Exists(path))
                {
                    var data = File.ReadAllBytes(path);
                    return Task.FromResult(data);
                }
            }
            catch (Exception e)
            {
                // TODO: something with e
            }
            return Task.FromResult<byte[]>(null);
        }

        public Task<string> ResourceStringAsync(Uri resourceUri)
        {
            var path = GetPath(resourceUri);
            try
            {
                if (File.Exists(path))
                {
                    var data = File.ReadAllText(path);
                    return Task.FromResult(data);
                }
            }
            catch (Exception e)
            {
                // TODO: something with e
                Console.WriteLine(e.Message);
            }
            return Task.FromResult<string>(null);
        }

        public Task<Uri[]> RequestAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Uri[]> RequestAvailableAsync(Uri path)
        {
            throw new NotImplementedException();
        }
    }
}
