using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.Protocol
{
    public class HeaderAsyncReader : AsyncReaderBase
    {
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public HeaderAsyncReader(HeaderAsyncReader prevHeaderReader)
        {
            Segments = prevHeaderReader.GetLeftBuffer();
        }

        public HeaderAsyncReader()
        {
            Segments = new ArraySegmentList<byte>();
        }

        #region ICommandAsyncReader Members

        public override StringCommandInfo FindCommand(byte[] readBuffer, int offset, int length)
        {
            Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset, length));

            int? result = Segments.SearchMark(offset, length, m_HeaderTerminator);

            if (!result.HasValue || result.Value <= 0)
            {
                NextCommandReader = new HeaderAsyncReader(this);
                return null;
            }

            string header = Encoding.UTF8.GetString(Segments.ToArrayData(0, result.Value));

            int left = Segments.Count - result.Value - m_HeaderTerminator.Length;

            Segments.ClearSegements();

            if (left > 0)
            {
                Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));
            }

            NextCommandReader = new DataAsyncReader(this);
            return new StringCommandInfo("HEAD", header, new string[] { });
        }

        #endregion
    }
}
