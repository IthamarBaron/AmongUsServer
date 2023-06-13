using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const int TICKS_PER_SEC = 30; // How many ticks per second
    public const float MS_PER_TICK = 1000f / TICKS_PER_SEC; // How many milliseconds per tick
    public const float MAP_SPAWN_Y = 10.779f;
    public static Vector3[] mapSpwanLocations = { new Vector3(221.13f, MAP_SPAWN_Y, 244.75f), new Vector3(221.31f, MAP_SPAWN_Y, 235.15f), new Vector3(215.37f, MAP_SPAWN_Y, 240.23f), new Vector3(225.05f, MAP_SPAWN_Y, 240.01f), new Vector3(216.51f, MAP_SPAWN_Y, 242.99f), new Vector3(224.17f, MAP_SPAWN_Y, 236.71f), new Vector3(223.54f, MAP_SPAWN_Y, 243.26f), new Vector3(215.79f, MAP_SPAWN_Y, 237.4f), new Vector3(218.7f, MAP_SPAWN_Y, 244.6f), new Vector3(218.08f, MAP_SPAWN_Y, 235.44f) };
    public const int TASKS_PER_PLAYER = 4;
    public static int IMPOSTORS = 1; // amout of impostors in the game | yes its not a const

    /// <summary>
    /// restarts the votes
    /// </summary>
    public static void ResetVotes()
    {
        for (int i = 0; i < ServerHandle.meetingVotes.Length; i++)
            ServerHandle.meetingVotes[i] = 0;
    }


    /// <summary>
    /// takes an array of the votes and will return the id of the player who got the most votes
    /// if vote ended on a tie or skip the method will return 0
    /// </summary>
    /// <param name="_votes">arry containing the votes</param>
    /// <returns></returns>
    public static int CountVotes(int[] _votes)
    {
        int skipVotes = _votes[0];
        int maxVotes = -1;
        int maxVotesIndex = -1;

        // Check if skip votes have the largest value
        bool skipVotesLargest = true;
        for (int i = 1; i < _votes.Length; i++)
        {
            if (_votes[i] > skipVotes)
            {
                skipVotesLargest = false;
                break;
            }
        }

        // Find the index with the highest number of votes
        for (int i = 1; i < _votes.Length; i++)
        {
            if (_votes[i] > maxVotes)
            {
                maxVotes = _votes[i];
                maxVotesIndex = i;
            }
            else if (_votes[i] == maxVotes)
            {
                maxVotesIndex = 0; // Set index to 0 for ties
            }
        }

        // Determine the result based on the conditions
        if (skipVotesLargest || maxVotesIndex == 0)
        {
            return 0; // Tie or skip votes have the largest value
        }
        else
        {
            return maxVotesIndex; // Index with the highest number of votes
        }
    }
}