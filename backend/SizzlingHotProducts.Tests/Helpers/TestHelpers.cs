using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.Tests.Helpers;

/// <summary>
/// Fluent builder for constructing <see cref="Order"/> objects in tests.
/// </summary>
internal class OrderBuilder
{
    private string _orderId = "O1";
    private string _customerId = "C1";
    private string _date = "23/04/2026";
    private string _status = "completed";
    private readonly List<OrderEntry> _entries = [];

    public static OrderBuilder Create() => new();

    public OrderBuilder WithId(string id) { _orderId = id; return this; }
    public OrderBuilder WithCustomer(string customerId) { _customerId = customerId; return this; }
    public OrderBuilder WithDate(string date) { _date = date; return this; }
    public OrderBuilder Cancelled() { _status = "cancelled"; return this; }
    public OrderBuilder WithProduct(string productId, int quantity = 1)
    {
        _entries.Add(new OrderEntry { Id = productId, Quantity = quantity });
        return this;
    }

    public Order Build() => new()
    {
        OrderId = _orderId,
        CustomerId = _customerId,
        Date = _date,
        Status = _status,
        Entries = [.. _entries]
    };
}

/// <summary>
/// Factory for common product sets used across tests.
/// </summary>
internal static class ProductFactory
{
    public static List<Product> StandardCatalogue() =>
    [
        new Product { Id = "P1", Name = "Ezy Storage 37L Flexi Laundry Basket - White" },
        new Product { Id = "P2", Name = "Aandleford Black Seaford Post Mounted Letterbox" },
        new Product { Id = "P3", Name = "Coolaroo 5.4m Square Graphite Premium Shade Sail Kit" },
        new Product { Id = "P4", Name = "Ozito 80W Soldering Iron" },
        new Product { Id = "P5", Name = "Richgro 25L All Purpose Garden Soil Mix" },
        new Product { Id = "P6", Name = "Arlec 160W Crystalline Solar Foldable Charging Kit" },
    ];

    public static List<Product> TwoProducts(
        string id1 = "PA", string name1 = "Alpha Product",
        string id2 = "PB", string name2 = "Beta Product") =>
    [
        new Product { Id = id1, Name = name1 },
        new Product { Id = id2, Name = name2 },
    ];
}
