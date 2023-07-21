using ExtratoClubeAPI.Services;
using ExtratoClubeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExtratoClubeAPI.Controllers
{
    [ApiController]
    [Route("api/extratoclube")]
    public class ExtratoClubeController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;

        public ExtratoClubeController(ICrawlerService crawlerService)
        {
            _crawlerService = crawlerService;
        }

        [HttpPost]
        public IActionResult ConsultarBeneficios([FromBody] Credencial dadosCliente)
        {
            // Validação dos dados do cliente e autenticação no portal EXTRATOCLUBE (simulação)
            if (dadosCliente.Usuario != "testekonsi" || dadosCliente.Senha != "testekonsi")
            {
                return BadRequest("Credenciais inválidas.");
            }

            // Chame o método do crawler para consultar os benefícios do CPF do cliente.
            var beneficios = _crawlerService.ConsultarBeneficios(dadosCliente);

            // Retorne a lista de benefícios encontrados em formato JSON.
            return Ok(beneficios);
        }
    }
}
