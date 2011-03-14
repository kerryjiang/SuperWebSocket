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

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketClientTest
    {
        protected WebSocketServer m_WebSocketServer;
        private AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private AutoResetEvent m_OpneCloseEvent = new AutoResetEvent(false);
        private string m_CurrentMessage = string.Empty;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_WebSocketServer = new WebSocketServer(new BasicSubProtocol(this.GetType().Assembly));
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
            webSocketClient.OnOpen +=new EventHandler(webSocketClient_OnOpen);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            Assert.IsTrue(webSocketClient.Connect());
            if(!m_OpneCloseEvent.WaitOne(1000))
                Assert.Fail("Failed to open session ontime");
            webSocketClient.Close();
        }

        [Test]
        public void SendMessageTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://127.0.0.1:911/websocket", "basic");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            Assert.IsTrue(webSocketClient.Connect());

            for (int i = 0; i < 10; i++)
            {
                string message = Guid.NewGuid().ToString();
                webSocketClient.Send("ECHO " + message);

                if (!m_MessageReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, m_CurrentMessage);
            }

            webSocketClient.Close();
        }


        [Test]
        public void CloseWebSocketTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://127.0.0.1:911/websocket", "basic");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_OnMessage);
            Assert.IsTrue(webSocketClient.Connect());

            webSocketClient.Send("QUIT");

            if (!m_OpneCloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        void webSocketClient_OnOpen(object sender, EventArgs e)
        {
            m_OpneCloseEvent.Set();
        }

        void webSocketClient_OnMessage(object sender, MessageEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
        }

        void webSocketClient_OnClose(object sender, EventArgs e)
        {
            m_OpneCloseEvent.Set();
        }
    }
}
