# RabbitRx Example
Example for link [RabbitMQ](https://github.com/rabbitmq/rabbitmq-dotnet-client) and [Reactive extensions](https://github.com/dotnet/reactive)

Now, it is possible to write logic of processing RabbitMQ messages in reactive style.

Please note, that using Reactive extensions for processing MQ messages is not recommended.

Any case, with `ReactiveRabbitMqObservable` you can try process messages in following way:

```C#
   using var messageProcessing = new ReactiveRabbitMqObservable<string>(
            model, // RabbitMQ IModel
            queueName,
            consumerTag,
            DeserializeMessage)
        .Buffer(TimeSpan.FromSeconds(1), 10) // Process batch every second or when receive 10 message
        .Do(ProcessMessage)
        .Ack() // Acks all not yet acked/nacked messages
        .Subscribe();
```


