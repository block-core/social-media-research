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
        #region Private Static Data Members

        private static readonly int bufferSize = 8192;

        #endregion

        #region Private Data Members

        private string _rootpath;

        public string RootPath
        {
            get { return _rootpath; }
            set { _rootpath = value; }
        }

        #endregion

        private ILocalFileBroker localFileBroker;
        public LocalFileService(ILocalFileBroker _localFileBroker)
        {
            localFileBroker = _localFileBroker;

            //ToDo: Inject RootPath
            //ToDo: Setup Logging
            RootPath = @"d:\temp\";
        }

        public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileAsync), fileName, path);
            cancelToken.ThrowIfCancellationRequested();

            return await localFileBroker.GetFileAsync(fileName, path, bufferSize, cancelToken);
        }

        //public Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        //{
        //    if (string.IsNullOrEmpty(fileName))
        //        throw new ArgumentNullException(nameof(fileName));
        //    if (string.IsNullOrEmpty(path))
        //        throw new ArgumentNullException(nameof(path));
        //    string fullPath = ResolvePath(fileName, path);
        //    if (!File.Exists(fullPath))
        //        throw new FileNotFoundException($"File \"{fullPath}\" not found.", fullPath);
        //    cancelToken.ThrowIfCancellationRequested();

        //    return localFileBroker.GetFileStreamAsync(fullPath, bufferSize, cancelToken);
        //}

        public Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            string fileNamePath = ResolvePath(fileName, null);
            string fullPath = ResolvePath(fileName, path).Replace(fileNamePath + '\\', "");
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File \"{fullPath}\" not found.", fullPath);
            cancelToken.ThrowIfCancellationRequested();

            return localFileBroker.GetFileDataAsync(fileName, fullPath, cancelToken);
        }

        public Task<IList<FileData>> GetDirectoryListingAsync(string path, string filter, CancellationToken cancelToken = default)
        {

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory \"{path}\" not found.");
            cancelToken.ThrowIfCancellationRequested();

            return localFileBroker.GetDirectoryListingAsync(path, filter, cancelToken);
        }

        public async Task UploadFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            string fullPath = ResolvePath(fileName, path);
            if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await localFileBroker.UploadFileAsync(fileName, fullPath, stream, bufferSize, cancelToken);
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

            string fullSourcePath = ResolvePath(fileName, sourcePath);
            string fullDestinationPath = ResolvePath(fileName, destinationPath);
            if (!File.Exists(fullSourcePath))
                throw new FileNotFoundException($"File \"{fullSourcePath}\" not found.", fullSourcePath);

            cancelToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(Path.GetDirectoryName(fullDestinationPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullDestinationPath));

            await localFileBroker.CopyFileAsync(fileName, fullSourcePath, fullDestinationPath, bufferSize, cancelToken);
        }

        #region Private Methods

        /// <summary>
        /// Resolves the path for the specified fileName and file path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The file path.</param>
        /// <returns>The resolved path.</returns>
        private string ResolvePath(string fileName, string path)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (RootPath == null)
                throw new InvalidOperationException("The local file root path is not defined.");

            if (string.IsNullOrEmpty(RootPath)
                || !Directory.Exists(RootPath)
            )
            {
                throw new InvalidOperationException("The local file  root path does not exist.");
            }

            path = Path.Combine(RootPath, path);

            string fileNamePath = Path.Combine(path, fileName).Replace('\\', '/').TrimEnd('/');
            if (!File.Exists(fileNamePath))
                throw new InvalidOperationException($"The file path: \"{fileNamePath}\", does not exist.");

            return fileNamePath;
        }

        #endregion

    }
}
