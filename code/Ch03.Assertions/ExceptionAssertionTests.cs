using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.5 异常断言:Throws/ThrowsAsync、ThrowsAny、paramName、inspector、Record。
public class ExceptionAssertionTests
{
    private readonly OrderService service = new();
    private static readonly OrderLine[] oneLine = [new OrderLine("SKU-1", 1, 5m)];

    [Fact]
    public void Throws_requires_the_exact_type_and_returns_the_exception()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => service.Create(null!, oneLine));

        // 拿到实例后继续断言消息、ParamName 等细节。
        Assert.Equal("customerId", ex.ParamName);
    }

    [Fact]
    public void Throws_does_not_accept_derived_types_use_ThrowsAny_for_those()
    {
        // 实际抛出的是 ArgumentNullException(派生自 ArgumentException),
        // 按 ArgumentException 精确匹配会失败。
        var mismatch = Record.Exception(() =>
            Assert.Throws<ArgumentException>(() => service.Create(null!, oneLine)));
        Assert.Contains("Exception type was not an exact match", mismatch!.Message);

        // ThrowsAny 接受该类型及其派生类。
        var ex = Assert.ThrowsAny<ArgumentException>(() => service.Create(null!, oneLine));
        Assert.Equal("customerId", ex.ParamName);
    }

    [Fact]
    public async Task ThrowsAsync_must_be_awaited()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("网络中断,发货失败。");
        });

        Assert.Equal("网络中断,发货失败。", ex.Message);
    }

    [Fact]
    public void Throws_with_paramName_checks_ArgumentException_ParamName_in_one_step()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>("lines",
            () => service.Create("cust-1", Array.Empty<OrderLine>()));
        Assert.Contains("(Parameter 'lines')", ex.Message);

        var wrongName = Record.Exception(() =>
            Assert.Throws<ArgumentNullException>("other", () => service.Create(null!, oneLine)));
        Assert.Contains("Incorrect parameter name", wrongName!.Message);
    }

    [Fact]
    public void Inspector_overload_returns_null_when_acceptable_or_a_message_when_not()
    {
        // inspector:Func<T, string?>。返回 null 表示检查通过;返回字符串就是失败原因。
        var ex = Assert.Throws<InvalidOperationException>(
            () => service.Ship(ShippedOrder(), "TR-2"),
            e => e.Message.Contains("不能发货") ? null : "异常消息应包含「不能发货」");

        Assert.Contains("不能发货", ex.Message);

        var rejected = Record.Exception(() =>
            Assert.Throws<InvalidOperationException>(
                () => service.Ship(ShippedOrder(), "TR-2"),
                e => e.Message.Contains("无关要求") ? null : "异常消息应包含「无关要求」"));
        Assert.Contains("异常消息应包含「无关要求」", rejected!.Message);
    }

    [Fact]
    public void Property_accessors_bind_to_the_func_overload()
    {
        var order = service.Create("cust-1", oneLine);

        // 求值属性 TrackingNumber 时抛异常 —— 绑定到 Throws<T>(Func<object?>)。
        var ex = Assert.Throws<InvalidOperationException>(() => order.TrackingNumber);
        Assert.Equal("订单尚未发货,没有运单号。", ex.Message);
    }

    [Fact]
    public void Direct_throw_lambdas_need_an_explicit_Action_variable()
    {
        // 坑:Assert.Throws<X>(() => throw new X()) 编译不过(CS0619)——
        // 只有 throw 的 lambda 会绑定到已过时的 Throws<T>(Func<Task>) 重载。
        // 对测试内直接抛出的代码,先存进 Action 变量再传入。
        Action throwBoom = () => throw new InvalidOperationException("boom");
        var ex = Assert.Throws<InvalidOperationException>(throwBoom);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public void Record_is_for_only_caring_whether_it_threw()
    {
        var order = service.Create("cust-1", oneLine);
        service.Ship(order, "TR-1");

        // Record.Exception 返回异常或 null,不做任何类型断言。
        var again = Record.Exception(() => service.Ship(order, "TR-2"));
        Assert.NotNull(again); // 只要"确实抛了"

        Assert.Null(Record.Exception(() => service.Total(order))); // 反向:断言不抛
    }

    [Fact]
    public async Task RecordAsync_is_the_awaitable_form()
    {
        var ex = await Record.ExceptionAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        });
        Assert.NotNull(ex);

        var none = await Record.ExceptionAsync(async () => await Task.Yield());
        Assert.Null(none);
    }

    private Order ShippedOrder()
    {
        var order = service.Create("cust-1", oneLine);
        service.Ship(order, "TR-1");
        return order;
    }
}
