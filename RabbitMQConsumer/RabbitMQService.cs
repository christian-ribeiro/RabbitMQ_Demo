using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQConsumer;

public class RabbitMQService
{
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMQService(string hostname)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        factory.DispatchConsumersAsync = true;
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void DeclareQueue(string queueName, bool durable, bool exclusive, bool autoDelete)
    {
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
    }

    public string CreateConsumer(Func<string[], Task> processMessageAction)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await processMessageAction([ea.ConsumerTag, message]);
        };

        string consumerTag = channel.BasicConsume(queue: channel.CurrentQueue, autoAck: true, consumer: consumer);
        return consumerTag;
    }

    public static void CancelConsumer(List<(RabbitMQService Service, string ConsumerTag)> listConsumer)
    {
        if (listConsumer.Count > 0)
        {
            var (Service, ConsumerTag) = listConsumer[0];
            Service.channel.BasicCancel(ConsumerTag);
            Console.WriteLine($"Consumer {ConsumerTag} cancelled");
        }
    }
}