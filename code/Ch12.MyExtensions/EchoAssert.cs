using System.Net.Sockets;
using System.Text;
using Xunit;

namespace Ch12.MyExtensions;

/// <summary>断言助手:向回环服务发送一行,要求原样返回;失败时给出可定位的断言消息。</summary>
public static class EchoAssert
{
    public static void RoundTrip(string host, int port, string payload)
    {
        using var client = new TcpClient();
        if (!client.ConnectAsync(host, port).Wait(2000))
            Assert.Fail($"2 秒内未能连接回环服务 {host}:{port}");

        client.ReceiveTimeout = 2_000;
        var stream = client.GetStream();
        var request = Encoding.UTF8.GetBytes(payload + "\n");
        stream.Write(request);

        // TCP 是字节流,一次 Read 不保证返回对端一次 Write 的全部内容。
        // 已知请求的字节数,ReadExactly 会处理分段到达并在连接提前关闭时明确失败。
        var buffer = new byte[request.Length];
        stream.ReadExactly(buffer);
        var echoed = Encoding.UTF8.GetString(buffer).TrimEnd('\n');
        Assert.Equal(payload, echoed);
    }
}
