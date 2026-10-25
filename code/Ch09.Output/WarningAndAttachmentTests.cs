using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.4:警告与附件。警告随通过的测试一起报告;`--fail-warns on`(MTP)/ `-failWarns`(原生)
/// 会把"通过但带警告"的测试判为失败。附件在 MTP 下落盘到 results 目录,并进入报告文件。
/// </summary>
public class WarningAndAttachmentTests
{
    [Fact]
    public void Reconcile_LedgerRowWithoutInvoice_AddsWarningAndDiffAttachment()
    {
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);

        TestContext.Current.AddAttachment("nightly-diff.txt", InvoiceReconciler.BuildDiffReport(InvoiceBook.Invoices, InvoiceBook.Ledger));
        TestContext.Current.AddWarning("台账行 INV-1004 没有对应发票,按孤立台账行处理");

        Assert.Equal(2, mismatched.Count);
    }

    [Fact]
    public void Reconcile_AttachesBinaryArtifactWithMediaType()
    {
        // 1x1 红色 PNG:演示二进制附件必须给 mediaType
        var onePixelPng = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

        TestContext.Current.AddAttachment("pixel.png", onePixelPng, "image/png");

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.Equal(2, mismatched.Count);
    }
}
