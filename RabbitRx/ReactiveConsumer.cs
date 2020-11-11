using System;
using RabbitMQ.Client;

namespace RabbitRx
{
    internal sealed class ReactiveConsumer<TData> : DefaultBasicConsumer
        where TData : notnull
    {
        private readonly IObserver<Message<TData>> observer;
        private readonly ExtractPayload<TData> extractPayload;

        public ReactiveConsumer(
            IModel model,
            IObserver<Message<TData>> observer,
            ExtractPayload<TData> extractPayload)
            : base(model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
            this.extractPayload = extractPayload ?? throw new ArgumentNullException(nameof(extractPayload));
        }

        public override void OnCancel(params string[] consumerTags)
        {
            if (consumerTags is null) throw new ArgumentNullException(nameof(consumerTags));

            base.OnCancel(consumerTags);

            observer.OnCompleted();
        }

        public override void HandleBasicDeliver(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IBasicProperties properties,
            ReadOnlyMemory<byte> body)
        {
            if (consumerTag is null) throw new ArgumentNullException(nameof(consumerTag));
            if (exchange is null) throw new ArgumentNullException(nameof(exchange));
            if (routingKey is null) throw new ArgumentNullException(nameof(routingKey));
            if (properties is null) throw new ArgumentNullException(nameof(properties));

            var deliveryInfo = new DeliveryInfo
            {
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Redelivered = redelivered,
                Exchange = exchange,
                RoutingKey = routingKey,
                Properties = properties,
            };

            var payload = extractPayload(body, deliveryInfo);
            var message = new Message<TData>(payload, deliveryInfo, Ack, Nack);

            observer.OnNext(message);
        }

        private void Ack(ulong deliveryTag)
        {
            Model.BasicAck(deliveryTag, multiple: false);
        }

        private void Nack(ulong deliveryTag, bool requeue)
        {
            Model.BasicNack(deliveryTag, multiple: false, requeue: requeue);
        }
    }
}
