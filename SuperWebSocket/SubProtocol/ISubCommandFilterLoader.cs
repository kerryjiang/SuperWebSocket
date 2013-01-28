using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperWebSocket.SubProtocol
{
    interface ISubCommandFilterLoader
    {
        void LoadSubCommandFilters(IEnumerable<SubCommandFilterAttribute> globalFilters);
    }
}
