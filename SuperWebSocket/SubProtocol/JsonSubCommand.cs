using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using Newtonsoft.Json;

namespace SuperWebSocket.SubProtocol
{
    public abstract class JsonSubCommand<TJsonCommandInfo> : JsonSubCommand<WebSocketSession, TJsonCommandInfo>
    {

    }

    public abstract class JsonSubCommand<TWebSocketSession, TJsonCommandInfo> : JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        
        protected string GetJsonResponse(TWebSocketSession session, object content)
        {
            return GetJsonResponse(session, Name, content);
        }

        protected string GetJsonResponse(TWebSocketSession session, string name, object content)
        {
            return GetJsonResponse(name, session.CurrentToken, content);
        }

        protected void SendJsonResponse(TWebSocketSession session, object content)
        {
            session.SendResponse(GetJsonResponse(session, content));
        }

        protected void SendJsonResponse(TWebSocketSession session, string name, object content)
        {
            session.SendResponse(GetJsonResponse(session, name, content));
        }
    }
}
