namespace Ch10.Reporting.Tests;

// 默认(或本工程的 assertEquivalentMaxDepth = 10)下,第三层的不匹配会被找到。
// 把深度上限调到 1(--assert-equivalent-max-depth 1,见 10.1)再跑这条测试,
// 它会变红:比较没有递归到第三层,抛出的异常变成了"超过最大深度"。
public class EquivalentDepthTests
{
    private sealed record Chain(string Name, Chain? Next);

    [Fact]
    public void Equivalent_detects_mismatch_at_depth_three()
    {
        var expected = new Chain("a", new Chain("b", new Chain("c", null)));
        var actual = new Chain("a", new Chain("b", new Chain("X", null)));

        var exception = Record.Exception(() => Assert.Equivalent(expected, actual));

        Assert.NotNull(exception);
        Assert.DoesNotContain("maximum depth", exception!.Message);
    }
}
