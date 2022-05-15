using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace FileDataBroker {
	/// <summary>
	/// The local file IO broker
	/// </summary>
	public class LocalFileDataBroker : IFileDataBroker
	{

		#region Private Static Data Members

		private static readonly int bufferSize = 4096;

        #endregion

        #region Private Data Members

        private string _rootpath;

        public string RootPath
        {
            get { return _rootpath; }
            set { _rootpath = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the LocalFileDataBroker.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LocalFileDataBroker(ILogger<LocalFileDataBroker> logger)
		{
			//ToDo: Inject RootPath
			//ToDo: Setup Logging
			RootPath = @"d:\temp\";
		}

		#endregion

		#region IFileIoBroker Members

		public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			//Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileAsync), fileName, path);
			cancelToken.ThrowIfCancellationRequested();
			byte[] file;
			using (Stream stream = await GetFileStreamAsync(fileName, path, cancelToken).ConfigureAwait(false))
			using (MemoryStream ms = new MemoryStream())
			{
				await stream.CopyToAsync(ms, 81920, cancelToken).ConfigureAwait(false);
				file = ms.ToArray();
			}
			return file;
		}

		public Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			string fullPath = ResolvePath(fileName, path);
			if (!File.Exists(fullPath))
				throw new FileNotFoundException($"File \"{fullPath}\" not found.", fullPath);
			cancelToken.ThrowIfCancellationRequested();
			//Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
			return Task.FromResult((Stream)new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous));
		}

		public Task<FileData> GetFileIoItemAsync(string fileName, string path, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			string fileNamePath = ResolvePath(fileName, null);
			string fullPath = ResolvePath(fileName, path);
			if (!File.Exists(fullPath))
				throw new FileNotFoundException($"File \"{fullPath}\" not found.", fullPath);
			cancelToken.ThrowIfCancellationRequested();
			//Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileIoItemAsync), fileName, path);
			FileInfo fi = new FileInfo(fullPath);
			FileData item = new FileData()
			{
				Path = fullPath.Replace(fileNamePath + '\\', ""),
				FileName = fi.Name,
				Size = fi.Length,
				LastModified = fi.LastWriteTime
			};

			return Task.FromResult(item);
		}

		public Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));
			string fileNamePath = ResolvePath(fileName, null);
			string directoryPath = ResolvePath(fileName, Path.GetDirectoryName(path));
			string filePrefix = Path.GetFileName(path);
			if (!Directory.Exists(directoryPath))
				throw new DirectoryNotFoundException($"Directory \"{directoryPath}\" not found.");
			cancelToken.ThrowIfCancellationRequested();
			//Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetDirectoryListingAsync), fileName, path);
			return Task.FromResult<IList<FileData>>(Directory.GetFiles(directoryPath, $"{filePrefix}*", SearchOption.AllDirectories)
				.Select(f =>
				{
					FileInfo fi = new FileInfo(f);
					return new FileData() { Path = f.Replace(fileNamePath + '\\', ""), FileName = fi.Name, Size = fi.Length, LastModified = fi.LastWriteTime };
				})
				.ToList());
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
			using (FileStream destinationStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
				await stream.CopyToAsync(destinationStream, bufferSize, cancelToken).ConfigureAwait(false);
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
			//Logger.LogTrace("{method}: {fileName}, {sourcePath}, {destinationPath}", nameof(CopyFileAsync), fileName, sourcePath, destinationPath);
			if (!Directory.Exists(Path.GetDirectoryName(fullDestinationPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(fullDestinationPath));
			using (FileStream sourceStream = new FileStream(fullSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
			using (FileStream destinationStream = new FileStream(fullDestinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
				await sourceStream.CopyToAsync(destinationStream, bufferSize, cancelToken).ConfigureAwait(false);
		}

		#endregion

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
				throw new InvalidOperationException("The local file fileName rootpath is not defined.");
			if (string.IsNullOrEmpty(RootPath)
				|| !Directory.Exists(RootPath)
			)
			{
				throw new InvalidOperationException("The local file fileName root path does not exist.");
			}

			string fileNamePath = Path.Combine(RootPath, fileName);
			if (!Directory.Exists(fileNamePath))
				throw new InvalidOperationException($"The \"{fileName}\" fileName does not exist.");
			return string.IsNullOrEmpty(path) ? fileNamePath : Path.Combine(fileNamePath, path.Replace('/', '\\'))?.TrimEnd('\\');
		}

		#endregion
	}
}
