using System.Runtime.CompilerServices;

namespace Ch06.Parallel.Tests;

// 默认执行顺序是"稳定随机"的:方法顺序与声明顺序无关,也不保证任何顺序。
// 这个类不做任何顺序断言,只把"我何时执行"写进测试输出——
// 用 `-showLiveOutput` 就能看到真实的执行次序(6.6 节),重复运行次序不变,
// 改变程序集唯一 ID 后次序改变(6.8 节)。

public sealed class RandomOrderDemoTests
{
    [Fact]
    public void DeclaredFirst() => Note();

    [Fact]
    public void DeclaredSecond() => Note();

    [Fact]
    public void DeclaredThird() => Note();

    [Fact]
    public void DeclaredFourth() => Note();

    [Fact]
    public void DeclaredFifth() => Note();

    [Fact]
    public void DeclaredSixth() => Note();

    private static void Note([CallerMemberName] string from = "") =>
        TestContext.Current.TestOutputHelper!.WriteLine($"executed: {from}");
}
