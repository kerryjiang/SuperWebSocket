using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Protocol;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket
{
    public class WebSocketProtocol : SocketProtocolBase, IAsyncProtocol<StringCommandInfo>
    {
        #region IAsyncProtocol Members

        public ICommandAsyncReader<StringCommandInfo> CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion
    }
}
