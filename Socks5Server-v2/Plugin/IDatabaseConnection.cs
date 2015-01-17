using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public interface IDatabaseConnection
    {

        /// <summary>
        /// True if virtual connection is busy / taken, false if not.
        /// </summary>
        /// <remarks>Don't forget to set IsBusy to false when releasing!</remarks>
        bool IsBusy { get; set; }

        /// <summary>
        /// Open a database transaction
        /// </summary>
        /// <returns>An IDatabaseTransaction implementation</returns>
        IDatabaseTransaction OpenTransaction();

    }
}
