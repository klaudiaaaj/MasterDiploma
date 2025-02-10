using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitmqSubscriber.Services
{
    public class RabbitMqSubscriberService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory _connectionFactory;
        private string _queueName;
        private readonly ILogger<RabbitMqSubscriberService> _logger;
        private readonly IRosContractor ros;

        public RabbitMqSubscriberService(IConfiguration configuration, ILogger<RabbitMqSubscriberService> logger, IRosContractor ros)
        {
            _configuration = configuration;
            _logger = logger;
            InitializeRabbitMQ();
            this.ros = ros;
        }

        private void InitializeRabbitMQ()
        {
            var ets = _configuration["RabbitMQHost"];
            var jsj = _configuration["RabbitMQPort"];
            _logger.LogInformation(ets);
            _logger.LogInformation(jsj);
            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _queueName = "Joystick-queue";
            _channel.QueueBind(queue: _queueName, routingKey: "Joystick-queue", exchange: "amq.topic");

            Console.WriteLine("--> Listenting on the Message Bus...");

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShitdown;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (ModuleHandle, ea) =>
            {
                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
                await Task.Run(() => _logger.LogInformation(notificationMessage));
                ros.GazeboContractor(notificationMessage);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        }

        public override void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }

            base.Dispose();
        }
        private void RabbitMQ_ConnectionShitdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection Shutdown");
        }
    }
}
