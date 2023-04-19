using Q3C273.Shared.Messages.ReverseProxy;
using System;
using System.Net;
using System.Net.Sockets;
using Ton618.Networking;

namespace Ton618.ReverseProxy
{
    public class ReverseProxyClient
    {
        public const int BUFFER_SIZE = 8192;

        public int ConnectionId { get; private set; }
        public Socket Handle { get; private set; }
        public string Target { get; private set; }
        public int Port { get; private set; }
        public Client Client { get; private set; }
        private byte[] _buffer;
        private bool _disconnectIsSend;

        public ReverseProxyClient(ReverseProxyConnect command, Client client)
        {
            ConnectionId = command.ConnectionId;
            Target = command.Target;
            Port = command.Port;
            Client = client;
            Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Non-Blocking connect, so there is no need for a extra thread to create
            Handle.BeginConnect(command.Target, command.Port, Handle_Connect, null);
        }

        private void Handle_Connect(IAsyncResult ar)
        {
            try
            {
                Handle.EndConnect(ar);
            }
            catch { }

            if (Handle.Connected)
            {
                try
                {
                    _buffer = new byte[BUFFER_SIZE];
                    Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
                }
                catch
                {
                    Client.Send(new ReverseProxyConnectResponse
                    {
                        ConnectionId = ConnectionId,
                        IsConnected = false,
                        LocalAddress = null,
                        LocalPort = 0,
                        HostName = Target
                    });
                    Disconnect();
                }

                var localEndPoint = (IPEndPoint)Handle.LocalEndPoint;
                Client.Send(new ReverseProxyConnectResponse
                {
                    ConnectionId = ConnectionId,
                    IsConnected = true,
                    LocalAddress = localEndPoint.Address.GetAddressBytes(),
                    LocalPort = localEndPoint.Port,
                    HostName = Target
                });
            }
            else
            {
                Client.Send(new ReverseProxyConnectResponse
                {
                    ConnectionId = ConnectionId,
                    IsConnected = false,
                    LocalAddress = null,
                    LocalPort = 0,
                    HostName = Target
                });
            }
        }

        private void AsyncReceive(IAsyncResult ar)
        {
            //Receive here data from e.g. a WebServer

            try
            {
                var received = Handle.EndReceive(ar);

                if (received <= 0)
                {
                    Disconnect();
                    return;
                }

                var payload = new byte[received];
                Array.Copy(_buffer, payload, received);
                Client.Send(new ReverseProxyData { ConnectionId = ConnectionId, Data = payload });
            }
            catch
            {
                Disconnect();
                return;
            }

            try
            {
                Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch
            {
                Disconnect();
                return;
            }
        }

        public void Disconnect()
        {
            if (!_disconnectIsSend)
            {
                _disconnectIsSend = true;
                //send to the Server we've been disconnected
                Client.Send(new ReverseProxyDisconnect { ConnectionId = ConnectionId });
            }

            try
            {
                Handle.Close();
            }
            catch { }

            Client.RemoveProxyClient(ConnectionId);
        }

        public void SendToTargetServer(byte[] data)
        {
            try
            {
                Handle.Send(data);
            }
            catch { Disconnect(); }
        }
    }
}
