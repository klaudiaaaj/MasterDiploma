using AzureServiceBusSubscriberQueue.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AzureServiceBusSubscriber
{

    [ApiController]
    [Route("api/asbConsumer/topic")]
    public class AzureServiceBusClientTopic : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _subscriptionName = string.Empty;
        private readonly ILogger<AzureServiceBusClientTopic> _logger;
        private readonly IAzureServiceBusConnectionFactory azureServiceBusConnectionFactory;
        public AzureServiceBusClientTopic(IConfiguration configuration, ILogger<AzureServiceBusClientTopic> logger, IAzureServiceBusConnectionFactory azureServiceBusGetConnection)
        {
            _configuration = configuration;
            //_subscriptionName = Environment.GetEnvironmentVariable("SUBSCRIPTION_NAME");
            _logger = logger;
            _logger.LogInformation(_subscriptionName);
            this.azureServiceBusConnectionFactory = azureServiceBusGetConnection;
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
                    var messages = await azureServiceBusConnectionFactory.messageReciver.ReceiveMessagesAsync(batchSize, TimeSpan.FromMinutes(2));

                    if (messages.Count == 0)
                    {
                        break;
                    }

                    foreach (var message in messages)
                    {
                        var messageBody = Encoding.UTF8.GetString(message.Body);
                        messagesResult.Add(messageBody);

                        await azureServiceBusConnectionFactory.messageReciver.CompleteMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return Ok(messagesResult);
        }
        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            try
            {
                var messages = await azureServiceBusConnectionFactory.messageReciver.ReceiveMessagesAsync(1, TimeSpan.FromMinutes(2));
                if (messages != null)
                {
                    var message = messages.FirstOrDefault();
                    await azureServiceBusConnectionFactory.messageReciver.CompleteMessageAsync(message);
                    return Ok(Encoding.UTF8.GetString(message.Body));
                }

                else throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
