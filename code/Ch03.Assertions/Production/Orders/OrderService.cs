using System.Text;

namespace Ch03.Assertions.Production.Orders;

public sealed class OrderService
{
    public event EventHandler<OrderShippedEventArgs>? Shipped;

    public Order Create(string customerId, IEnumerable<OrderLine> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

        var items = lines.ToList();
        if (items.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(lines), "订单至少要有一条明细。");

        var order = new Order
        {
            Id = $"ORD-{Guid.NewGuid():N}"[..12],
            CustomerId = customerId,
            PlacedAt = DateTimeOffset.UtcNow,
        };
        order.Lines.AddRange(items);
        return order;
    }

    public decimal Total(Order order) => order.Lines.Sum(l => l.LineTotal);

    public void Ship(Order order, string trackingNumber)
    {
        if (order.Status != "Created")
            throw new InvalidOperationException($"订单 {order.Id} 当前状态是 {order.Status},不能发货。");

        order.MarkShipped(trackingNumber);
        Shipped?.Invoke(this, new OrderShippedEventArgs(trackingNumber));
    }

    public string ReceiptFor(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"订单 {order.Id}");
        foreach (var line in order.Lines)
            sb.AppendLine($"{line.Sku} x{line.Quantity} 小计 {line.LineTotal:0.00}");
        sb.AppendLine($"合计 {Total(order):0.00}");
        return sb.ToString();
    }
}
