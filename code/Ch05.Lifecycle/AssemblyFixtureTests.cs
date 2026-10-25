using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Ch05.Lifecycle.Production;

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 跨类身份登记:每个测试类把"我看到的夹具实例"记进同一张表;
/// 断言每个类型只出现一个实例身份,即证明程序集夹具跨类共享。
/// </summary>
internal static class FixtureIdentity
{
    private static readonly ConcurrentDictionary<(Type Type, int Id), byte> Seen = new();

    public static void Record<T>(T fixture) =>
        Seen.TryAdd((typeof(T), RuntimeHelpers.GetHashCode(fixture)), 0);

    public static void AssertSingleInstanceOf<T>() =>
        Assert.Equal(1, Seen.Count(entry => entry.Key.Type == typeof(T)));
}

/// <summary>
/// 程序集夹具不需要任何标记接口:构造参数直接注入(与类夹具不同)。
/// </summary>
public sealed class RunClockSharingTests
{
    private readonly TestRunClock clock;

    public RunClockSharingTests(TestRunClock clock)
    {
        this.clock = clock;
        FixtureIdentity.Record(clock);
    }

    [Fact]
    public void Clock_ExistsBeforeAnyTest_AndIsBuiltOnce()
    {
        // 本测试没有构造过 TestRunClock:InstancesCreated==1 说明它先于一切测试被创建
        Assert.Equal(1, TestRunClock.InstancesCreated);
        Assert.True(clock.RunStartedAt <= DateTimeOffset.UtcNow);

        FixtureIdentity.AssertSingleInstanceOf<TestRunClock>();
    }

    [Fact]
    public async Task AuditSink_SameInstanceAsInjectedInOtherClass()
    {
        // 这个类用 GetFixture 取另一个类将用构造注入的同一个实例(见 5.7)
        var sink = await TestContext.Current.GetFixture<AuditSink>();
        FixtureIdentity.Record(sink);

        FixtureIdentity.AssertSingleInstanceOf<AuditSink>();
    }
}

/// <summary>AuditSink 以泛型形式注册;这个类用构造注入取用它。</summary>
public sealed class AuditSinkSharingTests
{
    private readonly AuditSink sink;

    public AuditSinkSharingTests(AuditSink sink)
    {
        this.sink = sink;
        FixtureIdentity.Record(sink);
    }

    [Fact]
    public void Append_LandsInTheSharedSink()
    {
        sink.Append("from-AuditSinkSharingTests");
        Assert.Contains("from-AuditSinkSharingTests", sink.Snapshot());
    }

    [Fact]
    public void Sink_IsTheSameInstance_AsSeenViaGetFixture()
    {
        FixtureIdentity.AssertSingleInstanceOf<AuditSink>();
    }
}
