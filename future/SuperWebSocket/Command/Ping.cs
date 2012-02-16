using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Ping<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketRequestInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Ping.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketRequestInfo requestInfo)
        {
            session.ProtocolProcessor.SendPong(session, requestInfo.Text);
        }
    }
}
