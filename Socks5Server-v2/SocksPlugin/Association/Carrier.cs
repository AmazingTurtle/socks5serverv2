using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Socks5S.SocksPlugin.Association
{
    public class Carrier
    {

        #region Members

        public Socks.Constants.Command Command { get; private set; }
        public DateTime CommandTime { get; private set; }
        public bool CommandSuccess { get; set; }
        public ICarrierData Data { get; private set; }

        public long WiredTx { get; set; }
        public long WiredRx { get; set; }

        #endregion

        #region Constructor, Functions

        public Carrier(Socks.Constants.Command command, params object[] args)
        {
            this.Command = command;
            this.CommandTime = DateTime.Now;
            if (command == Socks.Constants.Command.Connect)
            {
                this.Data = new ConnectProxy(
                    args[0] as IPEndPoint,
                    (ProtocolType)args[1],
                    args[2] as byte[]
                    );
            }
            else if(command == Socks.Constants.Command.UdpAssociate)
            {
                this.Data = new UdpProxy(
                    args[0] as IPEndPoint,
                    (ProtocolType)args[1],
                    args[2] as byte[]
                );
            }
            else
                throw new NotImplementedException("Did not implement Carrier type " + command.ToString());
        }

        /// <summary>
        /// Transmission for proxy sockeet from client socket
        /// </summary>
        /// <param name="data">Data from client socket to proxy socket</param>
        public void Transmission(byte[] data)
        {
            if(this.Command == Socks.Constants.Command.Connect)
            {
                ConnectProxy userData = (ConnectProxy)this.Data;
                if (userData.Client.IsConnected)
                    userData.Client.Send(data);
                else
                    return;
            }
            this.WiredTx += data.Length;
        }

        #endregion

    }
}
