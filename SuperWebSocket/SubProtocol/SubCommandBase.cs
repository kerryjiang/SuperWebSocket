using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.SubProtocol
{
    /// <summary>
    /// SubCommand base
    /// </summary>
    public abstract class SubCommandBase : SubCommandBase<WebSocketSession>
    {

    }

    /// <summary>
    /// SubCommand base
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public abstract class SubCommandBase<TWebSocketSession> : ISubCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        #region ISubCommand Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public abstract void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo);

        #endregion
    }
}
