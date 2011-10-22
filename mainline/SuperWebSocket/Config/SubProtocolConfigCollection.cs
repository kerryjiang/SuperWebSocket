using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperWebSocket.Config
{
    [ConfigurationCollection(typeof(SubProtocolConfig))]
    public class SubProtocolConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SubProtocolConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SubProtocolConfig)element).Name;
        }

        public new IEnumerator<SubProtocolConfig> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (SubProtocolConfig)base.BaseGet(i);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "protocol";
            }
        }
    }
}
