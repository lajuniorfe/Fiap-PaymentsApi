using Payments.API.Events;
using Payments.API.Messaging;

namespace Payments.API.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly IMessageBus messageBus;
        private readonly ILogger _logger;


        public PagamentoService(IMessageBus messageBus, ILoggerFactory loggerFactory)
        {
            this.messageBus = messageBus;
            this._logger = loggerFactory.CreateLogger<PagamentoService>();
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
            _logger.LogInformation("Enviado ao notification");

            return processado;
        }

        
    }
}
