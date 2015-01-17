using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTCPLib
{

    #region EventArgs

    public class OnClientDisconnectedEventArgs<T> : EventArgs
    {

        public T Client { get; private set; }

        public OnClientDisconnectedEventArgs(T client)
            : base()
        {
            this.Client = client;
        }

    }

    public class OnClientConnectedEventArgs<T>  : EventArgs
    {

        public T Client { get; private set; }

        public OnClientConnectedEventArgs(T client)
            : base()
        {
            this.Client = client;
        }

    }

    public class OnClientDataReceivedEventArgs<T> : EventArgs
    {

        public T Client { get; private set; }
        public byte[] Data { get; private set; }
        public System.Net.IPEndPoint RemoteEndPoint { get; private set; }

        public OnClientDataReceivedEventArgs(T client, byte[] data, System.Net.IPEndPoint remoteEndPoint)
            : base()
        {
            this.Client = client;
            this.Data = data;
            this.RemoteEndPoint = remoteEndPoint;
        }

    }

    #endregion

    #region Delegates

    public delegate void OnClientDisconnectedDelegate<T>(object sender, OnClientDisconnectedEventArgs<T> e);
    public delegate void OnClientConnectedDelegate<T>(object sender, OnClientConnectedEventArgs<T> e);
    public delegate void OnClientDataReceivedDelegate<T>(object sender, OnClientDataReceivedEventArgs<T> e);

    #endregion

}
