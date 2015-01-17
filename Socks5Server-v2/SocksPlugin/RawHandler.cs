using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.SocksPlugin
{
    public class RawHandler : Plugin.IRawHandler
    {

        #region Members

        private SocksPlugin _plugin;
        private Association.Carrier[] _associations;

        #endregion

        #region General

        public RawHandler(SocksPlugin plugin)
        {
            this._plugin = plugin;
        }

        public void Init(Association.Carrier[] _associatedProxyData)
        {
            this._associations = _associatedProxyData;
        }

        #endregion

        #region Raw event handler

        public bool OnClientDataReceived(AsyncTCPLib.OnClientDataReceivedEventArgs<AsyncTCPLib.VirtualClient> e) { return true; }

        public bool OnClientConnected(AsyncTCPLib.OnClientConnectedEventArgs<AsyncTCPLib.VirtualClient> e) { return true; }

        public void OnClientDisconnected(AsyncTCPLib.OnClientDisconnectedEventArgs<AsyncTCPLib.VirtualClient> e)
        {
            Association.Carrier userCarrier = this._associations[e.Client.Id];
            if (userCarrier != null)
            {
                long accountId = Program.GetInstance().Server.Clients[e.Client.Id].Data.AccountId;
                if (userCarrier.Command == Socks.Constants.Command.Connect)
                {
                    Association.ConnectProxy proxy = (Association.ConnectProxy)userCarrier.Data;
                    Commit.Info info = new Commit.Info(accountId);
                    if(userCarrier.WiredTx > 0 ||
                        userCarrier.WiredRx > 0)
                    {
                        info.Stats = new Commit.Stats(userCarrier.WiredTx, userCarrier.WiredRx);
                    }
                    info.Command = new Commit.Command(
                        userCarrier.Command,
                        e.Client.RemoteEndPoint,
                        proxy.Client.RemoteEndPoint,
                        userCarrier.WiredTx,
                        userCarrier.WiredRx,
                        userCarrier.CommandTime,
                        userCarrier.CommandSuccess
                    );
                    this._plugin.CommitInfo(info);
                }
                this._associations[e.Client.Id] = null;
            }
        }

        #endregion

    }
}
