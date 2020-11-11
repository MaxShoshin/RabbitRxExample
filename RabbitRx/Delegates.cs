using System;

namespace RabbitRx
{
    public delegate void Ack(ulong deliveryTag);

    public delegate void Nack(ulong deliveryTag, bool requeue);

    public delegate TData ExtractPayload<out TData>(ReadOnlyMemory<byte> body, DeliveryInfo deliveryInfo)
        where TData : notnull;
}
