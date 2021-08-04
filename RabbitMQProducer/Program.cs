
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitMQProducer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Tutorial6UsingRPC(args);
        }

        private static void Tutorial1And2(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //int count = 0;
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "testQueue",
                                     durable: true, //Flag utilizada para não perder a Queue mesmo que o server do RabbitMQ caia ou reinicie
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //while (true)
                //{
                var message = GetMessage(args);
                //message += count++;
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties(); //Flag utilizada para não perder as MENSAGENS mesmo que o server do RabbitMQ caia ou reinicie
                properties.Persistent = true;//MAS existe uma chance dessa mensagem não ser salva, para ter certeza, utilizar publisher confirms: https://www.rabbitmq.com/confirms.html

                channel.BasicPublish(exchange: "",
                                     routingKey: "testQueue",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine("Sent {0}", message);
                //}                
            }
        }

        private static void Tutorial3UsingFanOutExchange(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void Tutorial4UsingDirectExchange(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

                var severity = (args.Length > 0) ? args[0] : "info";

                var message = (args.Length > 1)
                          ? string.Join(" ", args.Skip(1).ToArray())
                          : "Hello World!";

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}', '{1}'", severity, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void Tutorial5UsingTopicExchange(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

                var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";

                var message = (args.Length > 1)
                          ? string.Join(" ", args.Skip(1).ToArray())
                          : "Hello World!";

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "topic_logs",
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}', '{1}'", routingKey, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void Tutorial6UsingRPC(string[] args)
        {
            var rpcClient = new RpcClient();

            Console.WriteLine(" [x] Requesting fib(30)");
            var response = rpcClient.Call("30");

            Console.WriteLine(" [.] Got '{0}'", response);
            rpcClient.Close();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}
