using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket
{
    public delegate void SessionEventHandler<TWebSocketSession>(TWebSocketSession session) where TWebSocketSession : WebSocketSession<TWebSocketSession>, new();

    public delegate void SessionEventHandler<TWebSocketSession, TEventArgs>(TWebSocketSession session, TEventArgs e) where TWebSocketSession : WebSocketSession<TWebSocketSession>, new();

    public class WebSocketServer : WebSocketServer<WebSocketSession>
    {
        public WebSocketServer(ISubProtocol<WebSocketSession> subProtocol)
            : base(subProtocol)
        {

        }

        public WebSocketServer()
            : base()
        {

        }
    }

    public abstract class WebSocketServer<TWebSocketSession> : AppServer<TWebSocketSession, WebSocketCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        public WebSocketServer(ISubProtocol<TWebSocketSession> subProtocol)
            : this()
        {
            m_SubProtocol = subProtocol;
        }

        public WebSocketServer()
            : base(new WebSocketProtocol())
        {

        }

        private ISubProtocol<TWebSocketSession> m_SubProtocol;

        private string m_WebSocketUriSufix;

        public new WebSocketProtocol Protocol
        {
            get
            {
                return (WebSocketProtocol)base.Protocol;
            }
        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<WebSocketCommandInfo> protocol)
        {
            string subProtocolValue = config.Options.GetValue("subProtocol");
            if (!string.IsNullOrEmpty(subProtocolValue))
            {
                ISubProtocol<TWebSocketSession> subProtocol;
                if (AssemblyUtil.TryCreateInstance<ISubProtocol<TWebSocketSession>>(subProtocolValue, out subProtocol))
                    m_SubProtocol = subProtocol;
            }

            if (m_SubProtocol != null)
                m_SubProtocol.Initialize(config);

            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            if (string.IsNullOrEmpty(config.Security) || "none".Equals(config.Security, StringComparison.OrdinalIgnoreCase))
                m_WebSocketUriSufix = "ws";
            else
                m_WebSocketUriSufix = "wss";

            return true;
        }

        private Dictionary<string, ISubCommand<TWebSocketSession>> m_SubProtocolCommandDict = new Dictionary<string, ISubCommand<TWebSocketSession>>(StringComparer.OrdinalIgnoreCase);

        private bool m_SubCommandsLoaded = false;

        private SessionEventHandler<TWebSocketSession> m_NewSessionConnected;

        public event SessionEventHandler<TWebSocketSession> NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        private SessionEventHandler<TWebSocketSession, CloseReason> m_SessionClosed;

        public event SessionEventHandler<TWebSocketSession, CloseReason> SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
        }

        private SessionEventHandler<TWebSocketSession, string> m_NewMessageReceived;

        public event SessionEventHandler<TWebSocketSession, string> NewMessageReceived
        {
            add
            {
                m_NewMessageReceived += value;
                this.CommandHandler += new CommandHandler<TWebSocketSession, WebSocketCommandInfo>(WebSocketServer_CommandHandler);
            }
            remove
            {
                m_NewMessageReceived -= value;
                this.CommandHandler -= new CommandHandler<TWebSocketSession, WebSocketCommandInfo>(WebSocketServer_CommandHandler);
            }
        }

        void WebSocketServer_CommandHandler(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (m_NewMessageReceived == null)
                return;

            m_NewMessageReceived(session, commandInfo.Data);
        }        

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, "[^0-9]", String.Empty);
            string k2 = Regex.Replace(secKey2, "[^0-9]", String.Empty);

            //Convert received string to 64 bit integer.
            Int64 intK1 = Int64.Parse(k1);
            Int64 intK2 = Int64.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
            byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
            //byte[] b3 = Encoding.UTF8.GetBytes(secKey3);
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            List<byte> bChallenge = new List<byte>();
            bChallenge.AddRange(b1);
            bChallenge.AddRange(b2);
            bChallenge.AddRange(b3);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge.ToArray());
            return hash;
        }

        private void SetCookie(TWebSocketSession session)
        {
            string cookieValue = session.Context[WebSocketConstant.Cookie];

            var cookies = new StringDictionary();

            if (!string.IsNullOrEmpty(cookieValue))
            {
                string[] pairs = cookieValue.Split(';');

                int pos;
                string key, value;

                foreach (var p in pairs)
                {
                    pos = p.IndexOf('=');
                    if (pos > 0)
                    {
                        key = p.Substring(0, pos).Trim();
                        pos += 1;
                        if (pos < p.Length)
                            value = p.Substring(pos).Trim();
                        else
                            value = string.Empty;
                        cookies.Add(key, value);
                    }
                }                
            }

            session.Cookies = cookies;
        }

        private void ProcessHandshakeRequest(TWebSocketSession session)
        {
            SetCookie(session);

            var secKey1 = session.Context.SecWebSocketKey1;
            var secKey2 = session.Context.SecWebSocketKey2;
            var secKey3 = session.Context.SecWebSocketKey3;
            var secWebSocketVersion = session.Context.SecWebSocketVersion;

            var responseBuilder = new StringBuilder();

            //draft-ietf-hybi-thewebsocketprotocol-06
            if ("6".Equals(secWebSocketVersion))
            {
                const string magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

                var secWebSocketKey = session.Context[WebSocketConstant.SecWebSocketKey];

                if (string.IsNullOrEmpty(secWebSocketKey))
                {
                    session.Close(CloseReason.ServerClosing);
                    return;
                }

                string secKeyAccept = string.Empty;

                try
                {
                    secKeyAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + magic)));
                }
                catch (Exception)
                {
                    session.Close(CloseReason.ServerClosing);
                    return;
                }

                responseBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
                responseBuilder.AppendLine("Upgrade: WebSocket");
                responseBuilder.AppendLine("Connection: Upgrade");
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Accept: {0}", secKeyAccept));
                responseBuilder.AppendLine();
                session.SendRawResponse(responseBuilder.ToString());
            }
            else
            {
                //Common for all websockets editions (v.75 & v.76)
                responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
                responseBuilder.AppendLine("Upgrade: WebSocket");
                responseBuilder.AppendLine("Connection: Upgrade");

                //Check if the client send Sec-WebSocket-Key1 and Sec-WebSocket-Key2
                if (String.IsNullOrEmpty(secKey1) && String.IsNullOrEmpty(secKey2))
                {
                    //No keys, v.75
                    if (!string.IsNullOrEmpty(session.Context.Origin))
                        responseBuilder.AppendLine(string.Format("WebSocket-Origin: {0}", session.Context.Origin));
                    responseBuilder.AppendLine(string.Format("WebSocket-Location: {0}://{1}{2}", m_WebSocketUriSufix, session.Context.Host, session.Context.Path));
                    responseBuilder.AppendLine();
                    session.SendRawResponse(responseBuilder.ToString());
                }
                else
                {
                    //Have Keys, v.76
                    if (!string.IsNullOrEmpty(session.Context.Origin))
                        responseBuilder.AppendLine(string.Format("Sec-WebSocket-Origin: {0}", session.Context.Origin));
                    responseBuilder.AppendLine(string.Format("Sec-WebSocket-Location: {0}://{1}{2}", m_WebSocketUriSufix, session.Context.Host, session.Context.Path));
                    responseBuilder.AppendLine();
                    session.SendRawResponse(responseBuilder.ToString());
                    //Encrypt message
                    byte[] secret = GetResponseSecurityKey(secKey1, secKey2, secKey3);
                    session.SendResponse(secret);
                }
            }
        }

        internal static void ParseHandshake(WebSocketContext context, TextReader reader)
        {
            string line;
            string firstLine = string.Empty;
            string prevKey = string.Empty;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                if (line.StartsWith("\t") && !string.IsNullOrEmpty(prevKey))
                {
                    string currentValue = context[prevKey];
                    context[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(':');

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(" ") && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                context[key] = value;
                prevKey = key;
            }

            var metaInfo = firstLine.Split(' ');

            context.Method = metaInfo[0];
            context.Path = metaInfo[1];
            context.HttpVersion = metaInfo[2];
        }

        public override void ExecuteCommand(TWebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (!session.Handshaked)
            {
                ProcessHandshakeRequest(session);
                session.Handshaked = true;

                if (m_NewSessionConnected != null)
                    m_NewSessionConnected(session);
            }
            else
            {
                if (m_SubCommandsLoaded)
                {
                    ExecuteSubCommand(session, commandInfo, m_SubProtocol.SubCommandParser.ParseSubCommand(commandInfo));
                }
                else
                {
                    base.ExecuteCommand(session, commandInfo);
                }
            }
        }

        private void ExecuteSubCommand(TWebSocketSession session, WebSocketCommandInfo rawCommandInfo, StringCommandInfo subCommandInfo)
        {
            ISubCommand<TWebSocketSession> subCommand;

            if (m_SubProtocolCommandDict.TryGetValue(subCommandInfo.Key, out subCommand))
            {
                session.Context.CurrentCommand = subCommandInfo.Key;
                subCommand.ExecuteCommand(session, subCommandInfo);
                session.Context.PrevCommand = subCommandInfo.Key;

                if (Config.LogCommand)
                    Logger.LogError(session, string.Format("Command - {0} - {1}", session.IdentityKey, subCommandInfo.Key));
            }
            else
            {
                session.HandleUnknownCommand(rawCommandInfo);
            }

            session.LastActiveTime = DateTime.Now;
        }

        protected override bool SetupCommands(Dictionary<string, ICommand<TWebSocketSession, WebSocketCommandInfo>> commandDict)
        {
            if (m_SubProtocol != null)
            {
                foreach (var command in m_SubProtocol.GetSubCommands())
                {
                    if (m_SubProtocolCommandDict.ContainsKey(command.Name))
                    {
                        Logger.LogError(string.Format("You have defined duplicated command {0} in your command assembly!", command.Name));
                        return false;
                    }

                    m_SubProtocolCommandDict.Add(command.Name, command);
                }

                m_SubCommandsLoaded = true;
            }
            else
            {
                base.SetupCommands(commandDict);
            }

            return true;
        }

        protected override void OnAppSessionClosed(object sender, AppSessionClosedEventArgs<TWebSocketSession> e)
        {
            if (m_SessionClosed != null)
                m_SessionClosed(e.Session, e.Reason);
        }
    }
}
