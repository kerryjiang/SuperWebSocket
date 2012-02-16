using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using Newtonsoft.Json;
using SuperSocket.SocketBase.Protocol;

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
        private string m_Name;

        private const string m_QueryTemplateA = "{0} {1} {2}";
        private const string m_QueryTemplateB = "{0} {1}";

        public JsonSubCommand()
        {
            m_Name = Name;
        }

        public override void ExecuteCommand(TWebSocketSession session, StringRequestInfo commandInfo)
        {
            if (string.IsNullOrEmpty(commandInfo.Data))
            {
                ExecuteJsonCommand(session, string.Empty, null);
                return;
            }

            var token = string.Empty;
            var data = commandInfo.Data;

            if (data[0] != '{')
            {
                int pos = data.IndexOf(' ');

                token = data.Substring(0, pos);
                data = data.Substring(pos + 1);
            }

            var jsonCommandInfo = JsonConvert.DeserializeObject<TJsonCommandInfo>(data);
            ExecuteJsonCommand(session, token, jsonCommandInfo);
        }

        protected abstract void ExecuteJsonCommand(TWebSocketSession session, string token, TJsonCommandInfo commandInfo);

        protected string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        private void SendJsonResponse(TWebSocketSession session, object content)
        {
            SendJsonResponse(session, string.Empty, content);
        }

        protected string GetJsonResponse(string token, object content)
        {
            return GetJsonResponse(m_Name, token, content);
        }

        protected string GetJsonResponse(string name, string token, object content)
        {
            if (string.IsNullOrEmpty(token))
                return string.Format(m_QueryTemplateB, name, SerializeObject(content));
            else
                return string.Format(m_QueryTemplateA, name, token, SerializeObject(content));
        }

        protected void SendJsonResponse(TWebSocketSession session, string token, object content)
        {
            session.SendResponse(GetJsonResponse(token, content));
        }
    }
}
