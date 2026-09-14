using Azure.Messaging.ServiceBus;
using Payments.API.Events;
using Payments.API.Services;
using System.Text.Json;

namespace Payments.API.Consumers
{
    public class OrderPlacedConsumer : BackgroundService
    {

        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<OrderPlacedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var connectionString = configuration["ServiceBusConnection"]
                ?? throw new InvalidOperationException("ServiceBusConnection não configurada.");

            var client = new ServiceBusClient(connectionString);

            _processor = client.CreateProcessor(
                "order-placed",
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("Consumer da fila order-placed iniciado.");

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

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var json = args.Message.Body.ToString();

                _logger.LogInformation("Mensagem recebida da fila order-placed: {Message}", json);

                var order =
                    JsonSerializer.Deserialize<OrderPlacedEvent>(json);

                if (order == null)
                {
                    _logger.LogWarning(
                        "Não foi possível desserializar OrderPlacedEvent.");

                    await args.DeadLetterMessageAsync(
                        args.Message,
                        "Mensagem inválida",
                        "Não foi possível desserializar OrderPlacedEvent.");

                    return;
                }

                using var scope =
                    _scopeFactory.CreateScope();

                var pagamentoService =
                    scope.ServiceProvider
                        .GetRequiredService<IPagamentoService>();

                await pagamentoService.ProcessarPagamento(order);

                await args.CompleteMessageAsync(args.Message);

                _logger.LogInformation(
                    "Pedido processado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar mensagem da fila order-placed.");

                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(
            ProcessErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Erro no Service Bus. Entity: {EntityPath}",
                args.EntityPath);

            return Task.CompletedTask;
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(
                cancellationToken);

            await _processor.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
