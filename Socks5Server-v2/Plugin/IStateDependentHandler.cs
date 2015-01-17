using System;
using System.Threading.Tasks;
using AsyncTCPLib;

namespace Socks5S.Plugin
{
    public interface IStateDependentHandler
    {

        #region State dependent event handler

        /// <summary>
        /// EventHandler for Socks.Client.AuthenticationState == Awaiting
        /// </summary>
        /// <returns>True if connection left open, false if client should be disconnected</returns>
        Task<bool> Awaiting(Socks.Client client, ISocksMessage message);

        /// <summary>
        /// EventHandler for Socks.Client.AuthenticationState == Authenticating
        /// </summary>
        /// <returns>True if connection left open, false if client should be disconnected</returns>
        Task<bool> Authenticating(Socks.Client client, ISocksMessage message);

        /// <summary>
        /// EventHandler for Socks.Client.AuthenticationState == Authenticated
        /// </summary>
        /// <returns>True if connection left open, false if client should be disconnected</returns>
        Task<bool> Command(Socks.Client client, ISocksMessage message);

        /// <summary>
        /// EventHandler for Socks.Client.AuthenticationState == Transmitting
        /// </summary>
        /// <returns>True if connection left open, false if client should be disconnected</returns>
        Task<bool> Transmission(Socks.Client client, byte[] message);


        #endregion

    }
}
