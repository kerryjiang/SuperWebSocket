using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.PerformanceTest.TestServer
{
    public class ECHO : JsonSubCommand<TestSession, string>
    {
        protected override void ExecuteJsonCommand(TestSession session, string commandInfo)
        {
            SendJsonResponse(session, commandInfo);
        }
    }
}
