using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Reflection;
using SuperSocket.SocketBase.Config;

namespace SuperWebSocket.SubProtocol
{
    public interface ISubProtocol
    {
        bool Initialize(IServerConfig config);

        ISubProtocolCommandParser SubCommandParser { get; }

        IEnumerable<ISubCommand> GetSubCommands();
    }
}
