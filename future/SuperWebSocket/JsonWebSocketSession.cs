using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SuperWebSocket
{
    public class JsonWebSocketSession : JsonWebSocketSession<JsonWebSocketSession>
    {

    }

    public class JsonWebSocketSession<TWebSocketSession> : WebSocketSession<TWebSocketSession>
        where TWebSocketSession : JsonWebSocketSession<TWebSocketSession>, new()
    {
        private const string m_QueryTemplate = "{0} {1}";

        private string GetJsonResponse(string name, object content)
        {
            return string.Format(m_QueryTemplate, name, JsonConvert.SerializeObject(content));
        }

        public void SendJsonResponse(string name, object content)
        {
            this.SendResponse(GetJsonResponse(name, content));
        }
    }
}
