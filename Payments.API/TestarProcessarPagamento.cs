using Microsoft.AspNetCore.Mvc;
using Payments.API.Events;
using Payments.API.Services;

namespace Payments.API
{
    [ApiController]
    [Route("api/[Controller]")]
    public class TestarProcessarPagamento :ControllerBase
    {
        private readonly IPagamentoService pagamentoService;

        public TestarProcessarPagamento(IPagamentoService pagamentoService)
        {
            this.pagamentoService = pagamentoService;
        }

        [HttpGet]
        public IActionResult TestarEnvioMensagem()
        {
            pagamentoService.ProcessarPagamento(new OrderPlacedEvent
            {
                GameId = Guid.NewGuid(),
                Price = 10,
                UserId = Guid.NewGuid()
            });

            return Ok("Mensagem enviada com sucesso!");
        }
    }
}
