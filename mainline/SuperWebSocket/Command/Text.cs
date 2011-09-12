using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Command
{
    public class Text : CommandBase<WebSocketSession, WebSocketCommandInfo>
    {
        public override string Name
        {
            get
            {
                return "1";
            }
        }

        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            session.AppServer.OnNewMessageReceived(session, commandInfo.Text);
        }
    }
}
