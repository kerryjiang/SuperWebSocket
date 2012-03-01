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
                ExecuteJsonCommand(session, null);
                return;
            }

            var token = string.Empty;
            var data = commandInfo.Data;

            if (data[0] != '{')
            {
                int pos = data.IndexOf(' ');

                session.CurrentToken = data.Substring(0, pos);
                data = data.Substring(pos + 1);
            }
            else
            {
                session.CurrentToken = string.Empty;
            }

            var jsonCommandInfo = JsonConvert.DeserializeObject<TJsonCommandInfo>(data);
            ExecuteJsonCommand(session, jsonCommandInfo);
        }

        protected abstract void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo);

        protected string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        protected string GetJsonResponse(TWebSocketSession session, object content)
        {
            return GetJsonResponse(session, m_Name, content);
        }

        protected string GetJsonResponse(TWebSocketSession session, string name, object content)
        {
            return GetJsonResponse(name, session.CurrentToken, content);
        }

        protected string GetJsonResponse(string token, object content)
        {
            return GetJsonResponse(m_Name, token, content);
        }

        protected string GetJsonResponse(string name, string token, object content)
        {
            string strOutput;

            //Needn't serialize primitive type object
            if (content.GetType().IsPrimitive)
                strOutput = content.ToString();
            else
                strOutput = SerializeObject(content);

            if (string.IsNullOrEmpty(token))
                return string.Format(m_QueryTemplateB, name, strOutput);
            else
                return string.Format(m_QueryTemplateA, name, token, strOutput);
        }

        protected void SendJsonResponse(TWebSocketSession session, object content)
        {
            session.SendResponse(GetJsonResponse(session, content));
        }

        protected void SendJsonResponse(TWebSocketSession session, string name, object content)
        {
            session.SendResponse(GetJsonResponse(session, name, content));
        }

        protected void SendJsonResponseWithToken(TWebSocketSession session, string name, string token, object content)
        {
            session.SendResponse(GetJsonResponse(name, token, content));
        }

        protected void SendJsonResponseWithToken(TWebSocketSession session, string token, object content)
        {
            session.SendResponse(GetJsonResponse(m_Name, token, content));
        }
    }
}
