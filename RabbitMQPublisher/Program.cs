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

    for (int i = 0; i < 1000000; i++)
    {
        string message = $"Hello World! Message {i}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: "queue_demo",
                             basicProperties: null,
                             body: body);
        Console.WriteLine($" [x] Sent {message}");
    }
}

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();