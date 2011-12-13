using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NUnit.Framework;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using SuperSocket.Common;


namespace SuperWebSocketTest
{
    public class WebSocketHybi10Test : WebSocketTest
    {
        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        protected override void Handshake(string protocol, out Socket socket, out System.IO.Stream stream)
        {
            var ip = "127.0.0.1";
            var port = 2012;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, Encoding.UTF8, false);
            var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 10);

            var secKey = Guid.NewGuid().ToString().Substring(0, 5);

            writer.WriteLine("GET /websock HTTP/1.1");
            writer.WriteLine("Upgrade: WebSocket");
            writer.WriteLine("Sec-WebSocket-Version: 8");
            writer.WriteLine("Connection: Upgrade");
            writer.WriteLine("Sec-WebSocket-Key: " + secKey);
            writer.WriteLine("Host: example.com");
            writer.WriteLine("Origin: http://example.com");

            if (!string.IsNullOrEmpty(protocol))
                writer.WriteLine("Sec-WebSocket-Protocol: {0}", protocol);

            writer.WriteLine("");
            writer.Flush();

            reader.ReadLine();

            var response = new StringDictionary();

            while (true)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                var arr = line.Split(':');

                response[arr[0]] = arr[1].Trim();
            }

            var expectedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secKey + m_Magic)));

            Assert.AreEqual(expectedKey, response["Sec-WebSocket-Accept"]);
        }

        [Test]
        public override void MessageTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            ArraySegmentList<byte> receivedBuffer = new ArraySegmentList<byte>();

            for (int i = 0; i < 10; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string currentCommand = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + currentCommand);

                var data = Encoding.UTF8.GetBytes(currentCommand);

                Console.WriteLine("Client Length:" + data.Length);

                int dataLen = SendMessage(stream, 1, data);
                stream.Flush();

                ReceiveMessage(stream, receivedBuffer, dataLen);

                Assert.AreEqual(dataLen, receivedBuffer.Count);
                Assert.AreEqual(0x01, receivedBuffer[0] & 0x01);
                Assert.AreEqual(0x80, receivedBuffer[0] & 0x80);
                Assert.AreEqual(0x00, receivedBuffer[1] & 0x80);

                int skip = 2;

                if (data.Length < 126)
                    Assert.AreEqual(data.Length, (int)(receivedBuffer[1] & 0x7F));
                else if (data.Length < 65536)
                {
                    Assert.AreEqual(126, (int)(receivedBuffer[1] & 0x7F));
                    var sizeData = receivedBuffer.ToArrayData(2, 2);
                    Assert.AreEqual(data.Length, (int)sizeData[0] * 256 + (int)sizeData[1]);
                    skip += 2;
                }
                else
                {
                    Assert.AreEqual(127, (int)(receivedBuffer[1] & 0x7F));

                    var sizeData = receivedBuffer.ToArrayData(2, 8);

                    long len = 0;
                    int n = 1;

                    for (int k = 7; k >= 0; k--)
                    {
                        len += (int)sizeData[k] * n;
                        n *= 256;
                    }

                    Assert.AreEqual(data.Length, len);
                    skip += 8;
                }

                Assert.AreEqual(currentCommand, Encoding.UTF8.GetString(receivedBuffer.ToArrayData(skip, data.Length)));

                receivedBuffer.ClearSegements();
            }

            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
        }

        [Test]
        public override void MessageBatchTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            ArraySegmentList<byte> receivedBuffer = new ArraySegmentList<byte>();

            for (int i = 0; i < 10; i++)
            {
                var sentMessages = new string[10];
                var sentMessageSizes = new int[10];
                var sentLengths = new int[sentMessages.Length];

                for (int j = 0; j < sentMessages.Length; j++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, Math.Min(messageSource.Length - 1, startPos + 1 + 5));

                    string currentCommand = messageSource.Substring(startPos, endPos - startPos);
                    sentMessages[j] = currentCommand;

                    Console.WriteLine("Client:" + currentCommand);
                    byte[] data = Encoding.UTF8.GetBytes(currentCommand);
                    Console.WriteLine("Client Length:" + data.Length);
                    sentMessageSizes[j] = data.Length;
                    int dataLen = SendMessage(stream, 1, data);
                    sentLengths[j] = dataLen;
                }

                stream.Flush();

                for (var j = 0; j < sentMessages.Length; j++)
                {
                    Console.WriteLine("Expected: " + sentLengths[j]);
                    ReceiveMessage(stream, receivedBuffer, sentLengths[j]);

                    int mlen = sentMessageSizes[j];

                    Assert.AreEqual(sentLengths[j], receivedBuffer.Count);
                    Assert.AreEqual(0x01, receivedBuffer[0] & 0x01);
                    Assert.AreEqual(0x80, receivedBuffer[0] & 0x80);
                    Assert.AreEqual(0x00, receivedBuffer[1] & 0x80);

                    int skip = 2;

                    if (mlen < 126)
                        Assert.AreEqual(mlen, (int)(receivedBuffer[1] & 0x7F));
                    else if (mlen < 65536)
                    {
                        Assert.AreEqual(126, (int)(receivedBuffer[1] & 0x7F));
                        var sizeData = receivedBuffer.ToArrayData(2, 2);
                        Assert.AreEqual(mlen, (int)sizeData[0] * 256 + (int)sizeData[1]);
                        skip += 2;
                    }
                    else
                    {
                        Assert.AreEqual(127, (int)(receivedBuffer[1] & 0x7F));

                        var sizeData = receivedBuffer.ToArrayData(2, 8);

                        long len = 0;
                        int n = 1;

                        for (int k = 7; k >= 0; k--)
                        {
                            len += (int)sizeData[k] * n;
                            n *= 256;
                        }

                        Assert.AreEqual(mlen, len);
                        skip += 8;
                    }

                    Assert.AreEqual(sentMessages[j], Encoding.UTF8.GetString(receivedBuffer.ToArrayData(skip, mlen)));

                    receivedBuffer.ClearSegements();
                    Console.WriteLine("Passed " + j);
                }
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        //No mask
        private int SendMessage(Stream outputStream, int opCode, byte[] data)
        {
            byte[] playloadData = data;

            int length = playloadData.Length;

            byte[] headData;

            if (length < 126)
            {
                headData = new byte[2];
                headData[1] = (byte)length;
            }
            else if (length < 65536)
            {
                headData = new byte[4];
                headData[1] = (byte)126;
                headData[2] = (byte)(length / 256);
                headData[3] = (byte)(length % 256);
            }
            else
            {
                headData = new byte[10];
                headData[1] = (byte)127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    headData[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }

            headData[0] = (byte)(opCode | 0x80);

            outputStream.Write(headData, 0, headData.Length);
            outputStream.Write(playloadData, 0, playloadData.Length);

            return headData.Length + playloadData.Length;
        }
    }
}
