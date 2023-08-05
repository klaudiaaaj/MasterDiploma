using Azure.Messaging.ServiceBus;
using Contracts.Models;
using System.Text;

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

        public async Task Send(IList<Joystic> message)
        {
            try
            {
                int maxBatchSizeBytes = 200 * 1024; //Defines the maximum size of a message batch in bytes(equivalent to 256 KB)
                List<ServiceBusMessage> serviceBusMessages = new List<ServiceBusMessage>();
                long currentBatchSizeBytes = 0; // Keeps track of the accumulated size of messages within the current batch

                // Iterating through the input message collection
                for (int i = 0; i < message.Count; i++)
                {
                    // Constructs a comma-separated message data by joining different attributes of the input message
                    var messageData = String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1, 
                        message[i].button_2, message[i].id.ToString());
                    var messageBytes = Encoding.UTF8.GetBytes(messageData);// Converts the message data into bytes using the UTF-8 encoding
                    var serviceBusMessage = new ServiceBusMessage(messageBytes); // Creates a new ServiceBusMessage instance using the message bytes

                    long messageSizeBytes = messageBytes.Length; // Calculates the size of the current message in bytes

                    // Checks if adding the current message to the batch would exceed the maximum batch size or the maximum message count
                    if (currentBatchSizeBytes + messageSizeBytes > maxBatchSizeBytes || serviceBusMessages.Count == 1800)
                    {  
                        // Sends the accumulated batch of messages to the Service Bus                    
                        await sender.SendMessagesAsync(serviceBusMessages);
                        serviceBusMessages.Clear(); // Clears the batch list
                        currentBatchSizeBytes = 0; // Resets the batch size tracker
                    }
                    // Adds the current message to the batch and updates the batch size
                    serviceBusMessages.Add(serviceBusMessage);
                    currentBatchSizeBytes += messageSizeBytes;
                }

                // Sends any remaining messages in the batch
                if (serviceBusMessages.Count > 0)
                {
                    await sender.SendMessagesAsync(serviceBusMessages);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

        }
    }
}
