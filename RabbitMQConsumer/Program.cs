// See https://aka.ms/new-console-template for more information
using RabbitMQConsumer;

Console.WriteLine("Hello, World!");

static void ProcessMessage(string message)
{
    Console.WriteLine($"Received message: {message}");
}

var service = new RabbitMQService("localhost");
service.DeclareQueue("queue_demo", durable: false, exclusive: false, autoDelete: false);

int consumers = 0;

do
{
    int environmentConsumers = Convert.ToInt32(Environment.GetEnvironmentVariable("RabbitMQConsumers", EnvironmentVariableTarget.Machine) ?? "10");
    if (consumers < environmentConsumers)
    {
        service.CreateConsumers("queue_demo", ProcessMessage);
        consumers++;
    }
    else if (consumers > environmentConsumers)
    {
        service.CancelConsumer();
        consumers--;
    }
} while (true);