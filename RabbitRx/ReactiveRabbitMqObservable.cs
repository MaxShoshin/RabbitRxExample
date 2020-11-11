using System;
using RabbitMQ.Client;

namespace RabbitRx
{
    public sealed class ReactiveRabbitMqObservable<TData> : IObservable<Message<TData>>
        where TData : notnull
    {
        private readonly IModel model;
        private readonly string queueName;
        private readonly string consumerTag;
        private readonly ExtractPayload<TData> extractPayload;

        public ReactiveRabbitMqObservable(
            IModel model,
            string queueName,
            string consumerTag,
            ExtractPayload<TData> extractPayload)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            this.consumerTag = consumerTag ?? throw new ArgumentNullException(nameof(consumerTag));
            this.extractPayload = extractPayload ?? throw new ArgumentNullException(nameof(extractPayload));
        }

        public IDisposable Subscribe(IObserver<Message<TData>> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            var tag = model.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumerTag: consumerTag,
                consumer: new ReactiveConsumer<TData>(model, observer, extractPayload));

            return new DisposableAction(() =>
            {
                model.BasicCancel(tag);
            });
        }

        private sealed class DisposableAction : IDisposable
        {
            private readonly Action action;

            public DisposableAction(Action action)
            {
                this.action = action;
            }

            public void Dispose()
            {
                action();
            }
        }
    }
}
