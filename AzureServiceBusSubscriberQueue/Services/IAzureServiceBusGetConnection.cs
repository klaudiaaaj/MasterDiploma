using Microsoft.Azure.ServiceBus.Core;

namespace AzureServiceBusSubscriberQueue.Services
{
    public interface IAzureServiceBusGetConnection
    {
        IMessageReceiver messageReciver { get; }

        void Dispose();
    }
}