using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsyncTCPLib
{
    public class Server<Client> where Client : VirtualClient
    {

        #region Members

        public IPEndPoint LocalEndPoint { get; private set; }
        public BufferManager @BufferManager { get; private set; }
        public int ClientLimit { get; private set; }

        public Socket ServerSocket { get; private set; }
        public bool IsRunning { get; private set; }

        private object _clientListLock;
        public Client[] Clients { get; private set; }

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Create a new generic asynchronous tcp server with limited number of clients and prereserved memory in a buffer manager
        /// </summary>
        /// <param name="localEndPoint">Server Address and port to bind and listen</param>
        /// <param name="bufferManager">Holds prereserved memory, is accessed by BufferManager[ClientID][0].Data</param>
        /// <param name="clientLimit">Must equal to the storage size of the buffer manager</param>
        /// <exception cref="System.ArgumentException">bufferManager.Storage.Length must equal to clientLimit</exception>
        public Server(IPEndPoint localEndPoint, BufferManager bufferManager, int clientLimit)
        {
            if (bufferManager.Storage.Length != clientLimit)
                throw new ArgumentException("bufferManager.Storage.Length must equal to clientLimit");

            this.LocalEndPoint = localEndPoint;
            this.BufferManager = bufferManager;
            this.ClientLimit = clientLimit;

            this.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.IsRunning = false;

            this._clientListLock = new Object();
            this.Clients = new Client[clientLimit];

            this.OnClientConnected = (s, e) => { };
            this.OnClientDisconnected = (s, e) => { };
            this.OnClientDataReceived = (s, e) => { };
        }

        /// <summary>
        /// Bind the server socket to the local end point and listen for connections with a backlog of 128
        /// </summary>
        public void Start()
        {
            this.ServerSocket.Bind(this.LocalEndPoint);
            this.ServerSocket.Listen(128);
            this.ServerSocket.BeginAccept(this._acceptTask, null);
        }

        /// <summary>
        /// Callback for asynchronous BeginAccept operation on server socket
        /// </summary>
        private void _acceptTask(IAsyncResult ar)
        {
            // accept connection
            Socket client = this.ServerSocket.EndAccept(ar);
            if (this.Clients.Where(x => x != null).Count() >= this.ClientLimit)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close(1000);
                return;
            }

            int freeIndex = Array.FindIndex(this.Clients, x => x == null);

            Client vclient = (Client)Activator.CreateInstance(typeof(Client), client, this.BufferManager[freeIndex].Data, freeIndex);
            lock (this._clientListLock)
                this.Clients[freeIndex] = vclient;

            // hook events
            vclient.OnClientDisconnected += (s, e) =>
            {
                this.OnClientDisconnected(this, new OnClientDisconnectedEventArgs<Client>(vclient));
                lock (this._clientListLock)
                    this.Clients[freeIndex] = null;
            };
            vclient.OnClientDataReceived += (s, e) =>
            {
                this.OnClientDataReceived(this, new OnClientDataReceivedEventArgs<Client>(vclient, e.Data, e.RemoteEndPoint));
            };

            // notify and begin receiving data
            this.OnClientConnected(this, new OnClientConnectedEventArgs<Client>(vclient));
            vclient.Begin();

            this.ServerSocket.BeginAccept(this._acceptTask, null);
        }

        #endregion

        #region Events

        public event OnClientConnectedDelegate<Client> OnClientConnected;
        public event OnClientDisconnectedDelegate<Client> OnClientDisconnected;
        public event OnClientDataReceivedDelegate<Client> OnClientDataReceived;

        #endregion

    }
}
