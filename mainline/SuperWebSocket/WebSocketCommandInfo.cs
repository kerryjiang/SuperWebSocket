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

        public WebSocketCommandInfo(IList<WebSocketDataFrame> frames, int left)
        {
            var opCode = frames[0].OpCode;
            Key = opCode.ToString();

            if (opCode != 2)
            {
                var stringBuilder = new StringBuilder();

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    var data = frame.InnerData.ToArrayData(frame.InnerData.Count - (int)frame.ActualPayloadLength - left, (int)frame.ActualPayloadLength);

                    if (frame.HasMask)
                    {
                        data = DecodeMask(data, frame.MaskKey);
                    }

                    stringBuilder.Append(Encoding.UTF8.GetString(data));
                }

                Text = stringBuilder.ToString();
            }
            else
            {
                var resultBuffer = new ArraySegmentList<byte>();

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    var data = frame.InnerData.ToArrayData(frame.InnerData.Count - (int)frame.ActualPayloadLength - left, (int)frame.ActualPayloadLength);

                    if (frame.HasMask)
                    {
                        data = DecodeMask(data, frame.MaskKey);
                    }

                    resultBuffer.AddSegment(new ArraySegment<byte>(data));
                }

                Data = resultBuffer.ToArrayData();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="left">The left.</param>
        public WebSocketCommandInfo(WebSocketDataFrame frame, int left)
        {
            Key = frame.OpCode.ToString();

            var data = frame.InnerData.ToArrayData(frame.InnerData.Count - (int)frame.ActualPayloadLength - left, (int)frame.ActualPayloadLength);

            if (frame.HasMask)
            {
                data = DecodeMask(data, frame.MaskKey);
            }

            if (frame.OpCode != 2)
                Text = Encoding.UTF8.GetString(data);
            else
                Data = data;
        }

        private byte[] DecodeMask(byte[] data, byte[] mask)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }

            return data;
        }

        public string Key { get; private set; }

        public string Text { get; private set; }

        public byte[] Data { get; private set; }
    }
}
