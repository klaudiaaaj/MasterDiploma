using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace AzureServiceBusSubscriberQueue.Services
{
    public class AzureServiceBusGetConnection : IDisposable, IAzureServiceBusGetConnection
    {
        private readonly IConfiguration _configuration;
        private readonly IMessageReceiver messageReceiver;
        public AzureServiceBusGetConnection( IConfiguration configuration)
        {
            _configuration = configuration;
            messageReceiver = new MessageReceiver(_configuration["AZURE_CONNECTION_STRING"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);
        }
        public IMessageReceiver messageReciver => messageReceiver;


        public async void Dispose()
        {
            await messageReceiver.CloseAsync();
        }
    }
}
