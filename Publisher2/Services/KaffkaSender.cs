using Confluent.Kafka;
using Contracts.Models;

namespace Publisher.Services
{
    public class KaffkaSender : IKaffkaSender
    {
        public KaffkaSender()
        {
        }

        public async Task Send(IList<Joystic> message)
        {
            string bootstrapServers = "localhost:9092"; // Adres serwera Kafka
            string topic = "my-topic"; // Nazwa tematu w Kafka
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            using (var producer = new ProducerBuilder<Null, Joystic>(config).Build())
            {
                try
                {
                    foreach (Joystic joystic in message)
                    {
                        var id = Guid.NewGuid();
                        var deliveryReport = await producer.ProduceAsync(topic, new Message<Null, Joystic> { Value = joystic });
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
