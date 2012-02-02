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

            if (total == SecKey3Len)
            {
                byte[] key = new byte[SecKey3Len];
                BufferSegments.CopyTo(key);
                Array.Copy(readBuffer, offset, key, BufferSegments.Count, length);
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key;
                BufferSegments.Clear();
                left = 0;
                if(Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession))
                    return HandshakeCommandInfo;
            }
            else if (total > SecKey3Len)
            {
                byte[] key = new byte[8];
                BufferSegments.CopyTo(key);
                Array.Copy(readBuffer, offset, key, BufferSegments.Count, SecKey3Len - BufferSegments.Count);
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key;
                BufferSegments.Clear();
                left = total - SecKey3Len;
                if(Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession))
                    return HandshakeCommandInfo;
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                left = 0;
                NextCommandReader = this;
                return null;
            }

            return null;
        }
    }
}
