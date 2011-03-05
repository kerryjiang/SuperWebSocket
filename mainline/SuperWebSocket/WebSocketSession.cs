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

    }

    public class WebSocketSession<TWebSocketSession> : AppSession<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : IAppSession<TWebSocketSession, WebSocketCommandInfo>, new()
    {
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }

        public new WebSocketContext Context
        {
            get { return (WebSocketContext)base.Context; }
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
            SocketSession.SendResponse(Context, new byte[] { WebSocketConstant.StartByte });
            base.SendResponse(message);
            SocketSession.SendResponse(Context, new byte[] { WebSocketConstant.EndByte });
        }

        public override void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(Context, new byte[] { WebSocketConstant.StartByte });
            base.SendResponse(message, paramValues);
            SocketSession.SendResponse(Context, new byte[] { WebSocketConstant.EndByte });
        }

        public void SendResponseAsync(string message)
        {
            Async.Run(() => SendResponse(message));
        }

        public void SendResponseAsync(string message, params object[] paramValues)
        {
            Async.Run(() => SendResponse(message, paramValues));
        }

        protected override SocketContext CreateSocketContext()
        {
            return new WebSocketContext();
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
