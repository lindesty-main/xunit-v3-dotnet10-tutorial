using System.Net.Sockets;

namespace Ch12.MyExtensions;

/// <summary>TCP 可达性探针:执行期的依赖门槛用它做运行时探测。</summary>
public static class TcpGate
{
    public static bool IsListening(string host, int port, int timeoutMilliseconds = 1000)
    {
        using var client = new TcpClient();
        try
        {
            return client.ConnectAsync(host, port).Wait(timeoutMilliseconds) && client.Connected;
        }
        catch (AggregateException exception) when (exception.InnerExceptions.All(e => e is SocketException))
        {
            return false;
        }
    }
}
