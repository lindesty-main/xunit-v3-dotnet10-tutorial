using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 故意写错的断言:用于文档展示失败输出。正文引用的输出片段来自去掉
// Skip 后的真实运行;跑完再恢复 Skip,保证章工程全绿。
public class AssertionFailureDemosTests
{
    private readonly OrderService service = new();
    private static readonly OrderLine[] oneLine = [new OrderLine("SKU-1", 1, 5m)];

    [Fact(Skip = "演示:字符串 Equal 的失败输出,见第 3 章 3.1 节")]
    public void Equal_string_failure() => Assert.Equal("ORD-1001", "ORD-1002");

    [Fact(Skip = "演示:两个打印相同的值的 Equal 失败,见第 3 章 3.1 节")]
    public void Equal_same_looking_numbers_failure() => Assert.Equal<object>(1, 1.0);

    [Fact(Skip = "演示:抛异常而不是断言失败的输出,见第 3 章 3.1 节")]
    public void Raw_exception_failure()
    {
        var empty = new OrderLine[0];
        _ = empty[3];
    }

    [Fact(Skip = "演示:True 带 userMessage 的失败输出,见第 3 章 3.2 节")]
    public void True_with_user_message_failure()
    {
        var refundApproved = false;
        Assert.True(refundApproved, "退款应当已经被批准。");
    }

    [Fact(Skip = "演示:集合顺序不符的 Equal 失败输出,见第 3 章 3.4 节")]
    public void Collection_order_failure() => Assert.Equal(new[] { 1, 2, 3 }, new[] { 1, 3, 2 });

    [Fact(Skip = "演示:没抛异常时 Throws 的失败输出,见第 3 章 3.5 节")]
    public void Throws_no_exception_failure() =>
        Assert.Throws<InvalidOperationException>(() => service.Total(service.Create("cust-1", oneLine)));

    [Fact(Skip = "演示:Multiple 一次报告两个失败,见第 3 章 3.6 节")]
    public void Multiple_two_failures()
    {
        var order = service.Create("cust-1", oneLine);
        Assert.Multiple(
            () => Assert.Equal("Shipped", order.Status),
            () => Assert.Contains("优惠", service.ReceiptFor(order)));
    }

    [Fact(Skip = "演示:OverrideMaxStringLength 缩短失败输出,见第 3 章 3.8 节")]
    public void OverrideMaxStringLength_failure()
    {
        Assert.OverrideMaxStringLength(10);
        Assert.Equal(new string('a', 100), new string('b', 100));
    }
}
