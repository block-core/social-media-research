using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using FileBaseSync;
using FileBaseSync.Models;
using Amazon.Runtime;
using Amazon.S3.Util;

namespace FileBaseSync
{
    /// <summary>
    /// The S3 file IO Broker.
    /// </summary>
    public class FileBaseDataBroker : IFileBaseBroker
    {
        #region Private Data Members
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration;
        public FileBaseCredentialsOptions CredentialOptions { get; private set; } = new FileBaseCredentialsOptions();
        private string accessKey = "Filebase Access Key";
        private string secretKey = "Filebase Secret Key";
        private string bucketName;
        //private string serviceUrl = "https://s3.filebase.com";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the FileBaseDataBroker.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FileBaseDataBroker(Microsoft.Extensions.Configuration.IConfiguration _configuration, ILogger<FileBaseDataBroker> logger)
        {
            configuration = _configuration;

            configuration.GetSection(CredentialOptions.AccessKey).Bind(CredentialOptions);

            CredentialOptions = _configuration.GetSection("FilebaseCredentials").Get<FileBaseCredentialsOptions>();

            accessKey = CredentialOptions.AccessKey;
            secretKey = CredentialOptions.SecretKey;

            bucketName = configuration["S3BucketName"];

            CreateBucketAsync().Wait();

        }

        #endregion

        #region IFileIoBroker Members

        public async Task<string> GeneratePreSignedURL(string key, double duration, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(duration)
                };
                //urlString = GetS3Client().GetPreSignedURL(request1);
                Action action = () => { urlString = GetS3Client().GetPreSignedURL(request1); };
                await Task.Run(action);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return urlString;
        }

