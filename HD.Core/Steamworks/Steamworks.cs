using System;
using  Steamworks;
using HD;

/// <summary>
/// A simple example of how to start the library in Unity. 
/// Make sure that you have the steam client running and have setup everything according to the 
/// Setup Instructions in the documentation, or the library will fail to load.
/// </summary>
public static class SteamWrapper
{
    static string status;

    /// <summary>
    /// Use this property to access the Steamworks API
    /// </summary>
    //public static Steam SteamInterface { get; private set; }

    public static void Start()
    {
        if (!SteamAPI.Init())
        {
            Utility.LogMessage("SteamAPI.Init() failed!");
        }

        Utility.LogMessage(Packsize.Test().ToString());

        //Steamworks.S

        //bool error = false;
        //try
        //{
        //    // Starts the library. This will, and can, only be done once.
        //    SteamInterface = Steam.Initialize();
        //}
        //catch (AlreadyLoadedException e)
        //{
        //    status = "The native dll is already loaded, this should not happen if ReleaseManagedResources is used and Steam.Initialize() is only called once.";
        //    Utility.LogMessage(status);
        //    Utility.LogMessage(e.Message);
        //    error = true;
        //}
        //catch (SteamInitializeFailedException e)
        //{
        //    status = "Could not initialize the native Steamworks API. This is usually caused by a missing steam_appid.txt file or if the Steam client is not running.";
        //    Utility.LogMessage(status);
        //    Utility.LogMessage(e.Message);
        //    error = true;
        //}
        //catch (SteamInterfaceInitializeFailedException e)
        //{
        //    status = "Could not initialize the wanted versions of the Steamworks API. Make sure that you have the correct Steamworks SDK version. See the documentation for more info.";
        //    Utility.LogMessage(status);
        //    Utility.LogMessage(e.Message);
        //    error = true;
        //}
        //catch (DllNotFoundException e)
        //{
        //    status = "Could not load a dll file. Make sure that the steam_api.dll/libsteam_api.dylib file is placed at the correct location. See the documentation for more info.";
        //    Utility.LogMessage(status);
        //    Utility.LogMessage(e.Message);
        //    error = true;
        //}

        //if (error)
        //{
        //    SteamInterface = null;
        //}
        //else
        //{
        //    status = "Steamworks initialized and ready to use.";
        //    Utility.LogMessage(status);

        //    // An event is used to notify us about any exceptions thrown from native code.
        //    SteamInterface.ExceptionThrown += ExceptionThrown;

        //    // Listen to when the game overlay is shown or hidden
        //    //SteamInterface.Friends.GameOverlayActivated += OverlayToggle;
        //}
    }

    public static bool SetStat(string statName)
    {
        return Steamworks.SteamUserStats.SetStat(statName, 1000);
    }

    public static bool SetAchivement(Achievement achievement)
    {
        return Steamworks.SteamUserStats.SetAchievement(achievement.ToString());
    }

    //static void OverlayToggle(GameOverlayActivated value)
    //{
    //    // This method is called when the game overlay is hidden or shown
    //    // NOTE: The overlay may not work when a game is run in the Unity editor
    //    // Build the game and "publish" it to a local content server and then start the game via the
    //    // steam client to make the overlay work.

    //    if (value.Active)
    //    {
    //        Utility.LogMessage("Overlay shown");
    //    }
    //    else
    //    {
    //        Utility.LogMessage("Overlay closed");
    //    }
    //}

    //static void ExceptionThrown(Exception e)
    //{
    //    // This method is called when an exception have been thrown from native code.
    //    // We print the exception so we can see what went wrong.

    //    Utility.LogMessage(e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
    //}

    //public static void Update()
    //{
    //    if (SteamInterface != null)
    //    {
    //        // Makes sure that callbacks are sent.
    //        // Make sure that you call this from some other place if you use 'Time.timeScale = 0' 
    //        // to pause the game.
    //        SteamInterface.Update();
    //    }
    //}

    //private void OnGUI()
    //{
    //    //GUILayout.Label(status);

    //    //// NOTE!
    //    //// Overlays might not work in the Unity editor, see the documentation for more information

    //    //if (GUILayout.Button("Show overlay"))
    //    //{
    //    //    // Will show the game overlay and show the Friends dialog. 
    //    //    SteamInterface.Friends.ActivateGameOverlay(OverlayDialog.Friends);
    //    //}
    //    //if (GUILayout.Button("Show overlay (webpage)"))
    //    //{
    //    //    // Will show the game overlay and open a web page.
    //    //    SteamInterface.Friends.ActivateGameOverlayToWebPage("http://ludosity.com");
    //    //}
    //}

    //private void OnApplicationQuit()
    //{
    //    // Always cleanup when shutting down
    //    Cleanup();
    //}

    public static void Cleanup()
    {
        SteamAPI.Shutdown();

        //if (SteamInterface != null)
        //{
        //    //if (Application.isEditor)
        //    //{
        //    //    // Only release managed handles if we run from inside the editor. This enables us 
        //    //    // to use the library again without restarting the editor.
        //    //    SteamInterface.ReleaseManagedResources();
        //    //}
        //    //else
        //    //{
        //    // We are running from a standalone build. Shutdown the library completely
        //    SteamInterface.Shutdown();
        //    //}

        //    SteamInterface = null;
        //}
    }
}