using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Binary<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.BinaryTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment commandInfo)
        {
            var frame = commandInfo as WebSocketDataFrame;

            if (!CheckFrame(frame))
            {
                session.Close();
                return;
            }

            if (frame.FIN)
            {
                if (session.Frames.Count > 0)
                {
                    session.Close();
                    return;
                }

                var data = GetWebSocketData(frame);
                session.AppServer.OnNewDataReceived(session, data);
            }
            else
            {
                session.Frames.Add(frame);
            }
        }
    }
}
