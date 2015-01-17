using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Constants
{
    public enum Command : byte
    {

        Connect = 0x01,
        Bind = 0x02,
        UdpAssociate = 0x03

    }
}
