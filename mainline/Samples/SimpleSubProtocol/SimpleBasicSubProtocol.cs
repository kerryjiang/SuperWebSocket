using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.SimpleSubProtocol
{
    public class SimpleBasicSubProtocol : BasicSubProtocol
    {
        public SimpleBasicSubProtocol()
            : base(typeof(SimpleBasicSubProtocol).Assembly)
        {

        }
    }
}
