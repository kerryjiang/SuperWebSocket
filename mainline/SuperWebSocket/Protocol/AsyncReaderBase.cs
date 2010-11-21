using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Protocol
{
    public abstract class AsyncReaderBase : ICommandAsyncReader<StringCommandInfo>
    {
        protected ArraySegmentList<byte> Segments { get; set; }

        #region ICommandAsyncReader Members

        public abstract StringCommandInfo FindCommand(byte[] readBuffer, int offset, int length);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return Segments;
        }

        public ICommandAsyncReader<StringCommandInfo> NextCommandReader { get; protected set; }

        #endregion
    }
}
