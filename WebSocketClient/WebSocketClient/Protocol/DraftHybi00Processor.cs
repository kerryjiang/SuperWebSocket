using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi00Processor : IProtocolProcessor
    {
        private static List<char> m_CharLib = new List<char>();
        private static List<char> m_DigLib = new List<char>();
        private static Random m_Random = new Random();

        static DraftHybi00Processor()
        {
            for (int i = 33; i <= 126; i++)
            {
                char currentChar = (char)i;

                if (char.IsLetter(currentChar))
                    m_CharLib.Add(currentChar);
                else if (char.IsDigit(currentChar))
                    m_DigLib.Add(currentChar);
            }
        }

        public ReaderBase CreateHandshakeReader()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(WebSocket websocket, string message)
        {
            throw new NotImplementedException();
        }

        public void SendCloseHandshake(WebSocket websocket, string closeReason)
        {
            throw new NotImplementedException();
        }

        public void SendPing(WebSocket websocket, string ping)
        {
            throw new NotImplementedException();
        }

        public void SendHandshake(WebSocket websocket)
        {
            string secKey1 = Encoding.UTF8.GetString(GenerateSecKey());

            string secKey2 = Encoding.UTF8.GetString(GenerateSecKey());

            byte[] secKey3 = GenerateSecKey(8);

            var handshakeBuilder = new StringBuilder();

#if SILVERLIGHT
            handshakeBuilder.AppendLine(string.Format("GET {0} HTTP/1.1", websocket.TargetUri.GetPathAndQuery()));
#else
            handshakeBuilder.AppendLine(string.Format("GET {0} HTTP/1.1", websocket.TargetUri.PathAndQuery));
#endif

            handshakeBuilder.AppendLine("Upgrade: WebSocket");
            handshakeBuilder.AppendLine("Connection: Upgrade");
            handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Key1: {0}", secKey1));
            handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Key2: {0}", secKey2));
            handshakeBuilder.AppendLine(string.Format("Host: {0}", websocket.TargetUri.Host));
            handshakeBuilder.AppendLine(string.Format("Origin: {0}", websocket.TargetUri.Host));

            if (!string.IsNullOrEmpty(websocket.SubProtocol))
                handshakeBuilder.AppendLine(string.Format("Sec-WebSocket-Protocol: {0}", websocket.SubProtocol));

            handshakeBuilder.AppendLine();
            handshakeBuilder.Append(Encoding.UTF8.GetString(secKey3, 0, secKey3.Length));

            byte[] handshakeBuffer = Encoding.UTF8.GetBytes(handshakeBuilder.ToString());

            websocket.Send(handshakeBuffer, 0, handshakeBuffer.Length);
        }

        private byte[] GenerateSecKey()
        {
            int totalLen = m_Random.Next(10, 20);
            return GenerateSecKey(totalLen);
        }

        private byte[] GenerateSecKey(int totalLen)
        {
            int spaceLen = m_Random.Next(1, totalLen / 2 + 1);
            int charLen = m_Random.Next(3, totalLen - 1 - spaceLen);
            int digLen = totalLen - spaceLen - charLen;

            List<char> source = new List<char>(totalLen);

            for (int i = 0; i < spaceLen; i++)
                source.Add(' ');

            for (int i = 0; i < charLen; i++)
            {
                source.Add(m_CharLib[m_Random.Next(0, m_CharLib.Count - 1)]);
            }

            for (int i = 0; i < digLen; i++)
            {
                source.Add(m_DigLib[m_Random.Next(0, m_DigLib.Count - 1)]);
            }

            List<char> mixedChars = new List<char>();

            for (int i = 0; i < totalLen - 1; i++)
            {
                int pos = m_Random.Next(0, source.Count - 1);
                mixedChars.Add(source[pos]);
                source.RemoveAt(pos);
            }

            mixedChars.Add(source[0]);

            return mixedChars.Select(c => (byte)c).ToArray();
        }
    }
}
