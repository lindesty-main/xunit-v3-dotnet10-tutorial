using Ch08.Timeouts.Production;

namespace Ch08.Timeouts.Tests;

/// <summary>
/// Timeout 的基本用法与真实语义。所有会失败的演示都挂着 Skip;
/// 去掉对应的 Skip 即可复现正文(8.1、8.2)里的失败输出。
/// </summary>
[Trait("kind", "unit")]
public class TimeoutSemanticsTests
{
    /// <summary>绿样例:时限给得宽裕、令牌传到位,Timeout 不参与,测试正常通过。</summary>
    [Fact(Timeout = 5_000)]
    public async Task FeedFetch_CompletesWellWithinTimeout()
    {
        var snapshot = await new StockFeedClient(pageDelayMs: 50)
            .FetchSnapshotAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(12, snapshot.Count);
    }

    /// <summary>8.1:v3 的 Timeout 对同步测试同样生效;生产代码每轮检查令牌,200ms 处被截停。</summary>
    [Fact(Timeout = 200, Skip = "演示:去掉 Skip 复现同步测试的超时失败(见 8.1)")]
    public void AuditLoop_ObservesToken_StopsAtTimeout()
    {
        var snapshot = Enumerable.Range(0, 200).Select(i => new LedgerEntry($"SKU-{i}", i)).ToList();
        StocktakeAggregator.AuditLedger(snapshot, rounds: 50_000_000, TestContext.Current.CancellationToken);
    }

    /// <summary>8.1:特性级 Timeout 作用于每一条数据行;150ms 的行按时通过,300ms 的行超时。</summary>
    [Theory(Timeout = 200, Skip = "演示:去掉 Skip 复现逐行超时(见 8.1)")]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FeedFetch_RowExceedsTheoryTimeout(int pages)
    {
        var snapshot = await new StockFeedClient(pageDelayMs: 150)
            .FetchSnapshotAsync(pages, TestContext.Current.CancellationToken);

        Assert.Equal(4 * pages, snapshot.Count);
    }

#pragma warning disable xUnit1051, xUnit1069 // 反例本身就是这两条规则警告的写法(见 8.2、8.3)
    /// <summary>8.2 反例:不把令牌传到任何会等待的地方。失败仍在 100ms 处报出,测试却没停,继续跑完。</summary>
    [Fact(Timeout = 100, Skip = "演示:去掉 Skip 复现'测试继续跑、异常迟到'的形态(见 8.2)")]
    public async Task FeedFetch_IgnoringToken_KeepsRunningAfterFailure()
        => await new StockFeedClient(pageDelayMs: 1_200).FetchSnapshotAsync(3);
#pragma warning restore xUnit1051, xUnit1069

    /// <summary>8.2 正例:同一个客户端,令牌传到位,100ms 处立即截停。</summary>
    [Fact(Timeout = 100, Skip = "演示:去掉 Skip 复现'立即截停'的超时失败(见 8.2)")]
    public async Task FeedFetch_PassesToken_StopsAtTimeout()
        => await new StockFeedClient(pageDelayMs: 1_200)
            .FetchSnapshotAsync(3, TestContext.Current.CancellationToken);
}
