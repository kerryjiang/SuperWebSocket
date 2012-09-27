using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Samples.CustomSession
{
    public class CRMSession : WebSocketSession<CRMSession>
    {
        public string Name { get; private set; }

        protected override void OnSessionStarted()
        {
            //Read name from path
            var name = Path;

            if (string.IsNullOrEmpty(name))
                name = "Anoy";
            else
                name = Path.TrimStart('/');

            Name = name;
        }
    }
}
