using Azure.Storage.Queues;
using System.Text.Json;
using HotelSync.Domain;

namespace HotelSync.Api.Services;

public interface IMessageService { Task SendRateUpdateAsync(RateUpdate update); }

public class MessageService : IMessageService
{
    private readonly QueueClient _queueClient;
    // Usando 127.0.0.1 para garantir que o Windows não tente HTTPS/SSL
    private const string ConnString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

    public MessageService()
    {
        _queueClient = new QueueClient(ConnString, "rate-updates", new QueueClientOptions {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        _queueClient.CreateIfNotExists();
    }

    public async Task SendRateUpdateAsync(RateUpdate update)
    {
        var json = JsonSerializer.Serialize(update);
        await _queueClient.SendMessageAsync(json);
    }
}
