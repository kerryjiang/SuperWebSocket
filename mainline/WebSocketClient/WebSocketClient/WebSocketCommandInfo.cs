using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace SuperWebSocket.WebSocketClient
{
    public class WebSocketCommandInfo : ICommandInfo
    {
        public WebSocketCommandInfo()
        {

        }

        public WebSocketCommandInfo(string key)
        {
            Key = key;
        }

        public WebSocketCommandInfo(string key, string text)
        {
            Key = key;
            Text = text;
        }

        public string Key { get; set; }

        public byte[] Data { get; set; }

        public string Text { get; set; }
    }
}
