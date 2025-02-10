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

        public Task Send(IList<Joystick> message)
        {
            // Establish a connection to the message broker using the connection factory.
            using var connection = _connectionFactory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            foreach (Joystick Joystick in message)
            {
                var id = Guid.NewGuid();

                channel.BasicPublish(exchange: "",
                                     routingKey: _queueName,
                                     basicProperties: null,
                                     body: Encoding.UTF8.GetBytes(String.Join(",", Joystick.time, Joystick.axis_1, Joystick.axis_2,
                                                                            Joystick.button_1, Joystick.button_2, id.ToString())));
            }

            return Task.CompletedTask;
        }

    }
}
