using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        /// <param name="quantity">The number of times to call the endpoint.</param>
        /// <param name="endPoint">The endpoint to call, if not provided, can also be provided in appsettings.json.</param>
        /// <param name="fileName">The name of the file to upload to the endpoint.</param>
        static async Task Main(
            int quantity = 10,
            string endPoint = null,
            string fileName = "Sample.json")
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json")
                .Build();

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException($"Parameter --quantity must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentOutOfRangeException($"Parameter --file-name is required.");
            }

            if (string.IsNullOrWhiteSpace(endPoint))
            {
                endPoint = configuration["FunctionEndPoint"];
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentOutOfRangeException($"Parameter --endPoint is required. It can be provide at runtime or stored in appsettings.json.");
            }

            using (var httpClient = new HttpClient())
            {
                var file = File.ReadAllBytes(fileName);

                var byteArrayContent = new ByteArrayContent(file);

                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                for (var index = 1; index <= quantity; index++)
                {
                    Console.WriteLine($"Uploading file {index} of {quantity}...");

                    var response = await httpClient.PostAsync(endPoint, new MultipartFormDataContent());

                    response.EnsureSuccessStatusCode();

                    Console.WriteLine("File uploaded.");
                }

            }

            Console.WriteLine("Press any key to exit...");

            Console.ReadKey();
        }
    }
}
