using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitMQConsumer;

public class RabbitMQMetrics
{
    private readonly Uri _baseUri = new Uri("http://localhost:15672/api/");
    private readonly string _username = "guest";
    private readonly string _password = "guest";
    private readonly HttpClient _httpClient;

    public RabbitMQMetrics()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = _baseUri;
        var authToken = Encoding.ASCII.GetBytes($"{_username}:{_password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
    }

    public async Task<(int Consumers, int Action)> GetQueueUsagePercentageAsync()
    {
        string queueName = "queue_demo";
        string vhost = Uri.EscapeDataString("/");
        HttpResponseMessage response = await _httpClient.GetAsync($"queues/{vhost}/{queueName}");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var queueDetails = JsonSerializer.Deserialize<Metrics>(content, options);

            if (queueDetails == null)
                return (0, 0);

            var consumerUtilisation = queueDetails.ConsumerUtilisation * 100;
            var consumers = queueDetails.Consumers;

            Console.WriteLine($"Utilisation: {consumerUtilisation}");
            Console.WriteLine($"Consumers: {consumers}");

            if (consumerUtilisation > 50 || consumers == 0)
                return (consumers, 1);
            else if (consumerUtilisation < 20 && consumers > 1)
                return (consumers, -1);
        }
        else
        {
            Console.WriteLine($"Failed to retrieve queue details. Status code: {response.StatusCode}");
        }

        return (0, 0);
    }

    public record Metrics([property: JsonPropertyName("consumer_capacity")] double ConsumerCapacity, [property: JsonPropertyName("consumer_utilisation")] double ConsumerUtilisation, [property: JsonPropertyName("consumers")] int Consumers);
}