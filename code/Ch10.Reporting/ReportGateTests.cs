using Ch10.Reporting.Production;

namespace Ch10.Reporting.Tests;

// ReportGateTests.PriceTag_is_printable(report=demo) 带 trait,
// 会出现在 CTRF 报告的 labels 字段里(见 10.4);
// Nightly_regression_placeholder 是故意 Skip 的,用来演示 --fail-skips on(见 10.6)。
public class ReportGateTests
{
    [Fact]
    [Trait("report", "demo")]
    public void PriceTag_is_printable()
    {
        var printer = new PriceTagPrinter();
        Assert.StartsWith("9,99", printer.FormatPrice(9.99));
    }

    [Fact(Skip = "nightly environment not yet stable")]
    public void Nightly_regression_placeholder()
    {
    }
}
