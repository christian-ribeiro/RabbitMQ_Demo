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
        //Se for utilizar o AsyncEventingBasicConsumer, é necessário colocar DispatchConsumersAsync = true
        var factory = new ConnectionFactory { HostName = hostname, DispatchConsumersAsync = true };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void DeclareQueue(string queueName, bool durable, bool exclusive, bool autoDelete)
    {
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
    }

    public string CreateConsumer(Func<(string ConsumerTag, string Message, IModel Channel, ulong DeliveryTag), Task> processMessageAction)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            //Chamada passando o Channel / DeliveryTag para utilizar com o autoAck = false
            await processMessageAction((ea.ConsumerTag, message, channel, ea.DeliveryTag));
        };

        //autoAck: false -> Desativa a confirmação automática de mensagens, que vai ser feito dentro do método ProcessMessage, com o Channel.BasicAck
        return channel.BasicConsume(queue: channel.CurrentQueue, autoAck: false, consumer: consumer);
    }

    //Remove o primeiro consumidor da lista
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