using System.Reflection;
using Ch07.Conditional.Production;
using Ch07.Conditional.Tests.Orderers;
using Xunit.v3;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.2.3 三种跳过的执行边界。类级 BeforeAfterTestAttribute + 构造器把到达过的阶段计数,
/// 最后一条见证用例把计数断言出来并打印成表 —— 这就是"跑到了哪一步"的直接证据。
/// 方法排序器在这里是必要的:见证用例必须读到前面几条测试留下的计数(第 6 章)。
/// </summary>
[RecordPipeline]
[TestMethodOrdererAttribute(typeof(DeclarationOrderMethodOrderer))]
[Trait("kind", "unit")]
[Trait("speed", "fast")]
public class PipelineEvidenceTests
{
    private readonly ITestOutputHelper output;

    public PipelineEvidenceTests(ITestOutputHelper output)
    {
        this.output = output;
        PipelineRecorder.Record(TestContext.Current.TestMethod!.MethodName, "类构造");
    }

    /// <summary>供特性级 SkipWhen 使用。</summary>
    public static bool AlwaysTrue => true;

    [Fact(Skip = "静态跳过:发现阶段就不参与执行")]
    public void A_StaticSkip() => Assert.Equal(1, 1);

    [Fact(Skip = "特性级跳过:执行期读属性", SkipWhen = nameof(AlwaysTrue))]
    public void B_AttributeDynamicSkip() => Assert.Equal(1, 1);

    [Fact]
    public void C_AssertDynamicSkip()
    {
        PipelineRecorder.Record(nameof(C_AssertDynamicSkip), "测试体");
        Assert.Skip("断言式跳过:决定发生在测试体里");
    }

    [Fact]
    public void D_WitnessCounts()
    {
        var names = new[] { nameof(A_StaticSkip), nameof(B_AttributeDynamicSkip), nameof(C_AssertDynamicSkip) };
        output.WriteLine(PipelineRecorder.Describe(names));

        // 静态跳过:什么都没发生
        Assert.Equal(0, PipelineRecorder.Count(nameof(A_StaticSkip), "类构造"));
        Assert.Equal(0, PipelineRecorder.Count(nameof(A_StaticSkip), "Before"));
        // 特性级跳过:实测同样没有构造测试类,BeforeAfterTest 也没执行
        Assert.Equal(0, PipelineRecorder.Count(nameof(B_AttributeDynamicSkip), "类构造"));
        Assert.Equal(0, PipelineRecorder.Count(nameof(B_AttributeDynamicSkip), "Before"));
        // 断言式跳过:整条流水线跑到 Assert.Skip 那一刻,After 仍然执行
        Assert.Equal(1, PipelineRecorder.Count(nameof(C_AssertDynamicSkip), "类构造"));
        Assert.Equal(1, PipelineRecorder.Count(nameof(C_AssertDynamicSkip), "Before"));
        Assert.Equal(1, PipelineRecorder.Count(nameof(C_AssertDynamicSkip), "测试体"));
        Assert.Equal(1, PipelineRecorder.Count(nameof(C_AssertDynamicSkip), "After"));
    }
}

/// <summary>把测试方法到达过的 Before / After 阶段记进 <see cref="PipelineRecorder"/>。</summary>
public sealed class RecordPipelineAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo testMethod, IXunitTest test) =>
        PipelineRecorder.Record(testMethod.Name, "Before");

    public override void After(MethodInfo testMethod, IXunitTest test) =>
        PipelineRecorder.Record(testMethod.Name, "After");
}
