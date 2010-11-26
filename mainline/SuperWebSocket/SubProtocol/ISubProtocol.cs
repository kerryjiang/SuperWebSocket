using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Reflection;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocol
    {
        ISubProtocolCommandParser GetSubProtocolCommandParser();

        Assembly GetSubCommandAssembly { get; }
    }
}
