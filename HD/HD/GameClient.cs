using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.Xna.Framework;
using System.Threading;
using ProtoBuf;
using Microsoft.Xna.Framework.Input;
using Ionic.Zlib;

#if WINDOWS
using System.Net.Sockets;
#endif

namespace HD
{
    public class GameClient
    {
        const double UpdatesToServerPerSecond = 30;
        const double UpdateSeconds = 1 / UpdatesToServerPerSecond;

#if WINDOWS
        internal TcpClient Client;
        internal Queue<UpdateToClient> WorkQueue = new Queue<UpdateToClient>();
#endif

        internal bool ServerNeedsResolution = true;
        internal Thread ClientThread;
        internal Queue<String> MessagesToServer = new Queue<String>();
        internal SlotType DragSlotType;
        internal int DragSlotNumber;
        internal int DragAmount;
        internal SlotType DropSlotType;
        internal int DropSlotNumber;
        internal bool ClearActivePlaceable;
        internal bool ClearCreativeMode;
        internal bool ReturnToOverworld;
        internal static string SetValue;

        public string ServerAddress;
        public int Port;
        public string LoginName;

#if WINDOWS
        NetworkStream networkStream;
#endif

        DeflateStream compressedReceiveStream;

        string password;
        double timeSinceLastUpdate = 0;
        List<Item> inventoryTemp;
        int playerId;
        static Menu previousMenu;
        List<Spawn> spawnsTemp;
        public bool ToggleGodMode;
        public CreativeMode? SetCreativeMode;

        public static void Connect(string server, string port, string loginName, string password)
        {
            var portNum = 1701;
            Int32.TryParse(port, out portNum);
            port = portNum.ToString();

            previousMenu = Main.CurrentMenu;

            Main.GameClient = new GameClient(server, portNum) { LoginName = loginName, password = password };

            GameSettings.Default.Server = server;
            GameSettings.Default.ServerPort = port;
            GameSettings.Default.LoginName = loginName;
            GameSettings.Default.Password = password;
            GameSettings.Default.Save();

            Main.CurrentMenu = null;
        }

        public GameClient(string server): this(server, 1701)
        {
        }

        public GameClient(string server, int port)
        {
            World.ServerPort = port;

            ServerAddress = server;
            Port = port;

#if WINDOWS
            ClientThread = new Thread(new ParameterizedThreadStart(ServerReadThread));
            ClientThread.Start(Client);
#endif
        }

#if WINDOWS

