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
    public class ADDX : JsonSubCommand<AddParameter>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, AddParameter commandInfo)
        {
            Async.Run((o) => Calculate(o), new
                {
                    Session = session,
                    Parameter = commandInfo,
                    Token = session.CurrentToken
                });
        }

        private void Calculate(dynamic state)
        {
            var session = state.Session as WebSocketSession;
            var parameter = state.Parameter as AddParameter;

            var result = new AddResult { Result = parameter.A + parameter.B };

            Thread.Sleep(5000);

            this.SendJsonResponseWithToken(session, state.Token, result);
        }
    }
}
