using Payments.API.Events;
using Payments.API.Messaging;
using Payments.API.Services;

namespace Payments.API.Consumers
{
    public class OrderPlacedConsumer: BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IPagamentoService _paymentService;

        public OrderPlacedConsumer(
            IMessageBus messageBus,
            IPagamentoService paymentService)
        {
            _messageBus = messageBus;
            _paymentService = paymentService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _messageBus.SubscribeAsync<OrderPlacedEvent>("order-placed", ProcessOrderAsync);

            return Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessOrderAsync(OrderPlacedEvent order)
        {
            var paymentResult = await _paymentService.ProcessarPagamento(order);

            await _messageBus.PublishAsync("payment-processed", paymentResult);
        }
    }
}
