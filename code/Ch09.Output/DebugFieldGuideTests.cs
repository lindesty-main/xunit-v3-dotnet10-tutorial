using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.6:失败输出的读法。六条演示测试都带 Skip;去掉 Skip 跑一次就能看到
/// 断言失败(Expected/Actual)、异常失败(堆栈)、长输出的默认截断。
/// </summary>
public class DebugFieldGuideTests(ITestOutputHelper output)
{
    [Fact(Skip = "演示:去掉 Skip 观察断言失败的四块信息(见 9.6)")]
    public void MismatchedRows_ExpectingOnlyDriftRow_FailsWithCollectionDiff()
    {
        output.WriteLine("对账批次 nightly-2026-10-22:发票 3 行 / 台账 3 行");

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        output.WriteLine("本次实际差异:{0}", string.Join(", ", mismatched.Select(row => row.Id)));

        Assert.Equal(["INV-1002"], mismatched.Select(row => row.Id));
    }

    [Fact(Skip = "演示:去掉 Skip 观察异常失败的堆栈定位(见 9.6)")]
    public void RowIdParsedAsAmount_ThrowsFormatException()
    {
        var row = InvoiceBook.Invoices[0];
        var amount = decimal.Parse(row.Id);

        Assert.True(amount > 0);
    }

    [Fact(Skip = "演示:去掉 Skip 观察集合差异的默认打印形态(见 9.6)")]
    public void RebuiltLedger_ComparingLongCollections_HitsDefaultPrintLimits()
    {
        var expected = Enumerable.Range(1, 40).Select(i => $"INV-{i:D4}").ToArray();
        var actual = Enumerable.Range(1, 40).Select(i => i == 37 ? "INV-9999" : $"INV-{i:D4}").ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact(Skip = "演示:去掉 Skip 观察集合打印的默认上限(见 9.6)")]
    public void NightlyDiff_ExpectingEmptyMismatchList_TruncatesCollectionPrint()
    {
        var rows = Enumerable.Range(1, 40).Select(i => new InvoiceRow($"INV-{i:D4}", "Fabrikam", i * 10m)).ToList();

        Assert.Empty(rows);
    }

    [Fact(Skip = "演示:去掉 Skip 观察字符串打印的默认上限(见 9.6)")]
    public void NightlyDiff_SearchingVendorInLongReport_TruncatesStringPrint()
    {
        var invoices = Enumerable.Range(1, 12).Select(i => new InvoiceRow($"INV-{i:D4}", "Fabrikam", i * 10m)).ToList();
        var ledger = invoices.ToDictionary(row => row.Id, row => row.Amount);
        var report = InvoiceReconciler.BuildDiffReport(invoices, ledger);

        Assert.Contains("Fabrikam Supply Co", report);
    }

    [Fact(Skip = "演示:去掉 Skip 观察 OverrideMaxEnumerableLength 放宽打印(见 9.6)")]
    public void NightlyDiff_RaisingLimitInsideTest_PrintsMoreItems()
    {
        var rows = Enumerable.Range(1, 40).Select(i => new InvoiceRow($"INV-{i:D4}", "Fabrikam", i * 10m)).ToList();

        Assert.OverrideMaxEnumerableLength(12);
        Assert.Empty(rows);
    }

    [Fact(Skip = "演示:去掉 Skip 观察长字符串的差异输出(见 9.6)")]
    public void DiffReport_ComparingTamperedReports_TruncatedByDefault()
    {
        var report = InvoiceReconciler.BuildDiffReport(InvoiceBook.Invoices, InvoiceBook.Ledger);

        Assert.Equal(report, report.Replace("Fabrikam", "Fabrikam Supply Co"));
    }
}
