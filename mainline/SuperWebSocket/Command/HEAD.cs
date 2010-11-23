using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace SuperWebSocket.Command
{
    public class HEAD : CommandBase<WebSocketSession, WebSocketCommandInfo>
    {
        public override void ExecuteCommand(WebSocketSession session, WebSocketCommandInfo commandInfo)
        {
            var secKey1 = session.Context.SecWebSocketKey1;
            var secKey2 = session.Context.SecWebSocketKey2;
            var secKey3 = session.Context.SecWebSocketKey3;

            var responseBuilder = new StringBuilder();

            //Common for all websockets editions (v.75 & v.76)
            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            responseBuilder.AppendLine("Upgrade: WebSocket");
            responseBuilder.AppendLine("Connection: Upgrade");
            responseBuilder.AppendLine("HTTP/1.1 101 Web Socket Protocol Handshake");

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
    }
}
