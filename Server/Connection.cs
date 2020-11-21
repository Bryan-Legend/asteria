using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using HD;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using ProtoBuf;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Ionic.Zlib;

namespace Server
{
    public class Connection
    {
        public Player Player;

        TcpClient client;
        Thread receiveThread;
        Thread sendThread;
        AutoResetEvent sendThreadWakeEvent = new AutoResetEvent(false);
        DeflateStream compressedSendStream;
        NetworkStream networkStream;
        Dictionary<int, EntityUpdate> sentUpdates = new Dictionary<int, EntityUpdate>();
        Queue<byte[]> sendQueue = new Queue<byte[]>();
        int lastPlayerEntityId;
        volatile bool isDisconnected;

        public Connection(TcpClient client)
        {
            this.client = client;
            client.Client.SendBufferSize = 65535;
            client.Client.NoDelay = true;
            networkStream = client.GetStream();
            compressedSendStream = new DeflateStream(networkStream, CompressionMode.Compress, true);
            compressedSendStream.FlushMode = FlushType.Sync;

            receiveThread = new Thread(new ThreadStart(ReadThread));
            receiveThread.Start();

            sendThread = new Thread(new ThreadStart(SendThread));
            sendThread.Start();

            MasterServer.Connections.Add(this);

            Utility.LogMessage("Connection from " + client.Client.RemoteEndPoint);
        }

        public void Login(string loginName, string password, string version, int skin)
        {
            string errorMessage = null;

            //if (version != null && version != Utility.Version) {
            //    errorMessage = String.Format("Client version ({0}) is not the same as the server version ({1}). ", version, Utility.Version);
            //} else {
                if (File.Exists(Player.GetSaveFilename(loginName))) {
                    var player = Player.Load(Player.GetSaveFilename(loginName));
                    if (!String.IsNullOrEmpty(player.Password) && player.Password != password)
                        errorMessage += "Incorrect password.";
                    else {
                        player.Password = password;
                        Player = player;
                    }
                } else {
                    errorMessage = CreatePlayer(loginName, password);
                }
            //}

            if (errorMessage == null) {
                Player.Skin = skin;

                if (String.IsNullOrEmpty(Player.SaveWorldName) || Player.SaveWorldName == World.Name) {
                    Player.Map = World.GetMap(Player.SaveMap);
                } else {
                    Player.StartPosition = Vector2.Zero;
                }

                if (Player.Map == null)
                    Player.Map = World.Overworld;

                lock (Player.Map) {
                    Player.Map.AddEntity(Player);
                }
                Player.Respawn();
                Player.IsMapChanged = true;

                World.Broadcast(Player.Name + " connected.", MessageType.System);
            } else {
                SendUpdate(new UpdateToClient() { DisconnectMessage = errorMessage });
                Utility.LogMessage(errorMessage);
            }
        }

        string CreatePlayer(string name, string password)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid) {
                name = name.Replace(c.ToString(), "");
            }

            if (name.Length < 3) {
                return "Player name must be at least 3 letters.";
            }

            if (File.Exists(Player.GetSaveFilename(name))) {
                return "There is already a player with that name.";
            }

            Player = new Player() { Name = name, Password = password };
            Player.Save();

