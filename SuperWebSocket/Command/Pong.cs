using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Pong<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.PongTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment commandInfo)
        {
            //Do nothing, last active time has been updated automatically

            var frame = commandInfo as WebSocketDataFrame;

            if (!CheckControlFrame(frame))
            {
                session.Close();
                return;
            }
        }
    }
}
