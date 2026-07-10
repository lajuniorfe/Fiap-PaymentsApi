using Payments.API.Events;
using Payments.API.Messaging;
using Payments.API.Services;

namespace Payments.API.Consumers
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderPlacedConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory)
        {
            _messageBus = messageBus;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageBus.SubscribeAsync<OrderPlacedEvent>("order-placed", ProcessOrderAsync);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessOrderAsync(OrderPlacedEvent order)
        {
            using var scope = _scopeFactory.CreateScope();

            var pagamentoService = scope.ServiceProvider.GetRequiredService<IPagamentoService>();

            await pagamentoService.ProcessarPagamento(order);

         
        }
    }
}
