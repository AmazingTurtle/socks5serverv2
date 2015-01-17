using System;
using System.IO;

namespace Socks5S.Socks
{
    public class SocksMessageFactory : Plugin.ISocksMessageFactory
    {
        public Plugin.ISocksMessage Create<T>(BinaryReader reader) where T : Plugin.ISocksMessage
        {
            Plugin.ISocksMessage message = (Plugin.ISocksMessage)Activator.CreateInstance(typeof(T));
            if(message.Parse(reader))
                return message;
            return null;
        }
    }
}
