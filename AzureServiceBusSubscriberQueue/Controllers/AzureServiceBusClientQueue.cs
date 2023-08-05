using AzureServiceBusSubscriber.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace AzureServiceBusSubscriber
{

    [ApiController]
    [Route("api/asbConsumer/queue")]
    public class AzureServiceBusClientQueue : Controller
    {
        private readonly IConfiguration _configuration;

        public AzureServiceBusClientQueue(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            try
            {
                // Create a TaskCompletionSource to track the completion of message reception
                var messageReceivedTaskCompletionSource = new TaskCompletionSource<string>();

                // Create a QueueClient instance to interact with the Azure Service Bus Queue
                var queueClient = new QueueClient(_configuration["AZURE_CONNECTION_STRING"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);

                // Register a message handler to process received messages
                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        try
                        {
                            // Convert message body to string
                            var messageBody = Encoding.UTF8.GetString(message.Body);

                            // Complete the message to mark it as processed
                            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                            // Set the result of the TaskCompletionSource to the received message
                            messageReceivedTaskCompletionSource.SetResult(messageBody);

                            // Close the QueueClient after message processing
                            await queueClient.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            // Set the TaskCompletionSource with an exception if an error occurs
                            messageReceivedTaskCompletionSource.SetException(ex);
                        }
                    },
                    new MessageHandlerOptions(async args => Console.WriteLine(args.Exception))
                    { MaxConcurrentCalls = 1, AutoComplete = true });

                // Wait for the TaskCompletionSource to complete (message received)
                var receivedMessage = await messageReceivedTaskCompletionSource.Task;

                // Return the received message as the response
                return Ok(receivedMessage);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return a 500 Internal Server Error
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {
            List<string> receivedMessages = new List<string>();
            int targetMessageCount = 300000;
            try
            {
                var queueClient = new QueueClient(_configuration["AZURE_CONNECTION_STRING"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);

                var messageHandlerOptions = new MessageHandlerOptions(async args => throw args.Exception)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 50, 
                };

                var messageReceivedTaskCompletionSource = new TaskCompletionSource<bool>();

                queueClient.RegisterMessageHandler(async (message, token) =>
                {
                    try
                    {
                        var messageBody = Encoding.UTF8.GetString(message.Body);
                        receivedMessages.Add(messageBody);

                        if (receivedMessages.Count >= targetMessageCount)
                        {
                            messageReceivedTaskCompletionSource.TrySetResult(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }, messageHandlerOptions);

                await messageReceivedTaskCompletionSource.Task;

                await queueClient.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred during message processing: {ex.Message}");
            }

            return Ok(receivedMessages);
        }
    }

}
