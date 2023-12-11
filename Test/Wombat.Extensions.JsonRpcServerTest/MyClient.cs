using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Wombat.Extensions.JsonRpc.Client;

namespace Wombat.Extensions.JsonRpcServerTest
{
    public class MyClient
    {
        private readonly SocketConnection _conn;

        public static async Task<JsonRpcClient> ConnectAsync(int port)
        {
#if true
            PipeOptions.Default, client.GetStream()
            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", port);
            return new JsonRpcClient(new Pipe(new PipeOptions { })) {
                Timeout = Debugger.IsAttached ? TimeSpan.FromHours(1) : TimeSpan.FromSeconds(1)
            };
#else
            var c = await SocketConnection.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));
            return new JsonRpcClient(c) {
                Timeout = Debugger.IsAttached ? TimeSpan.FromHours(1) : TimeSpan.FromSeconds(1)
            };
#endif
        }
    }
}
