using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusSubscriber.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private string ConnectionString = ""; // hidden
        private readonly ServiceBusClient client;
        private readonly ServiceBusSender sender;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor processor;
        private ManualResetEventSlim messageReceivedEvent;
        private string messageBody = string.Empty;

        public DataController(IConfiguration configuration)
        {
            _configuration = configuration;
            messageReceivedEvent = new ManualResetEventSlim(false);

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var tokenCredential = new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = "ab840be7-206b-432c-bd22-4c20fdc1b261" });
            client = new ServiceBusClient(_configuration["AzureConnectionString"], tokenCredential);
            sender = client.CreateSender(_configuration["Azure_QueueName"]);
            processor = client.CreateProcessor(_configuration["Azure_QueueName"], new ServiceBusProcessorOptions());
        }

        [HttpGet]
        public IActionResult GetSingleData()
        {
            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                processor.StartProcessingAsync();

                // Wait for message to be received
                messageReceivedEvent.Wait();

                // stop processing
                processor.StopProcessingAsync();

                return Ok(messageBody);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                string body = args.Message.Body.ToString();
                messageBody = body;

                 // complete the message. message is deleted from the queue.
                 await args.CompleteMessageAsync(args.Message);

                // Signal that the message has been received
                messageReceivedEvent.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing message: {ex.Message}");
            }
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
