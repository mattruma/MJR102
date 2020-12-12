using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class Function2
    {
        [FunctionName(nameof(Function2))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"{nameof(Function2)} function processed a request.");

            var multipartMemoryStreamProvider =
                new MultipartMemoryStreamProvider();

            if (!req.Content.IsMimeMultipartContent())
            {
                log.LogError("No file was provided.");

                return new BadRequestObjectResult("At least one file is required.");
            }

            await req.Content.ReadAsMultipartAsync(multipartMemoryStreamProvider);

            var fileNames = new List<string>();

            log.LogInformation($"Uploading {multipartMemoryStreamProvider.Contents.Count} files...");

            foreach (var file in multipartMemoryStreamProvider.Contents)
            {
                var fileInfo =
                    file.Headers.ContentDisposition;

                fileInfo.FileName =
                    fileInfo.FileName.Replace("\\", "").Replace("\"", "");

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(fileInfo.FileName)}";

                log.LogInformation($"Uploading {fileName}...");

                log.LogInformation(
                    JsonConvert.SerializeObject(fileInfo, Formatting.Indented));

                var storageContainerEndPoint =
                    Environment.GetEnvironmentVariable("StorageContainerEndPoint");

                // https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi
                // https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet

                // If running local, EnvironmentCredentials will be used, when running in Azure ManagedIdentityCredentials will be used

                var tokenCredential = new DefaultAzureCredential();

                var containerClient =
                    new BlobContainerClient(new Uri(storageContainerEndPoint), tokenCredential);

                containerClient.CreateIfNotExists();

                await containerClient.UploadBlobAsync(fileName, await file.ReadAsStreamAsync());

                log.LogInformation($"File uploaded.");

                fileNames.Add(fileName);
            }

            log.LogInformation($"Files uploaded.");

            return new OkObjectResult(fileNames);
        }
    }
}
