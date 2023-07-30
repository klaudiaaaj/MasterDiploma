using Azure.Identity;
using Azure.Messaging.ServiceBus;
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
         //   var tokenCredential = new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = "ab840be7-206b-432c-bd22-4c20fdc1b261" });
           // client = new ServiceBusClient(_configuration[", tokenCredential);
           // sender = client.CreateSender(_configuration["Azure_QueueName"]);
        }

        public async Task Send(IList<Joystic> message)
        {
            try
            {
                await using var serviceBusClient = new ServiceBusClient("Endpoint=sb://azure-service-bus-master.servicebus.windows.net/;SharedAccessKeyName=reciver;SharedAccessKey=i5GWDQb4JKtKEc/uRYp7kjzFYzUtTCX3N+ASbCUo4bY=;EntityPath=bulk-send");
                var sender = serviceBusClient.CreateSender("bulk-send");

                List<ServiceBusMessage> serviceBusMessages = new();
                var serviceBusMessageBatch = await sender.CreateMessageBatchAsync();

                for (int i = 0; i < message.Count(); i++)
                {
                    var message2 = String.Join(",", message[i].time, message[i].axis_1, message[i].axis_2, message[i].button_1, message[i].button_2, message[i].id.ToString());

                    if (!serviceBusMessageBatch.TryAddMessage(new ServiceBusMessage(message2)))
                    {
                        await sender.SendMessagesAsync(serviceBusMessageBatch);
                        serviceBusMessageBatch.Dispose();
                        serviceBusMessageBatch = await sender.CreateMessageBatchAsync();
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
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
