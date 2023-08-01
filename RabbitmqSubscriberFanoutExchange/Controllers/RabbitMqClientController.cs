using Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitmqSubscriber.Controllers
{
    [ApiController]
    [Route("api/rabbitMqConsumer/fanout")]
    public class RabbitMqClientController : Controller
    {
        private readonly IConfiguration _configuration;
        private ConnectionFactory _connectionFactory;
        private string _queueName;
        private readonly ILogger<RabbitMqClientController> _logger;
        private readonly TaskCompletionSource<Joystic> _completionSource = new TaskCompletionSource<Joystic>();
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public RabbitMqClientController(IConfiguration configuration, ILogger<RabbitMqClientController> logger)
        {
            _configuration = configuration;
            var rabbitMQPortValue = _configuration["RabbitMQPort"];
            var port = int.Parse(rabbitMQPortValue ?? "0");
            _logger = logger;
            var test = _configuration["RabbitMQHost"];
            var test2 = _configuration["RabbitMQPort"];
            _queueName = Environment.GetEnvironmentVariable("QUEUE_NAME");
            _logger.LogInformation( _queueName);
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = port };

            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("single")]
        public IActionResult GetSingleObjectById()
        {
            try
            {
                var factory = _connectionFactory;
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    var consumer = new EventingBasicConsumer(channel);

                    BasicGetResult result = channel.BasicGet(_queueName, autoAck: true);

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
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAllData()
        {
            try
            {
                var factory = _connectionFactory;
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    var dataList = new List<string>();

                    while (true)
                    {
                        BasicGetResult result = channel.BasicGet(_queueName, autoAck: true);

                        if (result != null)
                        {
                            var data = Encoding.UTF8.GetString(result.Body.ToArray());
                            dataList.Add(data);
                        }
                        else
                        {
                            break; // Wyjdź z pętli, jeśli kolejka jest pusta
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
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
