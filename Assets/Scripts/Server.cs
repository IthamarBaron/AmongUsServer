using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /// <summary>Starts the server.</summary>
    /// <param name="_maxPlayers">The maximum players that can be connected .</param>
    /// <param name="_port">The port to start the server on.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port); // python equivalent -> bind
        tcpListener.Start();//python equivalent -> listen
        tcpListener.BeginAcceptTcpClient(TCPConnectAccept, null); //accepting connections in a tcp thread -> tcp_accept_thread = threading.Thread(target=tcp_accept_connections)

        udpListener = new UdpClient(Port);// python equivalent-> bind udp
        udpListener.BeginReceive(ReceiveUDPData, null);

        Debug.Log($"Server started on port {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectAccept(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectAccept, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        if (!ServerHandle.gameStarted)
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        else
        {
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Game has already started!");
        }
        
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void ReceiveUDPData(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(ReceiveUDPData, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // make sure that the client is not being "impersonated" by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null); //python -> udp_listener.sendto(packet.to_array(), client_endpoint)
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
            Debug.Log("adding id "+i);
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.teleportToMap, ServerHandle.TeleportToMap },
            { (int)ClientPackets.getRoles, ServerHandle.GetRoles},
            { (int)ClientPackets.attemptKill,ServerHandle.AttemptKill},
            { (int)ClientPackets.updateTaskProgressServer, ServerHandle.UpdateTaskProgressServer},
            { (int)ClientPackets.callEmergencyMeeting, ServerHandle.CallEmergencyMeeting},
            { (int)ClientPackets.playerStartMadBayScan, ServerHandle.PlayerStartMadBayScan },
            { (int)ClientPackets.castVote, ServerHandle.CastVote},
            { (int)ClientPackets.gameStatus,ServerHandle.GameStatus}
        };
        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        //just closing the sockets nothing special
        tcpListener.Stop(); 
        udpListener.Close();
    }

    public static int GetPlayersConnected()
    {
        int _counter = 0;
        for (int i = 1; i < clients.Count; i++)
        {
            if (clients[i] != null && clients[i].player != null)
                _counter++;
        }
        return _counter;
    }

    /// <summary>
    /// Game status - impostor win / crewmate win / still playing
    /// </summary>
    /// <returns>String containing status</returns>
    public static string GatGameStatus()
    {
        int _livingImpostors = 0;
        int _livingCrewmates = 0;
        string _status = "";
        for (int i = 1; i <= GetPlayersConnected(); i++)
        {
            if (clients[i].player.isImpostor && clients[i].player.isAlive)
            {
                _livingImpostors++;
            }
            else
            {
                if (!clients[i].player.isImpostor && clients[i].player.isAlive)
                {
                    _livingCrewmates++;
                } 
            }

        }
        if (_livingImpostors == 0 || TaskManager.GetTaskProgress()>=0.93f)
        {
            _status = "crewmatewin";
        }
        else if (_livingImpostors >= _livingCrewmates)
        {
            _status = "impostorwin";
        }
        else
        {
            _status = "gaming";
        }

        return _status;
    }
}