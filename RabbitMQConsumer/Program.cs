// See https://aka.ms/new-console-template for more information
using RabbitMQConsumer;

Console.WriteLine("Hello, World!");

static void ProcessMessage(string message)
{
    Console.WriteLine($"Received message: {message}");
}

var service = new RabbitMQService("localhost");
service.DeclareQueue("queue_demo", durable: false, exclusive: false, autoDelete: false);
service.CreateConsumers("queue_demo", 5, ProcessMessage);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();