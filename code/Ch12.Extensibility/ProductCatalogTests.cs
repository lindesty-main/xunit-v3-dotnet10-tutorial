using Ch12.Extensibility.Production;

namespace Ch12.Extensibility.Tests;

/// <summary>
/// 含自定义 record 参数的 Theory:注册 ProductKeySerializer 后,
/// 每行数据都能被单独选中运行(显示名里带 ProductKey 的文本形式)。
/// </summary>
public class ProductCatalogTests
{
    public static TheoryData<ProductKey, string> ValidKeys => new()
    {
        { new ProductKey(1049, "widget24"), "widget24" },
        { new ProductKey(7, "gadgetX"), "gadgetX" },
        { new ProductKey(2112, "tool9"), "tool9" },
    };

    [Theory]
    [MemberData(nameof(ValidKeys))]
    public void ProductKey_RoundTripsThroughText(ProductKey key, string expectedSku)
    {
        Assert.Equal(expectedSku, key.Sku);
        Assert.Equal(key, ProductKey.Parse(key.ToString()));
    }

    [Theory]
    [InlineData("0:abc")]
    [InlineData("1049:")]
    [InlineData("abc:widget")]
    [InlineData("1049:bad-sku")]
    public void ProductKey_RejectsMalformedText(string text) =>
        Assert.False(ProductKey.TryParse(text, out _));
}
