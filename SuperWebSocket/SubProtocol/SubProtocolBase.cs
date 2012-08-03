using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    public abstract class SubProtocolBase<TWebSocketSession> : ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private SubProtocolBase()
        {

        }

        public SubProtocolBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes the sub protocol.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="protocolConfig">The protocol config.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public abstract bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig, ILogger logger);


        /// <summary>
        /// Gets or sets the sub command parser.
        /// </summary>
        /// <value>
        /// The sub command parser.
        /// </value>
        public IRequestInfoParser<SubRequestInfo> SubCommandParser { get; protected set; }

        /// <summary>
        /// Tries get the command.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public abstract bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
