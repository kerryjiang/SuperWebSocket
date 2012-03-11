using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Text<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.TextTag;
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
        {
            var frame = requestInfo as WebSocketDataFrame;

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

                var text = GetWebSocketText(frame);
                session.AppServer.OnNewMessageReceived(session, text);
            }
            else
            {
                session.Frames.Add(frame);
            }
        }
    }
}
