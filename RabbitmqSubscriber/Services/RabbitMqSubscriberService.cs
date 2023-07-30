//using Contracts.Models;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text;

//namespace RabbitmqSubscriber.Services
//{
//    public class RabbitMqSubscriberService : IRabbitMqSubscriberService
//    {
//        private readonly IConfiguration _configuration;
//        private IConnection _connection;
//        private IModel _channel;
//        private ConnectionFactory _connectionFactory;
//        private string _queueName;
//        private readonly ILogger<RabbitMqSubscriberService> _logger;
//        private readonly RosContractor ros;
//        private readonly TaskCompletionSource<Joystic> _completionSource = new TaskCompletionSource<Joystic>();
//        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

//        public RabbitMqSubscriberService(IConfiguration configuration, ILogger<RabbitMqSubscriberService> logger)
//        {
//            _configuration = configuration;
//            _logger = logger;
//            InitializeRabbitMQ();
//            ros = new RosContractor();
//        }

//        private void InitializeRabbitMQ()
//        {
//            var ets = _configuration["RabbitMQHost"];
//            var jsj = _configuration["RabbitMQPort"];
//            _logger.LogInformation(ets);
//            _logger.LogInformation(jsj);
//            _connectionFactory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
//            _connection = _connectionFactory.CreateConnection();
//            _channel = _connection.CreateModel();
//            bool durable = false;
//            bool exclusive = false;
//            bool autoDelete = false;
//            _queueName = "joystic-queue";
//            _channel.QueueDeclare(_queueName, durable, exclusive, autoDelete, null);
//            Console.WriteLine("--> Listenting on the Message Bus...");

//            _connection.ConnectionShutdown += RabbitMQ_ConnectionShitdown;
//        }

//        public async Task<string?> ExecuteAsyncSingle()
//        {
//            var consumer = new EventingBasicConsumer(_channel);
//            string? joystic = string.Empty;
//            consumer.Received += (model, eventArgs) =>
//            {
//                var body = eventArgs.Body.ToArray();
//                var message = Encoding.UTF8.GetString(body);
//                joystic = message;
//                Console.WriteLine($"Product message received: {message}");
//            };
//            //read the message
//           _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
//            _connection = null;

//            return joystic;
//        }

//        private void RabbitMQ_ConnectionShitdown(object sender, ShutdownEventArgs e)
//        {
//            Console.WriteLine("--> Connection Shutdown");
//        }
//    }
//}
