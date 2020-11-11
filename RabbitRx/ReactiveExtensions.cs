using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace RabbitRx
{
    public static class ReactiveExtensions
    {
        public static IObservable<Message<TData>> Ack<TData>(this IObservable<Message<TData>> source)
            where TData : notnull
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return source.Do(message =>
            {
                message.Ack();
            });
        }

        public static IObservable<IList<Message<TData>>> Ack<TData>(this IObservable<IList<Message<TData>>> source)
            where TData : notnull
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return source.Do(messages =>
            {
                foreach (var message in messages)
                {
                    message.Ack();
                }
            });
        }
    }
}
