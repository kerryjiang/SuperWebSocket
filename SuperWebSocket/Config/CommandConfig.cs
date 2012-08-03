using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperWebSocket.Config
{
    public class CommandConfig : ConfigurationElement
    {
        public NameValueCollection Options { get; private set; }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (Options == null)
            {
                Options = new NameValueCollection();
            }

            Options.Add(name, value);
            return true;
        }
    }
}
