using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
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

        public override WebSocketCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            var webSocketSession = session as IWebSocketSession;

            int total = BufferSegments.Count + length;

            if (total == 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.CloneRange(offset, length));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                left = 0;
                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else if (total > 8)
            {
                List<byte> key = new List<byte>();
                key.AddRange(BufferSegments.ToArrayData());
                key.AddRange(readBuffer.CloneRange(offset, 8 - BufferSegments.Count));
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key.ToArray();
                BufferSegments.ClearSegements();
                left = total - 8;
                Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession);
                return HandshakeCommandInfo;
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                left = 0;
                NextCommandReader = this;
                return null;
            }
        }
    }
}
