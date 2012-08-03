using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperWebSocket.Config
{
    [ConfigurationCollection(typeof(CommandConfig))]
    public class CommandConfigCollection : ConfigurationElementCollection
    {
        public CommandConfig this[int index]
        {
            get
            {
                return (CommandConfig)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CommandConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element.GetHashCode();
        }

        public new IEnumerator<CommandConfig> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (CommandConfig)base.BaseGet(i);
            }
        }
    }
}
