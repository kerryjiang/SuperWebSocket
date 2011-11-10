using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;
using SuperSocket.Common;

namespace SuperWebSocket.WebSocketClient.Reader
{
    abstract class ReaderBase : IClientCommandReader<WebSocketCommandInfo>
    {
        private readonly ArraySegmentList<byte> m_BufferSegments;

        /// <summary>
        /// Gets the buffer segments which can help you parse your commands conviniently.
        /// </summary>
        protected ArraySegmentList<byte> BufferSegments
        {
            get { return m_BufferSegments; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderBase"/> class.
        /// </summary>
        public ReaderBase()
        {
            m_BufferSegments = new ArraySegmentList<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderBase"/> class.
        /// </summary>
        /// <param name="previousCommandReader">The previous command reader.</param>
        public ReaderBase(ReaderBase previousCommandReader)
        {
            m_BufferSegments = previousCommandReader.BufferSegments;
        }

        public abstract WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);

        /// <summary>
        /// Gets or sets the next command reader.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        internal IClientCommandReader<WebSocketCommandInfo> NextCommandReader { get; set; }

        /// <summary>
        /// Adds the array segment into BufferSegment.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        protected void AddArraySegment(byte[] buffer, int offset, int length)
        {
            BufferSegments.AddSegment(new ArraySegment<byte>(buffer.CloneRange(offset, length)));
        }

        /// <summary>
        /// Clears the buffer segments.
        /// </summary>
        protected void ClearBufferSegments()
        {
            BufferSegments.ClearSegements();
        }
    }
}
