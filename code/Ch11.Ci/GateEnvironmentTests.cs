using Ch11.Ci.Production;

namespace Ch11.Ci.Tests;

/// <summary>
/// "CI 环境变量判定"本身也要被测到——按第 7 章的模式,两个分支各一条用例,
/// 每条只守自己的分支:另一条的跳过原因恰好说明"为什么这次不能判断"。
/// </summary>
[Trait("kind", "unit")]
public class GateEnvironmentTests
{
    [Fact]
    public void GatePolicy_FromEnvironment_FailsSkipsOnCi()
    {
        Assert.SkipUnless(RunEnvironment.IsCi, "本次运行没有 CI 环境变量,CI 分支由另一条用例守");

        Assert.True(GatePolicy.FromEnvironment().FailSkips);
    }

    [Fact]
    public void GatePolicy_FromEnvironment_AllowsSkipsLocally()
    {
        Assert.SkipWhen(RunEnvironment.IsCi, "本次运行是 CI,本地分支由另一条用例守");

        Assert.False(GatePolicy.FromEnvironment().FailSkips);
    }
}
