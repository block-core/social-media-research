using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.Threading.Tasks;

namespace Amazon.DocSamples.S3
{
    class CreateBucketTest
    {
        private const string bucketName = "new-filebase-bucket";
        private static IAmazonS3 s3Client;
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

            AWSCredentials creds = new BasicAWSCredentials(accessKey, secretKey);
            /*AmazonS3Client*/ s3Client = new AmazonS3Client(creds, config);
            {
                CreateBucketAsync().Wait();
            }

            static async Task CreateBucketAsync()
            {
                try
                {
                    if (!(await AmazonS3Util.DoesS3BucketExistAsync(s3Client, bucketName)))
                    {
                        var putBucketRequest = new PutBucketRequest
                        {
                            BucketName = bucketName,
                        };

                        PutBucketResponse putBucketResponse = await s3Client.PutBucketAsync(putBucketRequest);
                    }
                    string bucketLocation = await FindBucketLocationAsync(s3Client);
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                }
            }
            static async Task<string> FindBucketLocationAsync(IAmazonS3 client)
            {
                string bucketLocation;
                var request = new GetBucketLocationRequest()
                {
                    BucketName = bucketName
                };
                GetBucketLocationResponse response = await client.GetBucketLocationAsync(request);
                bucketLocation = response.Location.ToString();
                return bucketLocation;
            }
        }
    }
}