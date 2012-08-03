using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Plain<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.PlainTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment commandInfo)
        {
            var plainFragment = commandInfo as PlainFragment;

            session.AppServer.OnNewMessageReceived(session, plainFragment.Message);
        }
    }
}
