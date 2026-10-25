using System.Diagnostics;
using Ch08.Timeouts.Production;

namespace Ch08.Timeouts.Tests;

/// <summary>协作式取消:CancelCurrentTest 与令牌在测试、生产代码之间的透传链。</summary>
[Trait("kind", "unit")]
public class CooperativeCancelTests
{
    /// <summary>8.4:测试自己请求取消当前测试;第一个观察令牌的 API 抛出,测试失败。</summary>
    [Fact(Skip = "演示:去掉 Skip 复现测试被取消后的失败输出(见 8.4)")]
    public async Task ManualCancel_ObservedByToken_EndsTest()
    {
        TestContext.Current.CancelCurrentTest();

        await Task.Delay(5_000, TestContext.Current.CancellationToken);
        Assert.Fail("令牌被观察到了,这一行不会执行");
    }

    /// <summary>8.4:取消是请求不是强杀——请求发出后测试照常跑完,结果仍然是通过。</summary>
    [Fact]
    public void ManualCancel_Ignored_CompletesNormally()
    {
        TestContext.Current.CancelCurrentTest();

        Assert.True(TestContext.Current.CancellationToken.IsCancellationRequested);
        // 不观察令牌,方法正常返回——这条测试通过。
    }

    /// <summary>8.4:测试的令牌一路传进客户端;测试请求取消后,进行中的抓取被取消,异常由测试自己接住。</summary>
    [Fact]
    public async Task StockFeedClient_TestTokenRequested_CancelsInFlightFetch()
    {
        var client = new StockFeedClient(pageDelayMs: 400);
        var fetch = client.FetchSnapshotAsync(3, TestContext.Current.CancellationToken);

        TestContext.Current.CancelCurrentTest();

        var ex = await Record.ExceptionAsync(() => fetch);
        Assert.IsAssignableFrom<OperationCanceledException>(ex);
    }

    /// <summary>8.4:客户端把调用方令牌与自己的截止时间链接;截止先到,进行中的调用被取消。</summary>
    [Fact]
    public async Task StockFeedClient_PerCallDeadline_CancelsInFlightFetch()
    {
        var client = new StockFeedClient(pageDelayMs: 400); // 全量要 1.2s
        var watch = Stopwatch.StartNew();

        var ex = await Record.ExceptionAsync(() =>
            client.FetchSnapshotWithDeadlineAsync(TimeSpan.FromMilliseconds(500), CancellationToken.None));

        watch.Stop();
        Assert.IsAssignableFrom<OperationCanceledException>(ex);
        Assert.True(watch.ElapsedMilliseconds < 1_000,
            $"应在 500ms 截止处被取消,实际让调用方等了 {watch.ElapsedMilliseconds}ms");
    }
}
