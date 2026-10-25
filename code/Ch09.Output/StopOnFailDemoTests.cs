using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.6:`--stop-on-fail on`(MTP)/ `-stopOnFail`(原生)演示。四条测试同属一个集合,顺序执行:
/// 前三条都会失败,最后一条会通过。第一条失败之后,后面的测试不再启动。
/// 前三条都带 Skip;去掉 Skip 才能看到停止行为(最后一条始终通过,不加 Skip)。
/// </summary>
public class StopOnFailDemoTests
{
    [Fact(Skip = "演示:去掉 Skip 后本类前三条都会失败,配合 --stop-on-fail on 观察(见 9.6)")]
    public void NightBatch_AmountDrift_NotExpected()
    {
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.DoesNotContain("INV-1002", mismatched.Select(row => row.Id));
    }

    [Fact(Skip = "演示:去掉 Skip 后本类前三条都会失败,配合 --stop-on-fail on 观察(见 9.6)")]
    public void NightBatch_MissingLedgerRow_NotExpected()
    {
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.DoesNotContain("INV-1003", mismatched.Select(row => row.Id));
    }

    [Fact(Skip = "演示:去掉 Skip 后本类前三条都会失败,配合 --stop-on-fail on 观察(见 9.6)")]
    public void NightBatch_LedgerRowWithoutInvoice_NotExpected()
    {
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.Contains("INV-1004", mismatched.Select(row => row.Id));
    }

    /// <summary>9.6:这条会通过——但开了停止开关后,它排在失败之后,不会被执行。</summary>
    [Fact]
    public void NightBatch_MismatchCount_IsTwo()
    {
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.Equal(2, mismatched.Count);
    }
}
