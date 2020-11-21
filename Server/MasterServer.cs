using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HD;
using System.IO;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Timers;
using System.Net;

namespace Server
{
    public static class MasterServer
    {
        const float interval = 1000f / 60f;

        public static string SaveGame = null;

        public static DateTime Started = DateTime.UtcNow;
        public static DateTime LastUpdate = DateTime.UtcNow;
        public static List<Connection> Connections = new List<Connection>();
        public static int UpdatesPerSecond;

        static Timer timer;
        static TcpListener listener;
        static int updatesThisSecond;

        internal static void Start()
        {
            Player.OnPlayerSwap += (oldPlayer, newPlayer) =>
            {
                foreach (var connection in Connections)
                {
                    if (connection.Player == oldPlayer)
                        connection.Player = newPlayer;
                }
            };

            Directory.CreateDirectory(Utility.SavePath);

            ItemBase.LoadItemSizes();

            //if (SaveGame == null)
            //{
            //    foreach (var saveFile in Directory.EnumerateFiles(Utility.SavePath, "*.World").OrderBy(s => s).Take(1))
            //    {
            //        World.Load(saveFile);
            //    }
            //}
            //else
            //{
                var saveFile = Path.Combine(Utility.SavePath, SaveGame + ".World");
                if (File.Exists(saveFile))
                    World.Load(saveFile);
                else
                    Utility.LogMessage(saveFile + " not found.");
            //}

            if (World.Name == null)
            {
                Utility.LogMessage("World not found. Creating new save game.");

                if (SaveGame == null)
                    SaveGame = "Multiplayer";
                World.CreateWorld(SaveGame);
            }

            listener = new TcpListener(IPAddress.Any, World.ServerPort);
            listener.AllowNatTraversal(true); // not sure if this is needed.
            listener.Start();

            Utility.LogMessage("Listening on port " + World.ServerPort);

            timer = new Timer(interval);
            timer.Elapsed += (sender, e) => { Update(); };
            timer.AutoReset = false;

            timer.Start();
        }

        static void CheckForNewConnections()
        {
            if (listener.Pending())
            {
                var newClient = listener.AcceptTcpClient();
                var newConnection = new Connection(newClient);
            }
        }

        //static string lastDebugMessage;

        static void Update()
        {
            try
            {
                CheckForNewConnections();

                if (Connections.Count > 0 || ForceUpdate)
                {
                    ForceUpdate = false;
                    updatesThisSecond++;
                    if (LastUpdate.Second != DateTime.UtcNow.Second)
                    {
                        UpdatesPerSecond = updatesThisSecond;
                        updatesThisSecond = 0;
                    }

                    var gameTime = new GameTime(DateTime.UtcNow - Started, DateTime.UtcNow - LastUpdate);
                    LastUpdate = DateTime.UtcNow;

                    World.Update
                    (
                        gameTime,
                        (map) =>
                        {
                            // Call getCurrentFrame on each player so that the animations will finish. Normally this would be done in draw but on a remote server draw is never called.
                            foreach (var player in map.Players)
                            {
                                player.GetCurrentFrame();
                            }

                            var removedEntityIds = map.GetRemovedEntityIds();

                            foreach (var connection in Connections.ToArray())
                            {
                                if (connection.Player != null && connection.Player.Map == map)
                                    connection.Update(removedEntityIds);
                            }

                            foreach (var entity in map.Entities)
                                entity.SoundEvents.Clear();

                            map.SoundEvents.Clear();
                        }
                    );
                }
            }
            catch (Exception e)
            {
                Utility.LogMessage(e.ToString());
                WebServiceClient.ReportCrash(e);
            }

            timer.Start();
        }

        internal static void Stop()
        {
            foreach (var connection in Connections.ToArray())
            {
                connection.Disconnect();
            }
        }

        public static bool ForceUpdate { get; set; }
    }
}