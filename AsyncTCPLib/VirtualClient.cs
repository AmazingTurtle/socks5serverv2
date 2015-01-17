using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace AsyncTCPLib
{
    public abstract class VirtualClient
    {

        #region Members

        public Socket @Socket { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool IsConnected { get; private set; }
        private object _disconnectLock;
        private byte[] buffer;
        public int Id { get; private set; }

        // speed throttle
        public Throttle Download { get; private set; }
        public Throttle Upload { get; private set; }
        public ThrottleMode @ThrottleMode { get; private set; }

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Create a new VirtualClient instance by Activator.CreateInstance
        /// </summary>
        /// <param name="client"><see cref="System.Net.Sockets.Socket"/> of VirtualClient</param>
        /// <param name="assignedMemory">Assigned memory for overlapped IO events</param>
        /// <param name="id">Client ID</param>
        public VirtualClient(Socket client, byte[] assignedMemory, int id, ThrottleMode mode = AsyncTCPLib.ThrottleMode.NoThrottle)
        {
            this.Socket = client;
            this.buffer = assignedMemory;
            this.Id = id;

            this._disconnectLock = new Object();
            this.IsConnected = true;
            this.RemoteEndPoint = (IPEndPoint)this.Socket.RemoteEndPoint;

            this.Download = new Throttle(); // 100 kbsp
            this.Upload = new Throttle();
            this.ThrottleMode = mode;

            this.OnClientDisconnected = (s, e) => { };
            this.OnClientDataReceived = (s, e) => { };
        }

        /// <summary>
        /// Disconnect the virtual client by remote host
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

                this.OnClientDisconnected(this, new OnClientDisconnectedEventArgs<VirtualClient>(this));

                try
                {
                    this.Socket.Shutdown(SocketShutdown.Both);
                    this.Socket.Close(1000);
                }
                catch { }
            }
        }

        /// <summary>
        /// Start the asynchronous BeginReceive operation on Client socket
        /// </summary>
        internal void Begin()
        {
            try
            {
                this.Socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, this._IReceiveCallback, null);
            }
            catch { this.Disconnect(); }
         }

        /// <summary>
        /// Send data to the remote client, if connected
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public async Task Send(byte[] data)
        {
            if (this.IsConnected)
            {
                try
                {
                    if (this.ThrottleMode.HasFlag(ThrottleMode.Download))
                    {
                        int throttleTime = this.Download.ThrottleTime(data.Length);
                        if (throttleTime > 0)
                        {
                            await Task.Delay(throttleTime);
                            Console.WriteLine("Throttling download: {0}", throttleTime);
                        }
                    }
                    this.Socket.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch { this.Disconnect(); }
            }
        }

        #region Callback

        /// <summary>
        /// Callback for asynchronous BeginReceive operation on Client socket
        /// </summary>
        private async void _IReceiveCallback(IAsyncResult ar)
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
                    this.OnClientDataReceived(this, new OnClientDataReceivedEventArgs<VirtualClient>(this, recv, this.RemoteEndPoint));
                // if we're still connected afterwards
                if (this.IsConnected)
                {
                    if (this.ThrottleMode.HasFlag(ThrottleMode.Upload))
                    {
                        // throttle
                        int throttleTime = this.Upload.ThrottleTime(bufferSize);
                        if (throttleTime > 0)
                        {
                            await Task.Delay(throttleTime);
                            Console.WriteLine("Throttling upload: {0}", throttleTime);
                        }
                    }
                    this.Begin();
                }
            }
            else
                this.Disconnect();
        }

        #endregion

        #endregion

        #region Events

        public event OnClientDisconnectedDelegate<VirtualClient> OnClientDisconnected;
        public event OnClientDataReceivedDelegate<VirtualClient> OnClientDataReceived;

        #endregion

    }
}
