using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.SubProtocol
{
    /// <summary>
    /// Json SubCommand base
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo> : SubCommandBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private const string m_QueryTemplateA = "{0}-{1} {2}";
        private const string m_QueryTemplateB = "{0} {1}";

        private bool m_IsPrimitiveType = false;

        private Type m_CommandInfoType;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSubCommandBase&lt;TWebSocketSession, TJsonCommandInfo&gt;"/> class.
        /// </summary>
        public JsonSubCommandBase()
        {
            m_CommandInfoType = typeof(TJsonCommandInfo);

            if (m_CommandInfoType.IsPrimitive)
                m_IsPrimitiveType = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public override void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo)
        {
            if (string.IsNullOrEmpty(requestInfo.Data))
            {
                ExecuteJsonCommand(session, default(TJsonCommandInfo));
                return;
            }

            TJsonCommandInfo jsonCommandInfo;

            if (!string.IsNullOrEmpty(requestInfo.Token))
                session.CurrentToken = requestInfo.Token;

            if (!m_IsPrimitiveType)
                jsonCommandInfo = JsonConvert.DeserializeObject<TJsonCommandInfo>(requestInfo.Data);
            else
                jsonCommandInfo = (TJsonCommandInfo)Convert.ChangeType(requestInfo.Data, m_CommandInfoType);

            ExecuteJsonCommand(session, jsonCommandInfo);
        }

        /// <summary>
        /// Executes the json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandInfo">The command info.</param>
        protected abstract void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo);

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Gets the json response.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonResponse(string token, object content)
        {
            return GetJsonResponse(Name, token, content);
        }

        /// <summary>
        /// Gets the json response.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
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
