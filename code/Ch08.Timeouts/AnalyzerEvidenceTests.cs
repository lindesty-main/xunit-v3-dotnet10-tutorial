using System.Diagnostics;
using Ch08.Timeouts.Production;

namespace Ch08.Timeouts.Tests;

/// <summary>
/// 8.3:被 xUnit1069 / xUnit1051 警告过的写法修复后的形态。
/// 修复前的警告原文见正文;反例本体在 TimeoutSemanticsTests 的 pragma 块里。
/// </summary>
[Trait("kind", "unit")]
public class AnalyzerEvidenceTests
{
    /// <summary>xUnit1069 修复形态:设了 Timeout,测试体引用 TestContext.Current.CancellationToken。</summary>
    [Fact(Timeout = 2_000)]
    public void AuditLoop_AfterXUnit1069Fix_ReferencesToken()
    {
        var snapshot = new[] { new LedgerEntry("SKU-00", 5) };

        var discrepancies = StocktakeAggregator.AuditLedger(snapshot, rounds: 1_000, TestContext.Current.CancellationToken);

        Assert.Equal(0, discrepancies);
    }

    /// <summary>xUnit1051 修复形态:调用可接收令牌的方法时,把测试令牌传进去。</summary>
    [Fact]
    public async Task FeedFetch_AfterXUnit1051Fix_PassesTokenToCancellableCall()
    {
        var watch = Stopwatch.StartNew();

        await Task.Delay(50, TestContext.Current.CancellationToken); // 修复前:Task.Delay(50) 被 xUnit1051 警告

        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds >= 40, $"Task.Delay(50) 应真实等待,实际只等了 {watch.ElapsedMilliseconds}ms");
    }
}
