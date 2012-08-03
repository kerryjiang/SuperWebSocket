using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperWebSocket.Samples.JsonCommandAssembly.JsonObject;
using SuperWebSocket.SubProtocol;

namespace SuperWebSocket.Samples.JsonCommandAssembly.Command
{
    public class ADDX : AsyncJsonSubCommand<AddParameter>
    {
        protected override void ExecuteAsyncJsonCommand(WebSocketSession session, string token, AddParameter commandInfo)
        {
            var result = new AddResult { Result = commandInfo.A + commandInfo.B };

            Thread.Sleep(5000);

            this.SendJsonMessage(session, token, result);
        }
    }
}
