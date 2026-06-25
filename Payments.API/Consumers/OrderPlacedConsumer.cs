using Payments.API.Events;
using Payments.API.Messaging;
using Payments.API.Services;

namespace Payments.API.Consumers
{
    public class OrderPlacedConsumer: BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IPaymentService _paymentService;

        public OrderPlacedConsumer(
            IMessageBus messageBus,
            IPaymentService paymentService)
        {
            _messageBus = messageBus;
            _paymentService = paymentService;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            await _messageBus.SubscribeAsync<OrderPlacedEvent>(
                "order-placed",
                ProcessOrderAsync);
        }

        private async Task ProcessOrderAsync(
            OrderPlacedEvent order)
        {
            //processar o pagamento
            var paymentResult =
                await _paymentService.ProcessarPagamento(order);

            //publicar o evento de pagamento processado
            await _messageBus.PublishAsync(
                "payment-processed",
                paymentResult);
        }
    }
}
