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
using SuperSocket.Common;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketTest
    {
        protected WebSocketServer m_WebSocketServer;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_WebSocketServer = new WebSocketServer();
            m_WebSocketServer.Setup(new RootConfig(), new ServerConfig
                {
                    Port = 911,
                    Ip = "Any",
                    MaxConnectionNumber = 100,
                    Mode = SocketMode.Sync,
                    Name = "SuperWebSocket Server"
                }, SocketServerFactory.Instance);

            m_WebSocketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(m_WebSocketServer_NewMessageReceived);
            m_WebSocketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(m_WebSocketServer_NewSessionConnected);
            m_WebSocketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(m_WebSocketServer_SessionClosed);
        }

        void m_WebSocketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            Console.WriteLine("Server:" + e);
            session.SendResponse(e);
        }

        void m_WebSocketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            
        }

        void m_WebSocketServer_NewSessionConnected(WebSocketSession session)
        {
            
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

        protected void Handshake(out Socket socket, out Stream stream)
        {
            var ip = "127.0.0.1";
            var port = 911;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, Encoding.UTF8, false);
            var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

            writer.WriteLine("GET /websock HTTP/1.1");
            writer.WriteLine("Upgrade: WebSocket");
            writer.WriteLine("Connection: Upgrade");
            writer.WriteLine("Sec-WebSocket-Key2: 12998 5 Y3 1  .P00");
            writer.WriteLine("Host: example.com");
            writer.WriteLine("Sec-WebSocket-Key1: 4 @1  46546xW%0l 1 5");
            writer.WriteLine("Origin: http://example.com");
            writer.WriteLine("WebSocket-Protocol: sample");
            writer.WriteLine("");
            string secKey = "^n:ds[4U";
            writer.Write(secKey);
            writer.Flush();

            //secKey.ToList().ForEach(c => Console.WriteLine((int)c));

            for (var i = 0; i < 6; i++)
                Console.WriteLine(reader.ReadLine());

            char[] buffer = new char[16];

            int totalRead = 0;

            while (totalRead < 16)
            {
                int read = reader.Read(buffer, totalRead, buffer.Length - totalRead);

                if (read <= 0)
                    Assert.Fail("Connection closed!");

                totalRead += read;
            }

            Assert.AreEqual("8jKS'y:G*Co,Wxa-", new string(buffer));
        }

        [Test]
        public void HandshakeTest06()
        {
            Socket socket;
            Stream stream;

            var ip = "127.0.0.1";
            var port = 911;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, Encoding.UTF8, false);
            var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

            writer.WriteLine("GET /websock HTTP/1.1");
            writer.WriteLine("Upgrade: WebSocket");
            writer.WriteLine("Connection: keep-alive, Upgrade");
            writer.WriteLine("Host: example.com");
            writer.WriteLine("Sec-WebSocket-Version: 6");
            writer.WriteLine("Sec-WebSocket-Key: x3JJHMbDL1EzLkh9GBhXDw==");
            writer.WriteLine("Origin: http://example.com");
            writer.WriteLine("WebSocket-Protocol: sample");
            writer.WriteLine();
            writer.Flush();

            string line = string.Empty;

            string acceptKey = "Sec-WebSocket-Accept:";
            string acceptValue = string.Empty;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Sec-WebSocket-Accept:"))
                {
                    acceptValue = line.Substring(acceptKey.Length).Trim();
                }
            }

            Assert.AreEqual("HSmrc0sMlYUkAGmm5OPpG2HaGWk=", acceptValue);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test]
        public void HandshakeTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test]
        public virtual void MessageTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            ArraySegmentList<byte> receivedBuffer = new ArraySegmentList<byte>();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string currentCommand = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + currentCommand);

                stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
                byte[] data = Encoding.UTF8.GetBytes(currentCommand);
                stream.Write(data, 0, data.Length);
                stream.Write(new byte[] { WebSocketConstant.EndByte }, 0, 1);
                stream.Flush();

                ReceiveMessage(stream, receivedBuffer, data.Length + 2);
                Assert.AreEqual(data.Length + 2, receivedBuffer.Count);
                Assert.AreEqual(WebSocketConstant.StartByte, receivedBuffer[0]);
                Assert.AreEqual(WebSocketConstant.EndByte, receivedBuffer[receivedBuffer.Count - 1]);
                Assert.AreEqual(currentCommand, Encoding.UTF8.GetString(receivedBuffer.ToArrayData(1, receivedBuffer.Count - 2)));
                receivedBuffer.ClearSegements();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        protected void ReceiveMessage(Stream stream, ArraySegmentList<byte> commandBuffer, int predictCount)
        {
            byte[] buffer = new byte[1024];
            int thisRead = 0;
            int left = predictCount;

            while ((thisRead = stream.Read(buffer, 0, Math.Min(left, buffer.Length))) > 0)
            {
                Console.WriteLine("Current read: {0}", thisRead);
                commandBuffer.AddSegment(new ArraySegment<byte>(buffer.Take(thisRead).ToArray()));
                left -= thisRead;

                if (left <= 0)
                    break;
            }
        }
    }
}
