using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using SuperWebSocket.Client;
using System.Threading;
using SuperWebSocket.SubProtocol;
using System.Reflection;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketClientTest
    {
        protected WebSocketServer m_WebSocketServer;
        private AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private AutoResetEvent m_OpenEvent = new AutoResetEvent(false);
        private AutoResetEvent m_CloseEvent = new AutoResetEvent(false);
        private string m_CurrentMessage = string.Empty;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_WebSocketServer = new WebSocketServer(new BasicSubProtocol(new List<Assembly> { this.GetType().Assembly }));
            m_WebSocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 911,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Sync,
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
        public void ConnectionTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://127.0.0.1:911/websocket", "basic");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnOpen += new EventHandler(webSocketClient_OnOpen);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            webSocketClient.Connect();

            if(!m_OpenEvent.WaitOne(1000))
                Assert.Fail("Failed to open session ontime");

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        [Test]
        public void SendMessageTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://127.0.0.1:911/websocket", "basic");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnOpen += new EventHandler(webSocketClient_OnOpen);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            webSocketClient.Connect();

            if (!m_OpenEvent.WaitOne(1000))
                Assert.Fail("Failed to open session ontime");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                webSocketClient.Send("ECHO " + message);

                Console.WriteLine("Client:" + message);

                if (!m_MessageReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, m_CurrentMessage);
            }

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }


        [Test]
        public void CloseWebSocketTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://127.0.0.1:911/websocket", "basic");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnOpen += new EventHandler(webSocketClient_OnOpen);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            webSocketClient.Connect();

            if (!m_OpenEvent.WaitOne(1000))
                Assert.Fail("Failed to open session ontime");

            webSocketClient.Send("QUIT");

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        void webSocketClient_OnOpen(object sender, EventArgs e)
        {
            m_OpenEvent.Set();
        }

        void webSocketClient_OnMessage(object sender, MessageEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
        }

        void webSocketClient_OnClose(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }
    }
}
