using Ch07.Conditional.Production;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.4 分层:kind 表示"这层测试的边界",speed 表示"它值不值得在提交前跑"。
/// 类级 TraitAttribute 覆盖类里所有用例;方法上再补一个同名 Trait 就是多值。
/// </summary>
[Trait("kind", "unit")]
[Trait("speed", "fast")]
public class UnitLayerTests
{
    [Fact]
    public void Refund_NegativeQuantityRejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RefundCalculator.Refund(30.00m, 12.00m, -1));

    [Fact]
    public void TaxRules_KnownRegionsMapToZones() =>
        Assert.Equal("EU", TaxRules.ZoneOf("DE"));

    /// <summary>同一个 trait 名的多个值:这条同时属于 unit 与 smoke 两层。</summary>
    [Trait("kind", "smoke")]
    [Fact]
    public void TaxRules_UnknownRegionHasNoRule() =>
        Assert.False(TaxRules.HasRule("SG"));
}

[Trait("kind", "smoke")]
[Trait("speed", "fast")]
public class SmokeLayerTests
{
    [Fact]
    public void RefundFlow_ProducesWarningText()
    {
        var refund = RefundCalculator.Refund(30.00m, 12.00m, 5);
        var message = ReportFormatter.Warning("退款", $"{refund} 元已受理");
        Assert.Contains("退款", message);
        Assert.Contains("30", message);
    }
}

[Trait("kind", "integration")]
[Trait("speed", "slow")]
public class IntegrationLayerTests
{
    [Fact]
    public void Export_WritesInvoiceToConfiguredDirectory()
    {
        Assert.SkipUnless(DependencyProbe.ExportDirConfigured,
            "未配置 CH07_EXPORT_DIR;设 CH07_EXPORT_DIR=/tmp/ch07-out 再跑");

        var path = InvoiceWriter.WriteToConfiguredDirectory("smoke.csv", "id,amount\n1,12.00\n");
        Assert.Contains("1,12.00", File.ReadAllText(path));
    }

    [Fact]
    public void LegacyApi_HealthHandshake()
    {
        Assert.SkipUnless(DependencyProbe.LegacyApiAvailable,
            $"遗留计费 API 未监听 {DependencyProbe.LegacyApiHost}:{DependencyProbe.LegacyApiPort}");

        var (zone, _) = LegacyRateClient.FetchRate("EU");
        Assert.Equal("EU", zone);
    }
}
