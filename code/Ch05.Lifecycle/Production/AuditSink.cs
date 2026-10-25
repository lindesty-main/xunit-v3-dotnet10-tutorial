using System.Collections.Concurrent;

namespace Ch05.Lifecycle.Production;

/// <summary>
/// 追加式审计日志。第 5.5 节把它直接注册为程序集夹具:
/// 它被整个程序集的测试类共用,且程序集夹具不改变并行行为,实现必须线程安全。
/// </summary>
public sealed class AuditSink
{
    private readonly ConcurrentQueue<string> entries = new();

    public void Append(string message) => entries.Enqueue(message);

    public IReadOnlyList<string> Snapshot() => [.. entries];
}
