using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public abstract class SubCommandBase : SubCommandBase<WebSocketSession>
    {

    }

    public abstract class SubCommandBase<TWebSocketSession> : ISubCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        #region ISubCommand Members

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        public abstract void ExecuteCommand(TWebSocketSession session, StringCommandInfo commandInfo);

        #endregion
    }
}
