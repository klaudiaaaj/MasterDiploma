using Azure.Messaging.ServiceBus;

namespace AzureServiceBusSubscriberQueue.Services
{
    public class AzureServiceBusConnectionFactory : IDisposable, IAzureServiceBusConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusReceiver _reciver;
        private readonly string _subscriptionName = string.Empty;

        public AzureServiceBusConnectionFactory( IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
            _subscriptionName = Environment.GetEnvironmentVariable("SUBSCRIPTION_NAME");
            _reciver = _client.CreateReceiver(_configuration["AzureTopic"], _subscriptionName);
        }
        public ServiceBusReceiver messageReciver => _reciver;


        public async void Dispose()
        {
            await _reciver.CloseAsync();
        }
    }
}
