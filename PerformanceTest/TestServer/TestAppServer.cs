using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.PerformanceTest.TestServer
{
    public class TestAppServer : WebSocketServer<TestSession>
    {
        public TestAppServer()
            : base(new BasicSubProtocol<TestSession>())
        {

        }
    }
}
