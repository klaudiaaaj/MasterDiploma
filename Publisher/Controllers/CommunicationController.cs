using Microsoft.AspNetCore.Mvc;
using Publisher.Services;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("api/publisher/produce")]
    public class CommunicationController : Controller
    {
        public readonly IRabbitMqSenderDirect rabbitMqSenderDirect;
        public readonly IRabbitMqSenderFanout rabbitMqSenderFanout;
        public readonly IKaffkaSender kaffkaSender;
        public readonly IAzureServiceBusSender azureServiceBusSenderQueue;
        public readonly IAzureServiceBusSenderTopic azureServiceBusSenderTopic;
        public readonly IDataProducerService dataProducerService;
        public readonly ISqLiteRepo sqLiteRepo;

        public CommunicationController(IRabbitMqSenderDirect rabbitMqSender, IKaffkaSender kaffkaSender, IAzureServiceBusSender azureServiceBusSender, IDataProducerService dataProducerService, ISqLiteRepo sqLiteRepo, IAzureServiceBusSenderTopic azureServiceBusSenderTopic, IRabbitMqSenderFanout rabbitMqSenderFanout)
        {
            this.rabbitMqSenderDirect = rabbitMqSender;
            this.kaffkaSender = kaffkaSender;
            this.azureServiceBusSenderQueue = azureServiceBusSender;
            this.dataProducerService = dataProducerService;
            this.sqLiteRepo = sqLiteRepo;
            this.azureServiceBusSenderTopic = azureServiceBusSenderTopic;
            this.rabbitMqSenderFanout = rabbitMqSenderFanout;
        }

        [HttpPost("rabbitMq/direct")]
        public Task SendDataByRabbitMqDirect()
        {
            var data = dataProducerService.GetJoysticData();
            rabbitMqSenderDirect.Send(data);

            return Task.CompletedTask;
        }

        [HttpPost("rabbitMq/fanout")]
        public Task SendDataByRabbitMqFanout()
        {
            var data = dataProducerService.GetJoysticData();
            rabbitMqSenderFanout.Send(data);

            return Task.CompletedTask;
        }

        [HttpPost("kaffka")]
        public Task SendByKaffka()
        {
            var data = dataProducerService.GetJoysticData();
            kaffkaSender.Send(data);

            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusQueue")]
        public Task SendDataByAzureServiceBus()
        {
            var data = dataProducerService.GetJoysticData();
            azureServiceBusSenderQueue.Send(data);

            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusTopic")]
        public Task SendDataByAzureServiceBusTopic()
        {
            var data = dataProducerService.GetJoysticData();
            azureServiceBusSenderTopic.Send(data);

            return Task.CompletedTask;
        }

        [HttpPost("database")]
        public Task SendBtRestToDatabase()
        {
            var data = dataProducerService.GetJoysticData();
            if (data.Count > 0)
            {
                sqLiteRepo.InsertAllJoystics(data);
            }
            return Task.CompletedTask;
        }
    }
}
