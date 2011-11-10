using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Reader
{
    class HandshakeReader : ReaderBase
    {
        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            throw new NotImplementedException();
        }
    }
}
