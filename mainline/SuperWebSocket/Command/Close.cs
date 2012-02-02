using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Close<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Close.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            //the close handshake started from server side, now received a handshake response
            if (session.InClosing)
            {
                //Close the underlying socket directly
                session.Close(CloseReason.ClientClosing);
                return;
            }

            int closeStatusCode = commandInfo.CloseStatusCode;

            if (closeStatusCode <= 0)
                closeStatusCode = session.ProtocolProcessor.CloseStatusClode.NoStatusCode;

            //Send handshake response
            session.CloseWithHandshake(closeStatusCode, commandInfo.Text);
        }
    }
}
