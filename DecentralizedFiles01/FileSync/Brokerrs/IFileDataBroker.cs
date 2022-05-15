using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileBaseSync
{
    /// <summary>
    /// Defines file management broker functionality.
    /// </summary>
    public interface IFileDataBroker
    {

        #region Public Methods

        /// <summary>
        /// Gets the file from the specified fileName and path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The file path.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>The file.</returns>
        Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the file stream from the specified fileName and path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The file path.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>The file stream.</returns>
        Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the FileIoItem Information from the specified fileName and path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The file path.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>The file fileName item.</returns>
        Task<FileData> GetFileIoItemAsync(string fileName, string path, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the directory listing from the specified fileName and path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The directory path. Include trailing / for directories.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>A list of file fileName items.</returns>
        Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default);

        /// <summary>
        /// Uploads a file to the specified fileName and path from the stream.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="path">The path.</param>
        /// <param name="stream">The file stream.</param>
        /// <param name="cancelToken">The cancel token.</param>
        Task UploadFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default);

        /// <summary>
        /// Copies a file in the specified fileName from the source path to the destination path.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="cancelToken">The cancel token.</param>
        Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, CancellationToken cancelToken = default);

        #endregion

    }
}
