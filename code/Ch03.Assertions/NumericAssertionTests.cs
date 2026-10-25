using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.3 数值与时间断言:tolerance/precision/MidpointRounding、DateTime/DateTimeOffset precision。
public class NumericAssertionTests
{
    private static readonly OrderLine[] lines = [new OrderLine("SKU-1", 2, 10m), new OrderLine("SKU-2", 1, 5m)];

    [Fact]
    public void Floating_point_sum_is_not_the_decimal_literal()
    {
        var failure = Record.Exception(() => Assert.Equal(0.1 + 0.2, 0.3));

        // 0.1 + 0.2 == 0.30000000000000004,裸 Equal 必挂。
        Assert.NotNull(failure);
    }

    [Fact]
    public void Tolerance_allows_a_fixed_absolute_deviation()
    {
        Assert.Equal(0.1 + 0.2, 0.3, tolerance: 1e-9);
        Assert.NotEqual(0.1 + 0.2, 0.3, tolerance: 1e-20);
    }

    [Fact]
    public void Precision_rounds_both_sides_to_N_decimal_places()
    {
        Assert.Equal(0.1 + 0.2, 0.3, precision: 6);

        // 取整到 0 位小数:1.5 与 2.4 都归到 2。
        Assert.Equal(1.5, 2.4, precision: 0, rounding: MidpointRounding.ToEven);

        var failure = Record.Exception(() => Assert.Equal(1.5, 2.6, precision: 0));
        Assert.NotNull(failure);
    }

    [Fact]
    public void Decimal_precision_works_the_same_way()
    {
        Assert.Equal(10.123m, 10.124m, precision: 2);
    }

    [Fact]
    public void DateTime_and_DateTimeOffset_accept_a_precision_window()
    {
        var before = DateTimeOffset.UtcNow;
        var order = new OrderService().Create("cust-1", lines);

        // 断言"下单时间不早于记录前的时刻,误差 30 秒以内"。
        Assert.Equal(before, order.PlacedAt, precision: TimeSpan.FromSeconds(30));

        Assert.Equal(new DateTime(2026, 1, 1), new DateTime(2026, 1, 1, 0, 0, 1),
            precision: TimeSpan.FromMinutes(1));
    }
}
