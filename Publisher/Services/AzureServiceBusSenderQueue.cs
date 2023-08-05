using Azure.Messaging.ServiceBus;
using Contracts.Models;
using System.Text;
using static IronPython.Modules.PythonIterTools;

namespace Publisher.Services
{
    public class AzureServiceBusSenderQueue : IAzureServiceBusSender
    {
        private readonly ServiceBusClient client;
        private readonly ServiceBusSender sender;
        private readonly IConfiguration _configuration;

        public AzureServiceBusSenderQueue(IConfiguration configuration)
        {
            _configuration = configuration;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var connectionString = configuration["AZURE_CONNECTION_STRING"];
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(configuration["Azure_QueueName"]);
        }

        public async Task Send(IList<Joystick> message)
        {
            try
            {
                // Initialize an empty list to hold ServiceBusMessages
                List<ServiceBusMessage> serviceBusMessages = new List<ServiceBusMessage>();

                // Create a new message batch using the sender
                var serviceBusMessageBatch = await sender.CreateMessageBatchAsync();

                // Send the current message batch to the Service Bus
                await sender.SendMessagesAsync(serviceBusMessageBatch);

                // Iterate through the input message collection (up to 10,000 messages)
                for (int i = 0; i < message.Take(10000).Count(); i++)
                {
                    // Construct a comma-separated message data by joining different attributes of the input message
                    var messageBytes = Encoding.UTF8.GetBytes(String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1,
                        message[i].button_2, message[i].id.ToString()));

                    // Try to add the current message to the existing message batch
                    if (!serviceBusMessageBatch.TryAddMessage(new ServiceBusMessage(messageBytes)))
                    {
                        // If adding the message would exceed the batch size or message count, send the current batch and create a new one
                        await sender.SendMessagesAsync(serviceBusMessageBatch);
                        serviceBusMessageBatch.Dispose();
                        serviceBusMessageBatch = await sender.CreateMessageBatchAsync();
                    }
                }

                // Send any remaining messages in the last batch
                await sender.SendMessagesAsync(serviceBusMessageBatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // Clean up and dispose resources
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
