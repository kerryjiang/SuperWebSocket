using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public class BasicSubCommandParser : ISubProtocolCommandParser
    {
        #region ISubProtocolCommandParser Members

        public StringCommandInfo ParseSubCommand(WebSocketCommandInfo commandInfo)
        {
            var cmd = commandInfo.Text.Trim();
            int pos = cmd.IndexOf(' ');
            string name;
            string param;

            if (pos > 0)
            {
                name = cmd.Substring(0, pos);
                param = cmd.Substring(pos + 1);
            }
            else
            {
                name = cmd;
                param = string.Empty;
            }

            string[] paramArray;

            if (!string.IsNullOrEmpty(param))
            {
                paramArray = param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                paramArray = new string[0];
            }

            return new StringCommandInfo(name, param, paramArray);
        }

        #endregion
    }
}
