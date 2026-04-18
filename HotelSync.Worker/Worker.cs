using Azure.Storage.Queues;
using HotelSync.Domain;
using System.Text.Json;

namespace HotelSync.Worker;

/// <summary>
/// PT-BR: Consumidor assíncrono que processa as tarifas da fila.
/// EN: Async Consumer that processes rates from the queue.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly QueueClient _queueClient;
    private const string ConnString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _queueClient = new QueueClient(ConnString, "rate-updates", new QueueClientOptions {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        _queueClient.CreateIfNotExists();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // PT-BR: Polling da fila - Leitura da próxima mensagem disponível.
            // EN: Queue Polling - Reading the next available message.
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages: 1, cancellationToken: stoppingToken);
            
            foreach (var message in response.Value)
            {
                var update = JsonSerializer.Deserialize<RateUpdate>(message.MessageText);
                
                // PT-BR: Lógica de integração com sistemas externos (ex: Oracle, Amadeus).
                // EN: Integration logic with external systems (e.g., Oracle, Amadeus).
                _logger.LogInformation("Processing Rate for Hotel {HotelId}", update.HotelId);

                // PT-BR: Confirmação de processamento (Acknowledge) e remoção da fila.
                // EN: Processing confirmation (Acknowledge) and queue removal.
                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
