using System;
using System.IO;

namespace Socks5S.Plugin
{
    public interface ISocksMessageFactory
    {

        ISocksMessage Create<T>(BinaryReader reader) where T : ISocksMessage;

    }
}
