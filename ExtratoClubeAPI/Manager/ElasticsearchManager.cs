using Nest;

namespace ExtratoClubeAPI.Manager
{
    public class ElasticsearchManager
    {
        private readonly ElasticClient _elasticClient;
        private const string IndexName = "beneficios";

        public ElasticsearchManager()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
            _elasticClient = new ElasticClient(settings);
        }

        public void IndexarBeneficios(string cpf, List<string> beneficios)
        {
            var doc = new Document { Cpf = cpf, Beneficios = beneficios };

            var indexResponse = _elasticClient.IndexDocument(doc);

            if (!indexResponse.IsValid)
            {
                // Tratar o erro de indexação aqui, caso necessário
                // Por exemplo: registrar o erro em um log, notificar o desenvolvedor, etc.
                throw new Exception("Erro ao indexar os dados no Elasticsearch.");
            }
        }
    }

    public class Document
    {
        public string Cpf { get; set; }
        public List<string> Beneficios { get; set; }
    }
}
