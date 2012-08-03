using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Ping<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.PingTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment commandInfo)
        {
            var frame = commandInfo as WebSocketDataFrame;

            if (!CheckControlFrame(frame))
            {
                session.Close();
                return;
            }

            var data = GetWebSocketData(frame);

            session.ProtocolProcessor.SendPong(session, data);
        }
    }
}
