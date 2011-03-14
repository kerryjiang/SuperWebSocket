using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperWebSocket.Client
{
    public class DnsEndPoint : EndPoint
    {
        // Fields
        private AddressFamily m_Family;
        private string m_Host;
        private int m_Port;

        // Methods
        public DnsEndPoint(string host, int port)
            : this(host, port, AddressFamily.Unspecified)
        {
        }

        public DnsEndPoint(string host, int port, AddressFamily addressFamily)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("net_emptystringcall");
            }
            if ((port < 0) || (port > 0xffff))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (((addressFamily != AddressFamily.InterNetwork) && (addressFamily != AddressFamily.InterNetworkV6)) && (addressFamily != AddressFamily.Unspecified))
            {
                throw new ArgumentException("net_sockets_invalid_optionValue_all", "addressFamily");
            }
            this.m_Host = host;
            this.m_Port = port;
            this.m_Family = addressFamily;
        }

        public override bool Equals(object comparand)
        {
            DnsEndPoint point = comparand as DnsEndPoint;
            if (point == null)
            {
                return false;
            }
            return (((this.m_Family == point.m_Family) && (this.m_Port == point.m_Port)) && (this.m_Host == point.m_Host));
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.ToString());
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.m_Family, "/", this.m_Host, ":", this.m_Port });
        }

        // Properties
        public override AddressFamily AddressFamily
        {
            get
            {
                return this.m_Family;
            }
        }

        public string Host
        {
            get
            {
                return this.m_Host;
            }
        }

        public int Port
        {
            get
            {
                return this.m_Port;
            }
        }
    }
}
