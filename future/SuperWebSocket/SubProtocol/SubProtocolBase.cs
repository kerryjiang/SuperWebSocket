using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
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

        public string Name { get; private set; }

        public abstract bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig, ILog logger);

        public IRequestInfoParser<StringRequestInfo> SubCommandParser { get; protected set; }

        public abstract bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
