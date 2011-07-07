using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Protocol
{
    public class WebSocketSecKey3Reader : WebSocketReaderBase
    {
        public WebSocketSecKey3Reader(WebSocketReaderBase prevReader)
            : base(prevReader)
        {
            
        }

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            var webSocketSession = session as IWebSocketSession;

            int total = BufferSegments.Count + length;

            if (total == 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(length));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                NextCommandReader = new WebSocketDataReader(this);
                return CreateHeadCommandInfo();
            }
            else if (total > 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.Skip(offset).Take(8 - BufferSegments.Count));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                AddArraySegment(readBuffer, offset + 8 - BufferSegments.Count, total - 8, isReusableBuffer);
                NextCommandReader = new WebSocketDataReader(this);
                return CreateHeadCommandInfo();
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                NextCommandReader = this;
                return null;
            }
        }
    }
}
