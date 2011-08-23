using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using Newtonsoft.Json;

namespace SuperWebSocket.SubProtocol
{
    public abstract class JsonSubCommand<TJsonCommandInfo> : JsonSubCommand<WebSocketSession, TJsonCommandInfo>
        where TJsonCommandInfo : class, new()
    {

    }

    public abstract class JsonSubCommand<TWebSocketSession, TJsonCommandInfo> : SubCommandBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
        where TJsonCommandInfo : class, new()
    {
        public override void ExecuteCommand(TWebSocketSession session, StringCommandInfo commandInfo)
        {
            var jsonCommandInfo = JsonConvert.DeserializeObject<TJsonCommandInfo>(commandInfo.Data);
            ExecuteJsonCommand(session, jsonCommandInfo);
        }

        protected abstract void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo);

        protected string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
