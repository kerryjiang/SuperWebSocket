using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperWebSocket.WebSocketClient;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketClientTest
    {
        [Test]
        public void ConnectionTest()
        {
            WebSocket webSocketClient = new WebSocket("ws://202.173.231.121:2011/iphone/1232434", "json");
            webSocketClient.OnClose += new EventHandler(webSocketClient_OnClose);
            webSocketClient.OnMessage += new EventHandler<SuperWebSocket.WebSocketClient.MessageEventArgs>(webSocketClient_OnMessage);
            Assert.IsTrue(webSocketClient.Connect());
            webSocketClient.Close();
        }

        void webSocketClient_OnOpen(object sender, EventArgs e)
        {

        }

        void webSocketClient_OnMessage(object sender, SuperWebSocket.WebSocketClient.MessageEventArgs e)
        {

        }

        void webSocketClient_OnClose(object sender, EventArgs e)
        {

        }
    }
}
