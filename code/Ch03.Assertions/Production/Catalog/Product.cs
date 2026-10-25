namespace Ch03.Assertions.Production.Catalog;

public sealed class Product
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