            return null;
        }

        void ReadThread()
        {
            while (true) {
                try {
                    // blocks until a client sends a message
                    var update = ProtoBuf.Serializer.DeserializeWithLengthPrefix<UpdateToServer>(networkStream, PrefixStyle.Base128);

                    if (update.Disconnect) {
                        break;
                    }

                    if (update.ResolutionX != 0) {
                        Player.ClientResolutionWidth = update.ResolutionX;
                        Player.ClientResolutionHeight = update.ResolutionY;
                        terrainBuffer = null;
                        wallBuffer = null;
                    }

                    if (update.LoginName != null) {
                        Login(update.LoginName, update.Password, update.Version, update.Skin);
                    }
                    //Utility.LogMessage("UpdateToServer recieved");

                    if (Player != null) {
                        lock (Player.Map) {
                            Player.Keyboard = new KeyboardState(update.PressedKeys);
                            Player.MousePosition = new Point(update.MouseX, update.MouseY);
                            Player.MouseLeftButtonPressed = update.MouseLeftButtonPressed;
                            Player.MouseRightButtonPressed = update.MouseRightButtonPressed;
                            Player.SelectedActionSlot = update.SelectedActionSlot;
                            Player.GamePad = Utility.GamePadStateFromBytes(update.GamePad);

                            if (update.DragSlotType != SlotType.None)
                                Player.DoDrag(update.DragSlotType, update.DragSlotNumber, update.DragAmount);
                            if (update.DropSlotType != SlotType.None)
                                Player.DoDrop(update.DropSlotType, update.DropSlotNumber);

                            if (update.CraftRecipe != 0) {
                                Player.CraftRecipe = update.CraftRecipe;
                                Player.CraftAmount = update.CraftAmount;
                            }

                            if (update.SortInventory)
                                Player.SortInventory = true;

                            if (update.SetValue != null && Player.ActivePlaceable != null && Player.ActivePlaceable.CanEdit(Player))
                                Player.ActivePlaceable.Value = update.SetValue;

                            if (update.ClearActivePlaceable)
                                Player.ActivePlaceable = null;
                            if (update.ClearCreativeMode)
                                Player.CreativeMode = CreativeMode.None;
                            if (update.ToggleGodMode && Player.IsAdmin)
                                Player.IsGod = !Player.IsGod;
                            if (update.SetCreativeMode != null && Player.IsAdmin)
                                Player.CreativeMode = update.SetCreativeMode ?? CreativeMode.None;
                            if (update.ReturnToOverworld)
                                Player.ReturnToOverworld();

                            if (update.Messages != null) {
                                foreach (var message in update.Messages)
                                    World.MessageToServer(Player, message);
                            }
                        }
                    }

                    if (isDisconnected)
                        return;
                } catch (SocketException) {
                    //a socket error has occured
                    break;
                } catch (IOException) {
                    //a socket error has occured
                    break;
                } catch (Exception e) {
                    // an unknown exception
                    Utility.LogMessage(e.ToString());
                    break;
                }
            }

            Disconnect();
        }

        Rectangle terrainBufferBounds;
        byte[] terrainBuffer;
        byte[] wallBuffer;

        void TerrainUpdate(UpdateToClient update)
        {
            if (Player.ClientResolutionWidth != 0) {
                var lastTerrainBufferBounds = terrainBufferBounds;
                var playerScreenOffset = Player.GetScreenOffset();
                terrainBufferBounds = new Rectangle(playerScreenOffset.X / Map.BlockWidth - Utility.LightingMargin, playerScreenOffset.Y / Map.BlockHeight - Utility.LightingMargin, Player.ClientResolutionWidth / Map.BlockWidth + Utility.LightingMargin + Utility.LightingMargin, Player.ClientResolutionHeight / Map.BlockHeight + Utility.LightingMargin + Utility.LightingMargin);

                if (terrainBufferBounds.Right >= Player.Map.Width)
                    terrainBufferBounds.X = Player.Map.Width - terrainBufferBounds.Width;
                if (terrainBufferBounds.X < 0)
                    terrainBufferBounds.X = 0;

                if (terrainBufferBounds.Bottom >= Player.Map.Height)
                    terrainBufferBounds.Y = Player.Map.Height - terrainBufferBounds.Height;
                if (terrainBufferBounds.Y < 0)
                    terrainBufferBounds.Y = 0;

                update.MapUpdates = CreateUpdates(ref terrainBuffer, lastTerrainBufferBounds, terrainBufferBounds, Player.Map.Materials, Player.Map.Width);
                update.WallUpdates = CreateUpdates(ref wallBuffer, lastTerrainBufferBounds, terrainBufferBounds, Player.Map.WallMaterials, Player.Map.Width);
            }
        }

        static List<MapUpdate> CreateUpdates(ref byte[] buffer, Rectangle lastTerrainBufferBounds, Rectangle terrainBufferBounds, byte[] sourceData, int sourceDataWidth)
        {
            var result = new List<MapUpdate>();

            if (buffer == null) {
                var newMapUpdate = new MapUpdate() { X = terrainBufferBounds.X, Y = terrainBufferBounds.Y, Width = terrainBufferBounds.Width, Height = terrainBufferBounds.Height };
                newMapUpdate.LoadData(sourceData, sourceDataWidth);
                buffer = newMapUpdate.MapData;
                result.Add(newMapUpdate);
            } else {
                if (terrainBufferBounds.X > lastTerrainBufferBounds.X) {
                    var xOffset = terrainBufferBounds.X - lastTerrainBufferBounds.X;
                    var newMapUpdate = new MapUpdate() { X = terrainBufferBounds.Right - xOffset, Y = terrainBufferBounds.Y, Width = xOffset, Height = terrainBufferBounds.Height };
                    newMapUpdate.LoadData(sourceData, sourceDataWidth);
                    result.Add(newMapUpdate);

                    // offset and update terrainbuffer
                    if (xOffset < terrainBufferBounds.Width) {
                        for (var row = 0; row < terrainBufferBounds.Height; row++)
                            Array.Copy(buffer, row * terrainBufferBounds.Width + xOffset, buffer, row * terrainBufferBounds.Width, terrainBufferBounds.Width - xOffset);
                        LoadData(newMapUpdate.X, newMapUpdate.Y, newMapUpdate.Width, newMapUpdate.Height, terrainBufferBounds.Width - xOffset, 0, buffer, sourceData, sourceDataWidth);
                    } else
                        LoadData(terrainBufferBounds.X, terrainBufferBounds.Y, terrainBufferBounds.Width, terrainBufferBounds.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                } else if (terrainBufferBounds.X < lastTerrainBufferBounds.X) {
                    var xOffset = lastTerrainBufferBounds.X - terrainBufferBounds.X;
                    var newMapUpdate = new MapUpdate() { X = terrainBufferBounds.Left, Y = terrainBufferBounds.Y, Width = xOffset, Height = terrainBufferBounds.Height };
                    newMapUpdate.LoadData(sourceData, sourceDataWidth);
                    result.Add(newMapUpdate);

                    // offset and update terrainbuffer
                    if (xOffset < terrainBufferBounds.Width) {
                        for (var row = 0; row < terrainBufferBounds.Height; row++)
                            Array.Copy(buffer, row * terrainBufferBounds.Width, buffer, row * terrainBufferBounds.Width + xOffset, terrainBufferBounds.Width - xOffset);
                        LoadData(newMapUpdate.X, newMapUpdate.Y, newMapUpdate.Width, newMapUpdate.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                    } else
                        LoadData(terrainBufferBounds.X, terrainBufferBounds.Y, terrainBufferBounds.Width, terrainBufferBounds.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                }

                if (terrainBufferBounds.Y > lastTerrainBufferBounds.Y) {
                    var yOffset = terrainBufferBounds.Y - lastTerrainBufferBounds.Y;
                    var newMapUpdate = new MapUpdate() { X = terrainBufferBounds.X, Y = terrainBufferBounds.Bottom - yOffset, Width = terrainBufferBounds.Width, Height = yOffset };
                    newMapUpdate.LoadData(sourceData, sourceDataWidth);
                    result.Add(newMapUpdate);

                    // offset and update terrainbuffer
                    if (yOffset < terrainBufferBounds.Height) {
                        Array.Copy(buffer, terrainBufferBounds.Width * yOffset, buffer, 0, buffer.Length - terrainBufferBounds.Width * yOffset);
                        LoadData(newMapUpdate.X, newMapUpdate.Y, newMapUpdate.Width, newMapUpdate.Height, 0, terrainBufferBounds.Height - yOffset, buffer, sourceData, sourceDataWidth);
                    } else
                        LoadData(terrainBufferBounds.X, terrainBufferBounds.Y, terrainBufferBounds.Width, terrainBufferBounds.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                } else if (terrainBufferBounds.Y < lastTerrainBufferBounds.Y) {
                    var yOffset = lastTerrainBufferBounds.Y - terrainBufferBounds.Y;
                    var newMapUpdate = new MapUpdate() { X = terrainBufferBounds.X, Y = terrainBufferBounds.Top, Width = lastTerrainBufferBounds.Width, Height = yOffset };
                    newMapUpdate.LoadData(sourceData, sourceDataWidth);
                    result.Add(newMapUpdate);

                    // offset and update terrainbuffer
                    if (yOffset < terrainBufferBounds.Height) {
                        Array.Copy(buffer, 0, buffer, terrainBufferBounds.Width * yOffset, buffer.Length - terrainBufferBounds.Width * yOffset);
                        LoadData(newMapUpdate.X, newMapUpdate.Y, newMapUpdate.Width, newMapUpdate.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                    } else
                        LoadData(terrainBufferBounds.X, terrainBufferBounds.Y, terrainBufferBounds.Width, terrainBufferBounds.Height, 0, 0, buffer, sourceData, sourceDataWidth);
                }

                // scan for terrain change list
                for (var y = 0; y < terrainBufferBounds.Height; y++) {
                    for (var x = 0; x < terrainBufferBounds.Width; x++) {
                        var index = y * terrainBufferBounds.Width + x;
                        var sourceOffset = terrainBufferBounds.X + x + ((terrainBufferBounds.Y + y) * sourceDataWidth);
                        if (sourceOffset >= 0 && sourceOffset < sourceData.Length) {
                            var material = sourceData[sourceOffset];
                            if (buffer[index] != material) {
                                result.Add(new MapUpdate() { X = terrainBufferBounds.X + x, Y = terrainBufferBounds.Y + y, Material = (Material)material });
                                buffer[index] = material;
                            }
                        }
                    }
                }
            }

            return result;
        }

        static void LoadData(int x, int y, int width, int height, int destX, int destY, byte[] terrainBuffer, byte[] sourceData, int sourceDataWidth)
        {
            for (var row = 0; row < height; row++) {
                var offset = (y + row) * sourceDataWidth + x;
                var length = width;
                if (offset > sourceData.Length || offset + length < 0)
                    continue;
                if (offset + length >= sourceData.Length)
                    length = sourceData.Length - offset;
                else if (offset < 0) {
                    length += offset;
                    offset = 0;
                }

                Array.Copy(sourceData, offset, terrainBuffer, (row + destY) * width + destX, length);
            }
        }

        public void Disconnect()
        {
            if (!isDisconnected) {
                isDisconnected = true;
                MasterServer.Connections.Remove(this);

                if (client != null) {
                    client.Close();
                    client = null;
                }

                if (Player != null) {
                    World.Broadcast(Player.Name + " disconnected.", MessageType.System);

                    Player.Save();

                    Player.Map.RemoveEntity(Player);
                    Player = null;

                    World.Save();
                }

                MasterServer.ForceUpdate = true;

                sendThreadWakeEvent.Set(); // wake the send thread so that it will exit.
                sendThreadWakeEvent.Dispose();

                sendThread.Abort(); // these are very likely aborting the currently running thread. SO DON'T PUT ANY CODE AFTER THIS OR IT WON'T GET RUN!
                receiveThread.Abort();

                // DO NOT PUT CODE HERE.
            }
        }

        internal UpdateToClient PrepareUpdateToClient(int[] removedEntityIds)
        {
            var dirty = false;
            var result = new UpdateToClient();

            if (Player.IsMapChanged) {
                result.InitializeMapWidth = Player.Map.Width;
                result.InitializeMapHeight = Player.Map.Height;
                result.InitializeMapSeaLevel = Player.Map.SeaLevel;
                result.InitializeMapLavaLevel = Player.Map.LavaLevel;
                result.InitializeMapMusic = Player.Map.Music;
                result.InitializeMapExtraLives = Player.Map.ExtraLives;
                result.InitializeMapAmbientLight = Player.Map.AmbientLight;
                result.PlayerEntityId = Player.Id;
                dirty = true;
                sentUpdates.Clear();
                terrainBuffer = null;
                wallBuffer = null;
                Player.IsMapChanged = false;
                Player.InventoryChanged = true;

                //Utility.LogMessage("ID: {0} Map {1}", Player.Id, Player.Map.Name);
            }

            result.MapTimeOfDaySeconds = (int)Player.Map.GameTime.TimeOfDay.TotalSeconds;
            result.SoundEvents = Player.Map.SoundEvents.ToArray();

            Player.UpdateScreenRectangle(Player.GetScreenOffset(), Utility.LightingMargin * Map.BlockHeight);

            var additionalRemovedEntityIds = new List<int>();
            var updates = new List<EntityUpdate>();
            foreach (var entity in Player.Map.Entities) {
                if (Player.IsOnScreen(entity)) {
                    EntityUpdate previous;
                    var update = entity.PrepareForWire(sentUpdates.TryGetValue(entity.Id, out previous) ? previous : null, Player);
                    if (update != null) {
                        updates.Add(update);
                        if (previous == null)
                            sentUpdates[entity.Id] = update;
                    }
                } else {
                    if (entity != Player && sentUpdates.ContainsKey(entity.Id)) {
                        additionalRemovedEntityIds.Add(entity.Id);
                        sentUpdates.Remove(entity.Id);
                    }
                }
            }

            if (updates.Count > 0) {
                result.Entities = updates;
                dirty = true;
            }

            if (additionalRemovedEntityIds.Count > 0)
                removedEntityIds = removedEntityIds.Concat(additionalRemovedEntityIds).ToArray();

            if (removedEntityIds.Length > 0) {
                result.RemovedEntityIds = removedEntityIds;
                dirty = true;
            }

            if (lastPlayerEntityId != Player.Id) {
                result.PlayerEntityId = Player.Id;
                lastPlayerEntityId = Player.Id;
                dirty = true;
            }

            TerrainUpdate(result);
            if (result.MapUpdates != null && result.MapUpdates.Count > 0)
                dirty = true;
            if (result.WallUpdates != null && result.WallUpdates.Count > 0)
                dirty = true;

            if (Player.InventoryChanged) {
                result.Inventory = Player.Inventory;
                Player.InventoryChanged = false;
                dirty = true;
            }

            if (Player.IsMapSpawnDataChanged) {
                result.SpawnsChanged = true;
                result.Spawns = Player.Map.Spawns;
                Player.IsMapSpawnDataChanged = false;
                dirty = true;
            }

            if (Player.PendingFileTransfers.Count > 0) {
                result.FileTransfers = Player.PendingFileTransfers.ToArray();
                Player.PendingFileTransfers.Clear();
                dirty = true;
            }

            return dirty ? result : null;
        }

        internal void Update(int[] removedEntityIds)
        {
            if (Player != null) {
                if (client != null && client.Connected) {
                    var packet = PrepareUpdateToClient(removedEntityIds);
                    if (packet != null)
                        SendUpdate(packet);
                } else {
                    Disconnect();
                }
            }
        }

        void SendThread()
        {
            while (true) {
                try {
                    while (sendQueue.Count > 0) {
                        var update = sendQueue.Dequeue();

                        //var timer = new Stopwatch();
                        //timer.Start();

                        //long startTotalIn = 0;
                        //long startTotalOut = 0;

                        //if (compressedSendStream.Position != 0)
                        //{
                        //    startTotalIn = compressedSendStream.TotalIn;
                        //    startTotalOut = compressedSendStream.TotalOut;
                        //}

                        compressedSendStream.Write(update, 0, update.Length);

                        //Utility.LogMessage("Packet Size: {0} ({1} uncompressed) in {2:N2}ms", compressedSendStream.TotalOut - startTotalOut, compressedSendStream.TotalIn - startTotalIn, timer.Elapsed.TotalMilliseconds);
                    }
                } catch (SocketException) {
                    //a socket error has occured
                    break;
                } catch (IOException) {
                    //a socket error has occured
                    break;
                } catch (Exception e) {
                    // an unknown exception
                    Utility.LogMessage(e.ToString());
                    break;
                }

                sendThreadWakeEvent.WaitOne();

                if (isDisconnected)
                    return;
            }

            Disconnect();
        }

        void SendUpdate(UpdateToClient update)
        {
            // have to serialze here instead of using streams because there is no guarentee that the update objects won't get updated in the future before they are actually sent.
            // Really wish I didn't have to copy the data so much. It's just CPU and memory churn right?
            using (var stream = new MemoryStream()) {
                Serializer.SerializeWithLengthPrefix(stream, update, PrefixStyle.Base128);
                sendQueue.Enqueue(stream.ToArray());
            }

            sendThreadWakeEvent.Set();
        }
    }
}