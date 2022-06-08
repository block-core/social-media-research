using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace FileBaseSync.Services
{
    public class FileBaseService : IFileBaseService
    {
        private IConfiguration config;
        private static readonly int bufferSize = 8192;

        private IFileBaseBroker fileBaseBroker;
        public FileBaseService(IConfiguration _config, IFileBaseBroker _fileBaseBroker)
        {
            config = _config;
            fileBaseBroker = _fileBaseBroker;
        }

        public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            cancelToken.ThrowIfCancellationRequested();

            return await fileBaseBroker.GetFileAsync(fileName, path, bufferSize, cancelToken);
        }

        //public Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        //{
        //    if (string.IsNullOrEmpty(fileName))
        //        throw new ArgumentNullException(nameof(fileName));
        //    if (string.IsNullOrEmpty(path))
        //        throw new ArgumentNullException(nameof(path));
        //    cancelToken.ThrowIfCancellationRequested();

        //    return fileBaseBroker.GetFileStreamAsync(fileName, path, cancelToken);
        //}

        public Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            cancelToken.ThrowIfCancellationRequested();

            return fileBaseBroker.GetFileDataAsync(fileName, path, cancelToken);
        }

        public Task<IList<FileData>> GetDirectoryListingAsync(string path, string filter, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            cancelToken.ThrowIfCancellationRequested();

            //return fileBaseBroker.GetDirectoryListingAsync(fileName, path, cancelToken);
            return fileBaseBroker.GetDirectoryListingAsync(path,  filter, cancelToken);
        }


        public async Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentNullException(nameof(destinationPath));
            if (sourcePath.Equals(destinationPath))
                return;

            cancelToken.ThrowIfCancellationRequested();

            await fileBaseBroker.CopyFileAsync(fileName, sourcePath, destinationPath, bufferSize, cancelToken);
        }

        public async Task DeleteFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            cancelToken.ThrowIfCancellationRequested();

            await fileBaseBroker.DeleteFileAsync(fileName, path, cancelToken);
        }

        public async Task UpsertFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            cancelToken.ThrowIfCancellationRequested();

            bool fileExists = await fileBaseBroker.FileExistsAsync(fileName, path, cancelToken);
            if (!fileExists)
            {
                await fileBaseBroker.PutFileAsync(fileName, path, stream,  cancelToken);
            }
            else
            {
                await fileBaseBroker.UploadFileAsync(fileName, path, stream, bufferSize, cancelToken);
            }

        }

        public async Task<string> GeneratePreSignedURL(string key, double duration, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (duration < 1)
                throw new ArgumentNullException(nameof(duration));

            cancelToken.ThrowIfCancellationRequested();

            return await fileBaseBroker.GeneratePreSignedURL(key, duration, cancelToken);
        }
    }
}
