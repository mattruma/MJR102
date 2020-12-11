using System;

namespace FunctionApp1
{
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
