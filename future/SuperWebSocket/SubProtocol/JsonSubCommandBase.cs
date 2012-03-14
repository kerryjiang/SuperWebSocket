using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.SubProtocol
{
    public abstract class JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo> : SubCommandBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private const string m_QueryTemplateA = "{0} {1} {2}";
        private const string m_QueryTemplateB = "{0} {1}";

        private bool m_IsPrimitiveType = false;

        private Type m_CommandInfoType;

        public JsonSubCommandBase()
        {
            m_CommandInfoType = typeof(TJsonCommandInfo);

            if (m_CommandInfoType.IsPrimitive)
                m_IsPrimitiveType = true;
        }

        public override void ExecuteCommand(TWebSocketSession session, StringRequestInfo commandInfo)
        {
            if (string.IsNullOrEmpty(commandInfo.Data))
            {
                ExecuteJsonCommand(session, default(TJsonCommandInfo));
                return;
            }

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

            TJsonCommandInfo jsonCommandInfo;

            if (!m_IsPrimitiveType)
                jsonCommandInfo = JsonConvert.DeserializeObject<TJsonCommandInfo>(data);
            else
                jsonCommandInfo = (TJsonCommandInfo)Convert.ChangeType(data, m_CommandInfoType);

            ExecuteJsonCommand(session, jsonCommandInfo);
        }

        protected abstract void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo);

        protected string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        protected string GetJsonResponse(string token, object content)
        {
            return GetJsonResponse(Name, token, content);
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
    }
}
