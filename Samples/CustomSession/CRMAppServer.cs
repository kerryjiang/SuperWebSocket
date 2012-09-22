using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using System.Reflection;

namespace SuperWebSocket.Samples.CustomSession
{
    public class CRMAppServer : WebSocketServer<CRMSession>
    {
        public CRMAppServer()
            : base(new BasicSubProtocol<CRMSession>())
        {

        }
    }
}
