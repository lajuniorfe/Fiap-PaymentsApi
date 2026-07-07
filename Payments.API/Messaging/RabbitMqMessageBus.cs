using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Payments.API.Messaging
{
    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly IConfiguration _configuration;

        private IConnection? _connection;
        private IChannel? _channel;


        public RabbitMqMessageBus(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return;


            while (_connection == null || !_connection.IsOpen)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _configuration["RabbitMQ:Host"],
                        UserName = _configuration["RabbitMQ:Username"],
                        Password = _configuration["RabbitMQ:Password"]
                    };


                    _connection = await factory.CreateConnectionAsync();

                    _channel = await _connection.CreateChannelAsync();


                    Console.WriteLine("RabbitMQ conectado.");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Falha ao conectar no RabbitMQ: {ex.Message}");

                    await Task.Delay(
                        TimeSpan.FromSeconds(5));
                }
            }
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            await EnsureConnectionAsync();

            await _channel!.QueueDeclareAsync(
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

        public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler)
        {
            await EnsureConnectionAsync();

            await _channel!.QueueDeclareAsync(
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

            await _channel!.BasicConsumeAsync(
                 queue: queueName,
                 autoAck: true,
                 consumer: consumer);

            await Task.CompletedTask;
        }
    }
}
