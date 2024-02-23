using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQConsumer;

public class RabbitMQService
{
    private IConnection connection;
    private IModel channel;

    public RabbitMQService(string hostname)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void DeclareQueue(string queueName, bool durable, bool exclusive, bool autoDelete)
    {
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
    }

    public void CreateConsumers(string queueName, int numberOfConsumers, Action<string> processMessageAction)
    {
        for (int i = 0; i < numberOfConsumers; i++)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                processMessageAction(message);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            Console.WriteLine($"Consumer {i + 1} started");
        }
    }
}