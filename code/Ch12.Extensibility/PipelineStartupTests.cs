using Ch12.MyExtensions;

namespace Ch12.Extensibility.Tests;

/// <summary>
/// 管道启动:StartAsync 在任何发现/执行工作开始前就运行,
/// 所以第一条测试开始时,它写入的运行号一定已经存在。
/// </summary>
public class PipelineStartupTests(ITestOutputHelper output)
{
    [Fact]
    public void RunId_IsStampedBeforeAnyTest()
    {
        var runId = Environment.GetEnvironmentVariable(RunStampPipelineStartup.RunIdVariable);
        Assert.False(string.IsNullOrEmpty(runId), "StartAsync 应在任何测试开始前写入运行号");
        output.WriteLine($"本次运行的运行号:{runId}");
    }
}
