using System.Collections.Concurrent;
using Ch05.Lifecycle.Production;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 强类型集合:集合定义类用无参 [CollectionDefinition],测试类用
/// [Collection&lt;TDef&gt;](或等价的 [Collection(typeof(TDef))])直接指向定义类型,
/// 不再有"两边字符串名字必须一致"的隐性耦合(泛型特性要求 .NET 8+,本教程是 net10.0)。
/// </summary>
public sealed class ReservationLedgerFixture
{
    public static int InstancesCreated;
    public static readonly ConcurrentDictionary<Guid, byte> SeenIds = new();

    public Guid Id { get; } = Guid.NewGuid();

    public OrderLedger Ledger { get; } = new();

    public ReservationLedgerFixture() => Interlocked.Increment(ref InstancesCreated);
}

/// <summary>定义类同样从不被实例化;它只声明这个集合携带哪些夹具。</summary>
[CollectionDefinition]
public class InventoryReservationCollection : ICollectionFixture<ReservationLedgerFixture>
{
}

[Collection<InventoryReservationCollection>]
public class ReservationHoldTests
{
    [Fact]
    public async Task Hold_NotEnoughStock_ReserveFails()
    {
        // 这个类不写构造参数,用 TestContext.Current.GetFixture<T>() 取夹具(见 5.7)
        var fixture = (await TestContext.Current.GetFixture<ReservationLedgerFixture>())!;
        ReservationLedgerFixture.SeenIds.TryAdd(fixture.Id, 0);

        Assert.False(fixture.Ledger.Fulfill("not-an-order"));
    }

    [Fact]
    public async Task Fixture_IsSharedWithTheOtherTypedMember()
    {
        var fixture = (await TestContext.Current.GetFixture<ReservationLedgerFixture>())!;
        ReservationLedgerFixture.SeenIds.TryAdd(fixture.Id, 0);

        Assert.Equal(1, ReservationLedgerFixture.InstancesCreated);
        Assert.Single(ReservationLedgerFixture.SeenIds);
    }
}

[Collection(typeof(InventoryReservationCollection))]
public class ReservationReleaseTests
{
    private readonly ReservationLedgerFixture fixture;

    public ReservationReleaseTests(ReservationLedgerFixture fixture)
    {
        this.fixture = fixture;
        ReservationLedgerFixture.SeenIds.TryAdd(fixture.Id, 0);
    }

    [Fact]
    public void Release_CreatedOrder_StateIsFulfilled()
    {
        fixture.Ledger.Create("order-9");
        Assert.True(fixture.Ledger.Fulfill("order-9"));
        Assert.Equal(OrderLedger.OrderState.Fulfilled, fixture.Ledger.StateOf("order-9"));
    }

    [Fact]
    public void SameTypedCollection_TwoWaysToReference_StillOneFixture()
    {
        // [Collection<TDef>] 与 [Collection(typeof(TDef))] 指向同一个定义类型,
        // 因此两个类仍共用一个夹具实例
        Assert.Equal(1, ReservationLedgerFixture.InstancesCreated);
        Assert.Single(ReservationLedgerFixture.SeenIds);
    }
}
