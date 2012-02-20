using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Samples.CustomSession.JsonObject;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.CustomSession.Command
{
    /// <summary>
    /// Broadcast messages to sessions below to specific company
    /// </summary>
    public class BRCT : JsonSubCommand<CRMSession, BroadcastMessage>
    {
        protected override void ExecuteJsonCommand(CRMSession session, BroadcastMessage commandInfo)
        {
            if (commandInfo.CompanyId <= 0)
                return;

            string message = GetJsonResponse(session, "MSG", commandInfo);

            foreach(var s in session.AppServer.GetSessions(s => s.CompanyId == commandInfo.CompanyId))
            {
                s.SendResponseAsync(message);
            }
        }
    }
}
