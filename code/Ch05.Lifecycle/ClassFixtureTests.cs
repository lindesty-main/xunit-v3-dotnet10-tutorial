using System.Collections.Concurrent;
using Ch05.Lifecycle.Production;
using Xunit.v3;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 类夹具:昂贵的"加载目录"只发生一次。构造器里做初始化,
/// Dispose 在类内全部测试结束后被调用——IsDisposed 标志与 Dispose 里的时机守卫
/// 从两个方向证明时机(文档 5.3)。
/// </summary>
public sealed class WarmCatalogFixture : IDisposable, INotifyTestLifecycle
{
    public static int InstancesCreated;
    public static int TestsFinished;

    public Guid Id { get; } = Guid.NewGuid();

    public Catalog Catalog { get; }

    public bool IsDisposed { get; private set; }

    public WarmCatalogFixture()
    {
        InstancesCreated++;
        Catalog = new Catalog(skuCount: 5_000);
    }

    // 框架在每个测试的开始/结束时回调这里(INotifyTestLifecycle,见 5.7)。
    public void OnTestStarting(IXunitTest test)
    {
    }

    public void OnTestFinished(IXunitTest test) => Interlocked.Increment(ref TestsFinished);

    public void Dispose()
    {
        // 时机守卫:若 Dispose 先于全部测试结束运行,这里会抛错,
        // 报告里出现独立的 [Test Class Cleanup Failure ...] 条目(文档 5.8)。
        // 本类共有 3 个执行中的测试。
        if (TestsFinished < 3)
            throw new InvalidOperationException(
                $"fixture disposed too early: only {TestsFinished} test(s) finished");

        IsDisposed = true;
    }
}

/// <summary>
/// IClassFixture&lt;T&gt;:实现接口即声明"我要这个夹具";构造参数只是取用方式,
/// 加不加参数都不影响夹具的创建(文档 5.3)。
/// </summary>
public sealed class CatalogWithClassFixtureTests : IClassFixture<WarmCatalogFixture>
{
    private static readonly ConcurrentDictionary<Guid, byte> SeenFixtureIds = new();

    private readonly WarmCatalogFixture fixture;

    public CatalogWithClassFixtureTests(WarmCatalogFixture fixture)
    {
        this.fixture = fixture;
        SeenFixtureIds.TryAdd(fixture.Id, 0);
    }

    [Fact]
    public void Fixture_IsConstructedExactlyOnce_BeforeAnyTest()
    {
        Assert.Equal(1, WarmCatalogFixture.InstancesCreated);

        // 类内测试还在进行时,夹具不会被 Dispose
        Assert.False(fixture.IsDisposed);
    }

    [Fact]
    public void SameFixtureInstance_IsSharedByEveryTestInClass()
    {
        // SeenFixtureIds 由每个测试类实例的构造器登记:出现第二个 Id 就说明夹具不止一个
        Assert.Equal(1, WarmCatalogFixture.InstancesCreated);
        Assert.Single(SeenFixtureIds);
        Assert.False(fixture.IsDisposed);
    }

    [Fact]
    public void TryGetPrice_SeededSku_ReturnsGeneratedPrice()
    {
        Assert.True(fixture.Catalog.TryGetPrice(Catalog.SkuName(7), out var price));
        Assert.Equal(4.50m, price);
    }
}

/// <summary>
/// 陷阱:Skip 只是不执行测试体,类夹具照样被构造(文档 5.3 计数断言)。
/// </summary>
public sealed class CountingFixture
{
    public static int CtorCount;

    public CountingFixture() => Interlocked.Increment(ref CtorCount);
}

public sealed class SkippedTestStillBuildsFixtureTests : IClassFixture<CountingFixture>
{
    [Fact(Skip = "测试体不执行,但类夹具照样被构造(见文档 5.3)")]
    public void Skipped_IsNotExecuted()
    {
    }

    [Fact]
    public void ClassFixture_IsConstructed_EvenThoughAnotherTestIsSkipped()
    {
        Assert.Equal(1, CountingFixture.CtorCount);
    }
}
