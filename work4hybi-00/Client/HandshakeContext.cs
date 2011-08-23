using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Client
{
    class HandshakeContext
    {
        public ArraySegmentList<byte> HandshakeData { get; set; }

        public int ExpectedLength { get; set; }

        public byte[] ExpectedChallenge { get; set; }

        public HandshakeContext()
        {
            HandshakeData = new ArraySegmentList<byte>();
        }
    }
}
