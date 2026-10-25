using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.3 字符串断言:Contains/StartsWith/EndsWith、StringComparison、Equal 的四个 bool 开关。
public class StringAssertionTests
{
    private static readonly OrderLine[] lines = [new OrderLine("SKU-1", 2, 10m), new OrderLine("SKU-2", 1, 5m)];

    [Fact]
    public void Contains_StartsWith_EndsWith_check_substrings()
    {
        var service = new OrderService();
        var order = service.Create("cust-1", lines);
        var receipt = service.ReceiptFor(order);

        Assert.Contains($"合计 25.00{Environment.NewLine}", receipt);
        Assert.StartsWith("订单 ORD-", receipt);
        Assert.DoesNotContain("优惠", receipt);
    }

    [Fact]
    public void StringComparison_makes_matching_case_insensitive()
    {
        var message = "Order no. ORD-1001";

        Assert.Contains("ord-1001", message, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("order NO", message, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("ORD-1001", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INVOICE", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equal_string_takes_four_named_boolean_switches_not_a_StringComparison()
    {
        // 不存在 Equal(string, string, StringComparison) 重载;
        // 宽松比较走这四个默认 false 的命名参数。
        Assert.Equal("ORD-ABC", "ord-abc", ignoreCase: true);

        // \r\n、\r、\n 视为等价。
        Assert.Equal("a\r\nb", "a\nb", ignoreLineEndingDifferences: true);

        // 任意数量的连续水平空白视为等价("a  b" 与 "a b" 相同)。
        Assert.Equal("a  b", "a b", ignoreWhiteSpaceDifferences: true);

        // 连"没有空白"也视为等价:空白整体可有可无。
        Assert.Equal("a b\tc", "abc", ignoreAllWhiteSpace: true);
    }

    [Fact]
    public void Case_insensitive_equality_via_StringComparison_goes_through_Contains()
    {
        // 想要 "Equal 但忽略大小写" 的完整替代写法:
        // 要么 Equal(..., ignoreCase: true),要么用 Contains + OrdinalIgnoreCase 包住整串。
        var actual = "order-1001";
        Assert.Contains("order-1001", actual, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("order-1002", actual, StringComparison.OrdinalIgnoreCase);
    }
}
