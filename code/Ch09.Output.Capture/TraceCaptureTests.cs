using System.Diagnostics;
using Ch09.Output.Capture.Production;

namespace Ch09.Output.Capture.Tests;

/// <summary>
/// 9.2.2:程序集里加了 `[assembly: CaptureTrace]` 之后,`Debug` / `Trace` 的输出也进测试输出。
/// 边界:`Debug.WriteLine` 在 Release 编译里被编译器整体剔除(`DEBUG` 符号未定义),
/// 所以 Release 跑起来只剩 `Trace` 那行——用 `dotnet test -c Release` 可以当场验证。
/// </summary>
public class TraceCaptureTests(ITestOutputHelper output)
{
    [Fact]
    public void LegacyAuditWriter_DebugAndTrace_LandInThisTestsOutput()
    {
        output.WriteLine("[test] 看下面几行的前缀:Debug 与 Trace 各一条");

        var rows = new[]
        {
            new AuditRow("A-01", 100.00m, 100.00m),
            new AuditRow("A-02", 88.50m, 90.00m),
        };

        var unbalanced = LegacyAuditWriter.FindUnbalancedRows(rows);

        output.WriteLine("[test] 这里直接写 Debug/Trace,不经过任何生产代码");
        Debug.WriteLine("[direct-debug] 测试里直接写的 Debug");
        Trace.WriteLine("[direct-trace] 测试里直接写的 Trace");

        Assert.Single(unbalanced);
    }
}
