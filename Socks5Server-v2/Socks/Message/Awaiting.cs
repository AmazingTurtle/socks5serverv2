using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Message
{
    public class Awaiting : Plugin.ISocksMessage
    {

        public byte SocksVersion { get; private set; }
        public Constants.AuthenticationMethod[] AvailableMethods { get; private set; }

        public bool Parse(System.IO.BinaryReader reader)
        {
            if (reader.BaseStream.Length < 2)
                return false;

            this.SocksVersion = reader.ReadByte();
            byte numberOfMethods = reader.ReadByte();

            // 1 byte for SocksVersion, +1 byte for numberOfMethods, +{numberOfMethod} bytes
            if (reader.BaseStream.Length != 2 + numberOfMethods)
                return false;

            this.AvailableMethods = new Constants.AuthenticationMethod[numberOfMethods];
            for (byte i = 0; i < numberOfMethods; i++)
                this.AvailableMethods[i] = (Constants.AuthenticationMethod)reader.ReadByte();

            // Can't use the same method twice. Someone's kidding us
            bool hasDuplicates = this.AvailableMethods
                .GroupBy(x => x)
                .Where(y => y.Skip(1).Any())
                .Count() > 0;

            return !hasDuplicates;
        }

    }
}
