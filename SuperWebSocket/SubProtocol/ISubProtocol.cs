using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    /// <summary>
    /// Basic interface for subprotocol definition
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public interface ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Initializes the sub protocol.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="protocolConfig">The protocol config.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig, ILogger logger);

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the sub command parser.
        /// </summary>
        IRequestInfoParser<SubRequestInfo> SubCommandParser { get; }

        /// <summary>
        /// Tries get the command.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
