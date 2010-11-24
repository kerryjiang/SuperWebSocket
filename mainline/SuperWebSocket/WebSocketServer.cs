using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SuperWebSocket
{
    public delegate void SessionEventHandler(WebSocketSession session);

    public class WebSocketServer : AppServer<WebSocketSession, WebSocketCommandInfo>
    {
        public WebSocketServer()
            : base()
        {
            Protocol = new WebSocketProtocol();
        }

        private SessionEventHandler m_NewSessionConnected;

        public event SessionEventHandler NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        private SessionEventHandler m_SessionClosed;

        public event SessionEventHandler SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
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

        private void ProcessHeadRequest(WebSocketSession session)
        {
            var secKey1 = session.Context.SecWebSocketKey1;
            var secKey2 = session.Context.SecWebSocketKey2;
            var secKey3 = session.Context.SecWebSocketKey3;

            var responseBuilder = new StringBuilder();

            //Common for all websockets editions (v.75 & v.76)
            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");

            //Check if the client send Sec-WebSocket-Key1 and Sec-WebSocket-Key2
            if (String.IsNullOrEmpty(secKey1) && String.IsNullOrEmpty(secKey2))
            {
                //No keys, v.75
                responseBuilder.AppendLine(string.Format("WebSocket-Origin: {0}", session.Context.Origin));
                responseBuilder.AppendLine(string.Format("WebSocket-Location: ws://{0}{1}", session.Context.Host, session.Context.Path));
                responseBuilder.AppendLine();
            }
            else
            {
                //Have Keys, v.76
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Origin: {0}", session.Context.Origin));
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Location: ws://{0}{1}", session.Context.Host, session.Context.Path));
                responseBuilder.AppendLine();
                //Encrypt message
                byte[] secret = GetResponseSecurityKey(secKey1, secKey2, secKey3);
                responseBuilder.Append(secret);
            }

            session.SendRawResponse(responseBuilder.ToString());
        }

        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            if (commandInfo.CommandKey.Equals(WebSocketConstant.CommandHead))
            {
                ProcessHeadRequest(session);

                if (m_NewSessionConnected != null)
                    m_NewSessionConnected(session);
            }
            else
            {
                base.ExecuteCommand(session, commandInfo);
            }
        }

        protected override void OnSocketSessionClosed(object sender, SocketSessionClosedEventArgs e)
        {
            var session = this.GetAppSessionByIndentityKey(e.IdentityKey);

            base.OnSocketSessionClosed(sender, e);

            if (m_SessionClosed == null)
                m_SessionClosed(session);
        }
    }
}
