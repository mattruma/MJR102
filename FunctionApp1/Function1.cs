using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName(nameof(Function1))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] Function1Options options,
            ILogger log)
        {
            log.LogInformation($"{nameof(Function1)} function processed a request.");

            log.LogInformation("Sending {0:N0} message(s) every {1:N0} second(s) for {2:N0} second(s)", options.Quantity, options.Interval, options.Duration);

            var eventHubProducerClient =
                new EventHubProducerClient(
                    options.EventHubConnectionString);

            var timer = new Timer(options.Interval * 1000);

            timer.Elapsed += async (sender, e) => await OnTimerElapsed(eventHubProducerClient, options.Quantity, options.Message, log);

            var startedAt = DateTime.Now;
            var stopAt = startedAt.AddSeconds(options.Duration);

            timer.Start();

            log.LogInformation("{0}: Started", startedAt);

            while (true)
            {
                if (DateTime.Now > stopAt)
                {
                    timer.Stop();
                    break;
                }
            }

            log.LogInformation("{0}: Stopped", DateTime.Now);

            return new OkResult();
        }

        public static async Task SendMessageAsync(
            EventHubProducerClient eventHubProducerClient,
            string message)
        {
            var eventDataBatch =
                 await eventHubProducerClient.CreateBatchAsync();

            var eventData =
                new EventData(
                    Encoding.UTF8.GetBytes(message));

            eventDataBatch.TryAdd(eventData);

            await eventHubProducerClient.SendAsync(eventDataBatch);
        }

        public static async Task OnTimerElapsed(
            EventHubProducerClient eventHubProducerClient,
            int quantity,
            string message,
            ILogger log)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var tasks = new List<Task>();

            for (var messageIndex = 0; messageIndex < quantity; messageIndex++)
            {
                var messageToSend = message;

                messageToSend = messageToSend.Replace("{{CURRENTDATETIME}}", DateTime.UtcNow.ToString());

                tasks.Add(Task.Run(async () => await SendMessageAsync(eventHubProducerClient, messageToSend)));
            }

            var startedAt = DateTime.Now;

            await Task.WhenAll(tasks);

            var ts = stopWatch.Elapsed;

            var elapsedTime =
                (ts.Hours * 60 * 60 * 1000) + (ts.Minutes * 60 * 1000) + (ts.Seconds * 1000) + ts.Milliseconds;

            var elapsedTimeString = $"{ts.Milliseconds} millisecond(s)";

            if (elapsedTime > 1000) elapsedTimeString = $"{ts.Seconds} second(s)";

            log.LogInformation("Sent {1:N0} message(s), took {2}.", quantity, elapsedTimeString);
        }
    }

    public class Function1Options
    {
        public string EventHubConnectionString { get; set; }

        public int Quantity { get; set; } = 10;

        public int Interval { get; set; } = 1;

        public int Duration { get; set; } = 10;

        public string Message { get; set; } = "Hello, world!";

        public Function1Options()
        {
            this.EventHubConnectionString =
                Environment.GetEnvironmentVariable("EventHubConnectionString");

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Message")))
            {
                this.Message =
                    Environment.GetEnvironmentVariable("Message");
            }
        }
    }
}