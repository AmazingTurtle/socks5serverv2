using System;
using System.IO;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public interface ISocksMessage
    {

        /// <summary>
        /// Read the socks message information from binary reader into accessible properties
        /// </summary>
        /// <returns>True if parsing was successful, false if not</returns>
        bool Parse(BinaryReader reader);

    }
}
