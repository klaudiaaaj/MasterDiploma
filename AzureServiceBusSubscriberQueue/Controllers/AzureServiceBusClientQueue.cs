using AzureServiceBusSubscriber.Services;
using AzureServiceBusSubscriberQueue.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AzureServiceBusSubscriber
{

    [ApiController]
    [Route("api/asbConsumer/queue")]
    public class AzureServiceBusClientQueue : Controller, IDisposable
    {
        private readonly IAzureServiceBusGetConnection azureServiceBusGetConnection;
        private readonly IConfiguration _configuration;
        public AzureServiceBusClientQueue(IAzureServiceBusGetConnection azureServiceBusGetConnection, IConfiguration configuration)
        {
            this.azureServiceBusGetConnection = azureServiceBusGetConnection;
            this._configuration = configuration;
        }

        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            try
            {
                IMessageReceiver messageReceiver = azureServiceBusGetConnection.messageReciver;
                Message message = await messageReceiver.ReceiveAsync();
                await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                return Ok(Encoding.UTF8.GetString(message.Body));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {

            List<string> messagesResult = new List<string>();
            try
            {
                int batchSize = 256;
                while (true)
                {
                    IMessageReceiver messageReceiver = azureServiceBusGetConnection.messageReciver;
                    var messages = await azureServiceBusGetConnection.messageReciver.ReceiveAsync(batchSize);

                    if (messages.Count == 0)
                    {
                        break;
                    }

                    foreach (var message in messages)
                    {
                        var messageBody = Encoding.UTF8.GetString(message.Body);
                        messagesResult.Add(messageBody);

                        await azureServiceBusGetConnection.messageReciver.CompleteAsync(message.SystemProperties.LockToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred during message processing: {ex.Message}");
            }

            return Ok(messagesResult);
        }
    }

}
