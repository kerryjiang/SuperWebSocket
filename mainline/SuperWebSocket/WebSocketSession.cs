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
    public class WebSocketSession : AppSession<WebSocketSession, WebSocketCommandInfo>, IAsyncRunner
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

        internal bool Handshaked { get; set; }

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
            this.ExecuteAsync(w => SendResponse(message));
        }

        public void SendResponseAsync(string message, params object[] paramValues)
        {
            this.ExecuteAsync(w => SendResponse(message, paramValues));
        }

        protected override SocketContext CreateSocketContext()
        {
            return new WebSocketContext();
        }

        protected override void OnClosed()
        {
            
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
