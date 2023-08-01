using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Contracts.Models;
using System.Text;
using System.Text.Json;

namespace Publisher.Services
{
    public class AzureServiceBusSenderQueue : IAzureServiceBusSender
    {
        private string ConnectionString = ""; //hidden
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

        public async Task Send(IList<Joystic> message)
        {
            try
            {
                int maxBatchSizeBytes = 200 * 1024; // 256 KB
                List<ServiceBusMessage> serviceBusMessages = new List<ServiceBusMessage>();
                long currentBatchSizeBytes = 0;

                for (int i = 0; i < message.Count; i++)
                {
                    var messageData = String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1, message[i].button_2, message[i].id.ToString());
                    var messageBytes = Encoding.UTF8.GetBytes(messageData);
                    var serviceBusMessage = new ServiceBusMessage(messageBytes);

                    long messageSizeBytes = messageBytes.Length;

                    if (currentBatchSizeBytes + messageSizeBytes > maxBatchSizeBytes || serviceBusMessages.Count == 1800)
                    {
                        // Wysyłamy aktualną partię wiadomości, ponieważ dodanie następnej spowoduje przekroczenie maksymalnego rozmiaru
                        await sender.SendMessagesAsync(serviceBusMessages);
                        Console.WriteLine($"A batch of {serviceBusMessages.Count} messages has been published to the queue.");
                        serviceBusMessages.Clear();
                        currentBatchSizeBytes = 0;
                    }

                    serviceBusMessages.Add(serviceBusMessage);
                    currentBatchSizeBytes += messageSizeBytes;
                }

                // Wysyłamy pozostałe wiadomości w ostatniej partii
                if (serviceBusMessages.Count > 0)
                {
                    await sender.SendMessagesAsync(serviceBusMessages);
                    Console.WriteLine($"A batch of {serviceBusMessages.Count} messages has been published to the queue.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

        }
    }
}
