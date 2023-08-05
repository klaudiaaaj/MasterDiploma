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
        public readonly IList<Joystick> iJoystickList;

        public CommunicationController(IRabbitMqSenderDirect rabbitMqSender, IKaffkaSender kaffkaSender, IAzureServiceBusSender azureServiceBusSender, IDataProducerService dataProducerService, ISqLiteRepo sqLiteRepo, IAzureServiceBusSenderTopic azureServiceBusSenderTopic, IRabbitMqSenderFanout rabbitMqSenderFanout)
        {
            this.rabbitMqSenderDirect = rabbitMqSender;
            this.kaffkaSender = kaffkaSender;
            this.azureServiceBusSenderQueue = azureServiceBusSender;
            this.dataProducerService = dataProducerService;
            this.sqLiteRepo = sqLiteRepo;
            this.azureServiceBusSenderTopic = azureServiceBusSenderTopic;
            this.rabbitMqSenderFanout = rabbitMqSenderFanout;
            iJoystickList = dataProducerService.GetJoystickData();
        }
        [HttpGet("produce")]
        public Task ProduceData()
        {
            return Task.CompletedTask;
        }

        [HttpPost("rabbitMq/direct")]
        public Task SendDataByRabbitMqDirect()
        {
            var task = rabbitMqSenderDirect.Send(iJoystickList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("rabbitMq/fanout")]
        public Task SendDataByRabbitMqFanout()
        {
            var task = rabbitMqSenderFanout.Send(iJoystickList);
            task.Wait();
            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusQueue")]
        public Task SendDataByAzureServiceBus()
        {
            var task = azureServiceBusSenderQueue.Send(iJoystickList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("azureServiceBusTopic")]
        public Task SendDataByAzureServiceBusTopic()
        {

            var task = azureServiceBusSenderTopic.Send(iJoystickList);
            task.Wait();

            return Task.CompletedTask;
        }

        [HttpPost("database")]
        public Task SendBtRestToDatabase()
        {
            if (iJoystickList.Count > 0)
            {
                sqLiteRepo.InsertAllJoysticks(iJoystickList);
            }
            return Task.CompletedTask;
        }

        [HttpPost("cleanDatabase")]
        public Task CleanDatabase()
        {
            sqLiteRepo.ClearAllJoysticks();
            return Task.CompletedTask;
        }

    }
}
