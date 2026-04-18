using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace HotelSync.Tests;

public class IdempotencyIntegrationTests
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private const string BaseUrl = "http://localhost:5000/api/Rates";

    public IdempotencyIntegrationTests(ITestOutputHelper output)
    {
        _httpClient = new HttpClient();
        _output = output;
    }

    [Fact]
    public async Task Post_DuplicateRate_ShouldHandleGracefully()
    {
        // Arrange
        var idempotencyKey = $"clean-test-{Guid.NewGuid()}";
        var rateData = new { HotelId = 123, Price = 250.00 };

        _output.WriteLine("Iniciando teste de idempotência...");

        // Act 1: Primeira chamada
        var msg1 = CreateRequest(idempotencyKey, rateData);
        var response1 = await _httpClient.SendAsync(msg1);

        // Act 2: Segunda chamada (A que gerava o StackTrace visual)
        var msg2 = CreateRequest(idempotencyKey, rateData);
        var response2 = await _httpClient.SendAsync(msg2);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        _output.WriteLine("Teste finalizado com sucesso. Idempotência validada sem erros reais.");
    }

    private HttpRequestMessage CreateRequest(string key, object data)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        msg.Headers.Add("X-Idempotency-Key", key);
        msg.Content = JsonContent.Create(data);
        return msg;
    }
}
