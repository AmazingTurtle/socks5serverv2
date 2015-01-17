using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace AsyncTCPLib
{
    public class Client
    {

        #region Members

        public Socket @Socket { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool IsConnected { get; private set; }
        private object _disconnectLock;
        private byte[] buffer;
        private EndPoint _receiveEndPoint;

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Create a new TaskClient instance to send data to connections (tcp) or to a remote host (udp)
        /// </summary>
        /// <param name="remoteEndPoint">(Default) remote endpoint to connect or send data to</param>
        /// <param name="protocol">TCP or UDP protocol for client socket supported</param>
        public Client(IPEndPoint remoteEndPoint, ProtocolType protocol)
        {
            if (protocol == ProtocolType.Tcp)
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            else if (protocol == ProtocolType.Udp)
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            else
                throw new Exception("Only TCP and UDP protocol allowed");

            this.RemoteEndPoint = remoteEndPoint;
            this._disconnectLock = new Object();
            this.buffer = new byte[8192];

            this.OnClientConnected = (s, e) => { };
            this.OnClientDisconnected = (s, e) => { };
            this.OnClientDataReceived = (s, e) => { };
        }

        /// <summary>
        /// Create a new TaskClient instance to send data to connections (tcp) or to a remote host (udp)
        /// </summary>
        /// <param name="remoteEndPoint">(Default) remote endpoint to connect or send data to</param>
        /// <param name="protocol">TCP or UDP protocol for client socket supported</param>
        public Client(IPEndPoint remoteEndPoint, ProtocolType protocol, byte[] assignedBuffer)
        {
            if (protocol == ProtocolType.Tcp)
                this.Socket = new Socket(remoteEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            else if (protocol == ProtocolType.Udp)
                this.Socket = new Socket(remoteEndPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            else
                throw new Exception("Only TCP and UDP protocol allowed");

            this.RemoteEndPoint = remoteEndPoint;
            this._disconnectLock = new Object();
            this.buffer = assignedBuffer;

            this.OnClientConnected = (s, e) => { };
            this.OnClientDisconnected = (s, e) => { };
            this.OnClientDataReceived = (s, e) => { };
        }

        /// <summary>
        /// Disconnect the client from remote host
        /// </summary>
        public void Disconnect()
        {
            if (!this.IsConnected)
                return;

            lock (_disconnectLock)
            {
                if (!this.IsConnected)
                    return;

                this.IsConnected = false;

                this.OnClientDisconnected(this, new OnClientDisconnectedEventArgs<Client>(this));
                try
                {
                    this.Socket.Shutdown(SocketShutdown.Both);
                    this.Socket.Close(1000);
                }
                catch { }
            }
        }

        /// <summary>
        /// Connect to the remote host and begin receiving data on it
        /// </summary>
        /// <returns>True if connection was successful, false if not</returns>
        public bool Connect()
        {
            bool connectSuccess = false;
            try
            {

                if (this.Socket.ProtocolType == ProtocolType.Tcp)
                {
                    this.Socket.Connect(this.RemoteEndPoint);
                    connectSuccess = true;
                }
                else if (this.Socket.ProtocolType == ProtocolType.Udp)
                {
                    this.Socket.Bind(this.RemoteEndPoint);
                    connectSuccess = true;
                }
            }
            catch { }

            if (connectSuccess)
            {
                this.IsConnected = true;
                this.OnClientConnected(this, new OnClientConnectedEventArgs<Client>(this));
                this.Begin();
            }
            else
                this.OnClientDisconnected(this, new OnClientDisconnectedEventArgs<Client>(this));

            return connectSuccess;
        }

        /// <summary>
        /// Start the asynchronous BeginReceive operation on Client socket
        /// </summary>
        private void Begin()
        {
            try
            {
                if (this.Socket.ProtocolType == ProtocolType.Tcp)
                    this.Socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, this._IReceiveCallback, null);
                else if (this.Socket.ProtocolType == ProtocolType.Udp)
                {
                    this._receiveEndPoint = new IPEndPoint(this.RemoteEndPoint.Address, this.RemoteEndPoint.Port);
                    this.Socket.BeginReceiveFrom(this.buffer, 0, this.buffer.Length, SocketFlags.None, ref this._receiveEndPoint, this._IReceiveUDPCallback, null);
                }
            }
            catch { this.Disconnect(); }
        }

        /// <summary>
        /// Send data to the remote host, if connected
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void Send(byte[] data)
        {
            if (this.IsConnected)
            {
                try
                {
                    this.Socket.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch { this.Disconnect(); }
            }
        }

        /// <summary>
        /// Send data to a remote endpoint (UDP only)
        /// </summary>
        /// <param name="data">Data to be sent</param>
        /// <param name="remoteEndPoint">Target host to send data to</param>
        public void SendTo(byte[] data, IPEndPoint remoteEndPoint)
        {
            this.Socket.SendTo(data, 0, data.Length, SocketFlags.None, remoteEndPoint);
        }

        #region Callback

        /// <summary>
        /// Callback for asynchronous BeginReceive operation on Client socket
        /// </summary>
        private void _IReceiveCallback(IAsyncResult ar)
        {
            int bufferSize = 0;
            try
            {
                bufferSize = this.Socket.EndReceive(ar);
            }
            catch (ObjectDisposedException) { return; }
            catch (SocketException) { this.Disconnect(); }
            if (bufferSize > 0)
            {
                byte[] recv = new byte[bufferSize];
                Array.Copy(buffer, 0, recv, 0, recv.Length);
                // only fire callback if we're truly connected!
                if (this.IsConnected)
                    this.OnClientDataReceived(this, new OnClientDataReceivedEventArgs<Client>(this, recv, this.RemoteEndPoint));
                // if we're still connected afterwards
                if (this.IsConnected)
                    this.Begin();
            }
            else
                this.Disconnect();
        }

        /// <summary>
        /// Callback for asynchronous BeginReceiveFrom operation on Client socket
        /// </summary>
        private void _IReceiveUDPCallback(IAsyncResult ar)
        {
            int bufferSize = 0;
            try
            {
                bufferSize = this.Socket.EndReceiveFrom(ar, ref this._receiveEndPoint);
            }
            catch (ObjectDisposedException) { return; }
            catch (SocketException) { this.Disconnect(); }
            if (bufferSize > 0)
            {
                byte[] recv = new byte[bufferSize];
                Array.Copy(buffer, 0, recv, 0, recv.Length);
                // only fire callback if we're truly connected!
                if (this.IsConnected)
                    this.OnClientDataReceived(this, new OnClientDataReceivedEventArgs<Client>(this, recv, (IPEndPoint)this._receiveEndPoint));
                // if we're still connected afterwards
                if (this.IsConnected)
                    this.Begin();
            }
            else
                this.Disconnect();
        }

        #endregion

        #region Events

        public event OnClientConnectedDelegate<Client> OnClientConnected;
        public event OnClientDisconnectedDelegate<Client> OnClientDisconnected;
        public event OnClientDataReceivedDelegate<Client> OnClientDataReceived;

        #endregion

        #endregion

    }
}
