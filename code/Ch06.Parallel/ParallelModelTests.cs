using System.Diagnostics;

namespace Ch06.Parallel.Tests;

// 默认并行模型的最小观测台:两个互不相干的类 = 两个默认集合。
// 默认模式(collections)下它们与彼此并行;这里刻意不做任何并发断言——
// "能不能并行"取决于调度,断言只会带来不稳定;证据用真实输出(6.1 节)。

public sealed class SlowWindowAlphaTests
{
    [Fact]
    public void Alpha_HoldsTheThreadForOneSecond() =>
        HoldWindow.Await("alpha", 1000);
}

public sealed class SlowWindowBetaTests
{
    [Fact]
    public void Beta_HoldsTheThreadForOneSecond() =>
        HoldWindow.Await("beta", 1000);
}

/// <summary>把"占住线程一段时间"这件事变成可观察的输出:开始/结束时刻与承载线程。</summary>
internal static class HoldWindow
{
    public static void Await(string name, int milliseconds)
    {
        var output = TestContext.Current.TestOutputHelper!;
        var start = Stopwatch.GetTimestamp();
        output.WriteLine($"[{name}] start  {DateTime.Now:HH:mm:ss.fff} thread={Environment.CurrentManagedThreadId}");
        Thread.Sleep(milliseconds);
        var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        output.WriteLine($"[{name}] end    {DateTime.Now:HH:mm:ss.fff} after {elapsed:F0}ms thread={Environment.CurrentManagedThreadId}");
    }
}
