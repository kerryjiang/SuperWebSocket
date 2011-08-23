using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocolCommandParser
    {
        StringCommandInfo ParseSubCommand(WebSocketCommandInfo commandInfo);
    }
}
