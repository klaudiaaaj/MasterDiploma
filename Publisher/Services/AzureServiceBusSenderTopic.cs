using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Confluent.Kafka;
using Contracts.Models;
using System.Text;
using System.Text.Json;

namespace Publisher.Services
{
    public class AzureServiceBusSenderTopic : IAzureServiceBusSenderTopic
    {
        private string ConnectionString = ""; //hidden
                                              // private readonly ServiceBusClient client;
                                              //  private readonly ServiceBusSender sender;
        private readonly IConfiguration _configuration;

        public AzureServiceBusSenderTopic(IConfiguration configuration)
        {
            _configuration = configuration;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
        }

        public async Task Send(IList<Joystick> message)
        {

            var serviceBusClient = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
            var sender = serviceBusClient.CreateSender(_configuration["AzureTopic"]);
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
                await serviceBusClient.DisposeAsync();
            }
        }
    }
}
