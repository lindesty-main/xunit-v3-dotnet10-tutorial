using Ch07.Conditional.Production;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.2.5 环境分支:CI 与本地各有一条测试守自己的分支,另一条跳过。
/// 同一套用例在 CI(CI=1)与本地都能全绿,只是跳过的是不同那条。
/// </summary>
[Trait("kind", "unit")]
[Trait("speed", "fast")]
public class EnvironmentGateTests
{
    [Fact]
    public void ReportFormatter_UsesGithubActionsOnCi()
    {
        Assert.SkipUnless(RunEnvironment.IsCi, "本次运行没有 CI 环境变量,CI 分支由另一条用例守");
        Assert.Equal("github-actions", ReportFormatter.Select());
        Assert.Equal("::warning title=退款::30 元已受理", ReportFormatter.Warning("退款", "30 元已受理"));
    }

    [Fact]
    public void ReportFormatter_UsesConsoleLocally()
    {
        Assert.SkipWhen(RunEnvironment.IsCi, "本次运行是 CI,本地分支由另一条用例守");
        Assert.Equal("console", ReportFormatter.Select());
        Assert.Equal("WARN 退款: 30 元已受理", ReportFormatter.Warning("退款", "30 元已受理"));
    }
}
