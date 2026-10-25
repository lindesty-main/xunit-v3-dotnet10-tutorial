using System.Collections.Concurrent;

namespace Ch06.Parallel.Production;

/// <summary>
/// 线程安全的并发观测器:按命名作用域记录"正在执行"的测试数,并记住每个作用域的峰值。
/// 它会被多个测试类同时触碰,所以内部全部用原子操作;作用域按字符串键隔离,
/// 互不相关的测试组共用同一个实例也不会互相污染计数。
/// </summary>
public sealed class ConcurrencyProbe
{
    private readonly ConcurrentDictionary<string, int> active = new();
    private readonly ConcurrentDictionary<string, int> maxActive = new();

    /// <summary>标记一个作用域内的执行开始,返回的句柄在 Dispose 时标记执行结束。</summary>
    public IDisposable Enter(string scope)
    {
        int entering = active.AddOrUpdate(scope, 1, (_, current) => current + 1);
        maxActive.AddOrUpdate(scope, entering, (_, peak) => Math.Max(peak, entering));
        return new Lease(this, scope);
    }

    /// <summary>该作用域出现过的最大并发数;从未有测试进入过则为 0。</summary>
    public int MaxActive(string scope) =>
        maxActive.TryGetValue(scope, out int peak) ? peak : 0;

    private sealed class Lease(ConcurrencyProbe probe, string scope) : IDisposable
    {
        public void Dispose() =>
            probe.active.AddOrUpdate(scope, 0, (_, current) => current - 1);
    }
}
