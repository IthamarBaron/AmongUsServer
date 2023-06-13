using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ServerHandle
{
    public static int[] meetingVotes = new int[11];// this array will store the votes in a meeting
    private static int votesCollected = 0;

    public static bool gameStarted = false;
    /// <summary>
    /// we recived a welcome packet and now we send the player to the game
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">containing id and name</param>
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    /// <summary>
    /// handles movment 
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">containing the array of "movments"</param>
    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    /// <summary>
    /// Teleports all players to map
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet"> not intresting info</param>
    public static void TeleportToMap(int _fromClient, Packet _packet)
    {
        gameStarted = true;
        string _teleportTo = _packet.ReadString();
        for (int i = 1; i < Server.clients.Count; i++)
        {
            if (Server.clients[i]!=null)
            {
                if (Server.clients[i].player != null)
                {
                    Server.clients[i].player.TeleportPlayerToMap();
                }
            }
        }
    }

    /// <summary>
    /// generates roles for the players
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">amount of impostors want</param>
    public static void GetRoles(int _fromClient, Packet _packet)
    {
        int _impostorsWant = _packet.ReadInt();
        Constants.IMPOSTORS = _impostorsWant; // yes i know this is not a good use of constants
        int _playersConnected = Server.GetPlayersConnected();
        int _impostorSum = 0;
        for (int i = 0; i < _impostorsWant; i++)
        {
            System.Random rnd = new System.Random();
            int _impostorID = rnd.Next(1, _playersConnected+1);
            while (Server.clients[_impostorID].player.isImpostor) //cyceling untill we give impostor to a non impostor
            {
                _impostorID = rnd.Next(1, _playersConnected+1);
            }
            Server.clients[_impostorID].player.isImpostor = true;
            UnityEngine.Debug.Log("Player with ID: " + _impostorID + " is now impostor");
            if (i == 0)
                _impostorSum += _impostorID;
            else
                _impostorSum += _impostorID * 10;
        }
        ServerSend.SetRoles(_impostorSum);
    }

    /// <summary>
    /// Attempts to kill a player
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">containing the player we want to kill</param>
    public static void AttemptKill(int _fromClient, Packet _packet)
    {
        int _targetedId = _packet.ReadInt();
        UnityEngine.Debug.Log("attempted to kill player: "+ _targetedId);
        if (!Server.clients[_targetedId].player.isImpostor)
        {
            Server.clients[_targetedId].player.isAlive = false;
            UnityEngine.Debug.Log("player: " + _targetedId + " has been killed");

            ServerSend.EliminatePlayer(_targetedId);

        }
    }

    /// <summary>
    /// Updates task bar according to tasks status
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">not intresting changed to const...</param>
    public static void UpdateTaskProgressServer(int _fromClient, Packet _packet)
    {
        int _addToProggres = _packet.ReadInt();
        TaskManager.totalTasksCompleeted += 1;
        ServerSend.UpdateTaskProgressClient(TaskManager.GetTaskProgress());
    }

    /// <summary>
    /// letting clients know we started a madbay scan
    /// </summary>
    /// <param name="_fromClient">sender</param>
    /// <param name="_packet">not intresting info</param>
    public static void PlayerStartMadBayScan(int _fromClient, Packet _packet)
    {
        var temp = _packet.ReadString();
        ServerSend.ServerStartMadBayScan();
    }

    /// <summary>
    /// Starts an emergency meeting
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">Meeting code</param>
    public static void CallEmergencyMeeting(int _fromClient, Packet _packet)
    {
        Debug.Log("Request recived Vote Status:");
        Debug.Log(DebugVotes(meetingVotes));
        Debug.Log("Votes collected = "+ votesCollected);
        int _callerORBodyIdCode = _packet.ReadInt();
        string _infoString = "";
        int _identifier = 0;

        Debug.Log("nuber in meeting packet = "+ _callerORBodyIdCode);
        if (_callerORBodyIdCode>10)
        {
            _identifier = (_callerORBodyIdCode / 10) - 1;
            Debug.Log("identifier = "+ _identifier);
        }
        
        foreach (var client in Server.clients)
        {
            if (client.Value != null)
            {
                if (client.Value.player != null)
                {
                    if (client.Value.player.isAlive)
                    {
                        _infoString += client.Value.player.id.ToString();
                        _infoString += client.Value.player.username;
                        // adding marker to know where to split the string on the client side
                        _infoString += "!+@_#)$(%*^&";//this is gebrish so it will be unique and i hope no one will be name like this
                    }


                }
            }
        }
        ServerSend.StartEmergencyMeeting(_identifier,_infoString);
    }

    /// <summary>
    /// managing votes during emergency meeting
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">string containing vote info </param>
    public static void CastVote(int _fromClient, Packet _packet)
    {
        votesCollected++;
        string _voteInfo = _packet.ReadString();
        //I dont know why normal .Split() didnt work but thats what Chat GPT said to replace with...
        string[] _splitedData = _voteInfo.Split(new string[] { "!+@_#)$(%*^&" }, StringSplitOptions.None);

        int _playersInMeeting = int.Parse(_splitedData[0]);
        int _playerVotedFor = int.Parse(_splitedData[1]);
        Debug.Log("player with ID: [" + _fromClient + "] Voted for player: ["+ _playerVotedFor + "]");
        meetingVotes[_playerVotedFor]++;
        Debug.Log("Meeting Proggres: " + votesCollected + "/" + _playersInMeeting+" Players have voted.");

        if (votesCollected == _playersInMeeting)
        {
            Debug.Log("Meeting Has Ended!");
            ServerSend.EndMeeting(Constants.CountVotes(meetingVotes));
            votesCollected = 0;
            Constants.ResetVotes();
        }
    }

    /// <summary>
    /// sends game status
    /// </summary>
    /// <param name="_fromClient">sender id</param>
    /// <param name="_packet">not intresting info</param>
    public static void GameStatus(int _fromClient, Packet _packet)
    {
        string _temp = _packet.ReadString();
        ServerSend.GameStatus();
    }

    /// <summary>
    /// UNRELATED TO HANDLING USED FOR DEBUGING VOTES
    /// </summary>
    /// <param name="_arr">votes</param>
    /// <returns>string containing votes</returns>
    public static string DebugVotes(int[] _arr)
    {
        string _str = "";

        foreach (var item in meetingVotes)
        {
            _str = _str + "[" + item + "],";
        }
        return _str;
    }
}