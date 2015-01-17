using System;
using AsyncTCPLib;

namespace Socks5S.Plugin
{
    public interface IRawHandler
    {

        #region Raw event handler

        /// <summary>
        /// EventHandler for AsyncServer.OnClientDataReceived
        /// </summary>
        /// <returns>False if client should be disconnected, true if not</returns>
        bool OnClientDataReceived(OnClientDataReceivedEventArgs<VirtualClient> e);

        /// <summary>
        /// EventHandler for AsyncServer.OnClientConnected
        /// </summary>
        /// <returns>False if client should be disconnected, true if not</returns>
        bool OnClientConnected(OnClientConnectedEventArgs<VirtualClient> e);

        /// <summary>
        /// EventHandler for AsyncServer.OnClientDisconnected
        /// </summary>
        /// <param name="e"></param>
        void OnClientDisconnected(OnClientDisconnectedEventArgs<VirtualClient> e);

        #endregion

    }
}
