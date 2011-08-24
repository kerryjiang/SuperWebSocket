using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="dataOffset">The payload data offset in data frame.</param>
        public WebSocketCommandInfo(WebSocketDataFrame frame, int dataOffset)
        {
            Key = frame.OpCode.ToString();

            if (frame.OpCode != 2)
                Text = Encoding.UTF8.GetString(frame.InnerData.ToArrayData(dataOffset, (int)frame.ActualPayloadLength));
            else
                Data = frame.InnerData.ToArrayData(dataOffset, (int)frame.ActualPayloadLength);
        }

        public string Key { get; private set; }

        public string Text { get; private set; }

        public byte[] Data { get; private set; }
    }
}
