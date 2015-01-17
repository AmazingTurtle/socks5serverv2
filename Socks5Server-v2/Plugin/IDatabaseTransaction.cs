using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public interface IDatabaseTransaction : IDisposable
    {

        /// <summary>
        /// Commit transaction to database implementation
        /// </summary>
        void Commit();

    }
}
