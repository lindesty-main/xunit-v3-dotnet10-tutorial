namespace Ch06.Parallel.Tests;

// ── 具名集合:两个类挂进同一个名字 → 它们与彼此串行(与其余集合仍然并行) ──
// 探针作用域 "billing" 被两个类共用:MaxActive == 1 是"集合内串行"的确定性证明,
// 与两个测试谁先执行无关。

[Collection("billing")]
public sealed class BillingWindowOneTests
{
    [Fact]
    public void WindowOne_RunsSeriallyWithinBilling()
    {
        using (Probes.Shared.Enter("billing"))
            HoldWindow.Await("billing-1", 1000);
        Assert.Equal(1, Probes.Shared.MaxActive("billing"));
    }
}

[Collection("billing")]
public sealed class BillingWindowTwoTests
{
    [Fact]
    public void WindowTwo_RunsSeriallyWithinBilling()
    {
        using (Probes.Shared.Enter("billing"))
            HoldWindow.Await("billing-2", 1000);
        Assert.Equal(1, Probes.Shared.MaxActive("billing"));
    }
}

// ── 强类型集合:定义类不带名字,测试类用泛型特性直接指向它 ──
// 名字拼错会悄悄挂出一个新集合;指向类型则拼错就编译不过。

[CollectionDefinition]
public sealed class AuditTrailCollection;

[Collection<AuditTrailCollection>]
public sealed class AuditWindowOneTests
{
    [Fact]
    public void WindowOne_RunsSeriallyWithinAudit()
    {
        using (Probes.Shared.Enter("audit"))
            HoldWindow.Await("audit-1", 400);
        Assert.Equal(1, Probes.Shared.MaxActive("audit"));
    }
}

[Collection<AuditTrailCollection>]
public sealed class AuditWindowTwoTests
{
    [Fact]
    public void WindowTwo_RunsSeriallyWithinAudit()
    {
        using (Probes.Shared.Enter("audit"))
            HoldWindow.Await("audit-2", 400);
        Assert.Equal(1, Probes.Shared.MaxActive("audit"));
    }
}
