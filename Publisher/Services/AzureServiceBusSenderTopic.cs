using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Contracts.Models;
using System.Text;
using System.Text.Json;

namespace Publisher.Services
{
    public class AzureServiceBusSenderTopic : IAzureServiceBusSenderTopic
    {
        private readonly IConfiguration _configuration;

        public AzureServiceBusSenderTopic(IConfiguration configuration)
        {
            _configuration = configuration;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
        }

        public async Task Send(IList<Joystic> message)
        {
            try
            {
                var serviceBusClient = new ServiceBusClient(_configuration["AzureConnectionStringTopic"]);
                var sender = serviceBusClient.CreateSender(_configuration["AzureTopic"]);

                int batchSize = 1800;
                List<Task> sendingTasks = new List<Task>();

                for (int i = 0; i < message.Count(); i += batchSize)
                {
                    var batchMessages = message
                        .Skip(i)
                        .Take(batchSize)
                        .Select(msg => String.Join(",", msg.time, msg.axis_1, msg.axis_2, msg.button_1, msg.button_2, msg.id.ToString()))
                        .Select(data => new ServiceBusMessage(data))
                        .ToList();

                    // Wyślij partię wiadomości równolegle
                    sendingTasks.Add(sender.SendMessagesAsync(batchMessages));
                }

                // Czekaj na zakończenie wszystkich zadań wysyłania
                await Task.WhenAll(sendingTasks);

                await serviceBusClient.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
            }
        }
    }
}
