using Ch10.Reporting.Production;

namespace Ch10.Reporting.Tests;

// 方法名里刻意使用下划线、eq/ge 运算符别名和 U2212 转义序列,
// xunit.runner.json 的 methodDisplayOptions 会把它们变成可读的显示名(见 10.3)。
public class DisplayNamesDemoTests
{
    [Theory]
    [InlineData(1234.5, "1.234,50")]
    [InlineData(0.0, "0,00")]
    public void FormatPrice_eq_german_output(double amount, string expected)
    {
        var printer = new PriceTagPrinter();
        Assert.Equal(expected, printer.FormatPrice(amount));
    }

    [Fact]
    public void FormatPrice_U2212_negative_amount_keeps_minus()
    {
        var printer = new PriceTagPrinter();
        Assert.Equal("-42,00", printer.FormatPrice(-42));
    }

    [Fact]
    public void FormatPrice_ge_zero_never_uses_U2212()
    {
        var printer = new PriceTagPrinter();
        Assert.DoesNotContain("\u2212", printer.FormatPrice(7.5));
    }
}
