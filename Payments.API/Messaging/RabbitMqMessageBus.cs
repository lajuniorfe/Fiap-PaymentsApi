using Payments.API.Messaging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Users.AppService.events
{
    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly IConfiguration _configuration;

        public RabbitMqMessageBus(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            var connectionString =
                _configuration["RabbitMQConnection"]
                ?? throw new InvalidOperationException(
                    "RabbitMQConnection não configurada.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body);
        }
    }
}
