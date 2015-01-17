using System;
using global::MySql.Data.MySqlClient;

namespace Socks5S.Database.MySql
{
    public class Transaction : Plugin.IDatabaseTransaction
    {

        #region Members

        public MySqlTransaction @MySqlTransaction { get; private set; }

        #endregion

        #region Constructor, Functions

        public Transaction(MySqlTransaction mySqlTransaction)
        {
            this.MySqlTransaction = mySqlTransaction;
        }

        /// <summary>
        /// Commit transaction to database implementation
        /// </summary>
        public void Commit()
        {
            this.MySqlTransaction.Commit();
        }

        #endregion

        #region IDisposable Impl

        public void Dispose()
        {
            this.MySqlTransaction.Dispose();
        }

        #endregion

    }
}
