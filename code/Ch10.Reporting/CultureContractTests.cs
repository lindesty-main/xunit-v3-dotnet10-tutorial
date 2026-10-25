using System.Globalization;
using Ch10.Reporting.Production;

namespace Ch10.Reporting.Tests;

// 这一组测试断言的是"工程被配置成 de-DE"这个前提本身。
// culture 来自 testconfig.json(见 10.2):只在 MTP 模式下生效,
// 用原生 runner 直接跑这个工程时,它们会按操作系统 culture(en-US)失败。
public class CultureContractTests
{
    [Fact]
    public void CurrentCulture_IsPinnedByTestConfig()
    {
        Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
    }

    [Fact]
    public void FormatPrice_UsesGermanNumberFormat()
    {
        var printer = new PriceTagPrinter();
        Assert.Equal("1.234,50", printer.FormatPrice(1234.5));
    }

    [Fact]
    public void ParseValidUntil_InterpretsDayFirst()
    {
        var printer = new PriceTagPrinter();
        Assert.Equal(new DateTime(2026, 2, 1), printer.ParseValidUntil("01.02.2026"));
    }

    [Fact]
    public void FormatPriceInvariant_IsStableAcrossCultures()
    {
        var printer = new PriceTagPrinter();
        Assert.Equal("1,234.50", printer.FormatPriceInvariant(1234.5));
    }
}