        public async Task<byte[]> GetFileAsync(string fileName, string path, int bufferSize, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            byte[] file;
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileAsync), fileName, path);
            using (Stream stream = await GetFileStreamAsync(fileName, path, cancelToken).ConfigureAwait(false))
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, bufferSize, cancelToken).ConfigureAwait(false);
                file = ms.ToArray();
            }
            return file;
        }

        public async Task<Stream> GetFileStreamAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
            GetObjectRequest request = new GetObjectRequest() { BucketName = GetBucketName(), Key = path };
            GetObjectResponse response = null;

            cancelToken.ThrowIfCancellationRequested();

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

        public async Task<FileData> GetFileDataAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetFileStreamAsync), fileName, path);
            GetObjectRequest request = new GetObjectRequest() { BucketName = GetBucketName(), Key = path };
            GetObjectResponse response = null;

            cancelToken.ThrowIfCancellationRequested();

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

        //ToDo: Test if path should be used for bucketName, and if prefix should be empty
        public async Task<IList<FileData>> GetDirectoryListingAsync(/*string fileName, */string path, string filter, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(GetDirectoryListingAsync), fileName, path);
            //ListObjectsV2Request request = new ListObjectsV2Request() { BucketName = GetBucketName(fileName), Prefix = path };
            ListObjectsV2Request request = new ListObjectsV2Request() { BucketName = GetBucketName(), Prefix = filter };
            ListObjectsV2Response response = null;

            cancelToken.ThrowIfCancellationRequested();

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

        public async Task UploadFileAsync(string fileName, string path, Stream stream, int bufferSize, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {path}", nameof(UploadFileAsync), fileName, path);
            PutObjectRequest request = new PutObjectRequest() { BucketName = GetBucketName(), Key = path, InputStream = stream };
            PutObjectResponse response = null;

            cancelToken.ThrowIfCancellationRequested();

            using (IAmazonS3 s3client = GetS3Client())
                response = await s3client.PutObjectAsync(request, cancelToken).ConfigureAwait(false);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"S3 file upload failed: {response.HttpStatusCode}.");
        }

        public async Task CopyFileAsync(string fileName, string sourcePath, string destinationPath, int bufferSize, CancellationToken cancelToken = default)
        {
            //Logger.LogTrace("{method}: {fileName}, {sourcePath}, {destinationPath}", nameof(CopyFileAsync), fileName, sourcePath, destinationPath);
            CopyObjectRequest request = new CopyObjectRequest()
            {
                SourceBucket = GetBucketName(),
                SourceKey = sourcePath,
                DestinationBucket = GetBucketName(),
                DestinationKey = destinationPath
            };

            CopyObjectResponse response = null;

            cancelToken.ThrowIfCancellationRequested();

            using (IAmazonS3 s3client = GetS3Client())
                response = await s3client.CopyObjectAsync(request, cancelToken).ConfigureAwait(false);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new HttpRequestException($"S3 file copy failed: {response.HttpStatusCode}.");
        }

        public async Task DeleteFileAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            using (IAmazonS3 s3client = GetS3Client())
            {
                var request = new DeleteObjectRequest()
                {
                    BucketName = path,
                    Key = fileName
                };

                cancelToken.ThrowIfCancellationRequested();

                await s3client.DeleteObjectAsync(request, cancelToken);

            }
        }

        public async Task<bool> FileExistsAsync(string fileName, string path, CancellationToken cancelToken = default)
        {
            //ToDo: Is this method reliable? Research options
            using (IAmazonS3 s3client = GetS3Client())
            {
                try
                {
                    var request = new GetObjectMetadataRequest()
                    {
                        BucketName = path,
                        Key = fileName
                    };

                    cancelToken.ThrowIfCancellationRequested();

                    var response = await s3client.GetObjectMetadataAsync(request);

                    return true;
                }

                catch (Amazon.S3.AmazonS3Exception ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return false;

                    //status wasn't not found, so throw the exception
                    //throw;
                }

                return false;

            }
        }

        public async Task PutFileAsync(string fileName, string path, Stream stream, CancellationToken cancelToken = default)
        {
            using (IAmazonS3 s3client = GetS3Client())
            {
                var request = new PutObjectRequest()
                {
                    BucketName = path,
                    Key = fileName,
                    InputStream = stream,
                };

                cancelToken.ThrowIfCancellationRequested();

                await s3client.PutObjectAsync(request, cancelToken);

            }
        }

        private async Task CreateBucketAsync()
        {
            var s3Client = GetS3Client();
            try
            {
                if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName))
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                    };

                    PutBucketResponse putBucketResponse = await s3Client.PutBucketAsync(putBucketRequest);
                }
                string bucketLocation = await FindBucketLocationAsync(s3Client);
                return;
            }
            catch (AmazonS3Exception e)
            {
                throw new AmazonS3Exception("Error encountered on server when creating S3 bucket.", e);
            }
            catch (Exception e)
            {
                throw new Exception("Unknown error encountered on server when creating an S3 bucket.", e);
            }

        }




        #endregion

        #region Private Methods
        private async Task<string> FindBucketLocationAsync(IAmazonS3 client)
        {
            string bucketLocation;
            var request = new GetBucketLocationRequest()
            {
                BucketName = GetBucketName()
            };
            GetBucketLocationResponse response = await client.GetBucketLocationAsync(request);
            bucketLocation = response.Location.ToString();
            return bucketLocation;
        }
        /// <summary>
        /// Gets the S3 client.
        /// </summary>
        /// <returns>The S3 client.</returns>
        private IAmazonS3 GetS3Client()
        {
            AmazonS3Config s3Config = new AmazonS3Config()
            {
                ServiceURL = string.Format("<https://s3.filebase.com:443>"),
                UseHttp = true,
                ForcePathStyle = true,
                ProxyHost = "s3.filebase.com",
                ProxyPort = 443
            };

            AWSCredentials creds = new BasicAWSCredentials(accessKey, secretKey);
            return new AmazonS3Client(creds, s3Config);
        }

        /// <summary>
        /// Gets the bucket name.
        /// </summary>
        /// <param name="fileName">The file fileName.</param>
        /// <returns>The bucket name.</returns>
        private string GetBucketName()
        {
            return bucketName;
        }



        #endregion

    }
}
