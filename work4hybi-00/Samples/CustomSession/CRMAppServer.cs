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
        //Because the sample process requests by sub protocol, so passing sub protocol instance to parent class in the line below is required
        public CRMAppServer()
            : base(new BasicSubProtocol<CRMSession>(new List<Assembly>{ typeof(CRMAppServer).Assembly }))            
        {

        }
    }
}
