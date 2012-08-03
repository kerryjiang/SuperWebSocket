using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.Samples.CustomSession
{
    public class CRMSession : WebSocketSession<CRMSession>
    {
        public int CompanyId { get; private set; }

        protected override void OnHandShaked()
        {
            int companyId;

            //read companyId from cookie
            if (int.TryParse(Cookies["companyId"], out companyId))
                CompanyId = companyId;
        }
    }
}
