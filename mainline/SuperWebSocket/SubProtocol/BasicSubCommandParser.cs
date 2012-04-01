using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperWebSocket.SubProtocol
{
    public class BasicSubCommandParser : IRequestInfoParser<SubRequestInfo>
    {
        #region ISubProtocolCommandParser Members

        public SubRequestInfo ParseRequestInfo(string source)
        {
            var cmd = source.Trim();
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

            pos = name.IndexOf('-');

            string token = string.Empty;

            if (pos > 0)
            {
                token = name.Substring(pos + 1);
            }

            return new SubRequestInfo(name, token, param);
        }

        #endregion
    }
}
