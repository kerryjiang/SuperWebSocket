using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Protocol
{
    public class PlainFragment : IWebSocketFragment
    {
        public PlainFragment(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public string Key
        {
            get { return OpCode.PlainTag; }
        }
    }
}
