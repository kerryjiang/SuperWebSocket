using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Protocol
{
    class Rfc6455Processor : DraftHybi10Processor
    {
        private const string m_SecWebSocketVersion = "13";

        protected override string SecWebSocketVersion
        {
            get
            {
                return m_SecWebSocketVersion;
            }
        }
    }
}
