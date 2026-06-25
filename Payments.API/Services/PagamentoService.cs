using Payments.API.Events;
using Payments.API.Messaging;

namespace Payments.API.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly IMessageBus messageBus;

        public PagamentoService(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public async Task<PaymentProcessedEvent> ProcessarPagamento(OrderPlacedEvent order)
        {

            var aprovado = Random.Shared.Next(0, 2) == 1;

            var processado = new PaymentProcessedEvent
            {
                UserId = order.UserId,
                GameId = order.GameId,
                Price = order.Price,
                Status = aprovado
                       ? "Aprovado"
                       : "Rejeitado"
            };

            await messageBus.PublishAsync("payment-processed",processado);

            return processado;
        }

        
    }
}
