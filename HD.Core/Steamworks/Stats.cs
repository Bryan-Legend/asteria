//using System;
//using UnityEngine;
//using ManagedSteam;
//using ManagedSteam.Exceptions;

///// <summary>
///// A simple example script showing how to get and set Stats.
///// Remember to also add the Steamworks script to a object so it can setup the library.
///// 
///// You will have to add the stats in the Steamworks AppAdmin page to be able to successfully read 
///// and write the stat values.
///// </summary>
//public class Stats : MonoBehaviour
//{
//    private void Start()
//    {
//        if (Steamworks.SteamInterface == null)
//        {
//            // Startup of the library failed.
//            return;
//        }

//        IStats stats = Steamworks.SteamInterface.Stats;

//        // Register to the event that is raised when stats are received.
//        stats.UserStatsReceived += UserStatsReceived;
//        // Register to the event that is raised when stats are saved to the server.
//        stats.UserStatsStored += UserStatsStored;

//        // Ask the steam client to download stats for the current user.
//        stats.RequestCurrentStats();
//    }

//    /// <summary>
//    /// Called when stats for a user have been downloaded.
//    /// </summary>
//    /// <param name="value"></param>
//    private void UserStatsReceived(ManagedSteam.CallbackStructures.UserStatsReceived value)
//    {
//        // Make sure that the callback is for this game
//        if (value.GameID == Steamworks.SteamInterface.AppID)
//        {
//            if (value.Result != ManagedSteam.SteamTypes.Result.OK)
//            {
//                UnityEngine.Debug.LogError("Failed to download stats.");
//                return;
//            }

//            // The stats have been downloaded successfully

//            IStats stats = Steamworks.SteamInterface.Stats;

//            // Read the value of an INT stat named TestStatInt and a FLOAT stat named TestStatFloat.
//            int intData;
//            float floatData;
//            if (!stats.GetStat("TestStatInt", out intData))
//            {
//                UnityEngine.Debug.LogWarning("Failed to read TestStatInt");
//            }
//            if (!stats.GetStat("TestStatFloat", out floatData))
//            {
//                UnityEngine.Debug.LogWarning("Failed to read TestStatFloat");
//            }

//            UnityEngine.Debug.Log("TestStatInt = " + intData.ToString());
//            UnityEngine.Debug.Log("TestStatFloat = " + floatData.ToString());


//            // Change the stat values and save them
//            intData++;
//            floatData += 0.5f;
//            if (!stats.SetStat("TestStatInt", intData))
//            {
//                UnityEngine.Debug.LogWarning("Failed to write TestStatInt");
//            }
//            if (!stats.SetStat("TestStatFloat", floatData))
//            {
//                UnityEngine.Debug.LogWarning("Failed to write TestStatFloat");
//            }

//            // Tell the steam client that we want to upload the new stat values.
//            // This will cause the UserStatsStored method to be called once the upload is complete.
//            stats.StoreStats();
//        }
//    }

//    /// <summary>
//    /// Called when stats have been saved to the steam servers.
//    /// </summary>
//    /// <param name="value"></param>
//    private void UserStatsStored(ManagedSteam.CallbackStructures.UserStatsStored value)
//    {
//        // Make sure that the callback is for this game
//        if (value.GameID == Steamworks.SteamInterface.AppID)
//        {
//            if (value.Result == ManagedSteam.SteamTypes.Result.OK)
//            {
//                UnityEngine.Debug.Log("Stats saved to the server successfully.");
//            }
//            else
//            {
//                UnityEngine.Debug.LogWarning("Failed to save stats to the server.");
//            }
//        }
//    }

//}