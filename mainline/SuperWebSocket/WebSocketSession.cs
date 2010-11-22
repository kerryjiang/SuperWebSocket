using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket
{
    public class WebSocketSession : AppSession<WebSocketSession>
    {
        public WebSocketSession()
            : base(false)
        {

        }

        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }

        public new WebSocketContext Context
        {
            get { return (WebSocketContext)base.Context; }
        }

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
