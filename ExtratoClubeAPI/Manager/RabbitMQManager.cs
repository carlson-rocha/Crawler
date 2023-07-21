using RabbitMQ.Client;
using System.Text;

namespace ExtratoClubeAPI.Manager
{
    public class RabbitMQManager
    {
        private const string QueueName = "beneficios_queue";

        public void EnviarMatriculasParaFila(List<string> matriculas)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                foreach (var matricula in matriculas)
                {
                    var body = Encoding.UTF8.GetBytes(matricula);
                    channel.BasicPublish(exchange: "",
                                         routingKey: QueueName,
                                         basicProperties: null,
                                         body: body);
                }
            }
        }
    }
}
