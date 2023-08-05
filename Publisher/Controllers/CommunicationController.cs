using Contracts.Models;
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
        public readonly IList<Joystic> iJoysticList;

        public CommunicationController(IRabbitMqSenderDirect rabbitMqSender, IKaffkaSender kaffkaSender, IAzureServiceBusSender azureServiceBusSender, IDataProducerService dataProducerService, ISqLiteRepo sqLiteRepo, IAzureServiceBusSenderTopic azureServiceBusSenderTopic, IRabbitMqSenderFanout rabbitMqSenderFanout)
        {
            this.rabbitMqSenderDirect = rabbitMqSender;
            this.kaffkaSender = kaffkaSender;
            this.azureServiceBusSenderQueue = azureServiceBusSender;
            this.dataProducerService = dataProducerService;
            this.sqLiteRepo = sqLiteRepo;
            this.azureServiceBusSenderTopic = azureServiceBusSenderTopic;
            this.rabbitMqSenderFanout = rabbitMqSenderFanout;
            iJoysticList = dataProducerService.GetJoysticData();
        }
        [HttpGet("produce")]
        public Task ProduceData()
        {
            return Task.CompletedTask;
        }

        [HttpPost("rabbitMq/direct")]
        public Task SendDataByRabbitMqDirect()
        {
            var task = rabbitMqSenderDirect.Send(iJoysticList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("rabbitMq/fanout")]
        public Task SendDataByRabbitMqFanout()
        {
            var task = rabbitMqSenderFanout.Send(iJoysticList);
            task.Wait();
            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusQueue")]
        public Task SendDataByAzureServiceBus()
        {
            var task = azureServiceBusSenderQueue.Send(iJoysticList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusTopic")]
        public Task SendDataByAzureServiceBusTopic()
        {

            var task = azureServiceBusSenderTopic.Send(iJoysticList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("database")]
        public Task SendBtRestToDatabase()
        {
            if (iJoysticList.Count > 0)
            {
                sqLiteRepo.InsertAllJoystics(iJoysticList);
            }
            return Task.CompletedTask;
        }

        [HttpPost("cleanDatabase")]
        public Task CleanDatabase()
        {
            sqLiteRepo.ClearAllJoystics();
            return Task.CompletedTask;
        }

    }
}
