// See https://aka.ms/new-console-template for more information
using RabbitMQConsumer;

Console.WriteLine("queue_demo, World!");

Func<string[], Task> ProcessMessage = async (message) =>
{
    Console.WriteLine($"Consumidor {message[0]} processou: {message[1]}");
    await Task.Delay(1000);
};

List<(RabbitMQService, string consumerTag)> listConsumer = [];

do
{
    int environmentConsumers = Convert.ToInt32(Environment.GetEnvironmentVariable("RabbitMQConsumers", EnvironmentVariableTarget.Machine) ?? "10");
    if (listConsumer.Count < environmentConsumers)
    {
        var service = new RabbitMQService("localhost");
        service.DeclareQueue("queue_demo", durable: false, exclusive: false, autoDelete: false);
        var consumerTag = service.CreateConsumer(ProcessMessage);
        listConsumer.Add((service, consumerTag));
    }
    else if (listConsumer.Count > environmentConsumers)
    {
        RabbitMQService.CancelConsumer(listConsumer);
        listConsumer.RemoveAt(0);
    }
} while (true);