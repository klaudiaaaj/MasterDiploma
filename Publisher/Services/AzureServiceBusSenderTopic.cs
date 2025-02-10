using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Confluent.Kafka;
using Contracts.Models;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Publisher.Services
{
    public class AzureServiceBusSenderTopic : IAzureServiceBusSenderTopic
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient client;
        private readonly ServiceBusSender sender;
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
            try
            {
                var serviceBusClient = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
                var sender = serviceBusClient.CreateSender(_configuration["AzureTopic"]);

                List<ServiceBusMessage> serviceBusMessages = new List<ServiceBusMessage>();

                var serviceBusMessageBatch = await sender.CreateMessageBatchAsync();

                for (int i = 0; i < message.Count; i++)
                {
                    var messageBytes = Encoding.UTF8.GetBytes(String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1,
                        message[i].button_2, message[i].id.ToString()));

                    if (!serviceBusMessageBatch.TryAddMessage(new ServiceBusMessage(messageBytes)))
                    {
                        await sender.SendMessagesAsync(serviceBusMessageBatch);
                        serviceBusMessageBatch.Dispose();
                        serviceBusMessageBatch = await sender.CreateMessageBatchAsync();
                        serviceBusMessageBatch.TryAddMessage(new ServiceBusMessage(messageBytes));
                    }
                }

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
            }
        }
    }
}

