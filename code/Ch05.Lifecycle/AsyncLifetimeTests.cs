namespace Ch05.Lifecycle.Tests;

/// <summary>
/// IAsyncLifetime:构造器里不能 await,异步的连接/预热放到 InitializeAsync,
/// 异步清理放到 DisposeAsync(它来自 IAsyncDisposable)。
/// 同时实现 IDisposable 时,框架只调 DisposeAsync,不调 Dispose(文档 5.2 计数断言)。
/// </summary>
public sealed class AsyncCacheClientTests : IAsyncLifetime, IDisposable
{
    private static int ctorCount;
    private static int initializeCalls;
    private static int syncDisposeCalls;
    private static int asyncDisposeCalls;

    private readonly int instanceNumber;
    private bool connectionReady;

    public AsyncCacheClientTests() => instanceNumber = Interlocked.Increment(ref ctorCount);

    public async ValueTask InitializeAsync()
    {
        // 模拟异步建立连接、预热缓存:这些代码写不进构造器
        await Task.Delay(50, TestContext.Current.CancellationToken);
        connectionReady = true;
        Interlocked.Increment(ref initializeCalls);
    }

    public ValueTask DisposeAsync()
    {
        Interlocked.Increment(ref asyncDisposeCalls);
        return ValueTask.CompletedTask;
    }

    public void Dispose() => Interlocked.Increment(ref syncDisposeCalls);

    [Fact]
    public void InitializeAsync_CompletesBeforeTestBody_RunsForEveryTest()
    {
        Assert.True(connectionReady, $"instance #{instanceNumber} saw no InitializeAsync");

        // 每个已构造的实例都在自己的测试体之前完成了 InitializeAsync
        Assert.Equal(ctorCount, initializeCalls);

        // 此前每个实例都已被异步释放(未释放的恰好只有当前这一个)
        Assert.Equal(ctorCount - 1, asyncDisposeCalls);
    }

    [Fact]
    public void SyncDispose_IsNeverCalled_WhenDisposeAsyncIsAvailable()
    {
        // 同时实现 IDisposable + IAsyncDisposable:只有 DisposeAsync 被调用
        Assert.Equal(0, syncDisposeCalls);
        Assert.Equal(ctorCount - 1, asyncDisposeCalls);
    }
}

/// <summary>
/// v3 还支持只实现 IAsyncDisposable(不实现 IAsyncLifetime):
/// 只要异步清理、不要异步初始化时使用(v2 不支持这条)。
/// </summary>
public sealed class AsyncDisposeOnlyTests : IAsyncDisposable
{
    private static int ctorCount;
    private static int asyncDisposeCalls;

    public AsyncDisposeOnlyTests() => Interlocked.Increment(ref ctorCount);

    public ValueTask DisposeAsync()
    {
        Interlocked.Increment(ref asyncDisposeCalls);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void DisposeAsync_RanForPreviousInstance_BeforeNextTestStarts()
    {
        // 本实例构造时,上一个测试的实例已经被 DisposeAsync
        Assert.Equal(ctorCount - 1, asyncDisposeCalls);
    }

    [Fact]
    public void DisposeAsync_RunsAfterEveryTest()
    {
        // 到本测试体运行时,前面的测试实例都已被异步释放
        Assert.Equal(ctorCount - 1, asyncDisposeCalls);
    }
}
