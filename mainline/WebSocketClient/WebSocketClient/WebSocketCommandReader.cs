using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient
{
    class WebSocketCommandReader : IClientCommandReader<WebSocketCommandInfo>
    {
        public WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            throw new NotImplementedException();
        }

        public IClientCommandReader<WebSocketCommandInfo> NextCommandReader { get; protected set; }
    }
}
