using Ch09.Output.Capture.Production;

namespace Ch09.Output.Capture.Tests;

/// <summary>
/// 9.2:程序集里加了 `[assembly: CaptureConsole]` 之后,生产代码写进标准输出/标准错误的
/// 内容会被重定向进"这个测试的输出"。用 `--show-live-output on` / `-showLiveOutput` 当场看,
/// 或者让它失败,输出会跟在失败报告里。
/// </summary>
public class ConsoleCaptureTests(ITestOutputHelper output)
{
    private static readonly AuditRow[] Rows =
    [
        new("A-01", 100.00m, 100.00m),
        new("A-02", 88.50m, 90.00m),
    ];

    [Fact]
    public void LegacyAuditWriter_ConsoleAndStderr_LandInThisTestsOutput()
    {
        output.WriteLine("[test] 测试自己写的一行:和捕获来的日志进同一个缓冲区");

        var unbalanced = LegacyAuditWriter.FindUnbalancedRows(Rows);

        output.WriteLine("[test] 测试自己写的另一行:不平 {0} 行", unbalanced.Count);

        Assert.Single(unbalanced);
    }

    [Fact(Skip = "演示:去掉 Skip 观察捕获的输出如何跟在失败报告里(见 9.2)")]
    public void LegacyAuditWriter_ExpectingBalancedBooks_FailsAndShowsCapturedLog()
    {
        var unbalanced = LegacyAuditWriter.FindUnbalancedRows(Rows);

        Assert.Empty(unbalanced);
    }
}

/// <summary>
/// 9.2.1:同一个程序集里的另一个测试类,自己没有加任何特性——捕获是程序集级的,
/// 这里照样被捕获。这正是把 CaptureConsole/CaptureTrace 放进独立示例工程的原因。
/// </summary>
public class ImplicitlyCapturedTests(ITestOutputHelper output)
{
    [Fact]
    public void NoAttributeOnThisClass_StillCaptured()
    {
        output.WriteLine("[test] 只有这一行是我主动写的");
        Console.WriteLine("[implicit] 这个类没加特性,Console 输出仍然进了测试输出");

        var rows = new[] { new AuditRow("B-01", 10m, 10m) };
        Assert.Empty(LegacyAuditWriter.FindUnbalancedRows(rows));
    }
}
