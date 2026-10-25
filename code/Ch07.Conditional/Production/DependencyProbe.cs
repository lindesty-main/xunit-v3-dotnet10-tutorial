using System.Net.Sockets;

namespace Ch07.Conditional.Production;

/// <summary>
/// 外部依赖可用性判定:先看环境变量开关(强制开/关),没有开关再做真实探测。
/// 开关的存在是为了让"依赖没起来"和"依赖不存在"这两种情况都能被本地复现。
/// </summary>
public static class DependencyProbe
{
    public const string LegacyApiHost = "127.0.0.1";
    public const int LegacyApiPort = 18080;

    /// <summary>遗留计费 API:本章的 integration 层前置条件。</summary>
    public static bool LegacyApiAvailable =>
        Available("CH07_LEGACY_API", () => TcpOpen(LegacyApiHost, LegacyApiPort, 200));

    /// <summary>导出目录:配置了 CH07_EXPORT_DIR 才可能真的写出文件。</summary>
    public static bool ExportDirConfigured =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CH07_EXPORT_DIR"));

    public static bool Available(string switchName, Func<bool> probe) =>
        Environment.GetEnvironmentVariable(switchName) switch
        {
            "1" or "true" or "on" => true,
            "0" or "false" or "off" => false,
            _ => probe(),
        };

    /// <summary>端口探测:连不上或超时都算不可用,不抛异常。</summary>
    public static bool TcpOpen(string host, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        try
        {
            client.ConnectAsync(host, port)
                .Wait(TimeSpan.FromMilliseconds(timeoutMs));
            return client.Connected;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
