using FluentAssertions;
using SizzlingHotProducts.Core.Services;
using SizzlingHotProducts.Tests.Helpers;
using Xunit;

namespace SizzlingHotProducts.Tests;

/// <summary>
/// Unit tests for <see cref="SizzlingProductService"/>.
/// Each test class maps to a business rule (BR) or scenario.
/// </summary>
public class SizzlingProductServiceTests
{
    private readonly SizzlingProductService _sut = new();
    private static readonly DateOnly Today = new(2026, 4, 23);

    // ═══════════════════════════════════════════════════════════════════════
    // BR1 - Quantity is irrelevant; count product once per order
    // ═══════════════════════════════════════════════════════════════════════

    public class BR1_QuantityIgnored : SizzlingProductServiceTests
    {
        [Fact]
        public void SingleOrderWithHighQuantity_CountsAsOneSale()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithProduct("PA", quantity: 99).Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result.Should().NotBeNull();
            result!.SaleCount.Should().Be(1);
            result.ProductId.Should().Be("PA");
        }

        [Fact]
        public void MultipleProductsInOneOrder_EachCountsOnce()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1")
                    .WithProduct("PA", quantity: 5)
                    .WithProduct("PB", quantity: 10)
                    .Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            // Both products have 1 sale; PA ("Alpha Product") wins alphabetically.
            result!.SaleCount.Should().Be(1);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // BR2 - Same customer, same product, same day → counted only once
    // ═══════════════════════════════════════════════════════════════════════

    public class BR2_DuplicateCustomerOrderSameDay : SizzlingProductServiceTests
    {
        [Fact]
        public void SameCustomerOrdersSameProductTwiceOnSameDay_CountedOnce()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O2").WithCustomer("C1").WithProduct("PA").Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result!.SaleCount.Should().Be(1);
        }

