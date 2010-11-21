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
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }

        protected override void OnClosed()
        {
            
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
