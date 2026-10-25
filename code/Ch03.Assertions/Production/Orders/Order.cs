namespace Ch03.Assertions.Production.Orders;

public sealed class Order
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public DateTimeOffset PlacedAt { get; init; }
    public string Status { get; private set; } = "Created";
    public List<OrderLine> Lines { get; } = new();
    private string? trackingNumber;

    // 属性求值时抛异常:演示 Assert.Throws<T>(() => obj.Property) 的用法。
    public string TrackingNumber =>
        trackingNumber ?? throw new InvalidOperationException("订单尚未发货,没有运单号。");

    internal void MarkShipped(string tracking)
    {
        Status = "Shipped";
        trackingNumber = tracking;
    }
}
