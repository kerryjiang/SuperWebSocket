using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.CustomSession.Command
{
    /// <summary>
    /// Broadcast messages to sessions below to specific company
    /// </summary>
    public class MSG : SubCommandBase<CRMSession>
    {
        public override void ExecuteCommand(CRMSession session, SubRequestInfo requestInfo)
        {
            var message = session.Name + ": " + requestInfo.Body;

            foreach (var s in session.AppServer.GetAllSessions())
            {
                s.Send(message);
            }
        }
    }
}