        void ServerReadThread(object client)
        {
            try
            {
                Utility.StatusMessage = "Connecting to " + ServerAddress + " port " + Port;

                Main.ForceModal = true;

                Client = new TcpClient(ServerAddress, Port);
                Client.NoDelay = true; // if this is not set there will be terrible lag to the input to the server.

                networkStream = Client.GetStream();
                compressedReceiveStream = new DeflateStream(networkStream, CompressionMode.Decompress, true);

                Main.GameClient.SendUpdate(new UpdateToServer()
                {
                    LoginName = LoginName,
                    Password = Utility.CalculateHash(password),
                    Version = Utility.Version,
                    Skin = GameSettings.Default.SelectedSkin
                });

                Utility.StatusMessage = "";
            }
            catch (SocketException e)
            {
                Utility.StatusMessage = "";
                Main.ShowMessageBox(e.Message, tag => { Main.CurrentMenu = previousMenu; }, false);
                Disconnect();
                return;
            }
            finally
            {
                Main.ForceModal = false;
            }

            World.IsConnectedToServer = true;

            while (true)
            {
                try
                {
                    var update = ProtoBuf.Serializer.DeserializeWithLengthPrefix<UpdateToClient>(compressedReceiveStream, PrefixStyle.Base128);
                    WorkQueue.Enqueue(update);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                catch (SocketException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // network stream access
                    break;
                }
            }

            Disconnect();
        }
#endif

        public void Disconnect()
        {
            World.IsConnectedToServer = false;
            Main.GameClient = null;

#if WINDOWS

            if (Client != null)
            {
                if (Client.Connected)
                    SendUpdate(new UpdateToServer() { Disconnect = true });
                Client.Close();
                Client = null;
                Main.Quit();
            }
#endif
        }

        public void Update(GameTime gameTime)
        {
#if WINDOWS

            if (Client == null)
                return;

            if (!Client.Client.Connected)
            {
                Disconnect();
                return;
            }

            timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;

            if (Main.Player != null && timeSinceLastUpdate > UpdateSeconds)
            {
                var updateToServer = new UpdateToServer();

                if (ServerNeedsResolution)
                {
                    updateToServer.ResolutionX = Main.ResolutionWidth;
                    updateToServer.ResolutionY = Main.ResolutionHeight;
                    ServerNeedsResolution = false;
                }

                updateToServer.ClearActivePlaceable = ClearActivePlaceable;
                ClearActivePlaceable = false;
                updateToServer.ClearCreativeMode = ClearCreativeMode;
                ClearCreativeMode = false;
                updateToServer.SetValue = SetValue;
                SetValue = null;
                updateToServer.ToggleGodMode = ToggleGodMode;
                ToggleGodMode = false;
                updateToServer.SetCreativeMode = SetCreativeMode;
                SetCreativeMode = null;
                updateToServer.ReturnToOverworld = ReturnToOverworld;
                ReturnToOverworld = false;

                updateToServer.PressedKeys = Main.Player.Keyboard.GetPressedKeys();
                updateToServer.MouseX = Main.Player.MousePosition.X;
                updateToServer.MouseY = Main.Player.MousePosition.Y;
                updateToServer.MouseLeftButtonPressed = Main.Player.MouseLeftButtonPressed;
                updateToServer.MouseRightButtonPressed = Main.Player.MouseRightButtonPressed;
                updateToServer.SelectedActionSlot = Main.Player.SelectedActionSlot;
                updateToServer.GamePad = Main.Player.GamePad.GetBytes();

                if (DragSlotType != SlotType.None)
                {
                    updateToServer.DragSlotType = DragSlotType;
                    updateToServer.DragSlotNumber = DragSlotNumber;
                    updateToServer.DragAmount = DragAmount;
                    DragSlotType = SlotType.None;
                }

                if (DropSlotType != SlotType.None)
                {
                    updateToServer.DropSlotType = DropSlotType;
                    updateToServer.DropSlotNumber = DropSlotNumber;
                    DropSlotType = SlotType.None;
                }

                if (Main.Player.CraftRecipe != 0)
                {
                    updateToServer.CraftRecipe = Main.Player.CraftRecipe;
                    updateToServer.CraftAmount = Main.Player.CraftAmount;
                    Main.Player.CraftRecipe = 0;
                }

                if (Main.Player.SortInventory)
                {
                    updateToServer.SortInventory = true;
                    Main.Player.SortInventory = false;
                }

                if (MessagesToServer.Count > 0)
                {
                    updateToServer.Messages = MessagesToServer.ToArray();
                    MessagesToServer.Clear();
                }

                SendUpdate(updateToServer);
                timeSinceLastUpdate = 0;
            }

            while (WorkQueue.Count > 0)
            {
                HandleUpdate(WorkQueue.Dequeue());
            }
#endif
        }

#if WINDOWS

        void SendUpdate(UpdateToServer updateToServer)
        {
            Serializer.SerializeWithLengthPrefix(networkStream, updateToServer, PrefixStyle.Base128);
        }

        void HandleUpdate(UpdateToClient update)
        {
            if (update == null)
                return;

            if (update.InitializeMapWidth != 0)
            {
                //Utility.LogMessage("On new map packet queue size: {0}, update entitiy count {1}, update player id {2}", WorkQueue.Count, update.Entities == null ? 0 : update.Entities.Count, update.PlayerEntityId);

                Main.Map = new Map() { Width = update.InitializeMapWidth, Height = update.InitializeMapHeight, SeaLevel = update.InitializeMapSeaLevel, LavaLevel = update.InitializeMapLavaLevel, Music = update.InitializeMapMusic, ExtraLives = update.InitializeMapExtraLives, AmbientLight = update.InitializeMapAmbientLight };
                Main.Map.Materials = Enumerable.Repeat((byte)Material.Air, Main.Map.Width * Main.Map.Height).ToArray();
                Main.Map.WallMaterials = Enumerable.Repeat((byte)Material.Air, Main.Map.Width * Main.Map.Height).ToArray();
                Background.ClearMap();
                if (Main.Player != null)
                    Main.Player = null;

                if (spawnsTemp != null)
                    Main.Map.Spawns = spawnsTemp;
            }

            if (Main.Map != null)
            {
                Main.Map.Now = DateTime.UtcNow;
                Main.Map.GameTime = DateTime.MinValue + TimeSpan.FromSeconds(update.MapTimeOfDaySeconds);
            }
            // Maybe use this code if the packets are coming in out of order for some reason.
            // Should never happen with the TCP connections we're using.
            //else
            //{
            //    WorkQueue.Enqueue(update);
            //    return;
            //}

            if (update.MapUpdates != null)
            {
                foreach (var mapUpdate in update.MapUpdates)
                    mapUpdate.SetData(Main.Map.Materials, Main.Map.Width);
            }

            if (update.WallUpdates != null)
            {
                foreach (var mapUpdate in update.WallUpdates)
                    mapUpdate.SetData(Main.Map.WallMaterials, Main.Map.Width);
            }

            if (update.SoundEvents != null)
                Audio.PlaySoundEvents(update.SoundEvents);

            if (update.FileTransfers != null)
            {
                foreach (var transfer in update.FileTransfers)
                {
                    var filename = transfer.Save(ServerAddress, Port);
                    if (Main.Player != null)
                        Main.Player.MessageToClient("File saved to " + filename, MessageType.System);
                }
            }

            if (update.DisconnectMessage != null)
            {
                Disconnect();
                Main.ShowMessageBox(update.DisconnectMessage, tag => { Main.Quit(); }, false);
                return;
            }

            if (update.Entities != null && Main.Map != null)
            {
                foreach (var entityUpdate in update.Entities)
                {
                    var entity = Main.Map.FindEntity(entityUpdate.Id);
                    if (entity == null)
                    {
                        entity = (Entity)entityUpdate.TargetType.GetConstructor(Type.EmptyTypes).Invoke(null);
                        entity.Id = entityUpdate.Id;
                        Main.Map.AddEntity(entity);
                        //Utility.LogMessage("added entity " + entity.Id);
                    }

                    entity.ProcessUpdate(entityUpdate);

                    entity.OffsetBoundingBox = entity.BoundingBox;
                    entity.OffsetBoundingBox.Offset((int)entity.Position.X, (int)entity.Position.Y);

                    if (entityUpdate.SoundEvents != null)
                    {
                        foreach (var soundEvent in entityUpdate.SoundEvents)
                            Audio.PlaySound(soundEvent);
                    }
                }

                Main.Map.FlushEntities();
            }

            if (update.RemovedEntityIds != null)
            {
                foreach (var entityId in update.RemovedEntityIds)
                {
                    //Utility.LogMessage("removed entity " + entityId);
                    Main.Map.RemoveEntity(entityId);
                }
                Main.Map.FlushEntities();
            }

            if (update.PlayerEntityId != 0)
            {
                playerId = update.PlayerEntityId;
            }

            if (Main.Player == null && Main.Map != null)
            {
                Main.Player = Main.Map.FindEntity(playerId) as Player;
                if (Main.Player != null)
                {
                    Main.Player.IsMainPlayer = true;
                    if (inventoryTemp != null)
                    {
                        Main.Player.Inventory = inventoryTemp;
                        Main.Player.UpdateSlots();
                        inventoryTemp = null;
                    }
                }
                else
                {
                    //Utility.LogMessage("Unable to find player entity");
                    //Utility.LogMessage("On new map packet queue size: {0}, update entitiy count {1}, update player id {2}", WorkQueue.Count, update.Entities == null ? 0 : update.Entities.Count, update.PlayerEntityId);
                }
            }

            if (update.Inventory != null)
            {
                if (Main.Player == null)
                    inventoryTemp = update.Inventory;
                else
                {
                    Main.Player.Inventory = update.Inventory;
                    Main.Player.UpdateSlots();
                }
            }

            if (update.SpawnsChanged)
            {
                if (Main.Map == null)
                    spawnsTemp = update.Spawns;
                else
                {
                    if (update.Spawns == null)
                        Main.Map.Spawns.Clear();
                    else
                        Main.Map.Spawns = update.Spawns;
                }
            }
        }
#endif

        internal void MessageToServer(string text)
        {
            MessagesToServer.Enqueue(text);
        }
    }
}
