using Payments.API.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Payments.API.Consumers
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public OrderPlacedConsumer(
            IConfiguration configuration,
            ILogger<OrderPlacedConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var connectionString =
                _configuration["RabbitMQConnection"]
                ?? throw new InvalidOperationException(
                    "RabbitMQConnection não configurada.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            _connection =
                await factory.CreateConnectionAsync(
                    stoppingToken);

            _channel =
                await _connection.CreateChannelAsync(
                    cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: "order-placed",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Consumer da fila order-placed iniciado.");

            var consumer =
                new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                await ProcessMessageAsync(args);
            };

            await _channel.BasicConsumeAsync(
                queue: "order-placed",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Aplicação sendo encerrada
            }
        }

        private async Task ProcessMessageAsync(
            BasicDeliverEventArgs args)
        {
            try
            {
                var json =
                    Encoding.UTF8.GetString(
                        args.Body.ToArray());

                _logger.LogInformation(
                    "Mensagem recebida da fila order-placed: {Message}",
                    json);

                var order =
                    JsonSerializer.Deserialize<OrderPlacedEvent>(
                        json);

                if (order == null)
                {
                    _logger.LogWarning(
                        "Não foi possível desserializar OrderPlacedEvent.");

                    await _channel!.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                // Futuramente:
                //
                // await pagamentoService.ProcessarPagamento(order);

                _logger.LogInformation(
                    "Pedido processado com sucesso.");

                await _channel!.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar mensagem da fila order-placed.");

                await _channel!.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync(
                    cancellationToken);
            }

            if (_connection != null)
            {
                await _connection.CloseAsync(
                    cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}