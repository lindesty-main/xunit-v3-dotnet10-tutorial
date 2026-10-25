namespace Ch06.Parallel.Tests;

// 选择性退出并行:四个层级,各自只在对应模式下生效(规则见 6.4 节)。
// 本章项目默认按 collections 模式运行,所以下面所有断言在默认模式与 all 模式下都成立。

// ── 集合级退出:collections 与 all 模式都生效 ──
// 定义类上开 DisableParallelization → 这个集合整体不与任何其它测试并行。

[CollectionDefinition("nightly-window", DisableParallelization = true)]
public sealed class NightlyWindowCollection;

[Collection("nightly-window")]
public sealed class NightlyWindowOneTests
{
    [Fact]
    public void WindowOne_NeverConcurrentWithAnything()
    {
        using (Probes.Shared.Enter("nightly"))
            HoldWindow.Await("nightly-1", 600);
        Assert.Equal(1, Probes.Shared.MaxActive("nightly"));
    }
}

[Collection("nightly-window")]
public sealed class NightlyWindowTwoTests
{
    [Fact]
    public void WindowTwo_NeverConcurrentWithAnything()
    {
        using (Probes.Shared.Enter("nightly"))
            HoldWindow.Await("nightly-2", 600);
        Assert.Equal(1, Probes.Shared.MaxActive("nightly"));
    }
}

// ── 类级退出:只在 all 模式生效(collections 模式下类即集合,本来就串行) ──

[TestClass(DisableParallelization = true)]
public sealed class SerializedUnderAllModeTests
{
    [Fact]
    public void First_IsNotConcurrentInAllMode()
    {
        using (Probes.Shared.Enter("exclusive-class"))
            HoldWindow.Await("exclusive-1", 400);
        Assert.Equal(1, Probes.Shared.MaxActive("exclusive-class"));
    }

    [Fact]
    public void Second_IsNotConcurrentInAllMode()
    {
        using (Probes.Shared.Enter("exclusive-class"))
            HoldWindow.Await("exclusive-2", 400);
        Assert.Equal(1, Probes.Shared.MaxActive("exclusive-class"));
    }
}

// ── 方法级退出:只在 all 模式生效 ──

public sealed class MixedParallelismTests
{
    [Fact(DisableParallelization = true)]
    public void Isolated_UnderAllModeRunsAlone() =>
        HoldWindow.Await("isolated-fact", 400);

    [Fact]
    public void Parallel_UnderAllModeRunsWithEverything() =>
        HoldWindow.Await("parallel-fact", 400);
}

// ── 数据源级退出:DisableParallelization 写在数据特性上,作用于该特性产出的所有行;
//    只在 all 模式生效 ──

public sealed class DataSourceOptOutTests
{
    [Theory]
    [InlineData("runs-in-parallel")]
    [InlineData("isolated-by-attribute", DisableParallelization = true)]
    public void Rows(string expectation) =>
        HoldWindow.Await($"data-source:{expectation}", 300);
}

// ── 数据行级退出:逐行设置,只在 all 模式生效(第 4 章预告的兑现) ──

public sealed class DataRowOptOutTests
{
    public static IEnumerable<TheoryDataRow<string>> IsolatedAndParallelRows()
    {
        yield return new TheoryDataRow<string>("runs-in-parallel");
        yield return new TheoryDataRow<string>("isolated-by-row") { DisableParallelization = true };
    }

    [Theory]
    [MemberData(nameof(IsolatedAndParallelRows))]
    public void Rows(string expectation) =>
        HoldWindow.Await($"data-row:{expectation}", 300);
}
