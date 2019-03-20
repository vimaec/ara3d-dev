using System;
using System.Threading.Tasks;

namespace Ara3D
{
    /// <summary>
    /// Sample implementation loads chunks of memory from BFast
    /// </summary>
    public class DataSourceBFast : IDataSource
    {
        private readonly BFast _source;

        public Uri RootUri { get; }


        public DataSourceBFast(string filePath)
        {
            RootUri = new Uri(filePath);
            _source = BFastExtensions.ReadBFast(filePath);
        }

        private byte[] GetRaw(int idx)
            => _source.GetBuffer(idx).ToBytes();

        private Task<byte[]> GetBuffer(int idx)
            => Task.FromResult(GetRaw(idx));

        private Task<byte[]> GetBuffer(Uri uri)
            => GetBuffer(int.Parse(uri.OriginalString));

        public Task<byte[]> ResourceBytesAsync(Uri resourceUri)
            => GetBuffer(resourceUri);
 
        public Task<string> ResourceStringAsync(Uri resourceUri)
            => Task.FromResult(GetBuffer(resourceUri).Result.ToUtf8());

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
