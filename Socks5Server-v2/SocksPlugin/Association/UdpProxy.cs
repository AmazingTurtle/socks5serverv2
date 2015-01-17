using System;
using AsyncTCPLib;
using System.Net;
using System.Net.Sockets;

namespace Socks5S.SocksPlugin.Association
{
    public class UdpProxy : ICarrierData
    {

        #region Members

        /// <summary>
        /// Associated proxy client for data transmission
        /// </summary>
        public Client @Client { get; private set; }

        #endregion

        #region Constructor, Functions

        public UdpProxy(IPEndPoint remoteEndPoint, ProtocolType protocol, byte[] assignedBuffer)
        {
            this.Client = new Client(remoteEndPoint, protocol, assignedBuffer);
        }

        #endregion

    }
}
