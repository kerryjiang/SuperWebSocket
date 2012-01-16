using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Binary<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Binary.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            session.AppServer.OnNewDataReceived(session, commandInfo.Data);
        }
    }
}
