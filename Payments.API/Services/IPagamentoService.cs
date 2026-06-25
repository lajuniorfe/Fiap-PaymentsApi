using Payments.API.Events;

namespace Payments.API.Services
{
    public interface IPagamentoService
    {
        Task<PaymentProcessedEvent> ProcessarPagamento(OrderPlacedEvent order);
    }
}
