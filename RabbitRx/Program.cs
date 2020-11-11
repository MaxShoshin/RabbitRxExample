using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitRx
{
    public static class Program
    {
        public static void Main(string userName, string password)
        {
            var factory = new ConnectionFactory();
            factory.UserName = userName;
            factory.Password = password;

            var connection = factory.CreateConnection();
            var model = connection.CreateModel();

            model.BasicQos(prefetchSize: 0, prefetchCount: 5, true);

            DeclareQueueAndExchange(model);

            var cancellation = new CancellationTokenSource();

            StartPublishMessages(model, cancellation.Token);

            using var messageProcessing = new ReactiveRabbitMqObservable<string>(
                    model,
                    "queueName",
                    "consumerTag",
                    DeserializeMessage)
                .Buffer(TimeSpan.FromSeconds(1), 10) // Process batch every second or when receive 10 message. Please note that we have prefetch count 5, so we will receive 5 messages every second, not 10.
                .Do(ProcessMessage)
                .Ack() // Acks all processed messages and not acked/nacked messages
                .Subscribe();

            Thread.Sleep(10000);

            cancellation.Cancel();

            Console.WriteLine("Done");
        }

        private static void DeclareQueueAndExchange(IModel model)
        {
            model.ExchangeDeclare("test", "topic");
            model.QueueDeclare("queueName");
            model.QueueBind("queueName", "test", "#");
        }

        private static void ProcessMessage(IList<Message<string>> messages)
        {
            Console.WriteLine(DateTime.Now.ToString("O", CultureInfo.InvariantCulture) + " ======");
            foreach (var message in messages)
            {
                Console.WriteLine(message.Payload);
            }

            // You can use `message.Nack()` to reject message
        }

        private static string DeserializeMessage(ReadOnlyMemory<byte> body, DeliveryInfo deliveryInfo)
        {
            return Encoding.UTF8.GetString(body.Span);
        }

        private static void StartPublishMessages(IModel model, CancellationToken cancel)
        {
            Task.Run(() =>
                {
                    while (!cancel.IsCancellationRequested)
                    {
                        var bytes = Encoding.UTF8.GetBytes("From publisher at: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));

                        model.BasicPublish("test", "some", body: bytes);

                        Thread.Sleep(10);
                    }
                },
                cancel);
        }
    }
}
