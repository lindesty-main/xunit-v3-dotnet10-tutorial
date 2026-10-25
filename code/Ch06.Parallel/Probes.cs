using Ch06.Parallel.Production;

namespace Ch06.Parallel.Tests;

/// <summary>
/// 观测器必须跨多个测试类可见,所以是 static。这里 static 是安全的:
/// <see cref="ConcurrencyProbe"/> 本身线程安全,计数又按作用域键隔离——
/// 与第 5 章警告过的"普通 static 字段被并行测试互相踩"是两回事。
/// </summary>
internal static class Probes
{
    public static readonly ConcurrencyProbe Shared = new();
}
