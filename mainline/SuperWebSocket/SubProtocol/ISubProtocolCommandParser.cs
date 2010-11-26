using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocolCommandParser
    {
        SubProtocolCommand ParseSubProtocolCommand(WebSocketCommandInfo commandInfo);
    }
}
