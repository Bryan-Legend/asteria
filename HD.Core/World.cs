using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using ParallelTasks;
using Microsoft.Xna.Framework.Storage;
using Ionic.Zlib;
using System.Diagnostics;

#if WINDOWS
using Ionic.Zip;
using System.Xaml;
using System.Windows.Media.Imaging;
using System.Windows.Media;
#endif

namespace HD
{
    public static class World
    {
        public static List<Command> Commands = new List<Command>();
        public static Dictionary<String, Command> CommandsByName = new Dictionary<string, Command>();

        public static List<Map> Maps = new List<Map>();
        static Dictionary<string, Map> mapsByName = new Dictionary<string, Map>();

        public static string Name { get; set; }
        public static Map Overworld { get; set; }

        public static bool IsServer { get; set; }
        public static int ServerPort = 1701;

        static Queue<Map> added = new Queue<Map>();
        static Queue<Map> removed = new Queue<Map>();

        public static bool IsConnectedToServer { get; set; }

        public static List<Assembly> LoadedPlugins = new List<Assembly>();
        
        public static StorageDevice Device;

        public static void RegisterPlugin(Assembly assembly)
        {
            Utility.LogMessage("Registering Plugin Assembly " + assembly.FullName);

            LoadedPlugins.Add(assembly);

            var mainType = assembly.GetType("Global");
            if (mainType != null)
            {
                var registerMethod = mainType.GetMethod("Register");
                registerMethod.Invoke(null, null);
            }

            RegisterCommands(assembly);

            RecipeBase.Sort();
        }

        public static void RegisterPlugins()
        {
#if WINDOWS
            World.RegisterPlugin(Assembly.GetExecutingAssembly());
            World.RegisterPlugin(Assembly.LoadFile(Path.Combine(Utility.ContentDirectory, "HD.Asteria.dll")));

            var pluginPath = Path.Combine(Utility.SavePath, Path.Combine("Content", "Plugins"));
            if (Directory.Exists(pluginPath))
            {
                foreach (var filename in Directory.GetFiles(pluginPath, "*.dll"))
                {
                    World.RegisterPlugin(Assembly.LoadFile(filename));
                }
            }
#else
            RegisterPlugin(Assembly.GetExecutingAssembly());
#endif
        }

        static void RegisterCommands(Assembly assembly)
        {
            var count = 0;

            foreach (Type t in assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(Command)))
                {
                    var constructor = t.GetConstructor(new Type[] { });
                    if (constructor != null)
                    {
                        var command = (Command)constructor.Invoke(new object[] { });
                        Commands.Add(command);
                        CommandsByName[command.Name.ToLower()] = command;
                        count++;
                    }
                }
            }

