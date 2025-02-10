using Azure.Messaging.ServiceBus;

namespace AzureServiceBusSubscriberQueue.Services
{
    public interface IAzureServiceBusConnectionFactory
    {
        ServiceBusReceiver messageReciver { get; }

        void Dispose();
    }
}