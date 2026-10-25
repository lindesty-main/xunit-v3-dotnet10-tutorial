using System.Net;
using System.Net.Sockets;

namespace Ch12.MyExtensions;

/// <summary>
/// 进程内 TCP 回环服务:在固定端口上幂等地起一个逐行回显监听。
/// 作为可复用的测试基础设施,任何引用本库的测试工程都能起同款服务。
/// </summary>
public static class EchoServer
{
    private static readonly object gate = new();
    private static readonly HashSet<int> startedPorts = [];

    public static void EnsureListening(int port)
    {
        lock (gate)
        {
            if (startedPorts.Contains(port))
                return;

            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            // Start 成功后再记录端口;绑定失败时,后续调用仍可重试。
            startedPorts.Add(port);
            _ = Task.Run(() => AcceptLoopAsync(listener));
        }
    }

    private static async Task AcceptLoopAsync(TcpListener listener)
    {
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => ServeAsync(client));
        }
    }

    private static async Task ServeAsync(TcpClient client)
    {
        using (client)
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            while (true)
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                    break;
                await stream.WriteAsync(buffer.AsMemory(0, read));
            }
        }
    }
}
