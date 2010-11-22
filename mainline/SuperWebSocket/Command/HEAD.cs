using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;
using System.IO;

namespace SuperWebSocket.Command
{
    public class HEAD : StringCommandBase<WebSocketSession>
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            var headerDict = new StringDictionary();

            StringReader reader = new StringReader(commandInfo.CommandData);

            string line;
            
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                string[] lineInfo = line.Split(':');

                string key = lineInfo[0];
                if(!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = lineInfo[1];
                if (!string.IsNullOrEmpty(value))
                    value = value.TrimStart(' ');

                if (string.IsNullOrEmpty(key))
                    continue;

                session.Context[key] = value;
            }
        }
    }
}
