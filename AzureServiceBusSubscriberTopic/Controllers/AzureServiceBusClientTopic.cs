using Azure.Messaging.ServiceBus;
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
        public AzureServiceBusClientTopic(IConfiguration configuration, ILogger<AzureServiceBusClientTopic> logger)
        {
            _configuration = configuration;
            _subscriptionName = configuration["SUBSCRIPTION_NAME"];
            _logger = logger;
            _logger.LogInformation("Subscription", _subscriptionName);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {
            List<string> messagesResult = new List<string>();
            try
            {
                await using var serviceBusClient = new ServiceBusClient("Endpoint=sb://azure-service-bus-master.servicebus.windows.net/;SharedAccessKeyName=reciver;SharedAccessKey=i5GWDQb4JKtKEc/uRYp7kjzFYzUtTCX3N+ASbCUo4bY=;EntityPath=bulk-send");
                await using var receiver = serviceBusClient.CreateReceiver("bulk-send", _subscriptionName);
                while (messagesResult.Count < 1000)
                {

                    var messages = await receiver.ReceiveMessagesAsync(1000 - messagesResult.Count, TimeSpan.FromMinutes(2));
                    foreach (var message in messages)
                    {
                        var messageBody = Encoding.UTF8.GetString(message.Body);
                        messagesResult.Add(messageBody);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Ok(messagesResult);
        }
        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            List<string> messagesResult = new List<string>();
            try
            {
                await using var serviceBusClient = new ServiceBusClient("Endpoint=sb://azure-service-bus-master.servicebus.windows.net/;SharedAccessKeyName=reciver;SharedAccessKey=i5GWDQb4JKtKEc/uRYp7kjzFYzUtTCX3N+ASbCUo4bY=;EntityPath=bulk-send");
                await using var receiver = serviceBusClient.CreateReceiver("bulk-send", _subscriptionName);

                var messages = await receiver.ReceiveMessagesAsync(1, TimeSpan.FromMinutes(2));
                if (messages != null)
                {
                    var message = messages.FirstOrDefault();
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
