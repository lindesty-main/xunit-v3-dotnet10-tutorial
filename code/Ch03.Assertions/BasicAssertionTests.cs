using Ch03.Assertions.Production.Customers;
using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.2 基础断言:True/False、Null/NotNull、Same/NotSame、Equal/NotEqual。
public class BasicAssertionTests
{
    [Fact]
    public void True_and_False_check_boolean_conditions()
    {
        var refundApproved = true;
        var orderExpired = false;

        Assert.True(refundApproved);
        Assert.False(orderExpired);
    }

    [Fact]
    public void Null_and_NotNull_check_references()
    {
        Customer? pending = null;
        var customer = new Customer { Name = "Ada" };

        Assert.Null(pending);
        Assert.NotNull(customer);
    }

    [Fact]
    public void Same_and_NotSame_compare_references_not_values()
    {
        var customer = new Customer { Name = "Ada" };
        Customer sameReference = customer;
        var lookalike = new Customer { Name = "Ada" };

        Assert.Same(customer, sameReference);
        Assert.NotSame(customer, lookalike);
    }

    [Fact]
    public void Equal_compares_values_for_value_types_and_records()
    {
        Assert.Equal(42, 42);
        Assert.Equal("ORD-1001", "ORD-1001");

        // record 编译器合成的 Equals 按成员比较,所以 Equal 通过。
        Assert.Equal(new OrderLine("SKU-9", 2, 10m), new OrderLine("SKU-9", 2, 10m));
    }

    [Fact]
    public void Equal_on_classes_without_Equals_compares_by_identity()
    {
        var failure = Record.Exception(() =>
            Assert.Equal(new Customer { Name = "Ada" }, new Customer { Name = "Ada" }));
        // Customer 没有重写 Equals,Equal 退化为引用比较。
        Assert.NotNull(failure); // "Values differ"
        Assert.Contains("Values differ", failure!.Message);
    }

    [Fact]
    public void Equal_does_not_bridge_different_CLR_types()
    {
        var failure = Record.Exception(() => Assert.Equal<object>(1, 1.0));

        // 失败输出里两个值打印得一模一样(Expected: 1 / Actual: 1),
        // 差别只在类型(int 与 double)。这是最难读的断言失败之一。
        Assert.NotNull(failure);
        Assert.Contains("Expected: 1", failure!.Message);
    }

    [Fact]
    public void Equal_on_collections_compares_element_by_element_in_order()
    {
        var actual = new List<string> { "SKU-1", "SKU-2" };

        // 数组与 List 都可以:按序逐元素比较,不要求 CLR 集合类型相同。
        Assert.Equal(new[] { "SKU-1", "SKU-2" }, actual);

        var swapped = Record.Exception(() =>
            Assert.Equal(new[] { "SKU-1", "SKU-2" }, new[] { "SKU-2", "SKU-1" }));
        Assert.NotNull(swapped);
    }

    [Fact]
    public void NotEqual_is_the_negation()
    {
        Assert.NotEqual("ORD-1001", "ORD-1002");
        Assert.NotEqual(new OrderLine("SKU-9", 2, 10m), new OrderLine("SKU-9", 3, 10m));
    }
}
