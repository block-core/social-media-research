using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Amazon.DocSamples.S3
{
    class GetObjectTest
    {
        private const string bucketName = "filebase-bucket-name";
        private const string keyName = "object-name";
        private static IAmazonS3 client;
        private string accessKey = "Filebase Access Key";
        private string secretKey = "Filebase Secret Key";

        public GetObjectTest()
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
            ReadObjectDataAsync().Wait();
        }

        static async Task ReadObjectDataAsync()
        {
            string responseBody = "";
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                    string contentType = response.Headers["Content-Type"];
                    Console.WriteLine("Object metadata, Title: {0}", title);
                    Console.WriteLine("Content type: {0}", contentType);

                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                }
            }
            catch (AmazonS3Exception e)
            {
                // If bucket or object does not exist
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
            }
        }
    }
}