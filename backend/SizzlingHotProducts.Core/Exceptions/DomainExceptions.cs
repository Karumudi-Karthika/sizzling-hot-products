namespace SizzlingHotProducts.Core.Exceptions;

/// <summary>
/// Thrown when the required data files cannot be found or parsed.
/// </summary>
public class DataLoadException(string message, Exception? inner = null)
    : Exception(message, inner);

/// <summary>
/// Thrown when a product referenced in an order does not exist in the catalogue.
/// </summary>
public class UnknownProductException(string productId)
    : Exception($"Product '{productId}' referenced in orders but not found in the product catalogue.");
