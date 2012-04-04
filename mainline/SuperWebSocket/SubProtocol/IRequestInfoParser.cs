using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public interface IRequestInfoParser<TRequestInfo>
        where TRequestInfo : ICommandInfo
    {
        TRequestInfo ParseRequestInfo(string source);
    }
}
