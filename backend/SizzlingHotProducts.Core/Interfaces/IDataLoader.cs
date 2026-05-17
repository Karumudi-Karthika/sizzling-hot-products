using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.Core.Interfaces;

/// <summary>
/// Defines the contract for loading orders and products from a data source.
/// </summary>
public interface IDataLoader
{
    Task<IReadOnlyList<Order>> LoadOrdersAsync();
    Task<IReadOnlyList<Product>> LoadProductsAsync();
}
