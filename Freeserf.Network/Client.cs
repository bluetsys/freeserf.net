﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Freeserf.Network
{
    public class LocalClient : ILocalClient
    {
        TcpClient client = null;
        RemoteServer server = null;

        public LocalClient()
        {
            Ip = HostNetwork.GetLocalIpAddress();
        }

        public IPAddress Ip
        {
            get;
        }

        // TODO: get from server and set it here
        public uint PlayerIndex
        {
            get;
            private set;
        }

        // TODO: needed
        public Game Game
        {
            get;
            set;
        }

        public IRemoteServer Server => server;

        public LobbyData LobbyData
        {
            get;
            private set;
        } = null;

        public event EventHandler Disconnected;
        public event EventHandler LobbyDataUpdated;

        public void Disconnect()
        {
            try
            {
                SendDisconnect();
            }
            catch
            {
                // ignore
            }

            client?.Close();
            server?.Close();
            client = null;
            server = null;
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public bool JoinServer(string name, IPAddress ip)
        {
            // if already connected to another server, first disconnect
            if (server != null || client != null)
            {
                Disconnect();
            }

            try
            {
                client = new TcpClient(new IPEndPoint(Ip, 0));

                if (IPAddress.IsLoopback(ip))
                    client.Connect(HostNetwork.GetLocalIpAddress(), Global.NetworkPort);
                else
                    client.Connect(ip, Global.NetworkPort);

                server = new RemoteServer(name, ip, client);
                server.DataReceived += Server_DataReceived;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Server_DataReceived(IRemoteServer server, byte[] data)
        {
            var parsedData = NetworkDataParser.Parse(data);
            
            switch (parsedData.Type)
            {
                case NetworkDataType.Request:
                    HandleRequest(parsedData as RequestData);
                    break;
                case NetworkDataType.Heartbeat:
                    break;
                case NetworkDataType.LobbyData:
                    UpdateLobbyData(parsedData as LobbyData);
                    break;
                // TODO: other data
                default:
                    // TODO: ignore? track safely?
                    throw new ExceptionFreeserf("Unknown server data");
            }
        }

        private void HandleRequest(RequestData request)
        {
            switch (request.Request)
            {
                case Request.Disconnect:
                    // server closed or kicked player
                    client?.Close();
                    server?.Close();
                    client = null;
                    server = null;
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    break;
                case Request.Heartbeat:
                    // TODO
                    break;
                default:
                    // all other requests can not be send to a client
                    throw new ExceptionFreeserf(ErrorSystemType.Network, $"Request {request.ToString()} can not be send to client.");
            }

            RespondToRequest(request);
        }

        private void RespondToRequest(RequestData request)
        {
            if (request.Number == Global.SpontaneousMessage)
                return; // spontaneous messages don't need a response

            // TODO
            // ...
        }

        private void UpdateLobbyData(LobbyData data)
        {
            for (int i = 0; i < data.Players.Count; ++i)
            {
                if (!data.Players[i].IsHost && data.Players[i].Identification == Ip.ToString())
                {
                    PlayerIndex = (uint)i;
                    break;
                }
            }

            LobbyData = data;
            LobbyDataUpdated?.Invoke(this, EventArgs.Empty);
        }

        public byte RequestLobbyStateUpdate()
        {
            byte messageIndex = Global.GetNextMessageIndex();

            new RequestData(messageIndex, Request.LobbyData).Send(server);

            return messageIndex;
        }

        public byte RequestGameStateUpdate()
        {
            byte messageIndex = Global.GetNextMessageIndex();

            throw new NotImplementedException();

            return messageIndex;
        }

        public byte RequestMapStateUpdate()
        {
            byte messageIndex = Global.GetNextMessageIndex();

            throw new NotImplementedException();

            return messageIndex;
        }

        public byte RequestPlayerStateUpdate(uint playerIndex)
        {
            byte messageIndex = Global.GetNextMessageIndex();

            throw new NotImplementedException();

            return messageIndex;
        }

        public void SendHeartbeat()
        {
            new Heartbeat(Global.GetNextMessageIndex(), (byte)PlayerIndex).Send(server);
        }

        public void SendDisconnect()
        {
            new RequestData(Global.GetNextMessageIndex(), Request.Disconnect).Send(server);
        }
    }

    public class RemoteClient : IRemoteClient
    {
        readonly TcpClient client = new TcpClient();

        public RemoteClient(uint playerIndex, ILocalServer server, TcpClient client)
        {
            Ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            PlayerIndex = playerIndex;
            Server = server;
            this.client = client;
        }

        public IPAddress Ip
        {
            get;
        }

        public uint PlayerIndex
        {
            get;
        }

        public ILocalServer Server
        {
            get;
        }

        public IPAddress GetIP()
        {
            throw new NotImplementedException();
        }

        public void SendHeartbeat()
        {
            new Heartbeat(Global.GetNextMessageIndex(), (byte)PlayerIndex).Send(this);
        }

        public void SendDisconnect()
        {
            new RequestData(Global.GetNextMessageIndex(), Request.Disconnect).Send(this);
        }

        public void Send(byte[] rawData)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    client.GetStream().Write(rawData, 0, rawData.Length);
                }
                catch (System.IO.IOException)
                {
                    if (client.Connected)
                        throw;
                }
            }            
        }

        public void SendLobbyDataUpdate(byte messageIndex, LobbyServerInfo serverInfo, List<LobbyPlayerInfo> players)
        {
            new LobbyData(messageIndex, serverInfo, players).Send(this);
        }

        public void SendGameStateUpdate(Game game)
        {
            throw new NotImplementedException();
        }

        public void SendPlayerStateUpdate(Player player)
        {
            throw new NotImplementedException();
        }

        public void SendMapStateUpdate(Map map)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientFactory : IClientFactory
    {
        public ILocalClient CreateLocal()
        {
            return new LocalClient();
        }
    }
}
