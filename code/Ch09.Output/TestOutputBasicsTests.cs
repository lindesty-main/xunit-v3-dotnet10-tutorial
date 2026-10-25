using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.1:ITestOutputHelper 的构造注入、四个写入重载,以及"输出只在失败时显示"的默认行为。
/// 通过 `--show-live-output on`(MTP)或 `-showLiveOutput`(原生 runner)可看到实时输出。
/// </summary>
public class TestOutputBasicsTests(ITestOutputHelper output)
{
    [Fact]
    public void WriteAndWriteLine_UsePlainAndFormattedOverloads()
    {
        output.Write("Write 不追加换行,");
        output.WriteLine("WriteLine 追加换行。");

        output.Write("格式化 Write:发票 {0} 行 / 台账 {1} 行", InvoiceBook.Invoices.Length, InvoiceBook.Ledger.Count);
        output.WriteLine("格式化 WriteLine:容差 {0:0.000} / 比例 {1:P1}", 0.005m, 0.0125m);

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        output.WriteLine("对账差异 {0} 行:{1}", mismatched.Count, string.Join(", ", mismatched.Select(row => row.Id)));

        Assert.Equal(2, mismatched.Count);
    }

    /// <summary>9.1:把输出器当参数传下去,辅助方法也能写测试输出(等价入口是 <c>TestContext.Current.TestOutputHelper</c>)。</summary>
    [Fact]
    public void HelperMethod_ReceivesOutputHelper_WritesIntoSameBuffer()
    {
        DescribeRows(output, InvoiceBook.Invoices);

        Assert.Equal(3, InvoiceBook.Invoices.Length);
    }

    private static void DescribeRows(ITestOutputHelper output, IReadOnlyList<InvoiceRow> rows)
    {
        output.WriteLine($"共 {rows.Count} 行发票:");
        foreach (var row in rows)
            output.WriteLine($"  {row.Id} {row.Vendor} {row.Amount:0.00}");
    }

    /// <summary>9.1:失败测试的输出会跟在失败报告里;这条测试的 <c>output</c> 内容只在它失败时才可见。</summary>
    [Fact(Skip = "演示:去掉 Skip 观察失败测试携带的输出(见 9.1)")]
    public void FindMismatchedRows_ExpectingCleanRun_FailsAndCarriesItsOutput()
    {
        output.WriteLine("本次对账输入:发票 3 行,台账 3 行");
        output.WriteLine("期望:0 行差异");

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        output.WriteLine("实际差异:{0}", string.Join(", ", mismatched.Select(row => row.Id)));

        Assert.Empty(mismatched);
    }
}
