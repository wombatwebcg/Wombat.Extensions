﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Wombat.Extensions.JsonRpc.Server;

namespace Wombat.Extensions.JsonRpcServerTest
{
	public class MyServer : JsonRpcServer
    {
        readonly SocketListener _listener;
        public ManualResetEventSlim ClientConnected { get; } = new ManualResetEventSlim();
        readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>();

        public MyServer(int port)
        {
            _listener = new SocketListener();
            _listener.Start(new IPEndPoint(IPAddress.Any, port));
            _listener.OnConnection(OnConnection);
        }

        private Task OnConnection(SocketConnection connection)
        {
            IClient client = AttachClient(connection.GetRemoteIp(), connection);
            ClientConnected.Set();
            return _tcs.Task;
        }

        public override void Dispose()
        {
            _tcs.TrySetCanceled();
            _listener?.Stop();
            base.Dispose();
        }

        internal class StaticHandler
        {
            public static void SpeedNoArgs()
            {
            }

            public static TestResponseData TestResponse(int n = 0, string s = "")
            {
                return new TestResponseData {
                    Number = 432,
                    StringArray = new[] { "a", "b", "c", "d", "e" },
                    Text1 = "Some text"
                };
            }
        }

        internal class TestResponseData
        {
            public int Number { get; set; }
            public string Text1 { get; set; }
            public string[] StringArray { get; set; }
        }
    }
}
