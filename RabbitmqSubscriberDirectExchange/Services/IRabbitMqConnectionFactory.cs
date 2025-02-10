using RabbitMQ.Client;

namespace RabbitmqSubscriberDirectExchange.Services
{
    public interface IRabbitMqConnectionFactory
    {
        IModel Channel { get; }
        IConnection Connection { get; }
        string QueueName { get; set; }
    }
}