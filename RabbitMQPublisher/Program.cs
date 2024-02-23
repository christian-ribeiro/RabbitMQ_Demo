using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "queue_demo",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
    int count = 0;
    do
    {
        string message = $"Hello World! Message {count++}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: "queue_demo",
                             basicProperties: null,
                             body: body);
        Console.WriteLine($" [x] Sent {message}");
    } while (true);
}