            Utility.LogMessage("{0} commands registered.", count);
        }

        public static string ExecuteCommand(Player player, string commandName)
        {
            return ExecuteCommand(player, commandName, null);
        }

        public static string ExecuteCommand(Player player, string commandName, string parameter)
        {
            commandName = commandName.ToLower();
            if (!CommandsByName.ContainsKey(commandName))
                throw new NotImplementedException("Unknown command: " + commandName);
            else
                return CommandsByName[commandName].Execute(player, parameter);
        }

        public static void MessageToServer(Player player, string message)
        {
            //CharacterStore.LogChat(Character, message);
            Utility.LogMessage(player.Name + ": " + message);

            var args = message.Split(' ');
            if (args.Length > 0)
            {
                string commandName;
                if (args[0].StartsWith("/"))
                {
                    commandName = args[0].ToLower().TrimStart('/');
                    var index = message.IndexOf(' ');
                    if (index == -1)
                        message = String.Empty;
                    else
                        message = message.Substring(index + 1);
                }
                else
                    commandName = "say";

                string result;
                if (!CommandsByName.ContainsKey(commandName))
                    result = "Unknown command";
                else
                {
                    var command = CommandsByName[commandName];
                    if (command.IsAvailable(player))
                        result = command.Execute(player, message);
                    else
                        result = "Insufficent Rights";
                }

                if (!String.IsNullOrEmpty(result))
                    player.MessageToClient(result, MessageType.System);
            }
        }

        public static IOrderedEnumerable<string> GetPlayerFilenames()
        {
#if WINDOWS
            return Directory.GetFiles(Utility.SavePath, "*.Player").OrderBy(s => s);
#else
            using (var container = GetStorageContainer())
            {
                return container.GetFileNames("*.player").OrderBy(s => s);
            }
#endif
        }

        public static IOrderedEnumerable<string> GetWorldFilenames()
        {
#if WINDOWS
            return Directory.GetFiles(Utility.SavePath, "*.World").OrderBy(s => s);
#else
            using (var container = GetStorageContainer())
            {
                return container.GetDirectoryNames().OrderBy(s => s);
            }
#endif
        }

        public static void DeleteFile(string filename)
        {
#if WINDOWS
            File.Delete(filename);
#else
            using (var container = GetStorageContainer())
            {
                container.DeleteFile(filename);
            }
#endif
        }

        public static string GetSaveFilename(string name)
        {
#if WINDOWS
            return Path.Combine(Utility.SavePath, name + ".World");
#else
            return name + ".World";
#endif
        }

        public static string SaveMap(Map map, string saveName)
        {
#if WINDOWS
            Utility.StatusMessage = "Saving World";
            var startTime = DateTime.UtcNow;

            var filename = Path.Combine(Utility.SavePath, saveName + ".Map");

            using (var saveFile = new ZipFile(filename + ".new"))
            {
                Save(map, saveFile, Name);
                saveFile.Save();
            }

            if (File.Exists(filename))
                File.Replace(filename + ".new", filename, filename + ".backup");
            else
                File.Move(filename + ".new", filename);

            Utility.LogMessage("Map Saved in " + (DateTime.UtcNow - startTime).ToString());
            Utility.StatusMessage = "";

            return filename;
#else
            throw new NotImplementedException();
#endif
        }

        public static string Save()
        {
            lock (Maps)
            {
                Utility.StatusMessage = "Saving World";
                var startTime = DateTime.UtcNow;
                var filename = GetSaveFilename(Name);

#if WINDOWS
                using (var saveFile = new ZipFile(filename + ".new"))
                {
                    foreach (var map in Maps)
                    {
                        Save(map, saveFile, Name);
                    }
                    saveFile.Save();
                }

                if (File.Exists(filename))
                    File.Replace(filename + ".new", filename, filename + ".backup");
                else
                    File.Move(filename + ".new", filename);
#else
                using (var container = GetStorageContainer())
                {
                    if (container.DirectoryExists(filename))
                    {
                        foreach (var file in container.GetFileNames(Path.Combine(filename, "*")))
                            container.DeleteFile(file);
                        container.DeleteDirectory(filename);
                    }

                    container.CreateDirectory(filename);

                    foreach (var map in Maps)
                    {
                        Save(map, container, filename);
                    }
                }
#endif

                Utility.LogMessage("World Saved in " + (DateTime.UtcNow - startTime).ToString());
                Utility.StatusMessage = "";
                return filename;
            }
        }

        public static StorageContainer GetStorageContainer()
        {            
            var result = Device.BeginOpenContainer("Save Game", null, null);
            result.AsyncWaitHandle.WaitOne();
            var container = Device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            return container;
        }

#if WINDOWS
        static void Save(Map map, ZipFile saveFile, string worldName)
#else
        static void Save(Map map, StorageContainer container, string directoryName)
