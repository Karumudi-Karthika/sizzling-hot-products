using System.Text.Json;
using SizzlingHotProducts.Core.Exceptions;
using SizzlingHotProducts.Core.Interfaces;
using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.API.Services;

/// <summary>
/// Loads orders and products from the JSON files in the Data directory.
/// Results are cached in-memory after the first load (files don't change at runtime).
/// </summary>
public class JsonDataLoader : IDataLoader
{
    private readonly string _dataDirectory;
    private readonly ILogger<JsonDataLoader> _logger;

    // Simple in-memory cache — avoids re-reading disk on every request.
    private IReadOnlyList<Order>? _cachedOrders;
    private IReadOnlyList<Product>? _cachedProducts;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonDataLoader(IWebHostEnvironment env, ILogger<JsonDataLoader> logger)
    {
        _dataDirectory = Path.Combine(env.ContentRootPath, "Data");
        _logger = logger;
    }

    public async Task<IReadOnlyList<Order>> LoadOrdersAsync()
    {
        if (_cachedOrders is not null) return _cachedOrders;

        _cachedOrders = await LoadJsonFile<List<Order>>("orders.json");
        _logger.LogInformation("Loaded {Count} orders from disk.", _cachedOrders.Count);
        return _cachedOrders;
    }

    public async Task<IReadOnlyList<Product>> LoadProductsAsync()
    {
        if (_cachedProducts is not null) return _cachedProducts;

        _cachedProducts = await LoadJsonFile<List<Product>>("products.json");
        _logger.LogInformation("Loaded {Count} products from disk.", _cachedProducts.Count);
        return _cachedProducts;
    }

    private async Task<T> LoadJsonFile<T>(string fileName)
    {
        var path = Path.Combine(_dataDirectory, fileName);

        if (!File.Exists(path))
            throw new DataLoadException($"Data file not found: {path}");

        try
        {
            await using var stream = File.OpenRead(path);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
            return result ?? throw new DataLoadException($"File '{fileName}' deserialised to null.");
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"Failed to parse '{fileName}': {ex.Message}", ex);
        }
    }
}
