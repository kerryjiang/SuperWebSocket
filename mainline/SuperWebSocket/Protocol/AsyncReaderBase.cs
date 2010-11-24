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
    public abstract class AsyncReaderBase : ICommandAsyncReader<WebSocketCommandInfo>
    {
        protected ArraySegmentList<byte> Segments { get; set; }

        #region ICommandAsyncReader Members

        public abstract WebSocketCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return Segments;
        }

        public ICommandAsyncReader<WebSocketCommandInfo> NextCommandReader { get; protected set; }

        #endregion

        protected WebSocketCommandInfo CreateHeadCommandInfo()
        {
            return new WebSocketCommandInfo(WebSocketConstant.CommandHead, string.Empty);
        }
    }
}
