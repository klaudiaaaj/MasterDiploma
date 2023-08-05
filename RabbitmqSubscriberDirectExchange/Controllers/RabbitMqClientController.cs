using Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitmqSubscriber.Controllers
{
    [ApiController]
    [Route("api/rabbitMqConsumer/direct")]
    public class RabbitMqClientController : Controller
    {
        private readonly IConfiguration _configuration;
        private ConnectionFactory _connectionFactory;
        private string _queueName;
        private readonly TaskCompletionSource<Joystick> _completionSource = new TaskCompletionSource<Joystick>();
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public RabbitMqClientController(IConfiguration configuration)
        {
            _configuration = configuration;
            var rabbitMQPortValue = _configuration["RabbitMQPort"];
            var port = int.Parse(rabbitMQPortValue ?? "0");

            var test = _configuration["RabbitMQHost"];
            var test2 = _configuration["RabbitMQPort"];
            _queueName = "Joystick-queue";
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = port };

            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            _configuration = configuration;
        }

        [HttpGet("single")]
        public IActionResult GetSingleObjectById()
        {
            try
            {
                // Create a connection factory instance based on the injected _connectionFactory
                var factory = _connectionFactory;

                // Using a connection and a channel within a using block to ensure proper resource disposal
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declare a queue with specified properties
                    channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    // Create a consumer for the channel
                    var consumer = new EventingBasicConsumer(channel);

                    // Attempt to get a message from the queue
                    BasicGetResult result = channel.BasicGet(_queueName, autoAck: true);

                    // Check if a message was retrieved from the queue
                    if (result != null)
                    {
                        // Convert the message body to a UTF-8 encoded string
                        var data = Encoding.UTF8.GetString(result.Body.ToArray());

                        // Return an HTTP 200 OK response with the retrieved data
                        return Ok(data);
                    }
                    else
                    {
                        // If no message was found, return an HTTP 404 NotFound response
                        return NotFound("No data available in the queue.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process
                // Return an HTTP 500 Internal Server Error response with the error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        [HttpGet("all")]
        public IActionResult GetAllData()
        {
            try
            {
                var factory = _connectionFactory; // Create an instance of the connection factory

                // Establish connection and channel within using blocks for proper resource management
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declare a queue with specific properties
                    channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var dataList = new List<string>(); // Initialize a list to store retrieved data

                    // Continuously retrieve messages from the queue until it is empty
                    while (true)
                    {
                        // Attempt to retrieve a message from the queue
                        BasicGetResult result = channel.BasicGet(_queueName, autoAck: true);

                        if (result != null)
                        {
                            // Convert the message body to a UTF-8 encoded string and add to the list
                            var data = Encoding.UTF8.GetString(result.Body.ToArray());
                            dataList.Add(data);
                        }
                        else
                        {
                            // Exit the loop if the queue is empty
                            break;
                        }
                    }

                    if (dataList.Count > 0)
                    {
                        // Return an HTTP 200 OK response containing the list of retrieved data
                        return Ok(dataList);
                    }
                    else
                    {
                        // If no data was retrieved, return an HTTP 404 NotFound response
                        return NotFound("No data available in the queue.");
                    }
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
