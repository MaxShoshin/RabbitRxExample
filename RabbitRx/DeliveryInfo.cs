using RabbitMQ.Client;

namespace RabbitRx
{
    public record DeliveryInfo
    {
        public ulong DeliveryTag { get; init; }

        public string ConsumerTag { get; init; }

        public bool Redelivered { get; init; }

        public string Exchange { get; init; }

        public string RoutingKey { get; init; }

        public IBasicProperties Properties { get; init; }
    }
}
