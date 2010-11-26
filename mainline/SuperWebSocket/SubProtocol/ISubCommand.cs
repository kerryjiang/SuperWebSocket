using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubCommand
    {
        string Name { get; }
        void ExecuteCommand(WebSocketSession session, SubProtocolCommand commandInfo);
    }
}
