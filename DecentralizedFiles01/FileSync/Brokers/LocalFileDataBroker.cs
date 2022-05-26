using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace FileBaseSync
{
    /// <summary>
    /// The local file IO broker
    /// </summary>
    public class LocalFileDataBroker : ILocalFileBroker
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the LocalFileDataBroker.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LocalFileDataBroker(ILogger<LocalFileDataBroker> logger)
        {

        }

        #endregion

        #region IFileIoBroker Members

        public async Task<byte[]> GetFileAsync(string fileName, string path, int bufferSize, CancellationToken cancelToken = default)
        {

            byte[] file;
            using (Stream stream = await GetFileStreamAsync(path, bufferSize, cancelToken).ConfigureAwait(false))
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, bufferSize, cancelToken).ConfigureAwait(false);
                file = ms.ToArray();
            }
            return file;
        }

        public Task<Stream> GetFileStreamAsync(string path, int bufferSize, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
            return Task.FromResult((Stream)new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous));
        }

        public Task<FileData> GetFileDataAsync(string fileName, string fullPath, CancellationToken cancelToken = default)
        {

            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileDataAsync), fileName, path);
            FileInfo fi = new FileInfo(fullPath);
            FileData item = new FileData()
            {
                Path = fullPath,
                FileName = fi.Name,
                Size = fi.Length,
                LastModified = fi.LastWriteTime
            };

            return Task.FromResult(item);
        }

        public Task<IList<FileData>> GetDirectoryListingAsync(string path, string filter, CancellationToken cancelToken = default)
        {
            string filePrefix = filter;
            path = path.Replace(path + '\\', "");
            //string filePrefix = Path.GetFileName(path);
            cancelToken.ThrowIfCancellationRequested();
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetDirectoryListingAsync), fileName, path);
            //return Task.FromResult<IList<FileData>>(Directory.GetFiles(directoryPath, $"{filePrefix}*", SearchOption.AllDirectories)
            return Task.FromResult<IList<FileData>>(Directory.GetFiles(path, $"{filePrefix}*", SearchOption.AllDirectories)
                .Select(f =>
                {
                    FileInfo fi = new FileInfo(f);
                    return new FileData() { Path = f, FileName = fi.Name, Size = fi.Length, LastModified = fi.LastWriteTime };
                })
                .ToList());
        }

        public async Task UploadFileAsync(string fileName, string path, Stream stream, int bufferSize, CancellationToken cancelToken = default)
        {

            using (FileStream destinationStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await stream.CopyToAsync(destinationStream, bufferSize, cancelToken).ConfigureAwait(false);
        }

        public async Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, int bufferSize, CancellationToken cancelToken = default)
        {

            cancelToken.ThrowIfCancellationRequested();

            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (FileStream destinationStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream, bufferSize, cancelToken).ConfigureAwait(false);
        }

        #endregion

    }
}
