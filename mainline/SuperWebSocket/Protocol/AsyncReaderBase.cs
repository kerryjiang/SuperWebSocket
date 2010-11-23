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
    public abstract class AsyncReaderBase : ICommandAsyncReader<StringCommandInfo>
    {
        protected ArraySegmentList<byte> Segments { get; set; }

        #region ICommandAsyncReader Members

        public abstract StringCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return Segments;
        }

        public ICommandAsyncReader<StringCommandInfo> NextCommandReader { get; protected set; }

        #endregion

        protected StringCommandInfo CreateHeadCommandInfo()
        {
            return new StringCommandInfo(WebSocketConstant.CommandHead, string.Empty, new string[] { });
        }
    }
}
