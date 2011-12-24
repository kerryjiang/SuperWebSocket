using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;
using SuperSocket.Common;

namespace SuperWebSocket
{
    public class WebSocketCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="text">The text data.</param>
        public WebSocketCommandInfo(string key, string text)
        {
            Key = key;
            Text = text;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="text">The text data.</param>
        public WebSocketCommandInfo(string text)
        {
            Key = "1";
            Text = text;
        }

        public WebSocketCommandInfo(IList<WebSocketDataFrame> frames)
        {
            var opCode = frames[0].OpCode;
            Key = opCode.ToString();

            int offset, length;

            if (opCode != 2)
            {
                var stringBuilder = new StringBuilder();

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                    length = (int)frame.ActualPayloadLength;

                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    stringBuilder.Append(frame.InnerData.Decode(Encoding.UTF8, offset, length));
                }

                Text = stringBuilder.ToString();
            }
            else
            {
                var resultBuffer = new byte[frames.Sum(f => (int)f.ActualPayloadLength)];

                int copied = 0;

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                    length = (int)frame.ActualPayloadLength;

                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    frame.InnerData.CopyTo(resultBuffer, offset, copied, length);
                }

                Data = resultBuffer;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="left">The left.</param>
        public WebSocketCommandInfo(WebSocketDataFrame frame)
        {
            Key = frame.OpCode.ToString();

            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
            int length = (int)frame.ActualPayloadLength;

            if (frame.HasMask)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            if (frame.OpCode != 2)
            {
                Text = frame.InnerData.Decode(Encoding.UTF8, offset, length);
                Console.WriteLine("Decoded: {0}, Len: {1}, Text: {2}", Text, length, Encoding.UTF8.GetString(frame.InnerData.ToArrayData(offset, length)));
            }
            else
            {
                Data = frame.InnerData.ToArrayData(offset, length);
            }
        }

        public string Key { get; private set; }

        public string Text { get; private set; }

        public byte[] Data { get; private set; }
    }
}
