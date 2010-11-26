using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;

namespace SuperWebSocket.Protocol
{
    public class SecKey3AsyncReader : AsyncReaderBase
    {
        public SecKey3AsyncReader(AsyncReaderBase prevReader)
        {
            Segments = prevReader.GetLeftBuffer();
        }

        public override WebSocketCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            int total = Segments.Count + length;

            var socketContext = context as WebSocketContext;

            if (total == 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(Segments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(length));
                socketContext.SecWebSocketKey3 = key.ToArray();
                Segments.ClearSegements();
                NextCommandReader = new DataAsyncReader(this);
                return CreateHeadCommandInfo();
            }
            else if (total > 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(Segments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(8 - Segments.Count));
                socketContext.SecWebSocketKey3 = key.ToArray();
                Segments.ClearSegements();
                AddArraySegment(readBuffer, offset + 8 - Segments.Count, total - 8, isReusableBuffer);
                NextCommandReader = new DataAsyncReader(this);
                return CreateHeadCommandInfo();
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                NextCommandReader = new SecKey3AsyncReader(this);
                return null;
            }
        }
    }
}
