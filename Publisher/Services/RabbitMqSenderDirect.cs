using Contracts.Models;
using RabbitMQ.Client;
using System.Text;

namespace Publisher.Services
{
    public class RabbitMqSenderDirect : IRabbitMqSenderDirect
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory _connectionFactory;
        private readonly string _queueName;

        public RabbitMqSenderDirect(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
            _queueName = _configuration["RabbitMQQuueName"];
        }

        public Task Send(IList<Joystic> message)
        {
            // Establish a connection to the message broker using the connection factory.
            using var connection = _connectionFactory.CreateConnection();

            // Create a channel within the established connection to interact with the message broker.
            using var channel = connection.CreateModel();

            // Declare a message queue with specific properties.
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Iterate through each Joystic object in the provided message list.
            foreach (Joystic joystic in message)
            {
                // Generate a new unique identifier (GUID) for the message.
                var id = Guid.NewGuid();

                // Publish a message to the specified queue.
                channel.BasicPublish(exchange: "",
                                     routingKey: _queueName,
                                     basicProperties: null,
                                     body: Encoding.UTF8.GetBytes(String.Join(",", joystic.time, joystic.axis_1, joystic.axis_2,
                                                                            joystic.button_1, joystic.button_2, id.ToString())));
            }

            // Indicate the completion of the message sending process.
            return Task.CompletedTask;
        }

    }
}