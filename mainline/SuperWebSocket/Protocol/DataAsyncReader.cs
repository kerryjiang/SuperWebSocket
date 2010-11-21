using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Protocol
{
    public class DataAsyncReader : AsyncReaderBase
    {
        public DataAsyncReader(HeaderAsyncReader headerReader)
        {
            Segments = headerReader.GetLeftBuffer();
        }

        #region ICommandAsyncReader Members

        public override StringCommandInfo FindCommand(byte[] readBuffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
