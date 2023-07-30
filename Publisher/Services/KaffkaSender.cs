using Confluent.Kafka;
using Contracts.Models;

namespace Publisher.Services
{
    public class KaffkaSender : IKaffkaSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KaffkaSender> _logger;

        public KaffkaSender(IConfiguration configuration, ILogger<KaffkaSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Send(IList<Joystic> message)
        {
            string bootstrapServers = $"{ _configuration["KaffkaHost"]}:{ _configuration["KaffkaPort"]}";
            // Adres serwera Kafka
            string topic = "testtopic";
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    foreach (Joystic joystic in message)
                    {
                        var id = Guid.NewGuid();
                        var deliveryReport = await producer.ProduceAsync(topic, new Message<Null, string> { Value = joystic.axis_1 });
                        Console.WriteLine($"Wiadomość wysłana do Kafka. Temat: {deliveryReport.Topic}, Partycja: {deliveryReport.Partition}, Offset: {deliveryReport.Offset}");
                    }
                }
                catch (ProduceException<Null, string> ex)
                {
                    Console.WriteLine($"Wystąpił błąd podczas wysyłania wiadomości do Kafka: {ex.Error.Reason}");
                }
            }
        }
    }
}
