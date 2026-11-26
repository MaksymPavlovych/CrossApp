namespace Infrastructure.Persistence;

// Журнал надходжень. Quantity у ProductRow це поточний залишок, а партії це незмінний аудит.
public sealed class BatchRow
{
    public int Id { get; set; }
    public string ProductId { get; set; } = "";
    public int Amount { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }

    public ProductRow Product { get; set; } = null!;
}
