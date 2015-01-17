using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.SocksPlugin.Commit
{
    public struct Command
    {

        public Socks.Constants.Command SocksCommand;
        public IPEndPoint ClientEndPoint;
        public IPEndPoint ProxyEndPoint;
        public long WiredTx;
        public long WiredRx;
        public DateTime CommandTime;
        public bool Success;

        public Command(Socks.Constants.Command socksCommand, IPEndPoint clientEndPoint, IPEndPoint proxyEndPoint, long wiredTx, long wiredRx, DateTime commandTime, bool success)
        {
            this.SocksCommand = socksCommand;
            this.ClientEndPoint = clientEndPoint;
            this.ProxyEndPoint = proxyEndPoint;
            this.WiredTx = wiredTx;
            this.WiredRx = wiredRx;
            this.CommandTime = commandTime;
            this.Success = success;
        }

    }
}
