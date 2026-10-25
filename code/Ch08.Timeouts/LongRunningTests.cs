using Ch08.Timeouts.Production;

namespace Ch08.Timeouts.Tests;

/// <summary>8.5:挂起检测。这条测试约 3 秒(不设 Timeout);用 -longRunning 1 / --long-running 1 观察它的报告形态。</summary>
[Trait("kind", "unit")]
[Trait("speed", "slow")]
public class LongRunningTests
{
    /// <summary>慢但合法的测试:挂起检测只是把它报告出来,结果仍是通过。</summary>
    [Fact]
    public async Task FeedFetch_SlowRound_IsReportedWhileStillRunning()
    {
        var snapshot = await new StockFeedClient(pageDelayMs: 1_000)
            .FetchSnapshotAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(12, snapshot.Count);
    }
}
