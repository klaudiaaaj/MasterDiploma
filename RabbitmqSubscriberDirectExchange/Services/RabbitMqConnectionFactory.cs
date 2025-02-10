using RabbitMQ.Client;
using static IronPython.SQLite.PythonSQLite;

namespace RabbitmqSubscriberDirectExchange.Services
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;
        private string _queueName = string.Empty;


        public RabbitMqConnectionFactory(IConfiguration configuration)
        {

            _configuration = configuration;
            var rabbitMQPortValue = _configuration["RabbitMQPort"];
            var port = int.Parse(rabbitMQPortValue ?? "0");
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = port };
            QueueName = "Joystick-queue";
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public IModel Channel => _channel;

        public IConnection Connection => _connection;

        public string QueueName { get => _queueName; set => _queueName = value; }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
