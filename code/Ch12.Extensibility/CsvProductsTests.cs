using Ch12.MyExtensions;

namespace Ch12.Extensibility.Tests;

/// <summary>
/// 自定义数据源:[CsvData] 把内嵌 CSV 解析成数据行;行首 ! 的行带行级 Skip;
/// 特性上的 Label/Traits 命名参数套到每条数据行,可在测试里经 TestContext 读到。
/// </summary>
public class CsvProductsTests(ITestOutputHelper output)
{
    private const string InventoryCsv = """
        1049,Widget,19.99
        2112,Gadget,3.50
        !2600,RetiredWidget,0.00
        """;

    [Theory]
    [CsvData(InventoryCsv, Label = "inventory-csv", Traits = new[] { "source", "csv" })]
    public void InventoryRow_PriceIsPositive(int id, string name, decimal price)
    {
        output.WriteLine($"行 {id}({name})价格 {price},标签 = {TestContext.Current.Test?.TestLabel}");
        Assert.True(price > 0m, $"行 {id} 的价格必须为正");
    }
}
