using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Socks5S.Socks;
using Socks5S.Socks.Message;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Socks5S.SocksPlugin
{
    public class StateDependentHandler : Plugin.IStateDependentHandler
    {

        #region Members

        private Association.Carrier[] _associations;

        private AsyncTCPLib.BufferManager _bufferManager;

        #endregion

        #region General

        internal void Init(Association.Carrier[] _associatedProxyData, int clientLimit, int bufferSize)
        {
            this._associations = _associatedProxyData;
            this._bufferManager = new AsyncTCPLib.BufferManager(clientLimit, 0);
            this._bufferManager.InitSubsequent(0, bufferSize);
        }

        #endregion

        #region State dependent event handler

        /// <summary>
        /// IStateDependentHandler.Awaiting implementation for AuthenticationState.Awaiting
        /// </summary>
        /// <returns>True if connection left open, false if not</returns>
        Task<bool> Plugin.IStateDependentHandler.Awaiting(Client client, Plugin.ISocksMessage message)
        {
            Awaiting typedMessage = message as Awaiting;
            if (typedMessage.SocksVersion == Socks.Constants.Socks.Version)
            {
                // TODO: Make other authentication methods available. Not?
                if (typedMessage.AvailableMethods.Contains(Socks.Constants.AuthenticationMethod.UsernamePassword))
                {
                    client.Data.AuthenticationMethod = Socks.Constants.AuthenticationMethod.UsernamePassword;
                    client.Data.AuthenticationState = Socks.Constants.AuthenticationState.Authenticating;
                    client.Send(new byte[] { Socks.Constants.Socks.Version, (byte)client.Data.AuthenticationMethod });
                    return Task.FromResult(true);
                }
                else
                    client.Send(new byte[] { Socks.Constants.Socks.Version, (byte)Socks.Constants.AuthenticationMethod.NoAcceptableMethod });
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// IStateDependentHandler.Authenticating implementation for AuthenticationState.Authenticating
        /// </summary>
        /// <returns>True if connection left open, false if not</returns>
        async Task<bool> Plugin.IStateDependentHandler.Authenticating(Client client, Plugin.ISocksMessage message)
        {
            if (client.Data.AuthenticationMethod == Socks.Constants.AuthenticationMethod.UsernamePassword)
            {
                Socks5S.Socks.Message.Authentication.UsernamePassword typedMessage = message as Socks5S.Socks.Message.Authentication.UsernamePassword;
                if (typedMessage.Version == 1)
                {
                    if (await client.Data.Authenticate(typedMessage.Username, typedMessage.Password))
                    {
                        // 0x00 means success, everything else is failure
                        Program.GetInstance().Log.DebugFormat("Authentication (Method: {0}) successful for {1}: {2}", client.Data.AuthenticationMethod, client.RemoteEndPoint, typedMessage.Username);
                        client.Data.AuthenticationState = Socks.Constants.AuthenticationState.Authenticated;
                        client.Send(new byte[] { typedMessage.Version, 0x00 });
                        return true;
                    }
                }
                else
                    Program.GetInstance().Log.DebugFormat("Authentication (Method: {0}) failed for {1}: {2}, invalid version {3}", client.Data.AuthenticationMethod, client.RemoteEndPoint, typedMessage.Username, typedMessage.Version);
            }
            else
                throw new NotImplementedException("AuthenticationMethod " + client.Data.AuthenticationMethod.ToString() + " not implemented yet");

            Program.GetInstance().Log.DebugFormat("Authentication (Method: {0}) failed for {1}", client.Data.AuthenticationMethod, client.RemoteEndPoint);
            client.Send(new byte[] { Socks.Constants.Socks.Version, 0xFF });
            return false;
        }

        /// <summary>
        /// IStateDependentHandler.Command implementation for AuthenticationState.Authenticated
        /// </summary>
        /// <returns>True if connection left open, false if not</returns>
        async Task<bool> Plugin.IStateDependentHandler.Command(Client client, Plugin.ISocksMessage message)
        {
            bool returnData = false;

            Socks5S.Socks.Message.Command typedMessage = message as Socks5S.Socks.Message.Command;
            if (typedMessage.SocksCommand != Socks.Constants.Command.Connect)
                Program.GetInstance().Log.Info("NICE");
            if (typedMessage.SocksVersion == Socks.Constants.Socks.Version)
            {

                IPEndPoint remoteEndPoint = new IPEndPoint(typedMessage.DestinationAddress, typedMessage.DestinationPort);

                using(MemoryStream responseStream = new MemoryStream())
                using(BinaryWriter responseWriter = new BinaryWriter(responseStream))
                {
                    if(typedMessage.AddressType == Socks.Constants.AddressType.Domain &&
                        typedMessage.DestinationAddress == null)
                    {
                        responseWriter.Write(Socks.Constants.Socks.Version);
                        responseWriter.Write((byte)Socks.Constants.Reply.HostUnreachable);
                        responseWriter.Write((byte)0);
                        responseWriter.Write((byte)Socks.Constants.AddressType.Domain);
                        responseWriter.Write((byte)typedMessage.Domain.Length);
                        responseWriter.Write(Encoding.UTF8.GetBytes(typedMessage.Domain));
                        responseWriter.Write((short)IPAddress.HostToNetworkOrder(typedMessage.DestinationPort));
                        responseWriter.Flush();

                        byte[] responseHostUnreachableData = responseStream.GetBuffer();
                        Array.Resize(ref responseHostUnreachableData, (int)responseStream.Length);

                        client.Send(responseHostUnreachableData);
                    }
                    else
                    {
                        if (this._associations[client.Id] == null)
                        {
                            Association.Carrier carrier = null;

                            try
                            {
                                if(typedMessage.SocksCommand == Socks.Constants.Command.Connect)
                                    carrier = new Association.Carrier(Socks.Constants.Command.Connect, remoteEndPoint, ProtocolType.Tcp, this._bufferManager[client.Id].Data);
                                else if(typedMessage.SocksCommand == Socks.Constants.Command.UdpAssociate)
                                    carrier = new Association.Carrier(Socks.Constants.Command.UdpAssociate, remoteEndPoint, ProtocolType.Udp, this._bufferManager[client.Id].Data);
                            }
                            catch(NotImplementedException)
                            {
                                responseWriter.Write(Socks.Constants.Socks.Version);
                                responseWriter.Write((byte)Socks.Constants.Reply.CommandNotSupported);
                                responseWriter.Write((byte)0);
                                responseWriter.Write((byte)(typedMessage.AddressType == Socks.Constants.AddressType.Domain ? Socks.Constants.AddressType.IPv4 : typedMessage.AddressType));
                                responseWriter.Write(typedMessage.DestinationAddress.GetAddressBytes());
                                responseWriter.Write((short)IPAddress.HostToNetworkOrder(typedMessage.DestinationPort));
                                responseWriter.Flush();

                                byte[] errorResponse = responseStream.GetBuffer();
                                Array.Resize(ref errorResponse, (int)responseStream.Length);

                                client.Send(errorResponse);
                            }

                            // terminate the function on catch, can't return asynchronous on catch
                            if(carrier == null)
                                return false;

                            bool connectSuccess = false;
                            carrier.Data.Client.OnClientDisconnected += (s, e) =>
                            {
                                if (connectSuccess)
                                    Program.GetInstance().Log.DebugFormat("Proxy connection {0} quit connection to {1}", remoteEndPoint, client.RemoteEndPoint);
                                client.Disconnect();
                            };
                            carrier.Data.Client.OnClientConnected += (s, e) =>
                            {
                                connectSuccess = true;
                                Program.GetInstance().Log.DebugFormat("Client {0} opened connection on {1} successfully", client.RemoteEndPoint, remoteEndPoint);
                            };
                            carrier.Data.Client.OnClientDataReceived += (s, e) =>
                            {
                                carrier.WiredRx += e.Data.Length;
                                //Program.GetInstance().Log.DebugFormat("Recv: {0}", BitConverter.ToString(e.Data));
                                client.Send(e.Data);
                            };
                            this._associations[client.Id] = carrier;

                            await Task.Run(() => { carrier.Data.Client.Connect(); });

                            if (!connectSuccess)
                                Program.GetInstance().Log.DebugFormat("Client {0} connection to {1} failed", client.RemoteEndPoint, remoteEndPoint);
                            carrier.CommandSuccess = connectSuccess;

                            responseWriter.Write(Socks.Constants.Socks.Version);
                            responseWriter.Write(connectSuccess ? (byte)Socks.Constants.Reply.Succeeded : (byte)Socks.Constants.Reply.ConnectionRefused);
                            responseWriter.Write((byte)0);

                            if(typedMessage.DestinationAddress.AddressFamily == AddressFamily.InterNetwork)
                                responseWriter.Write((byte)Socks.Constants.AddressType.IPv4);
                            else if(typedMessage.DestinationAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                responseWriter.Write((byte)Socks.Constants.AddressType.IPv6);

                            responseWriter.Write(typedMessage.DestinationAddress.GetAddressBytes());
                            responseWriter.Write((short)IPAddress.HostToNetworkOrder(typedMessage.DestinationPort));
                            responseWriter.Flush();

                            byte[] responseCommand = responseStream.GetBuffer();
                            Array.Resize(ref responseCommand, (int)responseStream.Length);

                            client.Data.AuthenticationState = Socks.Constants.AuthenticationState.Transmitting;
                            client.Send(responseCommand);
                            returnData = connectSuccess;
                        }
                        else
                            throw new Exception("Proxy data already associated for client id " + client.Id.ToString() + ", " + client.RemoteEndPoint.ToString());
                    }
                    /*
                    // Command not supported response
                    {
                        responseWriter.Write(Socks.Constants.Socks.Version);
                        responseWriter.Write((byte)Socks.Constants.Reply.CommandNotSupported);
                        responseWriter.Write((byte)0);
                        responseWriter.Write((byte)(typedMessage.AddressType == Socks.Constants.AddressType.Domain ? Socks.Constants.AddressType.IPv4 : typedMessage.AddressType));
                        responseWriter.Write(typedMessage.DestinationAddress.GetAddressBytes());
                        responseWriter.Write((short)IPAddress.HostToNetworkOrder(typedMessage.DestinationPort));
                        responseWriter.Flush();

                        byte[] responseData = responseStream.GetBuffer();
                        Array.Resize(ref responseData, (int)responseStream.Length);

                        client.Send(responseData);
                    }
                    */
                }
            }
            return returnData;
        }

        /// <summary>
        /// IStateDependentHandler.Transmission implementation for AuthenticationState.Transmitting
        /// </summary>
        /// <returns>True if connection left open, false if not</returns>
        Task<bool> Plugin.IStateDependentHandler.Transmission(Client client, byte[] message)
        {
            Association.Carrier userCarrier = this._associations[client.Id];
            if (userCarrier != null)
            {
                userCarrier.Transmission(message);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        #endregion

    }
}
