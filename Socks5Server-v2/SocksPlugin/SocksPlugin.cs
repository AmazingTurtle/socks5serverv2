using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socks5S.SocksPlugin
{
    public class SocksPlugin : Plugin.IPlugin
    {
        #region Members

        /// <summary>
        /// Plugin related configuration
        /// </summary>
        public Config.Socks Config { get; private set; }

        private Association.Carrier[] _associations;
        private List<Commit.Info> _commitInfo;
        private ManualResetEvent _commitReset;
        private Thread _commitProcessor;

        #endregion

        #region General

        /// <summary>
        /// Name of the internal socks plugin
        /// </summary>
        public string Name
        {
            get { return "Basic Socks Plugin"; }
        }

        /// <summary>
        /// Lowest ranking to ensure this gets invoked first
        /// </summary>
        public ushort Ranking
        {
            get { return 0; }
        }

        /// <summary>
        /// Implementation of Init function from socks plugin
        /// </summary>
        /// <returns>True if initialization was successful, false if not</returns>
        public bool Init(Program instance)
        {
            this._associations = new Association.Carrier[instance.Server.ClientLimit];
            ((StateDependentHandler)this.StateDependentHandler).Init(this._associations, instance.Config.Server.ClientLimit, this.Config.ProxyDataSize);
            ((RawHandler)this.RawHandler).Init(this._associations);
            this._commitProcessor.Start();
            return true;
        }

        /// <summary>
        /// Create a socks plugin with according state dependent handler
        /// </summary>
        public SocksPlugin()
        {
            var mappedConfig = new MappedConfiguration<Config.Socks>();
            mappedConfig.Deserialize(CompileVars.ConfigDirectory + "/socks.xml");
            this.Config = mappedConfig.Data;

            this._commitInfo = new List<Commit.Info>();
            this._commitReset = new ManualResetEvent(true);
            this._commitProcessor = new Thread(this._processCommits);

            this.RawHandler = new RawHandler(this);
            this.StateDependentHandler = new StateDependentHandler();
        }

        public void CommitInfo(Commit.Info info)
        {
            this._commitReset.WaitOne();
            this._commitReset.Reset();

            this._commitInfo.Add(info);

            this._commitReset.Set();
        }

        private async void _processCommits()
        {
            while(true)
            {
                this._commitReset.WaitOne();
                this._commitReset.Reset();

                if (this._commitInfo.Count > 0)
                {

                    Program programInstance = Program.GetInstance();

                    Plugin.IDatabaseConnection connection = await programInstance.Database.GetConnection();

                    using (Plugin.IDatabaseTransaction transaction = connection.OpenTransaction())
                    {

                        programInstance.Log.DebugFormat("Processing commit info, {0} commits", this._commitInfo.Count);
                        int totalQueries = 0;

                        for (int i = this._commitInfo.Count - 1; i >= 0; i--)
                        {
                            Commit.Info info = this._commitInfo[i];

                            if (info.Stats.HasValue)
                            {
                                await programInstance.Database.CommitStats(connection, info.AccountId, info.Stats.Value.WiredTx, info.Stats.Value.WiredRx);
                                totalQueries++;
                            }
                            if (info.Command.HasValue)
                            {
                                await programInstance.Database.CommitCommand(connection, info.AccountId, info.Command.Value.SocksCommand, info.Command.Value.ClientEndPoint, info.Command.Value.ProxyEndPoint, info.Command.Value.WiredTx, info.Command.Value.WiredRx, info.Command.Value.CommandTime, info.Command.Value.Success);
                                totalQueries++;
                            }

                            programInstance.Log.DebugFormat("Pci > stats: {0}, command: {1} for {2}", info.Stats.HasValue, info.Command.HasValue, info.AccountId);

                            this._commitInfo.RemoveAt(i);
                        }

                        programInstance.Log.DebugFormat("Processing commit info, commiting {0} queries to database impl", totalQueries);
                        transaction.Commit();
                    }

                    connection.IsBusy = false;
                }

                this._commitReset.Set();
                Thread.Sleep(2500); // process all 2.5 seconds
            }
        }

        #endregion

        #region Plugin

        public Plugin.IRawHandler RawHandler { get; private set; }

        public Plugin.IStateDependentHandler StateDependentHandler { get; private set; }

        #endregion

    }
}
