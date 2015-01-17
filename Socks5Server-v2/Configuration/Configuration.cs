using System;
using System.Xml.Serialization;

namespace Socks5S.Configuration
{
    [XmlRoot("Configuration")]
    public class Configuration
    {

        [XmlElement("Server")]
        public Server @Server { get; set; }

        [XmlElement("Memory")]
        public Memory @Memory { get; set; }

        [XmlElement("Database")]
        public Database @Database { get; set; }

    }
}
