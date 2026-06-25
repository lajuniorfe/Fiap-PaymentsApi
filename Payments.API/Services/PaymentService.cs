using Payments.API.Events;

namespace Payments.API.Services
{
    public class PaymentService : IPaymentService
    {
        public Task<PaymentProcessedEvent> ProcessarPagamento(OrderPlacedEvent order)
        {

            var aprovado = Random.Shared.Next(0, 2) == 1;

            return Task.FromResult(new PaymentProcessedEvent
                {
                    UserId = order.UserId,
                    GameId = order.GameId,
                    Price = order.Price,
                    Status = aprovado
                        ? "Aprovado"
                        : "Rejeitado"
                });
        }

        
    }
}
