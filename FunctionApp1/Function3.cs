using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    // https://www.pluralsight.com/guides/how-to-use-managed-identity-with-azure-blob-and-queue-storage
    public class Function3
    {
        private readonly BlobServiceClient _blobServiceClient;

        public Function3(
            BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        [FunctionName(nameof(Function3))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"{nameof(Function3)} function processed a request.");

            var multipartMemoryStreamProvider =
                new MultipartMemoryStreamProvider();

            if (!req.Content.IsMimeMultipartContent())
            {
                log.LogError("No file(s) were provided.");

                return new BadRequestObjectResult("At least one file is required.");
            }

            if (multipartMemoryStreamProvider.Contents.Any(x => !x.Headers.ContentDisposition.FileName.EndsWith(".json")))
            {
                log.LogError("Something other than a json file was sent.");

                return new BadRequestObjectResult("Only json files are supported.");
            }

            await req.Content.ReadAsMultipartAsync(multipartMemoryStreamProvider);

            var fileNames = new List<string>();

            log.LogInformation($"Uploading {multipartMemoryStreamProvider.Contents.Count} files...");

            var blobContainerClient =
                _blobServiceClient.GetBlobContainerClient(
                    Environment.GetEnvironmentVariable("StorageContainerName"));

            await blobContainerClient.CreateIfNotExistsAsync();

            foreach (var file in multipartMemoryStreamProvider.Contents)
            {
                var fileName =
                    file.Headers.ContentDisposition.FileName.Replace("\\", "").Replace("\"", "");

                log.LogInformation($"Uploading {fileName}...");

                var blobClient =
                    blobContainerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(
                    await file.ReadAsStreamAsync(),
                    new BlobHttpHeaders { ContentType = "application/json" },
                    conditions: null); // Setting conditions to null will overwrite the blob, if it exists

                log.LogInformation($"File uploaded.");

                fileNames.Add(fileName);
            }

            log.LogInformation($"Files uploaded.");

            return new OkObjectResult(fileNames);
        }
    }
}
