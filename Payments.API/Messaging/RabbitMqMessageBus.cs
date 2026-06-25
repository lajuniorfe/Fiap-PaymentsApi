using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Payments.API.Messaging
{
    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly IConnection _connection;
        private IChannel? _channel;

        public RabbitMqMessageBus(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"],
                UserName = configuration["RabbitMq:User"],
                Password = configuration["RabbitMq:Password"]
            };

            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
        }

        public async Task PublishAsync<T>(
            string queueName,
            T message)
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message));

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body);

            await Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(
            string queueName,
            Func<T, Task> handler)
        {
            _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer =
                new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                var json = Encoding.UTF8.GetString(
                    args.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler(message);
                }
            };

            _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
