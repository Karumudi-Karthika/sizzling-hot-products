using SizzlingHotProducts.Core.Interfaces;
using SizzlingHotProducts.Core.Models;

namespace SizzlingHotProducts.Core.Services;

/// <summary>
/// Calculates sizzling hot products by applying all business rules:
///
///   BR1 - Count a product once per order (regardless of quantity).
///   BR2 - If the same customer orders the same product multiple times on
///          the same day across different orders, count it only once.
///   BR3 - A cancelled order credits (removes) the original order's sales
///          from whichever day the original order was placed.
///   BR4 - On a sales tie, pick the product that sorts first alphabetically.
/// </summary>
public class SizzlingProductService : ISizzlingProductService
{
    /// <inheritdoc />
    public SizzlingHotProductsResponse GetSizzlingHotProducts(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly today)
    {
        var dates = Enumerable.Range(0, 3)
            .Select(i => today.AddDays(-i))
            .OrderBy(d => d)
            .ToList();

        var dailyResults = dates
            .Select(date => GetTopProductForDay(orders, products, date))
            .Where(r => r is not null)
            .Cast<DailySizzlingResult>()
            .ToList();

        var periodResult = GetTopProductForPeriod(
            orders, products, dates.First(), dates.Last());

        return new SizzlingHotProductsResponse
        {
            DailyResults = dailyResults,
            ThreeDayResult = periodResult ?? new PeriodSizzlingResult
            {
                PeriodStart = dates.First().ToString("dd/MM/yyyy"),
                PeriodEnd = dates.Last().ToString("dd/MM/yyyy")
            }
        };
    }

    /// <inheritdoc />
    public DailySizzlingResult? GetTopProductForDay(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly date)
    {
        var scores = CalculateProductScores(orders, products, date, date);

        if (scores.Count == 0) return null;

        var winner = PickWinner(scores, products);

        return new DailySizzlingResult
        {
            Date = date.ToString("dd/MM/yyyy"),
            ProductId = winner.productId,
            ProductName = winner.productName,
            SaleCount = winner.score
        };
    }

    /// <inheritdoc />
    public PeriodSizzlingResult? GetTopProductForPeriod(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly from,
        DateOnly to)
    {
        var scores = CalculateProductScores(orders, products, from, to);

        if (scores.Count == 0) return null;

        var winner = PickWinner(scores, products);

        return new PeriodSizzlingResult
        {
            PeriodStart = from.ToString("dd/MM/yyyy"),
            PeriodEnd = to.ToString("dd/MM/yyyy"),
            ProductId = winner.productId,
            ProductName = winner.productName,
            SaleCount = winner.score
        };
    }

    // ─── Private helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Core scoring engine. Returns a dictionary of productId → unique-customer-sale-count
    /// for all completed orders in [from, to], minus any that were later cancelled.
    /// </summary>
    private static Dictionary<string, int> CalculateProductScores(
        IReadOnlyList<Order> orders,
        IReadOnlyList<Product> products,
        DateOnly from,
        DateOnly to)
    {
        // Build a lookup of orderId → original order for fast cancellation resolution.
        // Where duplicate orderId entries exist, keep the first completed one we see.
        var orderById = orders
            .Where(o => o.IsCompleted)
            .GroupBy(o => o.OrderId)
            .ToDictionary(g => g.Key, g => g.First());

        // Collect the set of original orderIds that have been cancelled.
        // BR3: the sale credit is applied to the day the original order was placed.
        var cancelledOriginalIds = orders
            .Where(o => o.IsCancelled)
            .Select(o => o.OrderId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Valid completed orders in the date window that haven't been cancelled.
        var validOrders = orders
            .Where(o =>
                o.IsCompleted &&
                !cancelledOriginalIds.Contains(o.OrderId) &&
                o.ParsedDate >= from &&
                o.ParsedDate <= to)
            .ToList();

        // Build a set of known product IDs for quick lookup.
        var knownProductIds = products
            .Select(p => p.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Scores: productId → unique (customerId, date, productId) count.
        // BR1: quantity is irrelevant — an order either contains a product or not.
        // BR2: same customer, same product, same day across multiple orders = 1 sale.
        var seenSales = new HashSet<(string customerId, DateOnly date, string productId)>();
        var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var order in validOrders)
        {
            if (string.IsNullOrWhiteSpace(order.CustomerId)) continue;

            // Get distinct product IDs in this order (BR1 - quantity ignored).
            var distinctProductIds = order.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Id))
                .Select(e => e.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var productId in distinctProductIds)
            {
                // Skip unknown products (graceful degradation).
                if (!knownProductIds.Contains(productId)) continue;

                // BR2: deduplicate by (customer, date, product).
                var saleKey = (order.CustomerId, order.ParsedDate, productId);
                if (!seenSales.Add(saleKey)) continue;

                scores.TryGetValue(productId, out var current);
                scores[productId] = current + 1;
            }
        }

        return scores;
    }

    /// <summary>
    /// Given a scores map, picks the winner:
    /// highest score, then alphabetically by product name on a tie (BR4).
    /// </summary>
    private static (string productId, string productName, int score) PickWinner(
        Dictionary<string, int> scores,
        IReadOnlyList<Product> products)
    {
        var productNameById = products
            .ToDictionary(p => p.Id, p => p.Name, StringComparer.OrdinalIgnoreCase);

        var winner = scores
            .Select(kvp => (
                productId: kvp.Key,
                productName: productNameById.GetValueOrDefault(kvp.Key, kvp.Key),
                score: kvp.Value))
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.productName, StringComparer.OrdinalIgnoreCase)
            .First();

        return winner;
    }
}
