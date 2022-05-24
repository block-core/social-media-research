using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileBaseSync.Services
{
    internal class FileBaseService : IFileBaseService
    {

        private IFileBaseBroker fileBaseBroker;
        public FileBaseService(IFileBaseBroker _fileBaseBroker)
        {
            fileBaseBroker = _fileBaseBroker;
        }

        public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return await fileBaseBroker.GetFileAsync(fileName, path, cancelToken);
        }

        public Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return fileBaseBroker.GetFileStreamAsync(fileName, path, cancelToken);
        }

        public Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return fileBaseBroker.GetFileDataAsync(fileName, path, cancelToken);
        }

        public Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            return fileBaseBroker.GetDirectoryListingAsync(fileName, path, cancelToken);
        }

        public async Task UploadFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            await fileBaseBroker.UploadFileAsync(fileName, path, stream, cancelToken);
        }

        public async Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, CancellationToken cancelToken = default)
        {
            await fileBaseBroker.CopyFileAsync(fileName, sourcePath, destinationPath, cancelToken);
        }
    }
}
