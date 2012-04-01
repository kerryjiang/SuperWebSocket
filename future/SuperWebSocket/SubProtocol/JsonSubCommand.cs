using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using Newtonsoft.Json;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.SubProtocol
{
    /// <summary>
    /// JsonSubCommand
    /// </summary>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommand<TJsonCommandInfo> : JsonSubCommand<WebSocketSession, TJsonCommandInfo>
    {

    }

    /// <summary>
    /// JsonSubCommand
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommand<TWebSocketSession, TJsonCommandInfo> : JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the json response.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonResponse(TWebSocketSession session, object content)
        {
            return GetJsonResponse(session, Name, content);
        }

        /// <summary>
        /// Gets the json response.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonResponse(TWebSocketSession session, string name, object content)
        {
            return GetJsonResponse(name, session.CurrentToken, content);
        }

        /// <summary>
        /// Sends the json response.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonResponse(TWebSocketSession session, object content)
        {
            session.SendResponse(GetJsonResponse(session, content));
        }

        /// <summary>
        /// Sends the json response.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonResponse(TWebSocketSession session, string name, object content)
        {
            session.SendResponse(GetJsonResponse(session, name, content));
        }
    }
}
