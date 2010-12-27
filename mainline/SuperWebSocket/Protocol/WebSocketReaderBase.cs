using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    public abstract class WebSocketReaderBase : CommandReaderBase<WebSocketCommandInfo>
    {
        public WebSocketReaderBase(IAppServer appServer)
            : base(appServer)
        {

        }

        public WebSocketReaderBase(ICommandReader<WebSocketCommandInfo> previousCommandReader)
            : base(previousCommandReader)
        {

        }

        protected WebSocketCommandInfo CreateHeadCommandInfo()
        {
            return new WebSocketCommandInfo(string.Empty);
        }

        protected void AddArraySegment(byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            if (isReusableBuffer)
                BufferSegments.AddSegment(new ArraySegment<byte>(buffer.Skip(offset).Take(length).ToArray()));
            else
                BufferSegments.AddSegment(new ArraySegment<byte>(buffer, offset, length));
        }
    }
}
