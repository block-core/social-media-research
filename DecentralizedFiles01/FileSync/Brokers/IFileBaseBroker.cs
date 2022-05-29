using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileBaseSync
{
    /// <summary>
    /// Defines S3 file management broker functionality.
    /// </summary>
    public interface IFileBaseBroker:IFileDataBroker
    {
        //ToDo: If we create a local file version of this method, then move to IFileDataBroker
        Task DeleteFileAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task<bool> FileExistsAsync(string fileName, string path, CancellationToken cancelToken = default);
        Task PutFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default);
    }
}
