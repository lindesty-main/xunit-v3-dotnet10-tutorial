using Xunit.Sdk;
using Xunit.v3;

namespace Ch11.Ci.Tests.Orderers;

/// <summary>
/// 最小方法排序器:让同一个类里的测试按源码声明顺序执行(第 6 章的四级排序链)。
/// 本章只用它把 OrderCoupledTests 的顺序钉死,让"顺序耦合"这个反面教材
/// 在任何机器、任何路径上都以同一个顺序通过——它要演示的是"换个过滤器就露馅"。
/// </summary>
public sealed class DeclarationOrderMethodOrderer : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(
        IReadOnlyCollection<TTestMethod?> testMethods)
        where TTestMethod : ITestMethod =>
        [.. testMethods.OrderBy(m => ((IXunitTestMethod)m!).MetadataToken)];
}
