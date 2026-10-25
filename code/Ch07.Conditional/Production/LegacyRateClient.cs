using System.Globalization;
using System.Net.Sockets;

namespace Ch07.Conditional.Production;

/// <summary>被测代码:遗留计费 API 的裸 TCP 客户端。服务不在时连接就失败。</summary>
public static class LegacyRateClient
{
    public static (string Zone, decimal Rate) FetchRate(string zone)
    {
        using var client = new TcpClient(DependencyProbe.LegacyApiHost, DependencyProbe.LegacyApiPort);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);

        writer.WriteLine(zone);
        var line = reader.ReadLine() ?? throw new IOException("遗留计费 API 没有响应");
        var parts = line.Split('\t');
        return (parts[0], decimal.Parse(parts[1], CultureInfo.InvariantCulture));
    }
}
