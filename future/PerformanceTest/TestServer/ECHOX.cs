using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.PerformanceTest.TestServer
{
    public class ECHOX : JsonSubCommand<TestSession, ClientInfo>
    {
        protected override void ExecuteJsonCommand(TestSession session, ClientInfo commandInfo)
        {
            SendJson(session, commandInfo);
        }
    }
}
