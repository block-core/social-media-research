using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;

namespace Amazon.DocSamples.S3
{
    class DeleteObjectNonVersionedBucketTest
    {
        private const string bucketName = "filebase-bucket-name";
        private const string keyName = "object-name";
        private string accessKey = "Filebase Access Key";
        private string secretKey = "Filebase Secret Key";
        private static IAmazonS3 client;

        public DeleteObjectNonVersionedBucketTest()
        {

            accessKey = "filebase-access-key";
            secretKey = "filebase-secret-key";

            AmazonS3Config config = new AmazonS3Config()
            {
                ServiceURL = string.Format("<https://s3.filebase.com:443>"),
                UseHttp = true,
                ForcePathStyle = true,
                ProxyHost = "s3.filebase.com",
                ProxyPort = 443
            };
            client = new AmazonS3Client(config);

            DeleteObjectNonVersionedBucketAsync().Wait();
        }
        private static async Task DeleteObjectNonVersionedBucketAsync()
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                Console.WriteLine("Deleting an object");
                await client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
        }
    }
}