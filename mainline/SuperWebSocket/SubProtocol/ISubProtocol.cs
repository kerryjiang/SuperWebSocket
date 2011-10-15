using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Reflection;
using SuperSocket.SocketBase.Config;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig);

        string Name { get; }

        ICommandParser SubCommandParser { get; }

        bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
