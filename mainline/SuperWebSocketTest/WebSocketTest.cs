using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperWebSocket;
using SuperSocket.SocketBase.Config;
using System.Net.Sockets;
using System.IO;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketTest
    {
        private WebSocketServer m_WebSocketServer;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_WebSocketServer = new WebSocketServer();
            m_WebSocketServer.Setup(new ServerConfig
                {
                    Port = 911,
                    Ip = "Any",
                    MaxConnectionNumber = 100,
                    Mode = SocketMode.Async,
                    Name = "SuperWebSocket Server"
                }, SocketServerFactory.Instance);
        }

        [SetUp]
        public void StartServer()
        {
            m_WebSocketServer.Start();
        }

        [TearDown]
        public void StopServer()
        {
            m_WebSocketServer.Stop();
        }

        [Test]
        public void HeaderTest()
        {
            var ip = "127.0.0.1";
            var port = 911;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var address = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.Connect(address);

                var stream = new NetworkStream(socket);

                using(var reader = new StreamReader(stream, Encoding.UTF8, false))
                using(var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10))
                {
                    writer.WriteLine("GET /nothing HTTP/1.1");
                    writer.WriteLine("Upgrade: WebSocket");
                    writer.WriteLine("Connection: Upgrade");
                    writer.WriteLine("Host: {0}:{1}", ip, port);
                    writer.WriteLine("Origin: http://{0}:{1}", ip, port);
                    writer.WriteLine();
                    writer.Flush();

                    for (var i = 0; i < 4; i++)
                        Console.WriteLine(reader.ReadLine());
                }
            }
        }
    }
}
