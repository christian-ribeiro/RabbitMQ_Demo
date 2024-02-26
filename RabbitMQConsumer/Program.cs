using RabbitMQ.Client;
using RabbitMQConsumer;

//Simulação de método para teste de execução assíncrona com carga
//ConsumerTag apenas para identificar de qual consumidor veio a mensagem
//Channel e DeliveryTag apenas para confirmar o processamento da mensagem
Func<string, string, IModel, ulong, Task> ProcessMessage = async (string consumerTag, string message, IModel channel, ulong deliveryTag) =>
{
    Console.WriteLine($"Consumidor {consumerTag} processou: {message}");
    await Task.Delay(1000);

    //Confirma o processamento da mensagem (Só utilizar se o autoAck for false)
    channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
};

//Lista de consumidores ativos
List<(IModel Channel, string ConsumerTag)> listConsumer = [];

do
{
    //Para demonstração, foi adicionada a quantidade de consumidores em uma variável de ambiente - Machine (Valor padrão - 10)
    int environmentConsumers = Convert.ToInt32(Environment.GetEnvironmentVariable("RabbitMQConsumers", EnvironmentVariableTarget.Machine) ?? "10");
    if (listConsumer.Count < environmentConsumers)
    {
        //Declarando novo channel / consumer para que o async funcione corretamente, com 1 consumidor por canal
        var service = new RabbitMQService("localhost");
        service.DeclareQueue("queue_demo", durable: false, exclusive: false, autoDelete: false);

        var consumer = service.CreateConsumer(ProcessMessage);

        //Adicionando o Service e o ConsumerTag para que o consumidor possa ser cancelado posteriormente, se necessário
        listConsumer.Add(consumer);
    }
    else if (listConsumer.Count > environmentConsumers)
    {
        //Executa a função para cancelar o consumidor se a quantidade de consumidores ativos for maior que a quantidade da variável de ambiente
        RabbitMQService.CancelConsumer(listConsumer);
        listConsumer.RemoveAt(0);
    }
} while (true);