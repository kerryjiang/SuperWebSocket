using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;

namespace SuperWebSocket
{
    public class WebSocketSession : WebSocketSession<WebSocketSession>
    {
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }
    }

    public class WebSocketSession<TWebSocketSession> : AppSession<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string HttpVersion { get; set; }
        public string Host { get { return this.Items.GetValue<string>(WebSocketConstant.Host, string.Empty); } }
        public string Origin { get { return this.Items.GetValue<string>(WebSocketConstant.Origin, string.Empty); } }
        public string Upgrade { get { return this.Items.GetValue<string>(WebSocketConstant.Upgrade, string.Empty); } }
        public string Connection { get { return this.Items.GetValue<string>(WebSocketConstant.Connection, string.Empty); } }
        public string SecWebSocketKey1 { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey1, string.Empty); } }
        public string SecWebSocketKey2 { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey2, string.Empty); } }
        public string SecWebSocketVersion { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketVersion, string.Empty); } }
        public byte[] SecWebSocketKey3 { get; set; }

        protected override void OnInit()
        {
            this.Charset = Encoding.UTF8;
            base.OnInit();
        }

        public new WebSocketServer<TWebSocketSession> AppServer
        {
            get { return (WebSocketServer<TWebSocketSession>)base.AppServer; }
        }

        internal void SendRawResponse(string message)
        {
            base.SendResponse(message);
        }


        private bool m_Handshaked = false;

        internal bool Handshaked
        {
            get { return m_Handshaked; }
            set
            {
                m_Handshaked = value;
                if (m_Handshaked)
                    OnHandShaked();
            }
        }

        protected virtual void OnHandShaked()
        {

        }

        public StringDictionary Cookies { get; internal set; }

        public override void SendResponse(string message)
        {
            SocketSession.SendResponse(new byte[] { WebSocketConstant.StartByte });
            base.SendResponse(message);
            SocketSession.SendResponse(new byte[] { WebSocketConstant.EndByte });
        }

        public override void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(new byte[] { WebSocketConstant.StartByte });
            base.SendResponse(message, paramValues);
            SocketSession.SendResponse(new byte[] { WebSocketConstant.EndByte });
        }

        public void SendResponseAsync(string message)
        {
            Async.Run(() => SendResponse(message));
        }

        public void SendResponseAsync(string message, params object[] paramValues)
        {
            Async.Run(() => SendResponse(message, paramValues));
        }
    }
}
