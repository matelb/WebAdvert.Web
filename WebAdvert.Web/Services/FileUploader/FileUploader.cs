using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebAdvert.Web.Services.FileUploader.Interface;


namespace WebAdvert.Web.Services.FileUploader
{
    public class FileUploader : IFileUploader
    {

        private readonly IConfiguration configuration;

        public FileUploader(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<bool> UploadFileAsync(string fileName, Stream storageStream)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(message: "File name must be specified");

            var bucketName = configuration.GetValue<string>("ImageBucket");

            using (var client = new AmazonS3Client(RegionEndpoint.USEast2))
            {
                if(storageStream.Length > 0)
                {
                    if (storageStream.CanSeek)
                        storageStream.Seek(0, SeekOrigin.Begin);
                }

                var request = new PutObjectRequest
                {
                    AutoCloseStream = true,
                    BucketName = bucketName,
                    InputStream = storageStream,
                    Key = fileName
                };

                var response = await client.PutObjectAsync(request).ConfigureAwait(false);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            throw new NotImplementedException();
        }
    }
}
