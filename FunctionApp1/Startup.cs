using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(FunctionApp1.Startup))]
namespace FunctionApp1
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(
            IFunctionsHostBuilder builder)
        {
            var services =
                builder.Services;

            services.AddHttpClient();

            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(new Uri(Environment.GetEnvironmentVariable("StorageEndPoint")))
                    .WithCredential(new DefaultAzureCredential());
            });
        }
    }
}
