namespace HotelSync.Domain;

public class RateUpdate
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public decimal NewPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