#endif

        {
            lock (map)
            {
                map.IsPaused = true;

                lock (map.Entities)
                {
                    foreach (var player in map.Players)
                        map.Entities.Remove(player);

                    var spawnEntities = new List<Entity>();
                    foreach (var entity in map.Entities.ToArray())
                    {
                        var enemy = entity as Enemy;
                        if (enemy != null && enemy.Spawn != null)
                        {
                            map.Entities.Remove(enemy);
                            spawnEntities.Add(enemy);
                        }
                    }

#if WINDOWS
                    saveFile.UpdateEntry(Name + map.Name + ".xaml", XamlServices.Save(map));
                    saveFile.UpdateEntry(Name + map.Name + ".data", map.Materials);
                    saveFile.UpdateEntry(Name + map.Name + ".Walls.data", map.WallMaterials);

                    SaveMapImage(map, worldName);
#else
                    using (var stream = container.CreateFile(Path.Combine(directoryName, Name + map.Name + ".protobuf")))
                    {
                        ProtoBuf.Serializer.Serialize(stream, map);
                    }
                    using (var stream = new DeflateStream(container.CreateFile(Path.Combine(directoryName, Name + map.Name + ".data")), CompressionMode.Compress))
                    {
                        stream.Write(map.Materials, 0, map.Materials.Length);
                    }
                    using (var stream = new DeflateStream(container.CreateFile(Path.Combine(directoryName, Name + map.Name + ".Walls.data")), CompressionMode.Compress))
                    {
                        stream.Write(map.WallMaterials, 0, map.WallMaterials.Length);
                    }
#endif

                    foreach (var player in map.Players)
                        map.Entities.Add(player);

                    foreach (var entity in spawnEntities)
                        map.Entities.Add(entity);
                }

                map.IsPaused = false;
            }
        }

        static void SaveMapImage(Map map, string worldName)
        {
#if WINDOWS
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, "html"));

            var filename = Path.Combine(Utility.SavePath, Path.Combine("html", worldName + "." + map.Name + ".png"));

            var colors = new List<System.Windows.Media.Color>();
            foreach (var materialInfo in MaterialInfo.MaterialTypes)
            {
                colors.Add(System.Windows.Media.Color.FromArgb(materialInfo.DrawColor.A, materialInfo.DrawColor.R, materialInfo.DrawColor.G, materialInfo.DrawColor.B));
            }

            var palette = new BitmapPalette(colors);
            var image = BitmapSource.Create(map.Width, map.Height, 96, 96, PixelFormats.Indexed8, palette, map.Materials, map.Width);

            using (var stream = new FileStream(filename, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Interlace = PngInterlaceOption.Off;
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }
#endif
        }

#if WINDOWS
        public static string GetMapName(string filename)
        {
            using (var saveFile = new ZipFile(filename))
            {
                foreach (var zipEntryName in saveFile.EntryFileNames)
                {
                    if (zipEntryName.EndsWith(".xaml"))
                    {
                        using (var reader = saveFile[zipEntryName].OpenReader())
                        {
                            var result = XamlServices.Load(reader) as Map;
                            if (result != null)
                                return result.Name;
                        }
                    }
                }
            }

            return null;
        }
#endif

        public static Map Load(string filename)
        {
            Map result = null;
            Utility.LogMessage("Loading World " + filename);

            var isWorld = filename.ToUpperInvariant().EndsWith(".WORLD");
            if (isWorld)
            {
                Maps.Clear();
                mapsByName.Clear();
            }

#if WINDOWS
            using (var saveFile = new ZipFile(filename))
            {
                foreach (var zipEntryName in saveFile.EntryFileNames)
                {
                    if (zipEntryName.EndsWith(".xaml"))
                    {
                        result = Load(saveFile, zipEntryName.Substring(0, zipEntryName.Length - ".xaml".Length));
                        AddMap(result);
                    }
                }

                if (isWorld)
                {
                    Name = Path.GetFileNameWithoutExtension(filename);
                    FlushMaps();
                    Overworld = GetMap("Overworld");
                }
            }
#else
            using (var container = GetStorageContainer())
            {
                var files = container.GetFileNames(Path.Combine(filename, "*"));
                foreach (var file in files)
                    Debug.WriteLine(file);

                foreach (var file in container.GetFileNames(Path.Combine(filename, "*")))
                {
                    if (file.EndsWith(".protobuf"))
                    {
                        result = Load(container, file.Substring(0, file.Length - ".protobuf".Length));
                        AddMap(result);
                    }
                }

                if (isWorld)
                {
                    Name = Path.GetFileNameWithoutExtension(filename);
                    FlushMaps();
                    Overworld = GetMap("Overworld");
                }
            }
#endif

            return result;
        }

#if WINDOWS
        static Map Load(ZipFile saveFile, string name)
#else
        static Map Load(StorageContainer container, string name)
