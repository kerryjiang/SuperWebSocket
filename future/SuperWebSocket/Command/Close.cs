using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public class Close<TWebSocketSession> : CommandBase<TWebSocketSession, WebSocketRequestInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public override string Name
        {
            get
            {
                return OpCode.Close.ToString();
            }
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketRequestInfo requestInfo)
        {
            //the close handshake started from server side, now received a handshake response
            if (session.InClosing)
            {
                //Close the underlying socket directly
                session.Close(CloseReason.ClientClosing);
                return;
            }

            int closeStatusCode = requestInfo.CloseStatusCode;

            if (closeStatusCode <= 0)
                closeStatusCode = session.ProtocolProcessor.CloseStatusClode.NoStatusCode;

            //Send handshake response
            session.SendCloseHandshakeResponse(closeStatusCode);
            //After both sending and receiving a Close message, the server MUST close the underlying TCP connection immediately
            session.Close(CloseReason.ClientClosing);
        }
    }
}
