using Payments.API.Events;

namespace Payments.API.Services
{
    public interface IPaymentService
    {
        Task<PaymentProcessedEvent> ProcessarPagamento(OrderPlacedEvent order);
    }
}
