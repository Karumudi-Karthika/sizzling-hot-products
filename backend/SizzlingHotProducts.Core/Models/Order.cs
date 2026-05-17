namespace SizzlingHotProducts.Core.Models;

/// <summary>
/// Represents a customer order from the input data.
/// </summary>
public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public List<OrderEntry> Entries { get; set; } = [];
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Parses the Date string (dd/MM/yyyy) into a DateOnly.
    /// </summary>
    public DateOnly ParsedDate =>
        DateOnly.ParseExact(Date, "dd/MM/yyyy", null);

    public bool IsCancelled =>
        Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase);

    public bool IsCompleted =>
        Status.Equals("completed", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Represents a single product line within an order.
/// </summary>
public class OrderEntry
{
    public string Id { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
