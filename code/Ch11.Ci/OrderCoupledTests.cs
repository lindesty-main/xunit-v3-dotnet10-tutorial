using Ch11.Ci.Tests.Orderers;

namespace Ch11.Ci.Tests;

/// <summary>
/// 反面教材(11.6 的排查素材):两条测试通过静态字段传递状态,B 依赖 A 先跑。
/// 完整运行时它靠方法排序器钉死顺序而全绿;一旦 CI 用"只重跑失败用例"式的过滤器
/// 单独选中 B,前面那条不再执行,B 立刻红——本地全绿、CI 偶发红的最常见成因。
/// 正确做法是让每条测试自己准备自己的状态(对照 UnitDeploymentTests 的写法)。
/// </summary>
[TestMethodOrdererAttribute(typeof(DeclarationOrderMethodOrderer))]
[Trait("kind", "smoke")]
public class OrderCoupledTests
{
    private static string? deploymentMarker;

    [Fact]
    public void A_WritesDeploymentMarker() => deploymentMarker = "deployed";

    [Fact]
    public void B_ReadsDeploymentMarker() => Assert.Equal("deployed", deploymentMarker);
}
