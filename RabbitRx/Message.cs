using System;
using System.Threading;

namespace RabbitRx
{
    public sealed class Message<TData>
        where TData : notnull
    {
        private const int Accepted = 1;
        private const int Rejected = 2;

        private readonly Ack ack;
        private readonly Nack nack;
        private int acceptedRejectedStatus;

        public Message(TData payload, DeliveryInfo deliveryInfo, Ack ack, Nack nack)
        {
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            DeliveryInfo = deliveryInfo ?? throw new ArgumentNullException(nameof(deliveryInfo));

            this.ack = ack ?? throw new ArgumentNullException(nameof(ack));
            this.nack = nack ?? throw new ArgumentNullException(nameof(nack));
        }

        public TData Payload { get; }

        public DeliveryInfo DeliveryInfo { get; }

        public void Nack(bool requeue = true)
        {
            if (Interlocked.CompareExchange(ref acceptedRejectedStatus, Rejected, 0) == 0)
            {
                nack(DeliveryInfo.DeliveryTag, requeue);
            }
        }

        public void Ack()
        {
            if (Interlocked.CompareExchange(ref acceptedRejectedStatus, Accepted, 0) == 0)
            {
                ack(DeliveryInfo.DeliveryTag);
            }
        }
    }
}
