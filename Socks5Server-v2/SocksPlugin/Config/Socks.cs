using System;
using System.Xml.Serialization;

namespace Socks5S.SocksPlugin.Config
{
    [XmlRoot("SocksPlugin")]
    public class Socks
    {

        [XmlElement("ProxyDataSize")]
        public int ProxyDataSize { get; set; }

        [XmlElement("Restriction")]
        public Restriction @Restriction { get; set; }

    }
}
