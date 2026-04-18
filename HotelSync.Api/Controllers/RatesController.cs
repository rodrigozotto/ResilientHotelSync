using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSync.Api.Data;
using HotelSync.Domain;
using HotelSync.Api.Models;
using HotelSync.Api.Services;
using Microsoft.Data.SqlClient;

namespace HotelSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMessageService _messageService;
    private readonly ILogger<RatesController> _logger;

    public RatesController(AppDbContext context, IMessageService messageService, ILogger<RatesController> logger)
    {
        _context = context;
        _messageService = messageService;
        _logger = logger;
    }

    /// <summary>
    /// PT-BR: Atualiza o preço de uma tarifa de forma resiliente usando Idempotência e Mensageria.
    /// EN: Resiliently updates a rate price using Idempotency and Messaging.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateRate(
        [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey, 
        [FromBody] RateUpdateEntry request)
    {
        if (string.IsNullOrEmpty(idempotencyKey))
            return BadRequest("X-Idempotency-Key is required.");

        var update = new RateUpdate {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            HotelId = request.HotelId,
            NewPrice = request.Price,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        try
        {
            // PT-BR: Persistência no banco para garantir Idempotência via Unique Index.
            // EN: DB Persistence to ensure Idempotency via Unique Index.
            _context.RateUpdateLogs.Add(update);
            await _context.SaveChangesAsync(); 

            // PT-BR: Desacoplamento através de Mensageria (Async).
            // EN: Decoupling through Messaging (Async).
            await _messageService.SendRateUpdateAsync(update);

            return Accepted(new { RequestId = update.Id, Message = "Accepted" });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // PT-BR: Tratamento de duplicidade para garantir que o mesmo evento não seja processado 2x.
            // EN: Duplicity handling to ensure the same event isn't processed twice.
            _logger.LogInformation("Duplicate key detected: {Key}", idempotencyKey);
            return Ok(new { Message = "Already processed" });
        }
    }
}
