// See https://aka.ms/new-console-template for more information
using RabbitMQConsumer;

Console.WriteLine("queue_demo, World!");

Func<string[], Task> ProcessMessage = async (message) =>
{
    Console.WriteLine($"Consumidor {message[0]} processou: {message[1]}");
    await Task.Delay(5000);
};

int consumers = 0;

do
{
    var service = new RabbitMQService("localhost");
    service.DeclareQueue("queue_demo", durable: false, exclusive: false, autoDelete: false);
    int environmentConsumers = Convert.ToInt32(Environment.GetEnvironmentVariable("RabbitMQConsumers", EnvironmentVariableTarget.Machine) ?? "10");
    if (consumers < environmentConsumers)
    {
        service.CreateConsumers(ProcessMessage);
        consumers++;
    }
    else if (consumers > environmentConsumers)
    {
        service.CancelConsumer();
        consumers--;
    }
} while (true);