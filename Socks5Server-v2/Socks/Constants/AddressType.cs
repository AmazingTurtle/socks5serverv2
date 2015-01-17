using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Constants
{
    public enum AddressType : byte
    {

        IPv4 = 0x01,
        Domain = 0x03,
        IPv6 = 0x04

    }
}
