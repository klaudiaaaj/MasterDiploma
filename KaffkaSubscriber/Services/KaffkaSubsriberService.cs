using Kafka.Public;
using Kafka.Public.Loggers;
using System.Text;

public class KaffkaSubsriberService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ClusterClient _clusterClient;

    public KaffkaSubsriberService(IConfiguration configuration)
    {
        _configuration = configuration;
        _clusterClient = new ClusterClient(new Configuration
        {
            Seeds = "localhost:9092"
        }, new ConsoleLogger());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _clusterClient.ConsumeFromLatest("testtopic");
        _clusterClient.MessageReceived += record =>
         {
             var mes = Encoding.UTF8.GetString(record.Value as byte[]);
             Console.WriteLine($"Odebrano wiadomość z Kafka. Temat: {mes}");
         };
        //string bootstrapServers = $"{ _configuration["KaffkaHost"]}:{ _configuration["KaffkaPort"]}";

        //string topic = "testtopic";
        //var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        //using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        //{
        //    consumer.Subscribe(topic);

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            var consumeResult = consumer.Consume(stoppingToken);
        //            Console.WriteLine($"Odebrano wiadomość z Kafka. Temat: {consumeResult.Topic}, Partycja: {consumeResult.Partition}, Offset: {consumeResult.Offset}, Wiadomość: {consumeResult.Message.Value}");
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            // Zakończono oczekiwanie na nową wiadomość
        //        }
        //        catch (ConsumeException ex)
        //        {
        //            Console.WriteLine($"Wystąpił błąd podczas konsumowania wiadomości z Kafka: {ex.Error.Reason}");
        //        }
        //    }

        //    consumer.Close();
        //}
         await Task.CompletedTask;
    }
}
