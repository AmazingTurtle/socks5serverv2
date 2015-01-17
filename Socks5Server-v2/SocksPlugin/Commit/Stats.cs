using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.SocksPlugin.Commit
{
    public struct Stats
    {

        public long WiredTx;
        public long WiredRx;

        public Stats(long wiredTx, long wiredRx)
        {
            this.WiredTx = wiredTx;
            this.WiredRx = wiredRx;
        }

    }
}
