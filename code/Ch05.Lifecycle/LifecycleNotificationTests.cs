using Xunit.v3;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 4.0 管道通知接口:夹具在每个测试开始/结束时收到回调。
/// 这里用 INotifyTestLifecycleAsync 在每个测试开始前重置共享状态——
/// 夹具本身跨测试共享,但每个测试都从干净的起点开始(文档 5.7)。
/// 同步版 INotifyTestLifecycle 的 OnTestStarting/OnTestFinished 在 5.3 的
/// WarmCatalogFixture 里已经出现过;还有按集合/类/方法层回调的接口,见文档。
/// </summary>
public sealed class PerTestResetFixture : INotifyTestLifecycleAsync
{
    public int Counter { get; set; }

    public ValueTask OnTestStartingAsync(IXunitTest test)
    {
        Counter = 0;
        TestContext.Current.TestOutputHelper!.WriteLine(
            $"OnTestStartingAsync: reset Counter before [{test.TestDisplayName}]");
        return ValueTask.CompletedTask;
    }

    public ValueTask OnTestFinishedAsync(IXunitTest test)
    {
        TestContext.Current.TestOutputHelper!.WriteLine(
            $"OnTestFinishedAsync: Counter={Counter}");
        return ValueTask.CompletedTask;
    }
}

public sealed class PerTestResetTests : IClassFixture<PerTestResetFixture>
{
    private readonly PerTestResetFixture fixture;

    public PerTestResetTests(PerTestResetFixture fixture) => this.fixture = fixture;

    [Fact]
    public void SharedState_IsZero_BeforeEachTestStarts()
    {
        Assert.Equal(0, fixture.Counter);
        fixture.Counter = 7;
        Assert.Equal(7, fixture.Counter);
    }

    [Fact]
    public void SharedState_IsZero_EvenAfterAnotherTestLeftResidue()
    {
        // 另一个测试把 Counter 改到了 7(或 1):这里看到的仍是 0,
        // 说明 OnTestStartingAsync 在每个测试前都跑了
        Assert.Equal(0, fixture.Counter);
        fixture.Counter++;
        Assert.Equal(1, fixture.Counter);
    }
}
