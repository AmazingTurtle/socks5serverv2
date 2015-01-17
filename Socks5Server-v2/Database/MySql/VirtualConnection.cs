using System;
using MySql.Data.MySqlClient;

namespace Socks5S.Database.MySql
{
    public class VirtualConnection : Plugin.IDatabaseConnection
    {

        #region Members

        /// <summary>
        /// MySql.Data.MySqlClient.MySqlConnection instance received from MySql.Impl, generated at connection pool initialization
        /// </summary>
        public MySqlConnection Connection { get; private set; }

        /// <summary>
        /// Last time of interaction in milliseconds
        /// </summary>
        public long LastInteractionTime { get; private set; }

        /// <summary>
        /// Busy state of the virtual connection indicates if the connection is in use or not
        /// </summary>
        public bool IsBusy { get; set; }

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Create a new virtual connection wrapper to interact with database easier
        /// </summary>
        internal VirtualConnection(MySqlConnection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Create a new command wrapper to query over database easily
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <returns>A command wrapper instance</returns>
        public CommandWrapper Command(string query)
        {
            return new CommandWrapper(this, query);
        }

        /// <summary>
        /// Set the last interaction time to DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond to ensure reconnect before remotehost disconnect by wait timeout
        /// </summary>
        public void Interact()
        {
            this.LastInteractionTime = (long)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// Close and reset existing connection. Opens a new fresh connection
        /// </summary>
        public void Refresh()
        {
            if(this.Connection.State != System.Data.ConnectionState.Closed)
                this.Connection.Close();
            this.Connection.Open();
            this.Interact();
        }

        #endregion


        #region Impl

        public Plugin.IDatabaseTransaction OpenTransaction()
        {
            return new Transaction(this.Connection.BeginTransaction());
        }

        #endregion

    }
}
