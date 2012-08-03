using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using WebSocket4Net;

namespace SuperWebSocketTest
{
    public abstract class WebSocketTestBase
    {
        protected WebSocketServer WebSocketServer { get; set; }

        protected AutoResetEvent MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent DataReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent CloseEvent = new AutoResetEvent(false);
        protected string CurrentMessage { get; private set; }
        protected byte[] CurrentData { get; private set; }

        public WebSocketTestBase()
        {

        }

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            WebSocketServer = new WebSocketServer();
            WebSocketServer.NewDataReceived += new SessionEventHandler<WebSocketSession, byte[]>(WebSocketServer_NewDataReceived);
            WebSocketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(WebSocketServer_NewMessageReceived);
            WebSocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 2012,
                Ip = "Any",
                MaxConnectionNumber = 100,
                MaxCommandLength = 100000,
                Mode = SocketMode.Async,
                Name = "SuperWebSocket Server"
            }, SocketServerFactory.Instance);
        }

        protected void WebSocketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            session.SendResponse(e);
        }

        protected void WebSocketServer_NewDataReceived(WebSocketSession session, byte[] e)
        {
            session.SendResponse(e);
        }

        [SetUp]
        public void StartServer()
        {
            WebSocketServer.Start();
        }

        [TearDown]
        public void StopServer()
        {
            WebSocketServer.Stop();
        }

        protected WebSocket CreateClient()
        {
            return CreateClient(WebSocketVersion.Rfc6455, true);
        }

        protected WebSocket CreateClient(WebSocketVersion version)
        {
            return CreateClient(version, true);
        }

        protected WebSocket CreateClient(WebSocketVersion version, bool autoConnect)
        {
            var webSocketClient = new WebSocket(string.Format("ws://127.0.0.1:{0}/websocket", WebSocketServer.Config.Port), "basic", version);
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.DataReceived += new EventHandler<DataReceivedEventArgs>(webSocketClient_DataReceived);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);

            if (autoConnect)
            {
                webSocketClient.Open();

                if (!OpenedEvent.WaitOne(1000))
                    Assert.Fail("Failed to open");
            }
            
            return webSocketClient;
        }

        void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            CurrentMessage = e.Message;
            MessageReceiveEvent.Set();
        }

        void webSocketClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            CurrentData = e.Data;
            DataReceiveEvent.Set();
        }

        void webSocketClient_Closed(object sender, EventArgs e)
        {
            CloseEvent.Set();
        }

        void webSocketClient_Opened(object sender, EventArgs e)
        {
            OpenedEvent.Set();
        }
    }
}