        [Fact]
        public void DifferentCustomersOrderSameProduct_EachCounted()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O2").WithCustomer("C2").WithProduct("PA").Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result!.SaleCount.Should().Be(2);
        }

        [Fact]
        public void SameCustomerOrdersSameProductOnDifferentDays_EachCounted()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithDate("21/04/2026").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O2").WithCustomer("C1").WithDate("22/04/2026").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O3").WithCustomer("C1").WithDate("23/04/2026").WithProduct("PA").Build(),
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForPeriod(orders, products, new(2026, 4, 21), Today);

            result!.SaleCount.Should().Be(3, "one unique (customer, date, product) combo per day");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // BR3 - Cancelled orders remove the original sale
    // ═══════════════════════════════════════════════════════════════════════

    public class BR3_CancelledOrders : SizzlingProductServiceTests
    {
        [Fact]
        public void CancelledOrder_RemovesOriginalSaleFromCount()
        {
            var orders = new[]
            {
                // C1 ordered PA on day 1
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithDate("21/04/2026").WithProduct("PA").Build(),
                // C2 ordered PA on day 1
                OrderBuilder.Create().WithId("O2").WithCustomer("C2").WithDate("21/04/2026").WithProduct("PA").Build(),
                // C2 cancels O2 on day 2 → O2's sale should be removed
                OrderBuilder.Create().WithId("O2").WithDate("22/04/2026").Cancelled().Build()
            };
            var products = ProductFactory.TwoProducts();

            // Day 1 count for PA should be 1 (only C1's order; C2's O2 was cancelled)
            var result = _sut.GetTopProductForDay(orders, products, new(2026, 4, 21));

            result!.SaleCount.Should().Be(1);
        }

        [Fact]
        public void AllOrdersCancelled_ReturnsNoWinner()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O1").Cancelled().Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result.Should().BeNull();
        }

        [Fact]
        public void CancellationOfOneOrder_DoesNotAffectOtherCustomers()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O2").WithCustomer("C2").WithProduct("PA").Build(),
                // C1 cancels — C2's order should still count
                OrderBuilder.Create().WithId("O1").Cancelled().Build()
            };
            var products = ProductFactory.TwoProducts();

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result!.SaleCount.Should().Be(1);
            result.ProductId.Should().Be("PA");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // BR4 - Ties broken alphabetically by product name
    // ═══════════════════════════════════════════════════════════════════════

    public class BR4_AlphabeticTieBreak : SizzlingProductServiceTests
    {
        [Fact]
        public void TiedProducts_ReturnsFirstAlphabetically()
        {
            // "BBQ Tongs" < "Hammer" alphabetically
            var products = new[]
            {
                new SizzlingHotProducts.Core.Models.Product { Id = "P_H", Name = "Hammer" },
                new SizzlingHotProducts.Core.Models.Product { Id = "P_B", Name = "BBQ Tongs" },
            };
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithCustomer("C1").WithProduct("P_H").Build(),
                OrderBuilder.Create().WithId("O2").WithCustomer("C2").WithProduct("P_B").Build(),
            };

            var result = _sut.GetTopProductForDay(orders, products, Today);

            result!.ProductId.Should().Be("P_B", "BBQ Tongs sorts before Hammer");
            result.ProductName.Should().Be("BBQ Tongs");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Expected outcomes from the brief (integration-style smoke tests)
    // ═══════════════════════════════════════════════════════════════════════

    public class ExpectedOutcomes : SizzlingProductServiceTests
    {
        // Shared input data mirroring the repository's sample files.
        private static readonly List<SizzlingHotProducts.Core.Models.Order> SampleOrders =
        [
            // 21/04/2026
            new() { OrderId="O10", CustomerId="C1", Date="21/04/2026", Status="completed", Entries=[new(){Id="P1",Quantity=1}] },
            new() { OrderId="O20", CustomerId="C2", Date="21/04/2026", Status="completed", Entries=[new(){Id="P1",Quantity=1}] },
            new() { OrderId="O30", CustomerId="C2", Date="21/04/2026", Status="completed", Entries=[new(){Id="P2",Quantity=1}] },
            new() { OrderId="O31", CustomerId="C3", Date="21/04/2026", Status="completed", Entries=[new(){Id="P2",Quantity=1},new(){Id="P1",Quantity=2}] },
            new() { OrderId="O32", CustomerId="C32",Date="21/04/2026", Status="completed", Entries=[new(){Id="P2",Quantity=1}] },
            // 22/04/2026 - O30 cancelled
            new() { OrderId="O30", CustomerId="C2", Date="22/04/2026", Status="cancelled" },
            new() { OrderId="O40", CustomerId="C3", Date="22/04/2026", Status="completed", Entries=[new(){Id="P4",Quantity=2}] },
            new() { OrderId="O60", CustomerId="C3", Date="22/04/2026", Status="completed", Entries=[new(){Id="P4",Quantity=2},new(){Id="P1",Quantity=2}] },
            new() { OrderId="O70", CustomerId="C4", Date="22/04/2026", Status="completed", Entries=[new(){Id="P5",Quantity=2}] },
            new() { OrderId="O80", CustomerId="C5", Date="22/04/2026", Status="completed", Entries=[new(){Id="P1",Quantity=2}] },
            new() { OrderId="O81", CustomerId="C5", Date="22/04/2026", Status="completed", Entries=[new(){Id="P1",Quantity=10}] },
            // 23/04/2026
            new() { OrderId="O90",  CustomerId="C5", Date="23/04/2026", Status="completed", Entries=[new(){Id="P1",Quantity=1}] },
            new() { OrderId="O100", CustomerId="C3", Date="23/04/2026", Status="completed", Entries=[new(){Id="P4",Quantity=1},new(){Id="P6",Quantity=3}] },
        ];

        private static readonly List<SizzlingHotProducts.Core.Models.Product> SampleProducts =
            ProductFactory.StandardCatalogue();

        [Fact]
        public void Day_21Apr_TopProduct_IsEzyStorageBasket()
        {
            var result = _sut.GetTopProductForDay(SampleOrders, SampleProducts, new(2026, 4, 21));
            result!.ProductName.Should().Be("Ezy Storage 37L Flexi Laundry Basket - White");
        }

        [Fact]
        public void Day_22Apr_TopProduct_IsEzyStorageBasket()
        {
            var result = _sut.GetTopProductForDay(SampleOrders, SampleProducts, new(2026, 4, 22));
            result!.ProductName.Should().Be("Ezy Storage 37L Flexi Laundry Basket - White");
        }

        [Fact]
        public void Day_23Apr_TopProduct_IsArlecSolarKit()
        {
            var result = _sut.GetTopProductForDay(SampleOrders, SampleProducts, new(2026, 4, 23));
            result!.ProductName.Should().Be("Arlec 160W Crystalline Solar Foldable Charging Kit");
        }

        [Fact]
        public void Period_21To23Apr_TopProduct_IsEzyStorageBasket()
        {
            var result = _sut.GetTopProductForPeriod(
                SampleOrders, SampleProducts, new(2026, 4, 21), new(2026, 4, 23));
            result!.ProductName.Should().Be("Ezy Storage 37L Flexi Laundry Basket - White");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Edge cases
    // ═══════════════════════════════════════════════════════════════════════

    public class EdgeCases : SizzlingProductServiceTests
    {
        [Fact]
        public void EmptyOrders_ReturnsNull()
        {
            var result = _sut.GetTopProductForDay([], ProductFactory.TwoProducts(), Today);
            result.Should().BeNull();
        }

        [Fact]
        public void OrdersOutsideWindow_NotCounted()
        {
            var orders = new[]
            {
                // Only order is on a different day to the query
                OrderBuilder.Create().WithId("O1").WithDate("01/01/2025").WithProduct("PA").Build()
            };

            var result = _sut.GetTopProductForDay(orders, ProductFactory.TwoProducts(), Today);
            result.Should().BeNull();
        }

        [Fact]
        public void UnknownProductInOrder_IsSkippedGracefully()
        {
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithProduct("UNKNOWN_ID").Build()
            };
            // Product catalogue does not contain UNKNOWN_ID
            var result = _sut.GetTopProductForDay(orders, ProductFactory.TwoProducts(), Today);
            result.Should().BeNull("unknown products are skipped");
        }

        [Fact]
        public void ThreeDayResponse_ContainsAllThreeDays()
        {
            // Three orders on three different days
            var orders = new[]
            {
                OrderBuilder.Create().WithId("O1").WithDate("21/04/2026").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O2").WithDate("22/04/2026").WithProduct("PA").Build(),
                OrderBuilder.Create().WithId("O3").WithDate("23/04/2026").WithProduct("PA").Build(),
            };

            var result = _sut.GetSizzlingHotProducts(orders, ProductFactory.TwoProducts(), Today);

            result.DailyResults.Should().HaveCount(3);
            result.ThreeDayResult.Should().NotBeNull();
        }
    }
}
