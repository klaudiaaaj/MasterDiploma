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
                var messageReceivedTaskCompletionSource = new TaskCompletionSource<string>();
                var queueClient = new QueueClient(_configuration["AZURE_CONNECTION_STRING"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);

                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        try
                        {
                            var messageBody = Encoding.UTF8.GetString(message.Body);
                            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                            messageReceivedTaskCompletionSource.SetResult(messageBody);
                            await queueClient.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            messageReceivedTaskCompletionSource.SetException(ex);
                        }
                    },
                    new MessageHandlerOptions(async args => Console.WriteLine(args.Exception))
                    { MaxConcurrentCalls = 1, AutoComplete = true });

                var receivedMessage = await messageReceivedTaskCompletionSource.Task; // Czekaj na dostanie wiadomości lub na wyjątek

                return Ok(receivedMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {
            List<string> receivedMessages = new List<string>();
            int targetMessageCount = 300000;
            int maxWaitSeconds = 60; 
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
                        Console.WriteLine($"Error processing message: {ex.Message}");
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
