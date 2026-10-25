using System.Reflection;
using Xunit.v3;

namespace Ch12.MyExtensions;

/// <summary>
/// 运行时依赖门槛:Before(在测试类构造之后、测试体之前)对目标端口做 TCP 探测;
/// 不通则抛一条消息以 $XunitDynamicSkip$ 开头的异常 —— runner 会把这条测试
/// 记为"动态跳过",跳过原因就是该标记之后的文本。
/// 与特性级 SkipUnless 的区别:条件无法编译期/启动期判定,只能在执行期探测。
/// </summary>
public sealed class RequiresServiceAttribute(string host, int port) : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo testMethod, IXunitTest test)
    {
        if (!TcpGate.IsListening(host, port))
            throw new Exception(
                $"$XunitDynamicSkip$所需服务 {host}:{port} 未监听;" +
                "请先启动依赖(或用夹具把它带起来)再运行这条测试");
    }
}
