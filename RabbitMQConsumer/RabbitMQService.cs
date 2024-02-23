using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQConsumer;

public class RabbitMQService
{
    private IConnection connection;
    private IModel channel;
    private List<string> consumerTags = [];

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

    public void CreateConsumers(string queueName, Action<string> processMessageAction)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            processMessageAction(message + " => Tag " + ea.ConsumerTag);
        };

        string consumerTag = channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        consumerTags.Add(consumerTag);
        Console.WriteLine($"Consumer started -> " + consumerTag);
    }

    public void CancelConsumer()
    {
        if (consumerTags.Count > 0)
        {
            var consumerTag = consumerTags[0];
            channel.BasicCancel(consumerTag);
            consumerTags.RemoveAt(0);
            Console.WriteLine($"Consumer {consumerTag} cancelled");
        }
    }
}