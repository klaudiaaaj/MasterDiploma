using Contracts.Models;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using static IronPython.SQLite.PythonSQLite;

namespace Publisher.Services
{
    public class RabbitMqSenderFanout : IRabbitMqSenderFanout
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory _connectionFactory;

        public RabbitMqSenderFanout(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()

        {
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
        }

        public Task Send(IList<Joystick> message)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "my-fanout-exchange", type: ExchangeType.Fanout);
          
            channel.QueueDeclare("consumer1", durable: false, autoDelete: false, exclusive: false);
            channel.QueueDeclare("consumer2", durable: false, autoDelete: false, exclusive: false);
            channel.QueueDeclare("consumer3", durable: false, autoDelete: false, exclusive: false);
            channel.QueueDeclare("consumer4", durable: false, autoDelete: false, exclusive: false);
            channel.QueueDeclare("consumer5", durable: false, autoDelete: false, exclusive: false);

            channel.QueueBind("consumer1", "my-fanout-exchange", "");
            channel.QueueBind("consumer2", "my-fanout-exchange", "");
            channel.QueueBind("consumer3", "my-fanout-exchange", "");
            channel.QueueBind("consumer4", "my-fanout-exchange", "");
            channel.QueueBind("consumer5", "my-fanout-exchange", "");


            foreach (Joystick Joystick in message)
            {
                var id = Guid.NewGuid();
                channel.BasicPublish(exchange: "my-fanout-exchange",
                                                routingKey: "",
                                                basicProperties: null,
                                                body: Encoding.UTF8.GetBytes(String.Join(",", Joystick.time, Joystick.axis_1, Joystick.axis_2, Joystick.button_1, Joystick.button_2, id.ToString())));
            }

            return Task.CompletedTask;
        }
    }
}