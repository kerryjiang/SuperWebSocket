using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class HandshakeReader : ReaderBase
    {
        public HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            left = 0;

            this.AddArraySegment(readBuffer, offset, length);

            int? result = BufferSegments.SearchMark(m_HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
                return null;

            string handshake = Encoding.UTF8.GetString(BufferSegments.ToArrayData(0, result.Value));

            ParseHandshake(handshake);

            left = BufferSegments.Count - result.Value;
            BufferSegments.ClearSegements();

            return new WebSocketCommandInfo();
        }

        private void ParseHandshake(string handshake)
        {

        }
    }
}
