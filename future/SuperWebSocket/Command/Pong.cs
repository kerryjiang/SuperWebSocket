using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Pong<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketRequestInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Pong.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketRequestInfo requestInfo)
        {
            //Do nothing, last active time has been updated automatically
        }
    }
}
