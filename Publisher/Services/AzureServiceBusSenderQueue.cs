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
            client = new ServiceBusClient("Endpoint=sb://azure-service-bus-master.servicebus.windows.net/;SharedAccessKeyName=jmeter;SharedAccessKey=v0Y7610JrHUC1pCzCApZ8+0MRq4OTW9fC+ASbNK4WG0=;EntityPath=jmeter");
            sender = client.CreateSender(_configuration["Azure_QueueName"]);
        }

        public async Task Send(IList<Joystic> message)  
        {
            try
            {
                var list = message.Take(50);

                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                for (int i = 0; i < list.Count(); i++)
                {
                    var message2 = String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1, message[i].button_2, message[i].id.ToString());

                    if (!messageBatch.TryAddMessage(new ServiceBusMessage(Encoding.UTF8.GetBytes(message2))))
                    {
                        // if it is too large for the batch
                        throw new Exception($"The message {i} is too large to fit in the batch.");
                    }
                }
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {messageBatch.Count} messages has been published to the queue.");
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

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
