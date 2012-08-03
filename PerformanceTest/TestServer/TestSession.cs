using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.PerformanceTest.TestServer
{
    public class TestSession : JsonWebSocketSession<TestSession>
    {
        protected override void OnHandShaked()
        {
            base.OnHandShaked();

            if (AppServer.SessionCount <= 1)
                return;

            foreach (var x in AppServer.GetSessions(s => !s.SessionID.Equals(this.SessionID)))
            {
                x.SendJsonResponse("NEW", this.SessionID);
            }
        }
    }
}
