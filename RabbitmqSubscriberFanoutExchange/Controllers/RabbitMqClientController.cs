using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitmqSubscriberDirectExchange.Services;
using System.Text;

namespace RabbitmqSubscriber.Controllers
{
    [ApiController]
    [Route("api/rabbitMqConsumer/fanout")]
    public class RabbitMqClientController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqClientController> _logger;
        private readonly IRabbitMqConnectionFactory rabbitMqConnectionFactory;

        public RabbitMqClientController(IConfiguration configuration, ILogger<RabbitMqClientController> logger,  IRabbitMqConnectionFactory rabbitMqConnectionFactory)
        {
            _configuration = configuration;
            _logger = logger;
            this.rabbitMqConnectionFactory = rabbitMqConnectionFactory;
        }


        [HttpGet("all")]
        public IActionResult GetAllData()
        {
            try
            {
                rabbitMqConnectionFactory.Channel.QueueDeclare(queue: rabbitMqConnectionFactory.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                var dataList = new List<string>();

                while (true)
                {
                    BasicGetResult result = rabbitMqConnectionFactory.Channel.BasicGet(rabbitMqConnectionFactory.QueueName, autoAck: true);

                    if (result != null)
                    {
                        var data = Encoding.UTF8.GetString(result.Body.ToArray());
                        dataList.Add(data);
                    }
                    else
                    {
                        break;
                    }
                }

                if (dataList.Count > 0)
                {
                    return Ok(dataList);
                }
                else
                {
                    return NotFound("No data available in the queue.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("single")]
        public IActionResult GetSingleObjectById()
        {
            try
            {
                rabbitMqConnectionFactory.Channel.QueueDeclare(queue: rabbitMqConnectionFactory.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(rabbitMqConnectionFactory.Channel);
                BasicGetResult result = rabbitMqConnectionFactory.Channel.BasicGet(rabbitMqConnectionFactory.QueueName, autoAck: true);

                if (result != null)
                {
                    var data = Encoding.UTF8.GetString(result.Body.ToArray());
                    return Ok(data);
                }
                else
                {
                    return NotFound("No data available in the queue.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process
                // Return an HTTP 500 Internal Server Error response with the error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
