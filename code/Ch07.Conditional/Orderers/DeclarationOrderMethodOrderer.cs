using Xunit.Sdk;
using Xunit.v3;

namespace Ch07.Conditional.Tests.Orderers;

/// <summary>
/// 最小方法排序器:让同一个类里的测试按源码声明顺序执行。
/// 本章只在"证据测试需要读到前面几条测试留下的计数"时使用它;
/// 其余测试都不依赖顺序(第 6 章)。
/// </summary>
public sealed class DeclarationOrderMethodOrderer : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(
        IReadOnlyCollection<TTestMethod?> testMethods)
        where TTestMethod : ITestMethod =>
        [.. testMethods.OrderBy(m => ((IXunitTestMethod)m!).MetadataToken)];
}
