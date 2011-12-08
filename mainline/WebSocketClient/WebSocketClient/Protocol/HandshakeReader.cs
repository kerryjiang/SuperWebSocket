using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class HandshakeReader : ReaderBase
    {
        static HandshakeReader()
        {
            DefaultHandshakeCommandInfo = new WebSocketCommandInfo();
        }

        public HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        protected static readonly byte[] HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        protected static WebSocketCommandInfo DefaultHandshakeCommandInfo { get; private set; }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            left = 0;

            this.AddArraySegment(readBuffer, offset, length);

            int? result = BufferSegments.SearchMark(HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
                return null;

            string handshake = Encoding.UTF8.GetString(BufferSegments.ToArrayData(0, result.Value));

            ParseHandshake(handshake);

            left = BufferSegments.Count - result.Value - HeaderTerminator.Length;
            BufferSegments.ClearSegements();

            return DefaultHandshakeCommandInfo;
        }

        private void ParseHandshake(string handshake)
        {

        }
    }
}
