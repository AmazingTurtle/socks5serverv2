using System;
using System.Xml.Serialization;

namespace Socks5S.SocksPlugin.Config
{
    public class Restriction
    {

        [XmlAttribute("AllowIPv6")]
        public bool AllowIPv6 { get; set; }

        [XmlAttribute("AllowRemoteDNS")]
        public bool AllowRemoteDNS { get; set; }

    }
}
