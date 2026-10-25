using Ch12.MyExtensions;

namespace Ch12.Extensibility.Tests;

/// <summary>
/// 运行时依赖门槛:[RequiresService] 在 Before 里做 TCP 探测,不通就抛
/// $XunitDynamicSkip$ 开头的异常 → 该测试被记为动态跳过(原因=标记后的文本)。
/// 同时用 [IntegrationTest](Fact+Trait 封装)标记种类,可被 --filter-trait 筛出。
/// </summary>
public class RequiresServiceTests
{
    public const int EchoPort = 39817;    // 进程内回环服务:静态构造里起起来
    public const int AbsentPort = 39818;  // 故意没有任何服务在监听

    static RequiresServiceTests() => EchoServer.EnsureListening(EchoPort);

    [IntegrationTest]
    [RequiresService("127.0.0.1", EchoPort)]
    public void Echo_RoundTripReceivesPayload() =>
        EchoAssert.RoundTrip("127.0.0.1", EchoPort, "hello-ch12");

    [IntegrationTest]
    [RequiresService("127.0.0.1", AbsentPort)]
    public void LegacyMirror_RoundTripReceivesPayload() =>
        EchoAssert.RoundTrip("127.0.0.1", AbsentPort, "legacy-mirror");
}
