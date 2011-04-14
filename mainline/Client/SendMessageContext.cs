using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Client
{
    class SendMessageContext
    {
        public Encoder Encoder { get; set; }
        public char[] Message { get; set; }
        public int SentLength { get; set; }
        public bool Completed { get; set; }
    }
}
