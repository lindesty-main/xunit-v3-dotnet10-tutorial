using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.7 事件断言:RaisesAny/NotRaisedAny。
public class EventAssertionTests
{
    private static readonly OrderLine[] oneLine = [new OrderLine("SKU-1", 1, 5m)];

    [Fact]
    public void RaisesAny_attaches_detaches_and_captures_the_event()
    {
        var service = new OrderService();
        var order = service.Create("cust-1", oneLine);

        // 三个参数:挂接事件、摘除事件、触发事件的代码。
        var raised = Assert.RaisesAny<OrderShippedEventArgs>(
            handler => service.Shipped += handler,
            handler => service.Shipped -= handler,
            () => service.Ship(order, "TR-42"));

        // raised 携带 Sender 与事件参数(属性名是 Arguments)。
        Assert.Same(service, raised.Sender);
        Assert.Equal("TR-42", raised.Arguments.TrackingNumber);
    }

    [Fact]
    public void NotRaisedAny_verifies_that_no_event_fired()
    {
        var service = new OrderService();
        var order = service.Create("cust-1", oneLine);

        // 只计算金额,不会触发 Shipped。
        Assert.NotRaisedAny<OrderShippedEventArgs>(
            handler => service.Shipped += handler,
            handler => service.Shipped -= handler,
            () => service.Total(order));
    }
}
