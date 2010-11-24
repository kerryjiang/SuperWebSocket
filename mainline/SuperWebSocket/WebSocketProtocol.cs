using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Protocol;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket
{
    public class WebSocketProtocol : SocketProtocolBase, IAsyncProtocol<WebSocketCommandInfo>
    {
        #region IAsyncProtocol Members

        public ICommandAsyncReader<WebSocketCommandInfo> CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion
    }
}
