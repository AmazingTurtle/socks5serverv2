using System;
using System.Net;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public interface IDatabase
    {

        /// <summary>
        /// Post initializing database implementation
        /// </summary>
        void Init();

        /// <summary>
        /// Get a database connection for further interactions
        /// </summary>
        /// <returns>A valid database connection</returns>
        Task<IDatabaseConnection> GetConnection();

        /// <summary>
        /// Try to authenticate using the username/password method
        /// </summary>
        /// <param name="username">Username of user to be checked</param>
        /// <param name="password">According password of user to be checked</param>
        /// <returns>-1 if username/password combination is incorrect, > 0 if success: user id</returns>
        Task<AuthenticationInformation> Authenticate(string username, string password);
        Task<AuthenticationInformation> Authenticate(IDatabaseConnection clientImpl, string username, string password);

        /// <summary>
        /// Update tx/rx stats for user with according accountId
        /// </summary>
        /// <param name="accountId">According account id of user</param>
        /// <param name="newTx">Transfered bytes during transmission</param>
        /// <param name="newRx">Received bytes during transmission</param>
        /// <returns>null</returns>
        Task<object> CommitStats(long accountId, long newTx, long newRx);
        Task<object> CommitStats(IDatabaseConnection clientImpl, long accountId, long newTx, long newRx);

        /// <summary>
        /// Add a connection log in database
        /// </summary>
        /// <param name="accountId">According account id of user</param>
        /// <param name="clientEndPoint">RemoteEndPoint of the users active connection</param>
        /// <param name="proxyEndPoint">RemoteEndPoint of the proxy connection made by user</param>
        /// /// <returns>null</returns>
        Task<object> CommitCommand(long accountId, Socks.Constants.Command command, IPEndPoint clientEndPoint, IPEndPoint proxyEndPoint, long wiredTx, long wiredRx, DateTime commandTime, bool success);
        Task<object> CommitCommand(IDatabaseConnection clientImpl, long accountId, Socks.Constants.Command command, IPEndPoint clientEndPoint, IPEndPoint proxyEndPoint, long wiredTx, long wiredRx, DateTime commandTime, bool success);

    }
}
