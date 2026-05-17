using Microsoft.AspNetCore.Mvc;
using SizzlingHotProducts.Core.Interfaces;
using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.API.Controllers;

/// <summary>
/// Exposes endpoints for retrieving sizzling hot product results.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ISizzlingProductService _sizzlingService;
    private readonly IDataLoader _dataLoader;
    private readonly ILogger<ProductsController> _logger;

    // The assignment states: "assume today's date is 23/04/2026"
    // In production this would be injected via a clock abstraction (IDateTimeProvider)
    // so it can be mocked in tests without changing production code.
    private static readonly DateOnly DefaultToday = new(2026, 4, 23);

    public ProductsController(
        ISizzlingProductService sizzlingService,
        IDataLoader dataLoader,
        ILogger<ProductsController> logger)
    {
        _sizzlingService = sizzlingService;
        _dataLoader = dataLoader;
        _logger = logger;
    }

    /// <summary>
    /// Returns the daily top sizzling product for each of the past 3 days
    /// and the overall top product across the full 3-day window.
    /// </summary>
    /// <param name="today">
    /// Optional override for "today" (dd/MM/yyyy). Defaults to 23/04/2026 per the brief.
    /// </param>
    [HttpGet("sizzling-hot")]
    [ProducesResponseType(typeof(SizzlingHotProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SizzlingHotProductsResponse>> GetSizzlingHot(
        [FromQuery] string? today = null)
    {
        var resolvedToday = DefaultToday;

        if (!string.IsNullOrWhiteSpace(today))
        {
            if (!DateOnly.TryParseExact(today, "dd/MM/yyyy", out resolvedToday))
                return BadRequest($"Invalid date format '{today}'. Use dd/MM/yyyy.");
        }

        _logger.LogInformation("Calculating sizzling hot products for today={Today}", resolvedToday);

        var orders = await _dataLoader.LoadOrdersAsync();
        var products = await _dataLoader.LoadProductsAsync();

        var result = _sizzlingService.GetSizzlingHotProducts(orders, products, resolvedToday);
        return Ok(result);
    }

    /// <summary>
    /// Returns all available products from the catalogue.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll()
    {
        var products = await _dataLoader.LoadProductsAsync();
        return Ok(products);
    }
}
