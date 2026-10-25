using System.Collections.Concurrent;
using Ch05.Lifecycle.Production;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// xUnit 的实例模型:每个测试都新建一个测试类实例——构造器跑在每个测试之前,
/// Dispose 跑在每个测试之后。计数断言与测试输出共同证明这一点(见文档 5.1)。
/// </summary>
public sealed class NewInstancePerTestTests : IDisposable
{
    private static int ctorCount;
    private static int disposeCount;
    private static readonly ConcurrentDictionary<int, byte> SeenInstanceNumbers = new();

    private readonly int instanceNumber;
    private readonly ProductStore store = new();

    public NewInstancePerTestTests() =>
        instanceNumber = Interlocked.Increment(ref ctorCount);

    public void Dispose() => Interlocked.Increment(ref disposeCount);

    [Fact]
    public void Ctor_RunsBeforeEveryTest_EachTestSeesItsOwnInstance()
    {
        TestContext.Current.TestOutputHelper!.WriteLine(
            $"instance #{instanceNumber} (ctorCount={ctorCount}, disposeCount={disposeCount})");

        // 本实例是最新构造的那个:构造器与测试体之间没有别的测试类实例被构造
        Assert.Equal(ctorCount, instanceNumber);

        // 此前每个测试实例都已经被 Dispose:此刻未释放的恰好只有当前这一个
        Assert.Equal(ctorCount - 1, disposeCount);

        // 如果实例被跨测试共享,两个测试会报出同一个实例编号,TryAdd 失败
        Assert.True(SeenInstanceNumbers.TryAdd(instanceNumber, 0),
            $"instance #{instanceNumber} was reused by another test");
    }

    [Fact]
    public void BusinessState_IsFresh_ForEveryTest()
    {
        TestContext.Current.TestOutputHelper!.WriteLine(
            $"instance #{instanceNumber} (ctorCount={ctorCount}, disposeCount={disposeCount})");

        Assert.Equal(ctorCount, instanceNumber);
        Assert.Equal(ctorCount - 1, disposeCount);

        // store 是构造器里 new 出来的:这个测试看到的只有种子状态,
        // 哪怕另一个测试已经预占过库存,这里也不会少
        Assert.Equal(ProductStore.DemoSkuSeedQuantity, store.QuantityOf(ProductStore.DemoSku));

        Assert.True(store.TryReserve(ProductStore.DemoSku, 3));
        Assert.False(store.TryReserve(ProductStore.SoldOutSku, 1));
        Assert.Equal(ProductStore.DemoSkuSeedQuantity - 3, store.QuantityOf(ProductStore.DemoSku));
    }
}

/// <summary>
/// 反例:实例是每测例新建的,但 static 字段不属于实例——它跨测试、跨实例共享。
/// 把业务状态放进 static,测试就从"任意顺序都绿"退化成"看运气绿"(文档 5.1)。
/// </summary>
public static class SharedRegistry
{
    public static readonly List<string> Entries = [];
}

public sealed class StaticStateCouplingDemoTests
{
    // 临时去掉 Skip 再运行,能看到必然发生的失败:两行数据同属一个集合,必有一行先跑
    // (先后顺序不保证);后跑的那行看到 static 里被前一行留下的一条记录,
    // "干净上下文"的假设就破了——哪一行红看运气,但一定会红(文档 5.1)。
    [Theory(Skip = "演示 static 状态耦合:去掉 Skip 运行,真实失败输出见文档 5.1")]
    [InlineData("row-1")]
    [InlineData("row-2")]
    public void SameTheory_SharedStaticState_BreaksCleanContextAssumption(string row)
    {
        Assert.Empty(SharedRegistry.Entries);
        SharedRegistry.Entries.Add(row);
    }
}
