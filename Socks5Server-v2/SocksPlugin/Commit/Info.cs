using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.SocksPlugin.Commit
{
    public struct Info
    {

        public long AccountId;

        public Stats? @Stats;
        public Command? @Command;
        
        public Info(long accountId)
        {
            this.AccountId = accountId;
            this.Stats = null;
            this.Command = null;
        }

    }
}
