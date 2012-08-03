using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using System.Configuration;

namespace SuperWebSocket.Config
{
    public class SubProtocolConfig : ConfigurationElementBase
    {
        public SubProtocolConfig()
            : base(false)
        {

        }

        [ConfigurationProperty("type", IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
        }

        [ConfigurationProperty("commands")]
        public CommandConfigCollection Commands
        {
            get
            {
                return this["commands"] as CommandConfigCollection;
            }
        }
    }
}
