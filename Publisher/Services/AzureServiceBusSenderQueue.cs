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
        }

        public async Task Send(IList<Joystick> message)
        {
            try
            {
                var client = new ServiceBusClient(_configuration["AZURE_CONNECTION_STRING"]);
                var sender = client.CreateSender(_configuration["Azure_QueueName"]);


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
                await client.DisposeAsync();
            }
        }
    }
}
