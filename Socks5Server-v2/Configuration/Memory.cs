using System;
using System.Xml.Serialization;

namespace Socks5S.Configuration
{
    public class Memory
    {

        [XmlAttribute("ClientDataSize")]
        public int ClientDataSize { get; set; }

    }
}
