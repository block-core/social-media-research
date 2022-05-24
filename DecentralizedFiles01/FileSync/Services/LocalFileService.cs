using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileBaseSync.Services
{
    internal class LocalFileService : ILocalFileService
    {
        private ILocalFileBroker localFileBroker;
        public LocalFileService(ILocalFileBroker _localFileBroker)
        {
            localFileBroker = _localFileBroker;
        }

        public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return await localFileBroker.GetFileAsync(fileName, path, cancelToken);
        }

        public Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return localFileBroker.GetFileStreamAsync(fileName, path, cancelToken);
        }

        public Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return localFileBroker.GetFileDataAsync(fileName, path, cancelToken);
        }

        public Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return localFileBroker.GetDirectoryListingAsync(fileName, path, cancelToken);
        }

        public async Task UploadFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            await localFileBroker.UploadFileAsync(fileName, path, stream, cancelToken);
        }

        public async Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, CancellationToken cancelToken = default)
        {
            await localFileBroker.CopyFileAsync(fileName, sourcePath, destinationPath, cancelToken);
        }
    }
}
