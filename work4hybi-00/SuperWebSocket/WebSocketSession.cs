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
    public interface IWebSocketSession : IAppSession
    {
        string Method { get; set; }
        string Host { get; }
        string Path { get; set; }
        string HttpVersion { get; set; }
        string SecWebSocketVersion { get; }
        string Origin { get; }
        string UriScheme { get; }
        void SendRawResponse(string message);
        void SendResponse(string message);
        void SendResponse(byte[] data);
        IWebSocketServer AppServer { get; }
    }

    public class WebSocketSession : WebSocketSession<WebSocketSession>
    {
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }
    }

    public class WebSocketSession<TWebSocketSession> : AppSession<TWebSocketSession, WebSocketCommandInfo>, IWebSocketSession
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string HttpVersion { get; set; }
        public string Host { get { return this.Items.GetValue<string>(WebSocketConstant.Host, string.Empty); } }
        public string Origin { get { return this.Items.GetValue<string>(WebSocketConstant.Origin, string.Empty); } }
        public string Upgrade { get { return this.Items.GetValue<string>(WebSocketConstant.Upgrade, string.Empty); } }
        public string Connection { get { return this.Items.GetValue<string>(WebSocketConstant.Connection, string.Empty); } }
        public string SecWebSocketVersion { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketVersion, string.Empty); } }

        public new WebSocketServer<TWebSocketSession> AppServer
        {
            get { return (WebSocketServer<TWebSocketSession>)base.AppServer; }
        }

        IWebSocketServer IWebSocketSession.AppServer
        {
            get { return (IWebSocketServer)base.AppServer; }
        }

        public string UriScheme
        {
            get { return AppServer.UriScheme; }
        }

        internal void SendRawResponse(string message)
        {
            base.SendResponse(message);
        }

        void IWebSocketSession.SendRawResponse(string message)
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
                {
                    SetCookie();
                    OnHandShaked();
                }
            }
        }

        private void SetCookie()
        {
            string cookieValue = this.Items.GetValue<string>(WebSocketConstant.Cookie, string.Empty);

            var cookies = new StringDictionary();

            if (!string.IsNullOrEmpty(cookieValue))
            {
                string[] pairs = cookieValue.Split(';');

                int pos;
                string key, value;

                foreach (var p in pairs)
                {
                    pos = p.IndexOf('=');
                    if (pos > 0)
                    {
                        key = p.Substring(0, pos).Trim();
                        pos += 1;
                        if (pos < p.Length)
                            value = p.Substring(pos).Trim();
                        else
                            value = string.Empty;

                        cookies[key] = Uri.UnescapeDataString(value);
                    }
                }
            }

            this.Cookies = cookies;
        }

        protected virtual void OnHandShaked()
        {

        }

        public StringDictionary Cookies { get; private set; }

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
