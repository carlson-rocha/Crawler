using ExtratoClubeAPI.Manager;
using ExtratoClubeAPI.Models;
using Newtonsoft.Json;

namespace ExtratoClubeAPI.Services
{
    public class CrawlerService : ICrawlerService
    {
        private readonly RedisCacheManager _cacheManager;
        private readonly RabbitMQManager _rabbitMQManager;
        private readonly ElasticsearchManager _elasticsearchManager;

        public CrawlerService(RedisCacheManager cacheManager, RabbitMQManager rabbitMQManager, ElasticsearchManager elasticsearchManager)
        {
            _cacheManager = cacheManager;
            _rabbitMQManager = rabbitMQManager;
            _elasticsearchManager = elasticsearchManager;
        }

        public List<string> ConsultarBeneficios(Credencial dadosCliente)
        {
            // Verificar se os dados já estão no cache do Redis
            if (_cacheManager.TryGetBeneficios(dadosCliente.Cpf, out var jsonBeneficios))
            {
                return JsonConvert.DeserializeObject<List<string>>(jsonBeneficios);
            }
            else
            {
                // Realizar o crawling e obter os dados dos benefícios
                var beneficios = RealizarCrawling(dadosCliente);

                // Converter os dados para JSON
                jsonBeneficios = JsonConvert.SerializeObject(beneficios);

                // Salvar os dados no cache do Redis
                _cacheManager.SetBeneficios(dadosCliente.Cpf, jsonBeneficios);

                // Enviar as matrículas para a fila do RabbitMQ
                _rabbitMQManager.EnviarMatriculasParaFila(beneficios);

                // Indexar os dados no Elasticsearch
                _elasticsearchManager.IndexarBeneficios(dadosCliente.Cpf, beneficios);

                return beneficios;
            }
        }

        private bool RealizarLogin(Credencial dadosCliente)
        {
            // Implementar a lógica real para realizar o login no portal EXTRATOCLUBE
            // com as credenciais fornecidas. Retorne true se o login for bem-sucedido ou false caso contrário.
            // Por enquanto, vamos apenas simular o login bem-sucedido.

            return true;
        }

        private void NavegarMenuOpcoes()
        {
            // Implementar a lógica real para navegar no "MENU DE OPÇÕES"
            // no portal EXTRATOCLUBE e clicar em "BENEFÍCIOS DE UM CPF".
            // Por enquanto, vamos apenas simular a navegação.
        }

        private List<string> RealizarCrawling(Credencial dadosCliente)
        {
            if (RealizarLogin(dadosCliente))
            {
                NavegarMenuOpcoes();

                // Implementar a lógica real para realizar o crawling e obter os dados dos benefícios
                // Retornar uma lista de números de matrícula fictícia para simular o resultado do crawler.
                return new List<string> { "123456", "789012", "345678" };
            }
            else
            {
                return new List<string>();
            }
        }
    }

}
