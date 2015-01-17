using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Socks.Message.Authentication
{
    public class UsernamePassword : Plugin.ISocksMessage
    {

        public byte Version { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public bool Parse(System.IO.BinaryReader reader)
        {
            // 1 byte version, +1 byte usernameLength, +{usernameLength} bytes username, +1 byte passwordLength, +{passwordLength} bytes password
            if (reader.BaseStream.Length < 5)
                return false;
            this.Version = reader.ReadByte();

            // 1 byte usernameLength, +{usernameLength}, +1 byte passwordLength, +{passwordLength} bytes password
            byte usernameLength = reader.ReadByte();
            if (usernameLength == 0 || reader.BaseStream.PeekBytes() < usernameLength + 2)
                return false;
            this.Username = Encoding.UTF8.GetString(reader.ReadBytes(usernameLength));

            // 1 byte passwordLength, +{passwordLength} bytes password
            byte passwordLength = reader.ReadByte();
            if (passwordLength == 0 || reader.BaseStream.PeekBytes() != passwordLength)
                return false;
            this.Password = Encoding.UTF8.GetString(reader.ReadBytes(passwordLength));

            return true;

        }
    }
}
