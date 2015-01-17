using System;
using System.Collections.Generic;
using System.Linq;
using AsyncTCPLib;
using System.Threading.Tasks;

namespace Socks5S.Socks
{
    public class ClientData
    {

        #region Members

        public readonly VirtualClient Owner;

        public Constants.AuthenticationState AuthenticationState { get; set; }
        public Constants.AuthenticationMethod AuthenticationMethod { get; set; }

        public Plugin.AuthenticationInformation AuthenticationInfo { get; private set; }

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// A client data instance is required on the client implementing AsyncTCPLib.AsyncVirtualClient
        /// It interacts as controller for authentication and proxy transmission
        /// </summary>
        /// <param name="owner"></param>
        internal ClientData(VirtualClient owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Query database to authenticate with given credentials
        /// </summary>
        /// <param name="username">Username of account to be checked</param>
        /// <param name="password">According password of user</param>
        /// <returns>True if authentication was successful, false if not</returns>
        public async Task<bool> Authenticate(string username, string password)
        {
            if (this.AuthenticationInfo.Success)
                return false;
            this.AuthenticationInfo = await Program.GetInstance().Database.Authenticate(username, password);
            return this.AuthenticationInfo.Success;
        }

        #endregion

    }
}
