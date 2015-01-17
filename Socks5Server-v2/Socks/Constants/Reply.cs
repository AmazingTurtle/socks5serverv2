using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Constants
{
    public enum Reply : byte
    {

        Succeeded,
        GeneralServerFailure,
        ConnectionNotAllowedByRuleset,
        NetworkUnreachable,
        HostUnreachable,
        ConnectionRefused,
        TTLExpired,
        CommandNotSupported,
        AddressTypeNotSupported,
        UNASSIGNED

    }
}
