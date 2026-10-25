using Ch07.Conditional.Production;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.2 运行时决定跳过:断言式(Assert.Skip*)与特性级(SkipUnless/SkipWhen)。
/// </summary>
[Trait("kind", "integration")]
[Trait("speed", "slow")]
public class DynamicSkipTests
{
    /// <summary>供特性级 SkipUnless 使用:必须是指向 bool 的 public static 属性。</summary>
    public static bool LegacyApiReady => DependencyProbe.LegacyApiAvailable;

    // ---------- 断言式:测试体内部决定 ----------

    [Fact]
    public void LegacyApi_RateRoundTrip()
    {
        if (!DependencyProbe.LegacyApiAvailable)
            Assert.Skip(
                $"遗留计费 API 未监听 {DependencyProbe.LegacyApiHost}:{DependencyProbe.LegacyApiPort};" +
                "用 CH07_LEGACY_API=1 可以强制进入断言");

        var (zone, rate) = LegacyRateClient.FetchRate("EU");
        Assert.Equal("EU", zone);
        Assert.Equal(0.19m, rate);
    }

    [Fact]
    public void LegacyApi_BatchImportMatchesRate()
    {
        Assert.SkipUnless(DependencyProbe.LegacyApiAvailable,
            "遗留计费 API 不可用,批量导入无从比对");

        var (zone, rate) = LegacyRateClient.FetchRate("NO");
        Assert.Equal(0.25m, rate);
        Assert.Equal("NO", zone);
    }

    /// <summary>同一张表里,有规则的区参与断言,没规则的区只留哨兵。</summary>
    [Theory]
    [InlineData("DE", "EU")]
    [InlineData("NO", "NO")]
    [InlineData("SG", "SG")]
    public void TaxRule_OnlyCheckedWhereRuleExists(string region, string expectedZone)
    {
        Assert.SkipWhen(!TaxRules.HasRule(region), $"{region} 不在税则表内,保留用例但不参与断言");
        Assert.Equal(expectedZone, TaxRules.ZoneOf(region));
    }

    // ---------- 特性级:属性在执行期读取 ----------

    /// <summary>
    /// SkipUnless + SkipType:门属性可以放在别的类上。非 Linux 时跳过;本机是 Linux,所以这条真的执行。
    /// </summary>
    [Fact(Skip = "非 Linux 没有 /proc,进程探针无从校验",
        SkipUnless = nameof(RunEnvironment.IsLinux), SkipType = typeof(RunEnvironment))]
    public void ProcessProbe_SelfStatusReadable()
    {
        var status = ProcessProbe.ReadSelfStatus();
        Assert.Contains("Name:", status);
        Assert.Contains("Pid:", status);
    }

    /// <summary>属性级 SkipUnless:跳过消息仍来自 Skip。</summary>
    [Fact(Skip = "遗留 API 不在,批量导入用例挂起", SkipUnless = nameof(LegacyApiReady))]
    public void LegacyApi_ImportContract()
    {
        var (zone, _) = LegacyRateClient.FetchRate("EU");
        Assert.Equal("EU", zone);
    }

    /// <summary>
    /// 故意漏掉 Skip 的反例:标成 Explicit 让章工程默认全绿,
    /// 想看它的真实错误输出就单独跑(见文档 7.2.2)。
    /// </summary>
    [Fact(Explicit = true, SkipUnless = nameof(LegacyApiReady))]
    public void Probe_MissingSkipMessage() =>
        Assert.Fail("如果 Skip 配好了,这里根本不该执行");

    // ---------- SkipExceptions:把已知异常变成跳过 ----------

    [Fact(SkipExceptions = [typeof(ExportUnavailableException)])]
    public void InvoiceWriter_WritesToConfiguredDirectory()
    {
        var path = InvoiceWriter.WriteToConfiguredDirectory(
            "invoice-1.csv", "id,amount\n1,12.00\n");
        Assert.Equal("invoice-1.csv", Path.GetFileName(path));
        Assert.Contains("1,12.00", File.ReadAllText(path));
    }
}
