using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    /// <summary>
    /// sends to everyone the player that disconnected
    /// </summary>
    /// <param name="_playerId">disconnected player id</param>
    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            SendTCPDataToAll(_packet);
        }
    }
    /// <summary>
    /// sends to everyone the impostor's id
    /// </summary>
    /// <param name="_impostorsID">impostor's id</param>
    public static void SetRoles(int _impostorsID)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setRoles))
        {
            _packet.Write(_impostorsID);
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// sends to everyone who has been eliminated
    /// </summary>
    /// <param name="_targetId">player to eliminate</param>
    public static void EliminatePlayer(int _targetId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.eliminatePlayer))
        {
            _packet.Write(_targetId);
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// sends updates in the task progression 
    /// </summary>
    /// <param name="_taskProgres">the progres</param>
    public static void UpdateTaskProgressClient(float _taskProgres)
    {
        using (Packet _packet = new Packet((int)ServerPackets.updateTaskProgressClient))
        {
            _packet.Write(_taskProgres);
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// telling everyone to start an emergency meeting
    /// </summary>
    /// <param name="_identifier">Dead body reported / Button pressed</param>
    /// <param name="_infoString">info string containing alive players, ids and dividers</param>
    public static void StartEmergencyMeeting(int _identifier,string _infoString)
    {
        Debug.Log("sending Command To start meeting");
        using (Packet _packet = new Packet((int)ServerPackets.startEmergencyMeeting))
        {
            _packet.Write(_identifier);
            _packet.Write(_infoString);
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// ends a meeting and sends the result
    /// </summary>
    /// <param name="_meetingResult">player to eject (id)/skip/tie(0) </param>
    public static void EndMeeting(int _meetingResult)
    {
        if (_meetingResult != 0)
        {
            Server.clients[_meetingResult].player.isAlive = false;
        }
        using (Packet _packet  = new Packet((int)ServerPackets.endMeeting))
        {
            _packet.Write(_meetingResult);
            SendTCPDataToAll(_packet);
        }
    }
    /// <summary>
    /// telling everyone that a madbay scan has been started
    /// </summary>
    public static void ServerStartMadBayScan()
    {
        using (Packet _packet = new Packet((int)ServerPackets.serverStartMadBayScan))
        {
            SendTCPDataToAll(_packet);
        }
    }
    /// <summary>
    /// sends the game status to everyone
    /// </summary>
    public static void GameStatus()
    {
        string _status = Server.GatGameStatus();
        using (Packet _packet = new Packet((int)ServerPackets.gameStatus))
        {
            _packet.Write(_status);
            SendTCPDataToAll(_packet);
        }
    }
    

    #endregion
}