#endif
        {
            Map result;
#if WINDOWS
            using (var reader = saveFile[name + ".xaml"].OpenReader())
                result = XamlServices.Load(reader) as Map;

            result.Materials = new byte[result.Width * result.Height];
            result.WallMaterials = new byte[result.Width * result.Height];
            result.Now = DateTime.UtcNow;

            using (var reader = saveFile[name + ".data"].OpenReader())
            {
                reader.Read(result.Materials, 0, result.Materials.Length);
                CheckForInvalidMaterials(result.Materials);
            }

            using (var reader = saveFile[name + ".Walls.data"].OpenReader())
            {
                reader.Read(result.WallMaterials, 0, result.WallMaterials.Length);
                CheckForInvalidMaterials(result.Materials);
            }
#else
            using (var stream = container.OpenFile(name + ".protobuf", FileMode.Open))
            {
                result = ProtoBuf.Serializer.Deserialize<Map>(stream);
            }

            result.Materials = new byte[result.Width * result.Height];
            result.WallMaterials = new byte[result.Width * result.Height];
            result.Now = DateTime.UtcNow;

            using (var stream = new DeflateStream(container.OpenFile(name + ".data", FileMode.Open), CompressionMode.Decompress))
            {
                stream.Read(result.Materials, 0, result.Materials.Length);
            }
            using (var stream = new DeflateStream(container.OpenFile(name + ".Walls.data", FileMode.Open), CompressionMode.Decompress))
            {
                stream.Read(result.WallMaterials, 0, result.WallMaterials.Length);
            }
#endif

            foreach (var entity in result.Entities)
            {
                entity.Map = result;
                entity.Created = result.Now.AddMilliseconds(-Utility.Next(1000));

                if (entity is Player || entity is Projectile)
                    result.RemoveEntity(entity);

                var enemy = entity as Enemy;
                if (enemy != null)
                {
                    if (enemy.Type == null)
                        result.RemoveEntity(entity);
                    else
                    {
                        enemy.Initalize();
                    }
                }

                var placeable = entity as Placeable;
                if (placeable != null && (placeable.Type == null || placeable.Type.IsTemporary))
                    result.RemoveEntity(placeable);

                if (entity.IsSolid)
                    result.SolidEntities.Add(entity);

                result.EntitiesById[entity.Id] = entity;
            }

            foreach (var spawn in result.Spawns.ToArray())
            {
                if (spawn.Type == null)
                    result.Spawns.Remove(spawn);
            }

            result.FlushEntities();

            Utility.LogMessage(String.Format("{0} Loaded. {1}x{2} {3} Entities", name, result.Width, result.Height, result.Entities.Count));

            return result;
        }

        static void CheckForInvalidMaterials(byte[] materialData)
        {
            var maxMaterial = (byte)MaterialInfo.MaterialTypes.Length;
            Parallel.For(0, materialData.Length, (i) =>
            {
                if (materialData[i] >= maxMaterial)
                    materialData[i] = (byte)Material.Air;
            });
        }

        public static void CreateWorld(string name)
        {
            Maps.Clear();
            mapsByName.Clear();

            Name = name;
#if WINDOWS
            Overworld = CreateMap("Overworld", 6400, 5600); // overworld best be multiple of 400 for map generator hill calculations
#else
            Overworld = CreateMap("Overworld", 4000, 5200); // overworld best be multiple of 400 for map generator hill calculations
#endif

            var mapGenerator = CommandsByName["mapgenerate"] as IMapGenerator;
            if (mapGenerator == null)
                throw new Exception("Unable to find a mapgenerate command that implements IMapGenerator interface.");
            mapGenerator.Generate(Overworld);

#if WINDOWS
            foreach (var filename in Directory.GetFiles(Path.Combine(Utility.ContentDirectory, "Maps"), "*.map"))
                Load(filename);
#endif

            FlushMaps();

            Save();
        }

        public static Map CreateMap(string name, int width, int height)
        {
            var newMap = new Map() { Name = name, Width = width, Height = height };
            AddMap(newMap);
            return newMap;
        }

        public static bool MapExists(string name)
        {
            name = name.ToUpperInvariant();
            return mapsByName.ContainsKey(name);
        }

        static void AddMap(Map newMap)
        {
            // clean any items that no longer have types
            foreach (var entity in newMap.Entities)
            {
                var placeable = entity as Placeable;
                if (placeable != null && placeable.Inventory != null)
                {
                    foreach (var item in placeable.Inventory.ToArray())
                    {
                        if (item != null && item.Type == null)
                            placeable.RemoveItem(item);
                    }
                }
            }

            if (newMap.Id == 0 || (from m in Maps where m.Id == newMap.Id select m).Count() > 0) // if id is 0 or a map already exists with that id
                newMap.Id = GetNextMapId();

            added.Enqueue(newMap);
        }

        public static Map GetMap(string name)
        {
            if (name == null)
                return null;

            Map result = null;
            if (mapsByName.TryGetValue(name.ToUpperInvariant(), out result))
                return result;
            return null;
        }

        public static void DeleteMap(Map map)
        {
            foreach (var p in map.Players)
            {
                p.SendTo(World.Overworld);
            }
            removed.Enqueue(map);
        }

        public static void RenameMap(Map map, string name)
        {
            mapsByName.Remove(map.Name.ToUpperInvariant());
            map.Name = name;
            mapsByName[map.Name.ToUpperInvariant()] = map;
        }

        public static void Broadcast(string message, MessageType messageType)
        {
            Utility.LogMessage(message);

            foreach (var map in Maps)
            {
                foreach (var p in map.Players)
                {
                    p.MessageToClient(message, messageType);
                }
            }
        }

        internal static int GetNextMapId()
        {
            var result = 0;

            foreach (var map in Maps)
            {
                if (map.Id > result)
                    result = map.Id;
            }

            return result + 1;
        }

        public static string ResetMap(Map map)
        {
            if (removed.Contains(map)) // Don't reset on leave if it's already manual being reset.
                return null;

            var filename = Path.Combine(Utility.SavePath, map.Name + ".map");
            var saveFilename = filename;
            if (!File.Exists(filename))
                filename = Path.Combine(Utility.ContentDirectory, Path.Combine("Maps", map.Name + ".map"));
            if (!File.Exists(filename))
            {
                var result = String.Format("Unable to reset map {0}. {1} and {2} not found.", map.Name, saveFilename, filename);
                Utility.LogMessage(result);
                return result;
            }

            removed.Enqueue(map);

            var newMap = Load(filename);

            //map.FlushEntities();
            foreach (var player in map.Players)
            {
                var newPlayer = player.SendTo(newMap, player.Position);
                if (newPlayer.IsDead)
                    newPlayer.Respawn();
            }

            Utility.LogMessage(newMap.Name + " reset");
            return null;
        }

        public static void Update(GameTime gameTime, Action<Map> processBeforeEntitesFlushed = null)
        {
            // cap the max game tick lenght at 0.1 second so that everything doesn't die from instant built up gravity when the server lags.
            if (gameTime.ElapsedGameTime.TotalSeconds > 0.1)
                gameTime = new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(0.1));

            //Utility.LogMessage("Time Elapsed: " + gameTime.ElapsedGameTime);

            foreach (var map in Maps)
            {
                lock (map)
                {
                    if (map.Players.Count > 0 && !map.IsPaused)
                    {
                        map.TerrainThink(gameTime);
                        map.EntityThink(gameTime);
                        map.SpawnThink(gameTime);

                        if (processBeforeEntitesFlushed != null)
                            processBeforeEntitesFlushed(map);
                    }

                    map.FlushEntities();
                }
            }

            FlushMaps();
        }

        static void FlushMaps()
        {
            lock (Maps)
            {
                while (removed.Count > 0)
                {
                    var map = removed.Dequeue();
                    mapsByName.Remove(map.Name.ToUpperInvariant());
                    Maps.Remove(map);
                }

                while (added.Count > 0)
                {
                    var map = added.Dequeue();

                    if (MapExists(map.Name))
                        throw new Exception("Map named " + map.Name + " already exists.");

                    mapsByName[map.Name.ToUpperInvariant()] = map;
                    Maps.Add(map);
                }
            }
        }
    }
}