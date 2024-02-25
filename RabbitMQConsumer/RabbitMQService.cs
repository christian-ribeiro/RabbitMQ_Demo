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
        factory.DispatchConsumersAsync = true;
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void DeclareQueue(string queueName, bool durable, bool exclusive, bool autoDelete)
    {
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
    }

    public void CreateConsumers(Func<string[], Task> processMessageAction)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await processMessageAction([ea.ConsumerTag, message]);
        };

        string consumerTag = channel.BasicConsume(queue: channel.CurrentQueue, autoAck: true, consumer: consumer);
        consumerTags.Add(consumerTag);
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