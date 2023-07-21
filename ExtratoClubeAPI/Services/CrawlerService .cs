using ExtratoClubeAPI.Manager;
using ExtratoClubeAPI.Models;
using HtmlAgilityPack;
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

        public List<string> ConsultarBeneficios(Credencial credencial)
        {
            // Verificar se os dados já estão no cache do Redis
            if (_cacheManager.TryGetBeneficios(credencial.Cpf, out var jsonBeneficios))
            {
                return JsonConvert.DeserializeObject<List<string>>(jsonBeneficios);
            }
            else
            {
                // Realizar o crawling e obter os dados dos benefícios
                var beneficios = RealizarCrawling(credencial);

                // Converter os dados para JSON
                jsonBeneficios = JsonConvert.SerializeObject(beneficios);

                // Salvar os dados no cache do Redis
                _cacheManager.SetBeneficios(credencial.Cpf, jsonBeneficios);

                // Enviar as matrículas para a fila do RabbitMQ
                _rabbitMQManager.EnviarMatriculasParaFila(beneficios);

                // Indexar os dados no Elasticsearch
                _elasticsearchManager.IndexarBeneficios(credencial.Cpf, beneficios);

                return beneficios;
            }
        }

        private bool RealizarLogin(Credencial credencial, string url, out HtmlDocument paginaInicial)
        {
            // Criar uma instância do cliente HTTP
            var httpClient = new HttpClient();

            // Adicionar as credenciais de autenticação no header do request
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " +
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{credencial.Usuario}:{credencial.Senha}")));

            // Fazer a requisição para a página de login
            var response = httpClient.GetAsync(url).Result;

            // Verificar se o login foi bem-sucedido (código de resposta 200)
            if (response.IsSuccessStatusCode)
            {
                // Ler o conteúdo da página
                var conteudoPagina = response.Content.ReadAsStringAsync().Result;

                // Carregar o conteúdo da página no HtmlAgilityPack
                paginaInicial = new HtmlDocument();
                paginaInicial.LoadHtml(conteudoPagina);

                return true;
            }
            else
            {
                // O login falhou
                paginaInicial = null;
                return false;
            }
        }

        private bool NavegarMenuOpcoes(HtmlDocument paginaInicial, out HtmlDocument paginaMenuOpcoes)
        {
            // Procurar pelo menu "Opções" no HTML da página inicial
            var menuOpcoes = paginaInicial.DocumentNode.SelectSingleNode("//ul[contains(@class, 'menu')]/li[contains(text(), 'OPÇÕES')]");

            if (menuOpcoes != null)
            {
                // Procurar pelo sub-item de menu "BENEFÍCIOS DE UM CPF" dentro do menu "Opções"
                var subItemBeneficiosCPF = menuOpcoes.SelectSingleNode(".//li[contains(text(), 'BENEFÍCIOS DE UM CPF')]/a");

                if (subItemBeneficiosCPF != null)
                {
                    // Pegar a URL do sub-item "BENEFÍCIOS DE UM CPF"
                    var urlBeneficiosCPF = subItemBeneficiosCPF.Attributes["href"].Value;

                    // Criar uma instância do cliente HTTP
                    var httpClient = new HttpClient();

                    // Fazer a requisição para a URL do sub-item "BENEFÍCIOS DE UM CPF"
                    var response = httpClient.GetAsync(urlBeneficiosCPF).Result;

                    // Verificar se a requisição foi bem-sucedida (código de resposta 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Ler o conteúdo da página "BENEFÍCIOS DE UM CPF"
                        var conteudoPaginaBeneficiosCPF = response.Content.ReadAsStringAsync().Result;

                        // Carregar o conteúdo da página "BENEFÍCIOS DE UM CPF" no HtmlAgilityPack
                        paginaMenuOpcoes = new HtmlDocument();
                        paginaMenuOpcoes.LoadHtml(conteudoPaginaBeneficiosCPF);

                        return true;
                    }
                }
            }

            paginaMenuOpcoes = null;
            return false;
        }

        private List<string> RealizarCrawling(Credencial credencial)
        {
            string url = "http://extratoclube.com.br/";

            if (RealizarLogin(credencial, url, out var paginaInicial))
            {
                if (NavegarMenuOpcoes(paginaInicial, out var paginaMenuOpcoes))
                {
                    // Procurar o input "cpf_id" e adicionar o CPF do cliente
                    var inputCpf = paginaMenuOpcoes.DocumentNode.SelectSingleNode("//input[@name='cpf_id']");
                    if (inputCpf != null)
                    {
                        inputCpf.SetAttributeValue("value", credencial.Cpf);
                    }

                    // Procurar o botão de submit e executar o formulário
                    var botaoSubmit = paginaMenuOpcoes.DocumentNode.SelectSingleNode("//button[@type='submit']");
                    var formulario = botaoSubmit?.Ancestors("form").FirstOrDefault();
                    if (formulario != null)
                    {
                        var urlDestino = formulario.GetAttributeValue("action", "");
                        var metodoEnvio = formulario.GetAttributeValue("method", "");

                        if (metodoEnvio.ToLower() == "post")
                        {
                            // Criar uma instância do cliente HTTP
                            var httpClient = new HttpClient();

                            // Obter os dados de retorno após executar a url de destino do formulário (simulando um POST)
                            var response = httpClient.PostAsync(urlDestino, new StringContent("")).Result;

                            // Verificar se a requisição foi bem-sucedida (código de resposta 200)
                            if (response.IsSuccessStatusCode)
                            {
                                // Ler o conteúdo da página de retorno
                                var conteudoPaginaRetorno = response.Content.ReadAsStringAsync().Result;

                                // Carregar o conteúdo da página de retorno no HtmlAgilityPack
                                var paginaRetorno = new HtmlDocument();
                                paginaRetorno.LoadHtml(conteudoPaginaRetorno);

                                // Encontrar a lista de CPFs no HTML da página de retorno
                                var listaCpfs = paginaRetorno.DocumentNode.SelectNodes("//ul[contains(@class, 'lista')]/li");

                                // Criar uma lista para armazenar os CPFs encontrados
                                var cpfsEncontrados = new List<string>();

                                if (listaCpfs != null)
                                {
                                    foreach (var itemCpf in listaCpfs)
                                    {
                                        var cpf = itemCpf.InnerText.Trim();
                                        cpfsEncontrados.Add(cpf);
                                    }
                                }

                                return cpfsEncontrados;
                            }
                        }
                    }
                }
            }

            return new List<string>();
        }

    }
}
