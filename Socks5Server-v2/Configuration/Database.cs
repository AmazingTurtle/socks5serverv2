using System;
using System.Xml.Serialization;

namespace Socks5S.Configuration
{
    public class Database
    {

        public class DatabaseServer
        {

            [XmlAttribute("Host")]
            public string Host { get; set; }

            [XmlAttribute("Port")]
            public ushort Port { get; set; }

        }

        public class DatabaseLogin
        {

            [XmlAttribute("Username")]
            public string Username { get; set; }

            [XmlAttribute("Password")]
            public string Password { get; set; }

        }

        public class DatabaseConfig
        {
            [XmlAttribute("DatabaseName")]
            public string DatabaseName { get; set; }

            [XmlAttribute("PoolSize")]
            public byte PoolSize { get; set; }
        }

        [XmlAttribute("Driver")]
        public string Driver { get; set; }

        [XmlElement("Server")]
        public DatabaseServer Server { get; set; }

        [XmlElement("Login")]
        public DatabaseLogin Login { get; set; }


        [XmlElement("Config")]
        public DatabaseConfig Config { get; set; }
        
    }
}
