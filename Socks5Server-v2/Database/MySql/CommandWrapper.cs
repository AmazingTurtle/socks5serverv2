using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Socks5S.Database.MySql
{
    public class CommandWrapper
    {

        #region Members

        /// <summary>
        /// The parent object owning this instance must be a valid and connected virtual connection
        /// </summary>
        public readonly VirtualConnection Owner;

        /// <summary>
        /// SQL query string
        /// </summary>
        public string Query { get; private set; }

        /// <summary>
        /// Named parameters for prepared SQL statements
        /// </summary>
        public Dictionary<string, object> Parameters { get; private set; }

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Command wrapper to easily query over database
        /// </summary>
        public CommandWrapper(VirtualConnection owner, string query)
        {
            this.Owner = owner;
            this.Query = query;
            this.Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create and prepare a SQL command with parameters
        /// </summary>
        /// <returns></returns>
        protected MySqlCommand MakeCommand()
        {
            MySqlCommand command = new MySqlCommand(this.Query, this.Owner.Connection);
            foreach (KeyValuePair<string, object> kp in this.Parameters)
                command.Parameters.AddWithValue(kp.Key, kp.Value);
            return command;
        }

        /// <summary>
        /// Set a named parameter for prepared SQL staments
        /// </summary>
        /// <param name="param">Name of paramter</param>
        /// <param name="value">Value of parameter</param>
        /// <returns>this (CommandWrapper) to easily continue setting parameters in a row</returns>
        public CommandWrapper SetParameter(string param, object value)
        {
            this.Parameters.Add(param, value);
            return this;
        }

        /// <summary>
        /// Execute the query nonquery style
        /// </summary>
        /// <returns>Number of rows affected by this query</returns>
        public int Execute()
        {
            using (MySqlCommand command = this.MakeCommand())
                return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Execute and get the first column from first row returned by SQL server
        /// </summary>
        /// <returns>Scalar data returned by SQL server</returns>
        public object ExecuteScalar()
        {
            using (MySqlCommand command = this.MakeCommand())
                return command.ExecuteScalar();
        }

        /// <summary>
        /// Execute the query and read results into <see cref="System.Data.DataTable" />
        /// </summary>
        /// <returns>A filled <see cref="System.Data.DataTable" /></returns>
        public DataTable ReadTable()
        {
            DataTable dataTable = new DataTable();
            using (MySqlCommand command = this.MakeCommand())
            {
                command.ExecuteNonQuery();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }

        #endregion

    }
}
