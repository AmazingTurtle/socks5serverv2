using System;
using AsyncTCPLib;
using System.Net;
using System.Net.Sockets;

namespace Socks5S.Socks
{
    public class Client : VirtualClient
    {

        public readonly ClientData Data;

        public Client(Socket client, byte[] assignedMemory, int id)
            : base(client, assignedMemory, id, ThrottleMode.Download)
        {
            this.Data = new ClientData(this);
        }

    }
}
