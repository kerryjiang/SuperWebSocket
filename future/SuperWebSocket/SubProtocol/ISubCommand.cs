using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        string Name { get; }

        void ExecuteCommand(TWebSocketSession session, StringRequestInfo commandInfo);
    }
}
