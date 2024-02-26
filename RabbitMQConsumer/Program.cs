using RabbitMQ.Client;
using RabbitMQConsumer;
using System.Reflection.Metadata.Ecma335;

//Simulação de método para teste de execução assíncrona com carga
//ConsumerTag apenas para identificar de qual consumidor veio a mensagem
//Channel e DeliveryTag apenas para confirmar o processamento da mensagem
Func<string, string, IModel, ulong, Task> ProcessMessage = async (string consumerTag, string message, IModel channel, ulong deliveryTag) =>
{
    //Console.WriteLine($"Consumidor {consumerTag} processou: {message}");
    //await Task.Delay(1000);

    //Confirma o processamento da mensagem (Só utilizar se o autoAck for false)
    channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
};

//Lista de consumidores ativos
List<(IModel Channel, string ConsumerTag)> listConsumer = [];

do
{
    //Para demonstração, foi adicionada a quantidade de consumidores em uma variável de ambiente - Machine (Valor padrão - 10)
    var metris = await Metrics();
    if (metris.Action == 1 || metris.Consumers == 0)
    {
        //Declarando novo channel / consumer para que o async funcione corretamente, com 1 consumidor por canal
        var service = new RabbitMQService("localhost");
        service.DeclareQueue("queue_demo");

        var consumer = service.CreateConsumer(ProcessMessage);

        //Adicionando o Service e o ConsumerTag para que o consumidor possa ser cancelado posteriormente, se necessário
        listConsumer.Add(consumer);
    }
    else if (metris.Action == -1)
    {
        //Executa a função para cancelar o consumidor se a quantidade de consumidores ativos for maior que a quantidade da variável de ambiente
        RabbitMQService.CancelConsumer(listConsumer);
        listConsumer.RemoveAt(0);
    }
    Thread.Sleep(10000);
} while (true);

async Task<(int Consumers, int Action)> Metrics()
{
    var metrics = new RabbitMqMetrics();
    var response = await metrics.GetQueueUsagePercentageAsync();

    switch (response.Action)
    {
        case -1:
            await Console.Out.WriteLineAsync("Remover consumidor");
            break;
        case 0:
            await Console.Out.WriteLineAsync("Apenas existir");
            break;
        case 1:
            await Console.Out.WriteLineAsync("Criar consumidor");
            break;
    }

    return response;
}