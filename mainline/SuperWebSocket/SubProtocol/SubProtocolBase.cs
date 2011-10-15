using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

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

        public abstract bool Initialize(SuperSocket.SocketBase.Config.IServerConfig config, Config.SubProtocolConfig protocolConfig);

        public ICommandParser SubCommandParser { get; protected set; }

        public abstract bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
