using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.PerformanceTest.TestServer
{
    public class ECHOY : JsonSubCommand<TestSession, ClientInfo>
    {
        protected override void ExecuteJsonCommand(TestSession session, ClientInfo commandInfo)
        {
            var targetSession = session.AppServer.GetAppSessionByIndentityKey(commandInfo.TargetID);
            targetSession.SendJsonResponse("ECHOY", commandInfo);
        }
    }
}
