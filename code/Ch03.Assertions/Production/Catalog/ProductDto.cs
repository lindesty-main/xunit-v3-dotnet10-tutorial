namespace Ch03.Assertions.Production.Catalog;

// 与 Product 成员同名同形、类型不同:演示 Equivalent 的"跨类型结构比较"。
public sealed class ProductDto
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
