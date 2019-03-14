using System;
using System.IO;
using System.Threading.Tasks;

namespace Ara3D.DotNetUtilities
{
    /// <summary>
    /// Sample implementation of IResourceLoader that loads directly from
    /// the file system.
    /// Note the methods are actually synchronous despite returning Task
    /// </summary>
    class FilesystemLoader : IResourceLoader
    {
        private string _rootPath;

        public void SetRootUri(Uri root)
        {
            _rootPath = root.LocalPath;
        }

        public Task<bool> ResourceManifest(out string manifest)
        {
            return ResourceString(new Uri("manifest.json"), out manifest);
        }

        public Task<bool> ResourceBytes(Uri resourceUri, out byte[] data)
        {
            var path = Path.Combine(_rootPath, resourceUri.LocalPath);
            try
            {
                data = File.ReadAllBytes(path);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                // TODO: something with e
                data = null;
                return Task.FromResult(false);
            }  
        }

        public Task<bool> ResourceString(Uri resourceUri, out string data)
        {
            var path = Path.Combine(_rootPath, resourceUri.LocalPath);
            try
            {
                data = File.ReadAllText(path);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                // TODO: something with e
                data = null;
                return Task.FromResult(false);
            }
        }

        public Task<bool> RequestAvailable(out Uri[] available)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestAvailable(Uri path, out Uri[] available)
        {
            throw new NotImplementedException();
        }
    }
}
