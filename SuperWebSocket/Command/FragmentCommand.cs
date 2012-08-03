using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket.Protocol;

namespace SuperWebSocket.Command
{
    public abstract class FragmentCommand<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        protected Encoding Utf8Encoding { get; private set; }

        public FragmentCommand()
        {
            Utf8Encoding = Encoding.GetEncoding(65001, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        }

        protected bool CheckFrame(WebSocketDataFrame frame)
        {
            //Check RSV
            return (frame.InnerData[0] & 0x70) == 0x00;
        }

        protected bool CheckControlFrame(WebSocketDataFrame frame)
        {
            if (!CheckFrame(frame))
                return false;

            //http://tools.ietf.org/html/rfc6455#section-5.5
            //All control frames MUST have a payload length of 125 bytes or less and MUST NOT be fragmented
            if (!frame.FIN || frame.ActualPayloadLength > 125)
            {
                return false;
            }

            return true;
        }

        protected byte[] GetWebSocketData(IList<WebSocketDataFrame> frames)
        {
            int offset, length;

            var resultBuffer = new byte[frames.Sum(f => (int)f.ActualPayloadLength)];

            int copied = 0;

            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                length = (int)frame.ActualPayloadLength;

                if (length > 0)
                {
                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    frame.InnerData.CopyTo(resultBuffer, offset, copied, length);
                    copied += length;
                }
            }

            return resultBuffer;
        }

        protected string GetWebSocketText(IList<WebSocketDataFrame> frames)
        {
            var data = GetWebSocketData(frames);
            return Utf8Encoding.GetString(data);
        }

        protected byte[] GetWebSocketData(WebSocketDataFrame frame)
        {
            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
            int length = (int)frame.ActualPayloadLength;

            if (frame.HasMask && length > 0)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            byte[] data;

            if (length > 0)
                data = frame.InnerData.ToArrayData(offset, length);
            else
                data = new byte[0];

            return data;
        }

        protected string GetWebSocketText(WebSocketDataFrame frame)
        {
            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
            int length = (int)frame.ActualPayloadLength;

            if (frame.HasMask && length > 0)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            string text;

            if (length > 0)
            {
                text = frame.InnerData.Decode(Utf8Encoding, offset, length);
            }
            else
            {
                text = string.Empty;
            }

            return text;
        }
    }
}
