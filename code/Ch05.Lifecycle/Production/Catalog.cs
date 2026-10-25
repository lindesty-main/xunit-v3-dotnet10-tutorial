namespace Ch05.Lifecycle.Production;

/// <summary>
/// 只读价格目录。构造器模拟昂贵加载(生成大量 SKU),
/// 第 5.3 节把它包进类夹具,让一个测试类共用一份。
/// </summary>
public sealed class Catalog
{
    private readonly Dictionary<string, decimal> prices;

    public Catalog(int skuCount)
    {
        prices = new Dictionary<string, decimal>(skuCount);
        for (var i = 0; i < skuCount; i++)
            prices[SkuName(i)] = 1.00m + (i % 100) * 0.50m;
    }

    public static string SkuName(int index) => $"SKU-{index:D5}";

    public bool TryGetPrice(string sku, out decimal price) =>
        prices.TryGetValue(sku, out price);
}
