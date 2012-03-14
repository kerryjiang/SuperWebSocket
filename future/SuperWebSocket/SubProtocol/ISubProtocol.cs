using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket.Config;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        bool Initialize(IServerConfig config, SubProtocolConfig protocolConfig, ILog logger);

        string Name { get; }

        IRequestInfoParser<StringRequestInfo> SubCommandParser { get; }

        bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
