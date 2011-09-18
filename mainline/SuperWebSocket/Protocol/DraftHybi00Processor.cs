using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocket.Protocol
{
    class DraftHybi00Processor : ProtocolProcessorBase
    {
        private static readonly byte[] m_ZeroKeyBytes = new byte[0];

        public override bool Handshake(IWebSocketSession session, WebSocketReaderBase previousReader, out ICommandReader<WebSocketCommandInfo> dataFrameReader)
        {
            var secKey1 = session.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey1, string.Empty);
            var secKey2 = session.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey2, string.Empty);

            if (string.IsNullOrEmpty(secKey1) && string.IsNullOrEmpty(secKey2) && NextProcessor != null)
            {
                return NextProcessor.Handshake(session, previousReader, out dataFrameReader);
            }

            session.ProtocolProcessor = this;

            var secKey3 = session.Items.GetValue<byte[]>(WebSocketConstant.SecWebSocketKey3, m_ZeroKeyBytes);

            var responseBuilder = new StringBuilder();

            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");

            if (!string.IsNullOrEmpty(session.Origin))
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Origin: {0}", session.Origin));

            responseBuilder.AppendLine(string.Format("Sec-WebSocket-Location: {0}://{1}{2}", session.UriScheme, session.Host, session.Path));

            var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty));

            if (!string.IsNullOrEmpty(subProtocol))
                responseBuilder.AppendLine(string.Format("Sec-WebSocket-Protocol: {0}", subProtocol));

            responseBuilder.AppendLine();
            session.SendRawResponse(responseBuilder.ToString());
            //Encrypt message
            byte[] secret = GetResponseSecurityKey(secKey1, secKey2, secKey3);
            session.SendResponse(secret);

            dataFrameReader = new WebSocketDataReader(previousReader);

            return true;
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

        public override void SendMessage(IWebSocketSession session, string message)
        {
            var maxByteCount = Encoding.UTF8.GetMaxByteCount(message.Length) + 2;
            var sendBuffer = new byte[maxByteCount];
            sendBuffer[0] = WebSocketConstant.StartByte;
            int bytesCount = Encoding.UTF8.GetBytes(message, 0, message.Length, sendBuffer, 1);
            sendBuffer[1 + bytesCount] = WebSocketConstant.EndByte;

            session.SocketSession.SendResponse(sendBuffer, 0, bytesCount + 2);
        }

        public override void SendCloseHandshake(IWebSocketSession session)
        {
            session.SendResponse(WebSocketConstant.ClosingHandshake);
        }
    }
}
