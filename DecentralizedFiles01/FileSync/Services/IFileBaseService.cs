using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileBaseSync.Services
{
    public interface IFileBaseService
    {
        Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, CancellationToken cancelToken = default);
        Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task UpsertFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default);
        Task DeleteFileAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task<string> GeneratePreSignedURL(string key, double duration, CancellationToken cancelToken = default);
    }
}