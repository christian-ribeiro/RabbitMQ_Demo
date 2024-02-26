using RabbitMQ.Client;
using System.Text;

var (queueName, count) = ("queue_demo", 1);
var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

do
{
    string message = $"Hello World! Message {count++}";
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    Console.WriteLine($" [x] Sent {message}");
} while (true);