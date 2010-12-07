using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public abstract class SubCommandBase : ISubCommand
    {
        #region ISubCommand Members

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        public abstract void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo);

        #endregion
    }
}
