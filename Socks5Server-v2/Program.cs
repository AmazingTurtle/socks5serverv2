using System;
using System.Collections.Generic;
using System.Linq;
using AsyncTCPLib;
using log4net;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Net;
using System.Threading;

namespace Socks5S
{
    public class Program
    {

        #region Initialization

        /// <summary>
        /// Create the one and only Program instance
        /// </summary>
        private Program()
        {
            this.Log = LogManager.GetLogger(typeof(Program));
            // logging is required for unhandled exception filter
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            this.Log.Info("Socks5 server [TurtleDev] - v2 starting");

            // Create plugin loader
            this.PluginLoader = new Plugin.IPluginLoader[] {
                new Plugin.Default.FilePluginLoader(),
                new Plugin.Default.AssemblyPluginLoader()
            };

            // Create a socks message factory
            this.SocksMessageFactory = new Socks.SocksMessageFactory();

            // Load mapped configuration for Configuration.Configuration
            {
                this.Log.Info("Loading configuration");
                var mappedConfig = new MappedConfiguration<Configuration.Configuration>();
                mappedConfig.Deserialize(CompileVars.ConfigDirectory + "/configuration.xml");
                this.Config = mappedConfig.Data;
            }

            // Create a database implementation (depends on configuration)
            if (this.Config.Database.Driver == "mysql")
                this.Database = new Database.MySql.Impl(this.Config.Database);
            else
                throw new NotImplementedException("Database Driver '" + this.Config.Database.Driver + "' not implemented");

            // Create a buffer manager for async tcp server
            this.Log.InfoFormat("Creating buffer manager with {0}*{1} bytes",
                this.Config.Server.ClientLimit,
                this.Config.Memory.ClientDataSize);

            // ... and initialize it
            this.BufferManager = new BufferManager(this.Config.Server.ClientLimit, 0);
            this.BufferManager.InitSubsequent(0, this.Config.Memory.ClientDataSize);

            // part below isn't required anymore <- ruled by SockPlugin.SocksPlugin and SocksPlugin.AssociatedProxy
            //this.BufferManager.Storage.ForEach(x => x.InitSubsequent(0, this.Config.Memory.ClientProxyDataSize).All(y => y != null));

            // Create local IPEndPoint for async tcp server and write a info message
            IPEndPoint localEndPoint = new IPEndPoint(
                    IPAddress.Parse(this.Config.Server.Address),
                    this.Config.Server.Port);

            this.Log.InfoFormat("Creating server socket on {0}", localEndPoint);

            // Create the async tcp server with Socks.Client as generic virtual client type, and add event handlers
            this.Server = new Server<Socks.Client>(
                localEndPoint,
                this.BufferManager,
                this.Config.Server.ClientLimit);

            this.Server.OnClientConnected += OnClientConected;
            this.Server.OnClientDataReceived += OnClientDataReceived;
            this.Server.OnClientDisconnected += OnClientDisconnected;

            // Everything was created. Nice huh?
            Console.Title = "Socks5 server [TurtleDev] - v2";
        }

        #region Singleton

        private static Program _instance;
        private static object _instanceLock = new Object();
        public static Program GetInstance()
        {
            if (_instance == null)
            {
                lock(_instanceLock)
                {
                    if (_instance == null)
                        _instance = new Program();
                }
            }
            return _instance;
        }

        #endregion

        #region Run

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Process start parameters</param>
        static void Main(string[] args)
        {
            Program.GetInstance().Run();
        }

        /// <summary>
        /// Run the program - only once
        /// </summary>
        internal void Run()
        {
            // Create connection pool and connect virtual clients for database implementation
            this.Log.Info("Post initializing database implementation");
            this.Database.Init();
            this.Log.Info("Database implementation initialized");

            /* Well... this is kinda tricky...
             * There can be multiple plugin loaders, by default a FilePluginLoader and an AssemblyPluginLoader
             * So what I'm doing below is: get all plugins from each plugin loaders and concatenate results 
             * Then I order plugins ascending by their ranking and initialize them in that order.
             */
            this.Log.Info("Loading plugins...");
            {
                // Load plugins with each plugin loader and concentate
                IEnumerable<Plugin.IPlugin> plugins = new Plugin.IPlugin[] { };
                this.PluginLoader.ForEach(x => plugins = plugins.Concat(x.LoadPlugins(this.Config)));
                // concatenate all plugins loaded by each plugin loader
                foreach (Plugin.IPluginLoader loader in this.PluginLoader)
                    plugins = plugins.Concat(loader.LoadPlugins(this.Config));
                // orderby ranking ascending to get correct invocation stack
                this.Plugins = (from Plugin.IPlugin plugin in plugins orderby plugin.Ranking ascending select plugin).ToArray();
                foreach (Plugin.IPlugin plugin in this.Plugins)
                    plugin.Init(this);
            }
            this.Log.InfoFormat("{0} plugin(s) loaded", this.Plugins.Length);

            // Start the server and wait for connections
            this.Server.Start();
            this.Log.Info("It's up, Jim! Socks5 server is running gracefully");

            // Memory pressure grows and grows... We may clean up manually, to be secure - not sure if this is stupid or not
            while (true)
            {
                GC.Collect();
                Thread.Sleep(10 * 1000);
            }
        }

        #endregion

        #endregion

        #region Event Handler

        /// <summary>
        /// EventHandler for AsyncTCPLib.Server.OnClientConected
        /// </summary>
        private void OnClientConected(object sender, OnClientConnectedEventArgs<Socks.Client> e)
        {
            //this.Log.DebugFormat("Connected {0}", e.Client.RemoteEndPoint);
        }

