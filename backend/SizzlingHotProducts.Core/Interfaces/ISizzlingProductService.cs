using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.Core.Interfaces;

/// <summary>
/// Defines the contract for calculating sizzling hot products.
/// </summary>
public interface ISizzlingProductService
{
    /// <summary>
    /// Returns daily top products and the 3-day rolling winner
    /// for the past 3 days relative to <paramref name="today"/>.
    /// </summary>
    SizzlingHotProductsResponse GetSizzlingHotProducts(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly today);

    /// <summary>
    /// Returns the top sizzling product for a single day.
    /// </summary>
    DailySizzlingResult? GetTopProductForDay(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly date);

    /// <summary>
    /// Returns the top sizzling product over an arbitrary date range.
    /// </summary>
    PeriodSizzlingResult? GetTopProductForPeriod(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly from,
        DateOnly to);
}
