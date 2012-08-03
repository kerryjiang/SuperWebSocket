using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    public class HandshakeRequest : IWebSocketFragment
    {
        public string Key
        {
            get { return OpCode.HandshakeTag; }
        }
    }
}
