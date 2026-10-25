using Ch07.Conditional.Production;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.3 Explicit:默认不参与运行,汇总里计成 Not Run。
/// 这里的用例都自带临时目录,被显式要求运行时应当通过 —— 它们不是"待修的债",而是"手动才做的事"。
/// </summary>
[Trait("kind", "manual")]
public class ManualCheckTests
{
    [Fact(Explicit = true, DisplayName = "手工:重建演示数据并读回")]
    public void DemoDatabase_ReseedAndRead()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ch07-demo", Guid.NewGuid().ToString("N"));
        try
        {
            var path = InvoiceWriter.Write(directory, "invoice-9.csv", "id,amount\n9,99.00\n");
            Assert.Contains("9,99.00", File.ReadAllText(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(HandCheckedRowCounts))]
    public void DemoDatabase_HandCheckedRowCount(int invoiceCount, int expectedLines)
    {
        var directory = Path.Combine(Path.GetTempPath(), "ch07-demo", Guid.NewGuid().ToString("N"));
        try
        {
            var csv = string.Join('\n', Enumerable.Range(1, invoiceCount).Select(i => $"{i},{i}.00"));
            var path = InvoiceWriter.Write(directory, "bulk.csv", csv);
            Assert.Equal(expectedLines, File.ReadAllLines(path).Length);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>数据行级 Explicit:整条 Theory 照常跑,只有这些行等手动触发。</summary>
    public static TheoryData<int, int> HandCheckedRowCounts => new()
    {
        new TheoryDataRow<int, int>(3, 3) { Explicit = true, TestDisplayName = "手工:3 张发票" },
        new TheoryDataRow<int, int>(5, 5) { Explicit = true, TestDisplayName = "手工:5 张发票" },
    };
}
