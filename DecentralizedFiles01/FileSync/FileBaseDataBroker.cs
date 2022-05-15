using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;


namespace FileBaseSync
{
    /// <summary>
    /// The S3 file IO Broker.
    /// </summary>
    public class FileBaseDataBroker : IFileDataBroker
    {

        #region Private Data Members
        private const string accessKey = "Filebase Access Key";
        private const string secretKey = "Filebase Secret Key";
        private string bucketName;
        private string serviceUrl = "https://s3.filebase.com";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the FileBaseDataBroker.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FileBaseDataBroker(ILogger<FileBaseDataBroker> logger)
        {

            //ToDo: Inject Access Keys
            //ToDo: Inject BucketName

        }

        #endregion

        #region IFileIoBroker Members

        public async Task<byte[]> GetFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            cancelToken.ThrowIfCancellationRequested();
            byte[] file;
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileAsync), fileName, path);
            using (Stream stream = await GetFileStreamAsync(fileName, path, cancelToken).ConfigureAwait(false))
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, 81920, cancelToken).ConfigureAwait(false);
                file = ms.ToArray();
            }
            return file;
        }

        public async Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            cancelToken.ThrowIfCancellationRequested();
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
            GetObjectRequest request = new GetObjectRequest() { BucketName = GetBucketName(fileName), Key = path };
            GetObjectResponse response = null;
            using (IAmazonS3 s3client = GetS3Client())
            {
                response = await s3client.GetObjectAsync(request, cancelToken).ConfigureAwait(false);
                if ((response?.ContentLength ?? 0) == 0)
                {
                    response?.Dispose();
                    return null;
                }
                //return new ResourceStream(response.ResponseStream, response.ContentLength, response);
                return response.ResponseStream;
            }
        }

        public async Task<FileData> GetFileIoItemAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            cancelToken.ThrowIfCancellationRequested();
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
            GetObjectRequest request = new GetObjectRequest() { BucketName = GetBucketName(fileName), Key = path };
            GetObjectResponse response = null;
            using (IAmazonS3 s3client = GetS3Client())
            using (response = await s3client.GetObjectAsync(request, cancelToken).ConfigureAwait(false))
            {
                if ((response?.ContentLength ?? 0) == 0)
                    return null;
                FileData item = new FileData()
                {
                    Path = response.Key,
                    FileName = Path.GetFileName(response.Key),
                    Size = response.ContentLength,
                    LastModified = response.LastModified
                };

                return item;
            }
        }

        public async Task<IList<FileData>> GetDirectoryListingAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            cancelToken.ThrowIfCancellationRequested();
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetDirectoryListingAsync), fileName, path);
            ListObjectsV2Request request = new ListObjectsV2Request() { BucketName = GetBucketName(fileName), Prefix = path };
            ListObjectsV2Response response = null;
            using (IAmazonS3 s3client = GetS3Client())
                response = await s3client.ListObjectsV2Async(request, cancelToken).ConfigureAwait(false);
            return response?.S3Objects?.Select(s3Obj => new FileData()
            {
                Path = s3Obj.Key,
                FileName = Path.GetFileName(s3Obj.Key),
                Size = s3Obj.Size,
                LastModified = s3Obj.LastModified
            }).ToList();
        }

        public async Task UploadFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            cancelToken.ThrowIfCancellationRequested();
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(UploadFileAsync), fileName, path);
            PutObjectRequest request = new PutObjectRequest() { BucketName = GetBucketName(fileName), Key = path, InputStream = stream };
            PutObjectResponse response = null;
            using (IAmazonS3 s3client = GetS3Client())
                response = await s3client.PutObjectAsync(request, cancelToken).ConfigureAwait(false);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"S3 file upload failed: {response.HttpStatusCode}.");
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
            //Logger.LogTrace("{method}: {fileName}, {sourcePath}, {destinationPath}", nameof(CopyFileAsync), fileName, sourcePath, destinationPath);
            CopyObjectRequest request = new CopyObjectRequest()
            {
                SourceBucket = GetBucketName(fileName),
                SourceKey = sourcePath,
                DestinationBucket = GetBucketName(fileName),
                DestinationKey = destinationPath
            };
            CopyObjectResponse response = null;
            using (IAmazonS3 s3client = GetS3Client())
                response = await s3client.CopyObjectAsync(request, cancelToken).ConfigureAwait(false);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"S3 file copy failed: {response.HttpStatusCode}.");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the S3 client.
        /// </summary>
        /// <returns>The S3 client.</returns>
        private IAmazonS3 GetS3Client()
        {
            AmazonS3Config s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
            };

            return new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        /// <summary>
        /// Gets the bucket name.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <returns>The bucket name.</returns>
        private string GetBucketName(string fileName)
        {
            return bucketName;
        }

        #endregion

    }
}
