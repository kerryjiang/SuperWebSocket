using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Reflection;
using SuperSocket.SocketBase.Config;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        bool Initialize(IServerConfig config);

        string Name { get; }

        ISubProtocolCommandParser SubCommandParser { get; }

        bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
