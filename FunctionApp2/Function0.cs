using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FunctionApp2
{
    public static class Function0
    {
        [FunctionName(nameof(Function0))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"{nameof(Function0)} function processed a request.");

            return new OkObjectResult(
                new
                {
                    Name = nameof(FunctionApp2),
                    Version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                });
        }
    }
}
