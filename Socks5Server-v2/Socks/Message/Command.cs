using System;
using System.Net;
using System.Text;
using System.Linq;

namespace Socks5S.Socks.Message
{
    public class Command : Plugin.ISocksMessage
    {

        public byte SocksVersion { get; private set; }
        public Constants.Command SocksCommand { get; private set; }
        public Constants.AddressType AddressType { get; private set; }
        public string Domain { get; private set; }
        public IPAddress DestinationAddress { get; private set; }
        public ushort DestinationPort { get; private set; }

        public bool Parse(System.IO.BinaryReader reader)
        {
            // 1 byte socksVersion, +1 byte command, +1 byte reserved, +1 byte addressType, +{addressType} bytes dst.address, +2 bytes dst.Port
            if (reader.BaseStream.Length < 10)
                return false;

            this.SocksVersion = reader.ReadByte();

            this.SocksCommand = (Constants.Command)reader.ReadByte();
            if (!Enum.IsDefined(typeof(Constants.Command), this.SocksCommand))
                return false;

            if (reader.ReadByte() != 0)
                return false;

            this.AddressType = (Constants.AddressType)reader.ReadByte();
            if (!Enum.IsDefined(typeof(Constants.AddressType), this.AddressType))
                return false;

            if (this.AddressType == Constants.AddressType.IPv4)
            {
                if (reader.BaseStream.PeekBytes() != 6)
                    return false;
                this.DestinationAddress = new IPAddress((long)(uint)reader.ReadInt32());
            }
            else if (this.AddressType == Constants.AddressType.IPv6)
            {
                if (reader.BaseStream.PeekBytes() != 18)
                    return false;
                this.DestinationAddress = new IPAddress(reader.ReadBytes(16));
            }
            else if (this.AddressType == Constants.AddressType.Domain)
            {
                byte domainLength = reader.ReadByte();
                if (reader.BaseStream.PeekBytes() != domainLength + 2)
                    return false;
                this.Domain = Encoding.UTF8.GetString(reader.ReadBytes(domainLength));
                var dnsResults = Dns.GetHostEntry(this.Domain);
                this.DestinationAddress = dnsResults.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();
                if (this.DestinationAddress == null)
                    Program.GetInstance().Log.InfoFormat("Domain '{0}' could not be resolved", this.Domain);
            }

            this.DestinationPort = (ushort)IPAddress.NetworkToHostOrder(reader.ReadInt16());
            return true;
        }

    }
}
