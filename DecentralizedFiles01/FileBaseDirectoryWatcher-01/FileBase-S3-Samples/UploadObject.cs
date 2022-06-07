using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;

namespace Amazon.DocSamples.S3
{
    class UploadObjectTest
    {
        private const string bucketName = "filebase-bucket-name";
        private const string keyName = "object-name";
        private const string filePath = @"/path/to/object/to/upload";

        private static IAmazonS3 client;

        public static void Main()
        {
            string accessKey = "filebase-access-key";
            string secretKey = "filebase-secret-key";
            AmazonS3Config config = new AmazonS3Config()
            {
                ServiceURL = string.Format("<https://s3.filebase.com:443>"),
                UseHttp = true,
                ForcePathStyle = true,
                ProxyHost = "s3.filebase.com",
                ProxyPort = 443
            };
            client = new AmazonS3Client(config);
            WritingAnObjectAsync().Wait();
        }

        static async Task WritingAnObjectAsync()
        {
            try
            {
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = "sample text"
                };

                PutObjectResponse response1 = await client.PutObjectAsync(putRequest1);

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
            }
        }
    }
}