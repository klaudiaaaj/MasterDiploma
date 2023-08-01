using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
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
        public AzureServiceBusClientTopic(IConfiguration configuration, ILogger<AzureServiceBusClientTopic> logger)
        {
            _configuration = configuration;
            _subscriptionName = "sub1";
            //_subscriptionName = Environment.GetEnvironmentVariable("SUBSCRIPTION_NAME");
            _logger = logger;
            _logger.LogInformation(_subscriptionName);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {
            List<string> messagesResult = new List<string>();

            try
            {
                await using var serviceBusClient = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
                await using var receiver = serviceBusClient.CreateReceiver(_configuration["AzureTopic"], _subscriptionName);

                int batchSize = 256;
                while (true)
                {
                    var messages = await receiver.ReceiveMessagesAsync(batchSize, TimeSpan.FromMinutes(2));

                    if (messages.Count == 0)
                    {
                        break;
                    }

                    foreach (var message in messages)
                    {
                        var messageBody = Encoding.UTF8.GetString(message.Body);
                        messagesResult.Add(messageBody);

                        await receiver.CompleteMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred during message processing: {ex.Message}");
            }

            return Ok(messagesResult);
        }
        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            List<string> messagesResult = new List<string>();
            try
            {
                await using var serviceBusClient = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
                await using var receiver = serviceBusClient.CreateReceiver(_configuration["AzureTopic"], _subscriptionName);

                var messages = await receiver.ReceiveMessagesAsync(1, TimeSpan.FromMinutes(2));
                if (messages != null)
                {
                    var message = messages.FirstOrDefault();
                    await receiver.CompleteMessageAsync(message);
                    return Ok(Encoding.UTF8.GetString(message.Body));
                }

                else throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
