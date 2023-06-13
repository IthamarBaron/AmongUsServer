using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages all tasks done by crew mates
/// </summary>
public class TaskManager : MonoBehaviour
{
    public static int totalTasksCompleeted = 0;
    private static int totalTasksRequired;
    

    private bool taskRequiementsSet = false;

    void Update()
    {
        if (ServerHandle.gameStarted && !taskRequiementsSet)
        {
            totalTasksRequired = (Server.GetPlayersConnected()-Constants.IMPOSTORS) * Constants.TASKS_PER_PLAYER + GetPlayersWithMadBayScans();
            taskRequiementsSet = true;
            Destroy(this);
        }
    }


    /// <summary>
    /// counts players with madbay scans
    /// </summary>
    /// <returns>anout of players with madbay scans</returns>
    int GetPlayersWithMadBayScans()
    {
        int _playersWithMadBayScan = 0;
        for (int i = 1; i <= Server.GetPlayersConnected(); i++)
        {
            if (Server.clients[i] != null&& Server.clients[i].player != null &&Server.clients[i].player.id %3 == 0 && !Server.clients[i].player.isImpostor)
            {
                _playersWithMadBayScan++;
            }
        }
        return _playersWithMadBayScan;
    }

    public static float GetTaskProgress()
    {
        return (float)totalTasksCompleeted / totalTasksRequired;
    }

}
