namespace Ch05.Lifecycle.Production;

/// <summary>
/// 库存台账:每个 SKU 持有数量,预占不允许部分满足。
/// 这是被测对象,不是夹具——第 5.1 节里每个测试都通过测试类构造器拿到全新的它。
/// </summary>
public sealed class ProductStore
{
    public const string DemoSku = "SKU-A";
    public const int DemoSkuSeedQuantity = 10;
    public const string SoldOutSku = "SKU-SOLD-OUT";

    private readonly Dictionary<string, int> quantities = new();

    public ProductStore()
    {
        quantities[DemoSku] = DemoSkuSeedQuantity;
        quantities[SoldOutSku] = 0;
    }

    public int QuantityOf(string sku) =>
        quantities.TryGetValue(sku, out var quantity) ? quantity : 0;

    public bool TryReserve(string sku, int quantity)
    {
        if (quantity <= 0)
            return false;
        if (!quantities.TryGetValue(sku, out var available) || available < quantity)
            return false;

        quantities[sku] = available - quantity;
        return true;
    }

    public void Restock(string sku, int quantity) =>
        quantities[sku] = QuantityOf(sku) + quantity;
}
