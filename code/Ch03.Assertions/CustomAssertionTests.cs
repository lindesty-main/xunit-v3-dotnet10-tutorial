using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.8 自定义断言的用法,以及 OverrideMax* 对失败输出详略的控制。
public class CustomAssertionTests
{
    [Fact]
    public void Custom_assertions_chain_because_they_return_the_value()
    {
        var line = new OrderLine("SKU-1", 2, 10m);

        // 每个自定义断言返回被验证的值,可以链式串下去。
        line.HasTotal(20m).HasQuantityAtLeast(1);
    }

    [Fact]
    public void Assert_Fail_carries_the_message_you_wrote()
    {
        var order = new Order { Id = "ORD-7", CustomerId = "cust-1", PlacedAt = DateTimeOffset.UtcNow };

        var failure = Record.Exception(() => order.IsShipped());

        Assert.NotNull(failure);
        Assert.Contains("期望订单 ORD-7 处于 Shipped 状态,实际状态是 Created", failure!.Message);
    }

    [Fact]
    public void OverrideMaxStringLength_truncates_strings_in_failure_output()
    {
        // 只影响当前测试;把失败输出里字符串的打印长度压到 10 个字符。
        Assert.OverrideMaxStringLength(10);

        var failure = Record.Exception(() => Assert.Equal(new string('a', 100), new string('b', 100)));

        Assert.NotNull(failure);
        // 完整的一百个 a 不再出现在消息里,只剩截断后的片段。
        Assert.DoesNotContain(new string('a', 20), failure!.Message);
    }

    [Fact]
    public void OverrideMaxEnumerableLength_truncates_collections_in_failure_output()
    {
        Assert.OverrideMaxEnumerableLength(2);

        var failure = Record.Exception(() =>
            Assert.Equal(Enumerable.Range(1, 50), Enumerable.Range(2, 50)));

        Assert.NotNull(failure);
        // 集合只打印前两个元素,后面的元素不出现在消息里。
        Assert.DoesNotContain("49", failure!.Message);
    }
}
