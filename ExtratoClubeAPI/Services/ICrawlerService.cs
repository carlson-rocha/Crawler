using ExtratoClubeAPI.Models;

namespace ExtratoClubeAPI.Services
{
    public interface ICrawlerService
    {
        List<string> ConsultarBeneficios(Credencial dadosCliente);
    }
}
