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

        public RabbitMqSenderDirect(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()

        {
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
        }

        public Task Send(IList<Joystic> message)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "joystic-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            foreach (Joystic joystic in message)
            {
                var id = Guid.NewGuid();
                channel.BasicPublish(exchange: "",
                                                routingKey: "joystic-queue",
                                                basicProperties: null,
                                                body: Encoding.UTF8.GetBytes(String.Join(",", joystic.time, joystic.axis_1,                      joystic.axis_2, joystic.button_1, joystic.button_2, id.ToString())));
            }

            Console.WriteLine(" Press [enter] to exit.");

            return Task.CompletedTask;
        }
    }
}