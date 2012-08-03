using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperWebSocket.Protocol
{
    public interface IProtocolProcessor
    {
        bool CanSendBinaryData { get; }

        ICloseStatusCode CloseStatusClode { get; }

        IProtocolProcessor NextProcessor { get; set; }

        bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<IWebSocketFragment> dataFrameReader);

        void SendMessage(IWebSocketSession session, string message);

        void SendData(IWebSocketSession session, byte[] data, int offset, int length);

        void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason);

        void SendPong(IWebSocketSession session, byte[] pong);

        void SendPing(IWebSocketSession session, byte[] ping);

        int Version { get; }

        bool IsValidCloseCode(int code);
    }
}
