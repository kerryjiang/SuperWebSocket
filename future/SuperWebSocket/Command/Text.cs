using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Text<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketRequestInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Text.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketRequestInfo requestInfo)
        {
            session.AppServer.OnNewMessageReceived(session, requestInfo.Text);
        }
    }
}
