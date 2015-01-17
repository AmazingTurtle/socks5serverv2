using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Constants
{
    public enum AuthenticationState : byte
    {

        Awaiting,
        Authenticating,
        Authenticated,
        Transmitting // after successful command, transmitting data

    }
}
