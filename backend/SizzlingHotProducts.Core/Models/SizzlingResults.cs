namespace SizzlingHotProducts.Core.Models;

/// <summary>
/// The top sizzling hot product for a given day.
/// </summary>
public class DailySizzlingResult
{
    public string Date { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int SaleCount { get; set; }
}

/// <summary>
/// The top sizzling hot product over a date range.
/// </summary>
public class PeriodSizzlingResult
{
    public string PeriodStart { get; set; } = string.Empty;
    public string PeriodEnd { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int SaleCount { get; set; }
}

/// <summary>
/// Combined response: daily history + 3-day rolling winner.
/// </summary>
public class SizzlingHotProductsResponse
{
    public List<DailySizzlingResult> DailyResults { get; set; } = [];
    public PeriodSizzlingResult ThreeDayResult { get; set; } = new();
}
