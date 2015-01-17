using System;
using System.Xml.Serialization;

namespace Socks5S.Configuration
{
    public class Server
    {

        [XmlAttribute("Address")]
        public string Address { get; set; }

        [XmlAttribute("Port")]
        public ushort Port { get; set; }

        [XmlAttribute("ClientLimit")]
        public int ClientLimit { get; set; }

    }
}
