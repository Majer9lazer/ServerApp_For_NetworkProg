using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ServerApplication_For_NetworkProg.RabbitMq
{
    public class PublishMessageClass
    {
        private readonly IConnectionFactory _connectionFactory = new ConnectionFactory()
        {
            HostName =Program.Ip,
            UserName = Program.Login,
            Password = Program.Password,
            VirtualHost = Program.VirtualHost
        };
        public void PublishMessage<T>(T message, string queueName) where T : class
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();


            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);

        }
    }
}
