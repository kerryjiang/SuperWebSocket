using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperWebSocket.Client
{
    public class ErrorEventArgs : EventArgs
    {
        public SocketError Error { get; private set; }

        public string ErrorMessage { get; private set; }

        public ErrorEventArgs(SocketError error)
            : this(error, string.Empty)
        {

        }

        public ErrorEventArgs(SocketError error, string errorMessage)
        {
            Error = error;
            ErrorMessage = errorMessage;
        }
    }
}
