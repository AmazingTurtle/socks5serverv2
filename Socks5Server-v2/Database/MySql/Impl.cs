using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Socks5S.Database.MySql
{
    public class Impl : Plugin.IDatabase
    {

        #region Constants

        /// <summary>
        /// Time in milliseconds until sql server resets connection by wait timeout
        /// </summary>
        public static readonly int WaitTimeout = 28800 * 1000;

        #endregion

        #region Members

        /// <summary>
        /// Database configuration retrived through constructor from Program configuration
        /// </summary>
        public Configuration.Database Configuration { get; private set; }

        /// <summary>
        /// Pool of active MySqlConnection instances connected to the configured sql server
        /// </summary>
        public VirtualConnection[] ConnectionPool { get; private set; }

        /// <summary>
        /// Seperate blocking thread to ensure all connections won't timeout
        /// </summary>
        private Thread _checkConnectionsThread;

        /// <summary>
        /// Lock access to connection pool iteration on GetConnection
        /// </summary>
        private ManualResetEvent _iteratorLock;

        #endregion

        #region Constructor, Functions

        public Impl(Configuration.Database configuration)
        {
            this.Configuration = configuration;
            this.ConnectionPool = new VirtualConnection[configuration.Config.PoolSize];
            this._checkConnectionsThread = new Thread(_checkConnections);
            this._iteratorLock = new ManualResetEvent(true);
        }

        /// <summary>
        /// IDatabase.Init implementation to run _checkConnectionsThread
        /// </summary>
        public void Init()
        {
            string connectionString =
                String.Format(
                    "Server={0};Port={1};Database={2};Username={3};Password={4}",
                    this.Configuration.Server.Host,
                    this.Configuration.Server.Port,
                    this.Configuration.Config.DatabaseName,
                    this.Configuration.Login.Username,
                    this.Configuration.Login.Password
                );

            for (int i = 0; i < this.ConnectionPool.Length; i++)
            {
                this.ConnectionPool[i] = new VirtualConnection(new MySqlConnection(connectionString));
                this.ConnectionPool[i].Refresh();
            }
            this._checkConnectionsThread.Start();
        }

        /// <summary>
        /// Loop through all pooled connections and check last interaction timeout to ensure no client gets a connection reset by remote host (wait timeout)
        /// </summary>
        private void _checkConnections()
        {
            while (true)
            {
                long now = (long)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                for (int i = 0; i < this.ConnectionPool.Length; i++)
                    if (!this.ConnectionPool[i].IsBusy && this.ConnectionPool[i].LastInteractionTime + WaitTimeout < now)
                        this.ConnectionPool[i].Refresh();
                Thread.Sleep(1);
            }
        }

        public async Task<Plugin.IDatabaseConnection> GetConnection()
        {
            return await Task.Run(() =>
            {
                this._iteratorLock.WaitOne();
                this._iteratorLock.Reset();
                while (true)
                {
                    for (int i = 0; i < this.ConnectionPool.Length; i++)
                    {
                        if (!this.ConnectionPool[i].IsBusy)
                        {
                            this.ConnectionPool[i].IsBusy = true;
                            this._iteratorLock.Set();
                            return this.ConnectionPool[i];
                        }
                    }
                    // check in 1 ms again
                    System.Threading.Thread.Sleep(1);
                }
            });
        }

        #endregion

        #region IDatabase implementation

        /// <summary>
        /// IDatabase.Authenticate implementation for MySQL driver
        /// </summary>
        public Task<Plugin.AuthenticationInformation> Authenticate(Plugin.IDatabaseConnection clientImpl, string username, string password)
        {
            VirtualConnection connection = clientImpl as VirtualConnection;

            var tableResult = connection
                .Command("SELECT acc.id, acc.username, acc.email, acc.balance, subs.subscriptionType, subs.paidDate FROM `accounts` AS acc JOIN `subscriptions` AS subs ON acc.id = subs.accountId WHERE acc.username = @username AND acc.password = MD5(CONCAT(@password, acc.salt)) AND subs.paid = TRUE ORDER BY subs.paidDate DESC LIMIT 0,1;")
                .SetParameter("@username", username)
                .SetParameter("@password", password)
                .ReadTable();

            connection.IsBusy = false;

            // return default authentication information (success = false)
            if (tableResult.Rows.Count == 0)
                return Task.FromResult(new Plugin.AuthenticationInformation(false));
            else
            {
                var row = tableResult.Rows[0];
                string subscriptionTypeName = row["subscriptionType"] as string;
                subscriptionTypeName = subscriptionTypeName[0].ToString().ToUpper() + subscriptionTypeName.Substring(1);
                return Task.FromResult(
                    new Plugin.AuthenticationInformation(
                        (long)(int)row["id"],
                        row["username"] as string,
                        row["email"] as string,
                        double.Parse(row["balance"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US")),
                        (Plugin.SubscriptionType)Enum.Parse(typeof(Plugin.SubscriptionType), subscriptionTypeName),
                        ((DateTime)row["paidDate"]).AddDays(30)
                    )
                );
            }
        }
        public async Task<Plugin.AuthenticationInformation> Authenticate(string username, string password)
        {
            Plugin.IDatabaseConnection connection = await this.GetConnection();
            return await this.Authenticate(connection, username, password);
        }

        /// <summary>
        /// IDatabase.CommitStats implementation for MySQL driver
        /// </summary>
        public Task<object> CommitStats(Plugin.IDatabaseConnection clientImpl, long accountId, long newTx, long newRx)
        {
            VirtualConnection connection = clientImpl as VirtualConnection;

            connection
                .Command("UPDATE `accounts` AS acc SET acc.totalTx = acc.totalTx + @newTx, acc.totalRx = acc.totalRx + @newRx WHERE acc.id = @accountId;")
                .SetParameter("@newTx", newTx)
                .SetParameter("@newRx", newRx)
                .SetParameter("@accountId", accountId)
                .Execute();

            connection.IsBusy = false;
            return Task.FromResult<object>(null);
        }
        public async Task<object> CommitStats(long accountId, long newTx, long newRx)
        {
            Plugin.IDatabaseConnection connection = await this.GetConnection();
            return await this.CommitStats(connection, accountId, newTx, newRx);
        }

        /// <summary>
        /// IDatabase.CommitConnection implementation for MySQL driver
        /// </summary>
        public Task<object> CommitCommand(Plugin.IDatabaseConnection clientImpl, long accountId, Socks.Constants.Command command, System.Net.IPEndPoint clientEndPoint, System.Net.IPEndPoint proxyEndPoint, long wiredTx, long wiredRx, DateTime commandTime, bool success)
        {
            VirtualConnection connection = clientImpl as VirtualConnection;

            uint sourceAddr = BitConverter.ToUInt32(clientEndPoint.Address.GetAddressBytes(), 0);
            uint targetAddr = BitConverter.ToUInt32(proxyEndPoint.Address.GetAddressBytes(), 0);

            connection
                .Command("INSERT INTO `commands` (`accountId`, `commandType`, `sourceAddr`, `sourcePort`, `targetAddr`, `targetPort`, `wiredTx`, `wiredRx`, `timestamp`, `success`) VALUES (@accountId, @command, @sourceAddr, @sourcePort, @targetAddr, @targetPort, @wiredTx, @wiredRx, @commandTime, @success);")
                .SetParameter("@accountId", accountId)
                .SetParameter("@command", command.ToString())
                .SetParameter("@sourceAddr", sourceAddr)
                .SetParameter("@sourcePort", clientEndPoint.Port)
                .SetParameter("@targetAddr", targetAddr)
                .SetParameter("@targetPort", proxyEndPoint.Port)
                .SetParameter("@wiredTx", wiredTx)
                .SetParameter("@wiredRx", wiredRx)
                .SetParameter("@commandTime", commandTime)
                .SetParameter("@success", success)
                .Execute();

            connection.IsBusy = false;
            return Task.FromResult<object>(null);
        }
        public async Task<object> CommitCommand(long accountId, Socks.Constants.Command command, System.Net.IPEndPoint clientEndPoint, System.Net.IPEndPoint proxyEndPoint, long wiredTx, long wiredRx, DateTime commandTime, bool success)
        {
            Plugin.IDatabaseConnection connection = await this.GetConnection();
            return await this.CommitCommand(connection, accountId, command, clientEndPoint, proxyEndPoint, wiredTx, wiredRx, commandTime, success);
        }

        #endregion

    }
}
