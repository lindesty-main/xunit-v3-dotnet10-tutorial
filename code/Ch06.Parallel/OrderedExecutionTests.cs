using Ch06.Parallel.Tests.Orderers;

namespace Ch06.Parallel.Tests;

// 排序器特性可以打在程序集上(影响全程序集),也可以打在类上(只影响这个类,
// 6.7 节讲四种排序器各自的适用层级)。这里用类级:其余测试仍保持默认随机顺序。
//
// 与 6.6 的随机演示相反,这个类里的顺序断言(位置 == 期望位置)在重复运行下
// 是稳定的——因为顺序器把顺序钉死了。

[TestMethodOrderer(typeof(DeclarationOrderMethodOrderer))]
public sealed class DeclarationOrderDemoTests
{
    private static readonly string[] Expected = ["alpha", "bravo", "charlie"];
    private static readonly List<string> ExecutionOrder = [];

    // 声明顺序即期望执行顺序;排序器负责把默认的随机顺序"扶正"为声明顺序。
    [Fact]
    public void Alpha_RunsFirst() => Record("alpha");

    [Fact]
    public void Bravo_RunsSecond() => Record("bravo");

    [Fact]
    public void Charlie_RunsThird() => Record("charlie");

    private static void Record(string label)
    {
        var output = TestContext.Current.TestOutputHelper!;
        int position;
        lock (ExecutionOrder)
        {
            ExecutionOrder.Add(label);
            position = ExecutionOrder.Count;
        }
        output.WriteLine($"order #{position}: {label}");
        Assert.Equal(Array.IndexOf(Expected, label) + 1, position);
    }
}
