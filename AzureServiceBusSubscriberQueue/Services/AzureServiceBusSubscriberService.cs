using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Contracts.Models;
using RabbitmqSubscriber.Services;
using System.Text.Json;

namespace AzureServiceBusSubscriber.Services
{
    public class AzureServiceBusSubscriberService : BackgroundService
    {
        private string ConnectionString = ""; //hidden
        private readonly ServiceBusClient client;
        private readonly ServiceBusSender sender;
        private readonly IConfiguration _configuration;
        ServiceBusProcessor processor;
        private readonly IRosContractor ros;

        private readonly ILogger<AzureServiceBusSubscriberService> _logger;

        public AzureServiceBusSubscriberService(IConfiguration configuration, ILogger<AzureServiceBusSubscriberService> logger, IRosContractor ros)
        {
            _configuration = configuration;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var tokenCredential = new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = "ab840be7-206b-432c-bd22-4c20fdc1b261" });
            client = new ServiceBusClient(_configuration["AzureConnectionString"], tokenCredential);
            sender = client.CreateSender(_configuration["Azure_QueueName_Background"]);
            processor = client.CreateProcessor(_configuration["Azure_QueueName_Background"], new ServiceBusProcessorOptions());
            _logger = logger;
            this.ros = ros;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            // stop processing 
            await processor.StopProcessingAsync();
        }
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
            await Task.Run(() => _logger.LogInformation(body));
            ros.GazeboContractor(body);

        }
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