        /// <summary>
        /// EventHandler for AsyncTCPLib.Server.OnClientDisconnected
        /// </summary>
        private void OnClientDisconnected(object sender, OnClientDisconnectedEventArgs<Socks.Client> e)
        {
            var localEvent = new OnClientDisconnectedEventArgs<VirtualClient>(e.Client);
            foreach (Plugin.IPlugin plugin in this.Plugins)
            {
                if (plugin.HasRawHandler())
                    plugin.RawHandler.OnClientDisconnected(localEvent);
            }
        }

        /// <summary>
        /// EventHandler for AsyncTCPLib.Server.OnClientDataReceived
        /// </summary>
        private async void OnClientDataReceived(object sender, OnClientDataReceivedEventArgs<Socks.Client> e)
        {
            var localEvent = new OnClientDataReceivedEventArgs<VirtualClient>(e.Client, e.Data, e.RemoteEndPoint);

                
            Socks.Constants.AuthenticationState currentState = e.Client.Data.AuthenticationState;
            Plugin.ISocksMessage message = this.GetMessage(e.Data, currentState, e.Client.Data.AuthenticationMethod);

            if (currentState != Socks.Constants.AuthenticationState.Transmitting && message == null)
            {
                this.Log.Debug("Received invalid data from ");
                e.Client.Disconnect();
                return;
            }

            // go through each plugin to process data
            foreach (Plugin.IPlugin plugin in this.Plugins)
            {
                if (plugin.HasRawHandler())
                    plugin.RawHandler.OnClientDataReceived(localEvent);
                if (plugin.HasStateDependentHandler())
                {
                    // AuthenticationState.Awaiting
                    if (currentState == Socks.Constants.AuthenticationState.Awaiting)
                    {
                        if (!await plugin.StateDependentHandler.Awaiting(e.Client, message))
                        {
                            e.Client.Disconnect();
                            return;
                        }
                    }
                    // AuthenticationState.Authenticating, awaiting sub negotiation
                    else if (currentState == Socks.Constants.AuthenticationState.Authenticating)
                    {
                        if (!await plugin.StateDependentHandler.Authenticating(e.Client, message))
                        {
                            e.Client.Disconnect();
                            return;
                        }
                    }
                    // AuthenticationState.Authenticated
                    else if (currentState == Socks.Constants.AuthenticationState.Authenticated)
                    {
                        if (!await plugin.StateDependentHandler.Command(e.Client, message))
                        {
                            e.Client.Disconnect();
                            return;
                        }
                    }
                    else if (currentState == Socks.Constants.AuthenticationState.Transmitting)
                    {
                        if (!await plugin.StateDependentHandler.Transmission(e.Client, e.Data))
                        {
                            e.Client.Disconnect();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// EventHandler for AppDomain.CurrentDomain.UnhandledException
        /// </summary>
        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            int errorCode = (ex != null ? ex.HResult : int.MaxValue);
            this.Log.FatalFormat("Got an unhandled exception 0x{0:X8} (IsTerminating: {1})\r\n{2}", errorCode, e.IsTerminating, e.ExceptionObject.ToString());
            Environment.Exit(errorCode);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Create a message by a clients authentication state and authentication method
        /// </summary>
        /// <param name="data">Input data received from client</param>
        /// <param name="authenticationState">Clients current authentication state</param>
        /// <param name="authenticationMethod">If authentication state is Authenticating, a authenticationMethod is required to return a message according to sub negotation</param>
        /// <returns>A socks message, or null if Parsing failed</returns>
        private Plugin.ISocksMessage GetMessage(byte[] data, Socks.Constants.AuthenticationState authenticationState, Socks.Constants.AuthenticationMethod authenticationMethod)
        {
            Plugin.ISocksMessage message = null;

            using(MemoryStream stream = new MemoryStream(data))
            using(BinaryReader reader = new BinaryReader(stream))
            {
                if (authenticationState == Socks.Constants.AuthenticationState.Awaiting)
                {
                    message = this.SocksMessageFactory.Create<Socks.Message.Awaiting>(reader);
                }
                else if (authenticationState == Socks.Constants.AuthenticationState.Authenticating &&
                    authenticationMethod == Socks.Constants.AuthenticationMethod.UsernamePassword)
                {
                    message = this.SocksMessageFactory.Create<Socks.Message.Authentication.UsernamePassword>(reader);
                }
                else if (authenticationState == Socks.Constants.AuthenticationState.Authenticated)
                {
                    message = this.SocksMessageFactory.Create<Socks.Message.Command>(reader);
                }
            }

            return message;
        }

        #endregion

        #region Members

        /// <summary>
        /// Configuration deserialized from configuration.xml
        /// </summary>
        public readonly Configuration.Configuration Config;

        /// <summary>
        /// log4net ILog implementation from Logger.GetInstance(typeof(Program))
        /// </summary>
        public readonly ILog Log;

        /// <summary>
        /// Set of IPluginLoader implementations to load plugins on program start
        /// </summary>
        public Plugin.IPluginLoader[] PluginLoader { get; private set; }

        /// <summary>
        /// Concatenated set of IPlugin implementations from IPluginLoader implementations
        /// </summary>
        public Plugin.IPlugin[] Plugins { get; private set; }

        /// <summary>
        /// The socks message factory provides a generic Create function to create and parse socks messages
        /// </summary>
        public Plugin.ISocksMessageFactory SocksMessageFactory { get; private set; }

        /// <summary>
        /// Database driver implementation
        /// </summary>
        public Plugin.IDatabase Database { get; private set; }

        /// <summary>
        /// Holds buffer for asynchronous IO operation in (virtual) clients
        /// </summary>
        public BufferManager @BufferManager { get; private set; }

        /// <summary>
        /// Proxy server for incoming connections and proxy traffic.
        /// Manages connections and data transfer
        /// </summary>
        public Server<Socks.Client> @Server { get; private set; }

        #endregion

    }
}
