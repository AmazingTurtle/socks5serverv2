using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Constants
{
    public enum AuthenticationMethod : byte
    {

        NoAuthenticationRequired = 0x00,
        // General Security Service Application Programming Interface
        // standard implementations only in C and Java.
        GSSAPI = 0x01,
        UsernamePassword = 0x02,
        IANA_ASSIGNED = 0x03,
        RESERVED_PRIVATE = 0x80,
        NoAcceptableMethod = 0xFF

    }
}
