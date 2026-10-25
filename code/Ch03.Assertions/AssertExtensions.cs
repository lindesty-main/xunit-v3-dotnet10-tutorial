using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.8 自定义断言:扩展方法包装,返回被验证值以支持链式;失败消息用 Assert.Fail 写清楚。
public static class AssertExtensions
{
    public static OrderLine HasTotal(this OrderLine line, decimal expectedTotal)
    {
        Assert.Equal(expectedTotal, line.LineTotal);
        return line;
    }

    public static OrderLine HasQuantityAtLeast(this OrderLine line, int minimum)
    {
        if (line.Quantity < minimum)
            Assert.Fail($"明细 {line.Sku} 的数量是 {line.Quantity},低于要求的 {minimum}。");
        return line;
    }

    public static Order IsShipped(this Order order)
    {
        if (order.Status != "Shipped")
            Assert.Fail($"期望订单 {order.Id} 处于 Shipped 状态,实际状态是 {order.Status}。");
        return order;
    }
}
