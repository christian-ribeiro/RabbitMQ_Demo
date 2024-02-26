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

    public void DeclareQueue(string queueName)
    {
        //QueueDeclarePassive => Verifica se a fila existe sem modificar ela. Se não existir, o Rabbit retornará um erro
        channel.QueueDeclarePassive(queueName);
    }

    //processMessageAction => Função de teste executada após o consumidor ler a mensagem
    public (IModel Channel, string ConsumerTag) CreateConsumer(Func<string, string, IModel, ulong, Task> processMessageAction)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            //Chamada passando o Channel / DeliveryTag para utilizar com o autoAck = false
            await processMessageAction(ea.ConsumerTag, message, channel, ea.DeliveryTag);
        };

        //autoAck: false -> Desativa a confirmação automática de mensagens, que vai ser feito dentro do método ProcessMessage, com o Channel.BasicAck
        var consumerTag = channel.BasicConsume(queue: channel.CurrentQueue, autoAck: false, consumer: consumer);
        return (channel, consumerTag);
    }

    //Remove o primeiro consumidor da lista
    public static void CancelConsumer(List<(IModel Channel, string ConsumerTag)> listConsumer)
    {
        if (listConsumer.Count > 0)
        {
            var (Channel, ConsumerTag) = listConsumer[0];
            Channel.BasicCancel(ConsumerTag);
            Console.WriteLine($"Consumer {ConsumerTag} cancelled");
        }
    }
}