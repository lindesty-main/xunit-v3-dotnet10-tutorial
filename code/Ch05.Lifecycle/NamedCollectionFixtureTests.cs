using System.Collections.Concurrent;
using Ch05.Lifecycle.Production;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 集合夹具:多个测试类共用一个实例,直到集合内全部类跑完才释放。
/// 同一个测试集合内的类不会并行运行——EnterAndAssertExclusive 用并发计数
/// 证明这条串行保证(并行模型本身见第 6 章)。
/// </summary>
public sealed class SharedOrdersFixture
{
    public static int InstancesCreated;
    public static readonly ConcurrentDictionary<Guid, byte> SeenIds = new();

    private static int inFlight;

    public Guid Id { get; } = Guid.NewGuid();

    public OrderLedger Ledger { get; } = new();

    public SharedOrdersFixture() => Interlocked.Increment(ref InstancesCreated);

    /// <summary>进入:如果此刻有另一个测试也在集合内运行,说明串行保证被破坏。</summary>
    public void EnterAndAssertExclusive()
    {
        var current = Interlocked.Increment(ref inFlight);
        Assert.True(current == 1,
            $"{current} tests were in flight at once; classes in one collection must not run in parallel");
    }

    public void Leave() => Interlocked.Decrement(ref inFlight);
}

/// <summary>
/// 集合定义类:从不被实例化,只承载 [CollectionDefinition] 与 ICollectionFixture&lt;T&gt;。
/// </summary>
[CollectionDefinition("orders-shared")]
public class OrdersCollection : ICollectionFixture<SharedOrdersFixture>
{
}

[Collection("orders-shared")]
public class OrderCreationTests
{
    private readonly SharedOrdersFixture fixture;

    public OrderCreationTests(SharedOrdersFixture fixture)
    {
        this.fixture = fixture;
        SharedOrdersFixture.SeenIds.TryAdd(fixture.Id, 0);
    }

    [Fact]
    public async Task Create_NewOrder_StateIsCreated()
    {
        fixture.EnterAndAssertExclusive();
        try
        {
            // 故意留出一个时间窗:若两个集合成员类被并行调度,计数会到 2,上面的断言就会失败
            await Task.Delay(100, TestContext.Current.CancellationToken);

            fixture.Ledger.Create("order-1");
            Assert.Equal(OrderLedger.OrderState.Created, fixture.Ledger.StateOf("order-1"));
        }
        finally
        {
            fixture.Leave();
        }
    }

    [Fact]
    public void Fixture_IsSharedAcrossClasses_InSameCollection()
    {
        Assert.Equal(1, SharedOrdersFixture.InstancesCreated);
        Assert.Single(SharedOrdersFixture.SeenIds);
    }
}

[Collection("orders-shared")]
public class OrderFulfillmentTests
{
    private readonly SharedOrdersFixture fixture;

    public OrderFulfillmentTests(SharedOrdersFixture fixture)
    {
        this.fixture = fixture;
        SharedOrdersFixture.SeenIds.TryAdd(fixture.Id, 0);
    }

    [Fact]
    public async Task Fulfill_CreatedOrder_StateIsFulfilled()
    {
        fixture.EnterAndAssertExclusive();
        try
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);

            fixture.Ledger.Create("order-2");
            Assert.True(fixture.Ledger.Fulfill("order-2"));
            Assert.Equal(OrderLedger.OrderState.Fulfilled, fixture.Ledger.StateOf("order-2"));
        }
        finally
        {
            fixture.Leave();
        }
    }

    [Fact]
    public void Ledger_IsTheSameInstance_AcrossClassesOfTheCollection()
    {
        // 两个类的构造器都向 SeenIds 登记夹具 Id:只出现一个 Id,
        // 说明两个类拿到的是同一个夹具、同一个 Ledger;这里写入的状态经由它读回
        fixture.Ledger.Create("order-3");
        Assert.Equal(OrderLedger.OrderState.Created, fixture.Ledger.StateOf("order-3"));
        Assert.Equal(1, SharedOrdersFixture.InstancesCreated);
        Assert.Single(SharedOrdersFixture.SeenIds);
    }
}
