using Ch01.GettingStarted.Production;

namespace Ch01.GettingStarted.Tests;

public class PriceCalculatorTests
{
    private readonly PriceCalculator calculator = new();

    [Fact]
    public void Subtotal_AddsAllItemPrices()
    {
        var itemPrices = new[] { 19.90m, 5.00m, 75.05m };

        var subtotal = calculator.Subtotal(itemPrices);

        Assert.Equal(99.95m, subtotal);
    }

    [Fact]
    public void ApplyMemberDiscount_Reduces20WhenSubtotalReaches100()
    {
        var discounted = calculator.ApplyMemberDiscount(128.00m);

        Assert.Equal(108.00m, discounted);
    }

    // 故意写错的期望值:80 * 1.13 = 90.40,这里写成 90.30,用来演示失败输出长什么样。
    // 去掉 Skip 参数即可复现;带 Skip 是为了让本章工程始终全绿。
    [Fact(Skip = "演示用:故意失败的断言,去掉 Skip 参数即可复现")]
    public void TotalWithTax_AddsThirteenPercentTax()
    {
        var total = calculator.TotalWithTax(80.00m);

        Assert.Equal(90.30m, total);
    }
}
