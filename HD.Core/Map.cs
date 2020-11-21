using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Runtime.Serialization;
using ProtoBuf;
using ParallelTasks;

namespace HD
{
    [ProtoContract]
    public partial class Map
    {
        public const int BlockHeight = 8;
        public const int BlockWidth = 8;
        public const double TerrainUpdateInterval = 1.0 / 6.0;
        public const double SpawnUpdateInterval = 0.25;
        public const float DefaultGravity = 9.81f * 250;

        [ProtoMember(3)]
        public int Id { get; set; }

        int width;
        [ProtoMember(1)]
        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                PixelWidth = width * BlockWidth;
            }
        }

        int height;

        [ProtoMember(2)]
        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                PixelHeight = height * BlockHeight;
            }
        }

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        [ProtoMember(4)]
        public string Name { get; set; }
        //[ProtoMember(5)]
        public List<Entity> Entities { get; set; }
        [ProtoMember(6)]
        public List<Spawn> Spawns { get; set; }
        [ProtoMember(7)]
        public int NextEntityId { get; set; }
        //[TypeConverter(typeof(Vector2Converter))]
        //[ProtoMember(8, DataFormat=DataFormat.FixedSize)]
        public Vector2 StartingPosition { get; set; }
        [DefaultValue(DefaultGravity)]
        public float Gravity { get; set; }
        [DefaultValue(false)]
        public bool IsAutospawn { get; set; }
        [DefaultValue(false)]
        public bool IsTerrainLocked { get; set; }
        [DefaultValue(false)]
        public bool IsResetOnLeave { get; set; }
        [DefaultValue(false)]
        public bool IsResetOnDie { get; set; }
        [DefaultValue(0)]
        public int Tier { get; set; }
        [DefaultValue(0)]
        public int LockTier { get; set; }
        public string Music { get; set; }
        [DefaultValue(0)]
        public int ExtraLives { get; set; }
        [DefaultValue(0)]
        public byte AmbientLight { get; set; }
        [DefaultValue(true)]
        public bool DespawnOutOfRangeEnemies { get; set; }

        // this property is only here to avoid blowing up the save games. Remove after all maps are updated.
        //[DefaultValue(0)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public int LifeLimit
        //{
        //    get { return ExtraLives; }
        //    set { ExtraLives = value; }
        //}
        
        public Dictionary<int, Entity> EntitiesById;
        public List<Player> Players;
        public DateTime Now;
        public DateTime GameTime = DateTime.MinValue.AddHours(9);
        public bool IsPaused;
        //[ProtoMember(3)]
        public byte[] Materials;
        //[ProtoMember(4)]
        public byte[] WallMaterials;
        public List<Entity> SolidEntities = new List<Entity>();

        public List<SoundEvent> SoundEvents = new List<SoundEvent>();

        DateTime nextLongThink;

        List<Entity> addedEntities = new List<Entity>();
        List<Entity> removedEntities = new List<Entity>();

        // this property is only here to avoid blowing up the save games. Remove after all maps are updated.
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public int SpaceLevel { get; set; }

        public int SeaLevel { get; set; }
        public int SeaLevelInPixels { get { return SeaLevel * BlockHeight; } }
        public int LavaLevel { get; set; }
        public int LavaLevelInPixels { get { return LavaLevel * BlockHeight; } }

        public bool IsNight { get { return GameTime.Hour < 6 || GameTime.Hour > 18; } }

        public Map()
        {
            Gravity = DefaultGravity;
            Entities = new List<Entity>();
            EntitiesById = new Dictionary<int, Entity>();
            NextEntityId = 1;
            DespawnOutOfRangeEnemies = true;
            Spawns = new List<Spawn>();
            Players = new List<Player>();
        }

        public void RenderBrush(Vector2 position, Brush brush, Material material, int strength = 9)
        {
            RenderBrush(new Point((int)position.X, (int)position.Y), brush, material, strength);
        }

        public void RenderBrush(Point position, Brush brush, Material material, int strength = 9)
        {
            if (material > 0 && (int)material < MaterialInfo.MaterialTypes.Length) {
                var x = position.X / Map.BlockWidth - 8;
                var y = position.Y / Map.BlockWidth - 7;

                foreach (Point pixel in new BrushWalker(brush)) {
                    var targetMaterial = GetMaterial(x + pixel.X, y + pixel.Y);
                    if (MaterialInfo.Get(targetMaterial).Hardness <= strength)
                        SetMaterial(x + pixel.X, y + pixel.Y, material);
                }
            }
        }

        public void RenderWallBrush(Vector2 position, Brush brush, Material material, bool gasOnly = false)
        {
            RenderWallBrush(new Point((int)position.X, (int)position.Y), brush, material, gasOnly);
        }

        public void RenderWallBrush(Point position, Brush brush, Material material, bool gasOnly = false)
        {
            if (material > 0 && (int)material < MaterialInfo.MaterialTypes.Length) {
                var x = position.X / Map.BlockWidth - 8;
                var y = position.Y / Map.BlockWidth - 7;

                foreach (Point pixel in new BrushWalker(brush)) {
                    if (!gasOnly || MaterialInfo.IsGas(GetWallMaterial(x + pixel.X, y + pixel.Y)))
                        SetWallMaterial(x + pixel.X, y + pixel.Y, material);
                }
            }
        }

        public int Dig(Vector2 position, int strength, Brush brush, bool isFromPlayer)
        {
            if (IsTerrainLocked)
                return 0;

            var dropAmounts = new Dictionary<Material, int>();
            var x = (int)position.X / Map.BlockWidth - 8;
            var y = (int)position.Y / Map.BlockHeight - 7;
            foreach (Point pixel in new BrushWalker(brush))
            {
                var material = GetMaterial(pixel.X + x, pixel.Y + y);
                if (MaterialInfo.IsLooseOrSolid(material) && material != Material.Platform)
                {
                    var materialInfo = MaterialInfo.MaterialTypes[(int)material];
                    if (materialInfo.IsBreakable(strength))
                    {
                        if (LockTier > 0 && materialInfo.Hardness >= LockTier)
                            continue;

                        SetMaterial(pixel.X + x, pixel.Y + y, Material.Smoke);

                        var dropMaterial = materialInfo.DigMaterial == Material.None ? material : materialInfo.DigMaterial;
                        int amount;
                        if (dropAmounts.TryGetValue(dropMaterial, out amount))
                            dropAmounts[dropMaterial] = amount + 1;
                        else
                            dropAmounts[dropMaterial] = 1;
                    }
                }
            }

            var total = 0;
            foreach (var dropType in dropAmounts.Keys)
            {
                var materialInfo = MaterialInfo.MaterialTypes[(int)dropType];
                AddPickup(position, materialInfo.Item.Type, dropAmounts[dropType]);
                total += dropAmounts[dropType];
            }
            return total;
        }

        public Pickup AddPickup(Vector2 position, ItemId type, int amount)
        {
            return AddPickup(position, ItemBase.Get(type), amount);
        }

        public Pickup AddPickup(Vector2 position, ItemType type, int amount)
        {
            if (type == null)
                return null;
            var pickup = new Pickup() { Type = type, Amount = amount, Position = position };
            AddEntity(pickup);
            return pickup;
        }

        //void BenchmarkGetMaterial()
        //{
        //    var timer = new Stopwatch();
        //    timer.Start();

        //    for (var y = 0; y < Height; y++)
        //    {
        //        for (var x = 0; x < Width; x++)
        //        {
        //            var material = GetMaterial(x, y);
        //        }
        //    }

        //    Utility.LogMessage(timer.ElapsedMilliseconds.ToString());
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Material GetMaterialAtPixel(Vector2 pixelPosition)
        {
            return GetMaterial((int)pixelPosition.X / BlockWidth, (int)pixelPosition.Y / BlockHeight);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Material GetMaterialAtPixel(int x, int y)
        {
            return GetMaterial(x / BlockWidth, y / BlockHeight);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Material GetMaterial(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return (Material)Materials[y * Width + x];
            return Material.Boundry;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Material GetWallMaterialAtPixel(int x, int y)
        {
            return GetWallMaterial(x / BlockWidth, y / BlockHeight);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Material GetWallMaterial(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return (Material)WallMaterials[y * Width + x];
            return Material.Boundry;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaterialAtPixel(Vector2 pixelPosition, Material material)
        {
            SetMaterial((int)pixelPosition.X / BlockWidth, (int)pixelPosition.Y / BlockHeight, material);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaterial(int x, int y, Material material)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                Materials[y * Width + x] = (byte)material;
                setCount++;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaterialIfNotLooseOrSolid(int x, int y, Material material)
        {
            if (!MaterialInfo.IsLooseOrSolid(GetMaterial(x, y)))
                SetMaterial(x, y, material);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWallMaterial(int x, int y, Material material)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                WallMaterials[y * Width + x] = (byte)material;
                setCount++;
            }
        }

        double timeSinceLastSpawnThink = 0;
        public void SpawnThink(GameTime gameTime)
        {
            timeSinceLastSpawnThink += gameTime.ElapsedGameTime.TotalSeconds;

            // cap spawn think at 1 second max. So that you don't get tons of spawn thinks queued up.
            if (timeSinceLastSpawnThink > SpawnUpdateInterval + 1)
                timeSinceLastSpawnThink = SpawnUpdateInterval;

            if (timeSinceLastSpawnThink > SpawnUpdateInterval)
            {
                timeSinceLastSpawnThink -= SpawnUpdateInterval;

                foreach (var spawn in Spawns)
                    spawn.SpawnCheck(this);

                if (!IsAutospawn)
                    return;

                foreach (var player in Players)
                {
                    var tier = GetTier(player.Position);
                    if (tier > 1 || player.Position.Y > SeaLevelInPixels || IsNight)
                    {
                        if (player.Position.Y > SeaLevelInPixels && IsNight)
                            tier = player.GetMiningTier();

                        var enemyCountTotal = 0;

                        var screenRectangle = player.ScreenRectangle;
                        screenRectangle.Inflate(500, 500);

                        foreach (var entity in Entities)
                        {
                            var enemy = entity as Enemy;
                            if (enemy != null && screenRectangle.Contains(enemy.Position.ToPoint()))
                            {
                                enemyCountTotal++;
                            }
                        }

                        //Utility.LogMessage("{0} Enemies", enemyCountTotal);

                        switch (enemyCountTotal)
                        {
                            case 0:
                                break;
                            case 1:
                            case 2:
                                if (!Utility.Roll4())
                                    continue;
                                break;
                            case 3:
                            case 4:
                                if (!Utility.Roll8())
                                    continue;
                                break;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                if (!Utility.Roll32())
                                    continue;
                                break;
                            default:
                                continue;
                        }

                        var spawnPoint = Vector2.Zero;
                        var spawnMaterial = Material.None;
                        for (var tryCount = 0; tryCount < 10; tryCount++)
                        {
                            spawnPoint = new Vector2(screenRectangle.X + Utility.Next(screenRectangle.Width), screenRectangle.Y + Utility.Next(screenRectangle.Height));
                            spawnMaterial = GetMaterialAtPixel((int)spawnPoint.X, (int)spawnPoint.Y);

                            if (MaterialInfo.IsLooseOrSolid(spawnMaterial))
                                continue;

                            var isLiquid = MaterialInfo.IsLiquid(spawnMaterial);
                            var possibleEnemyTypes = new List<EnemyType>();
                            foreach (var enemyType in EnemyBase.Types)
                            {
                                if (
                                        enemyType.IsAutoSpawn &&
                                        enemyType.MinSpawnTier <= tier &&
                                        enemyType.MaxSpawnTier >= tier &&
                                        enemyType.IsSwimming == isLiquid &&
                                        (enemyType.SpawnRateLimitInSeconds <= 0 || enemyType.LastSpawn.AddSeconds(enemyType.SpawnRateLimitInSeconds) < Now)
                                    )
                                {
                                    possibleEnemyTypes.Add(enemyType);
                                }
                            }

                            //Utility.LogMessage("{0} Possible Enemies", possibleEnemyTypes.Count);

                            if (possibleEnemyTypes.Count > 0)
                            {
                                var spawnEnemyType = possibleEnemyTypes[Utility.Next(possibleEnemyTypes.Count)];

                                if (!spawnEnemyType.IsFlying)
                                {
                                    spawnPoint = FindGround(spawnPoint);
                                    spawnPoint.Y -= spawnEnemyType.SpriteHeight * 2;
                                }

                                if (spawnEnemyType.SpawnAtCeiling)
                                {
                                    spawnPoint = FindCeiling(spawnPoint);
                                    spawnPoint.Y += spawnEnemyType.SpriteHeight / 2;
                                }

                                // check for on screen players
                                var boundingBox = spawnEnemyType.BoundingBox;
                                boundingBox.Offset((int)spawnPoint.X, (int)spawnPoint.Y);
                                var isOnScreen = false;
                                foreach (var playerOnScreen in Players)
                                {
                                    if (playerOnScreen.IsOnScreen(boundingBox))
                                        isOnScreen = true;
                                }
                                if (isOnScreen)
                                    continue;

                                if (spawnEnemyType.SpawnRateLimitInSeconds > 0)
                                    spawnEnemyType.LastSpawn = Now;

                                AddEnemy(spawnEnemyType, spawnPoint);
                                tryCount = 10;

                                //Utility.LogMessage(spawnEnemyType.Name + " spawned.");
                            }
                        }
                    }
                }
            }
        }

        public void InitialSpawnCheck(Vector2 position)
        {
            foreach (var spawn in Spawns)
            {
                var spawned = spawn.SpawnCheck(this, position, false);

                if (spawned != null)
                    spawned.SoundEvents.Clear(); // mute spawn sounds when porting etc.
            }
        }

        //public bool IsEmpty(Rectangle rectangle)
        //{
        //    for (int y = rectangle.Top; y <= rectangle.Bottom; y += BlockWidth)
        //    {
        //        for (int x = rectangle.Left; x <= rectangle.Right; x += BlockWidth)
        //        {
        //            if (MaterialInfo.IsLooseOrSolid(GetMaterialAtPixel(x, y)))
        //                return false;
        //        }
        //    }
        //    return true;
        //}

        public Vector2 FindGround(Vector2 position)
        {
            while (!MaterialInfo.IsLooseOrSolid(GetMaterialAtPixel(position)))
            {
                position.Y += BlockHeight;
                if (position.Y > PixelHeight)
                    break;
            }
            return position;
        }

        public Vector2 FindCeiling(Vector2 position)
        {
            while (!MaterialInfo.IsLooseOrSolid(GetMaterialAtPixel(position)))
            {
                position.Y -= BlockHeight;
                if (position.Y < 0)
                {
                    position.Y = 0;
                    break;
                }
            }
            return position;
        }

        static int[] columnOrder;
        static int[] screenRowOrder;
        static int[] screenColumnOrder;

        int setCount = 0;

        double timeSinceLastTerrainUpdate = 0;

        public void TerrainThink(GameTime gameTime)
        {
            Now = DateTime.UtcNow;
            GameTime = GameTime.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds);

            var timer = new Stopwatch();
            timer.Start();

            //var terrainThinkCount = 0;

            timeSinceLastTerrainUpdate += gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastTerrainUpdate > 1)
                timeSinceLastTerrainUpdate = 1;

            if (timeSinceLastTerrainUpdate > TerrainUpdateInterval)
            {
                timeSinceLastTerrainUpdate -= TerrainUpdateInterval;

                setCount = 0;

                DoThinkBroad(Width, 25);
                //terrainThinkCount += Width * 25;

                // TODO: Don't double think when 2 players are on the screen.
                foreach (var player in Players)
                {
                    var screenOffset = player.GetScreenOffset();
                    var thinkScreenWidth = player.ClientResolutionWidth / BlockWidth;
                    var thinkScreenHeight = player.ClientResolutionHeight / BlockHeight;
                    var playerScreenThink = new Rectangle((int)screenOffset.X / BlockWidth, (int)screenOffset.Y / BlockHeight, thinkScreenWidth, thinkScreenHeight);
                    playerScreenThink.Inflate(50, 50);
                    DoThinkScreen(playerScreenThink);
                    //terrainThinkCount += playerScreenThink.Width * playerScreenThink.Height;
                }
            }

            //if (terrainThinkCount > 0)
            //    Utility.DebugMessage = String.Format("Blocks: {1} Entities: {2} Terrain Time: {0}ms SetCount: {3}", timer.ElapsedMilliseconds, terrainThinkCount, Entities.Count, setCount);
        }

        public void EntityThink(GameTime gameTime)
        {
            foreach (var entity in Entities)
            {
                entity.Think(gameTime);
            }

            if (nextLongThink < Now)
            {
                nextLongThink = Now.AddSeconds(0.25);

                foreach (var entity in Entities)
                {
                    entity.LongThink(gameTime);
                }
            }
        }

        public void FlushEntities()
        {
            if (addedEntities.Count > 0 || removedEntities.Count > 0)
            {
                lock (addedEntities)
                {
                    foreach (var added in addedEntities)
                    {
                        Entities.Add(added);
                        if (added.IsSolid && !SolidEntities.Contains(added))
                            SolidEntities.Add(added);
                        var player = added as Player;
                        if (player != null)
                            Players.Add(player);
                    }
                    addedEntities.Clear();
                }

                lock (removedEntities)
                {
                    foreach (var removed in removedEntities)
                    {
                        if (removed.IsSolid)
                            SolidEntities.Remove(removed);
                        Entities.Remove(removed);

                        var player = removed as Player;
                        if (player != null)
                        {
                            Players.Remove(player);

                            if (Players.Count == 0 && IsResetOnLeave)
                                World.ResetMap(this);
                        }
                    }
                    removedEntities.Clear();
                }
            }
        }

        bool isResetting = false;
        public bool ResetOnDieCheck(Player respawningPlayer)
        {
            if (isResetting)
                return false;

            //Utility.LogMessage(respawningPlayer.Name + " ResetOnDieCheck");

            if (IsResetOnDie && Players.Count > 0)
            {
                foreach (var player in Players) {
                    if (player != respawningPlayer && !player.IsDead)
                        return false;
                }
                isResetting = true;
                //Utility.LogMessage(respawningPlayer.Name + " Resetting");
                World.ResetMap(this);
                //isResetting = false;
                return true;
            }
            return false;
        }

        public bool CanRespawn()
        {
            if (!IsResetOnDie || Players.Count <= 1)
                return true;

            return false;
            //var livingCount = (from p in Players where !p.IsDead select p).Count();
            //return livingCount <= 0;
        }

        void DoThinkScreen(Rectangle screen)
        {
            if (screenRowOrder == null || screenRowOrder.Length != screen.Height)
            {
                screenRowOrder = new int[screen.Height];
                for (var count = 0; count < screen.Height; count++)
                {
                    screenRowOrder[count] = screen.Height - count - 1;
                }

                //shuffle
                for (var count = 0; count < screen.Height; count++)
                {
                    var target = Utility.Next(screen.Height);
                    var temp = screenRowOrder[target];
                    screenRowOrder[target] = screenRowOrder[count];
                    screenRowOrder[count] = temp;
                }
            }

            if (screenColumnOrder == null || screenColumnOrder.Length != screen.Width)
            {
                screenColumnOrder = new int[screen.Width];
                for (var count = 0; count < screen.Width; count++)
                {
                    screenColumnOrder[count] = count;
                }

                // shuffle
                for (var count = 0; count < screen.Width; count++)
                {
                    var target = Utility.Next(screen.Width);
                    var temp = screenColumnOrder[target];
                    screenColumnOrder[target] = screenColumnOrder[count];
                    screenColumnOrder[count] = temp;
                }
            }

            //foreach (var tileY in screenRowOrder)
            Parallel.ForEach(screenRowOrder, tileY =>
            //Parallel.For(0, screen.Height, tileY =>
            {
                var isLongThink = Utility.Roll32();
                SwapRandom(screenColumnOrder);

                foreach (var tileX in screenColumnOrder)
                {
                    var tile = MaterialThink(tileX + screen.X, tileY + screen.Y);
                    if (isLongThink)
                        LongThink(tileX + screen.X, tileY + screen.Y, tile);
                }
            });

            //Utility.DebugMessage2 = String.Format("{0}, {1}, {2}", lastRow, screen.Width, screen.Height);
        }


        void DoThinkBroad(int width, int height)
        {
            //if (rowOrder == null || rowOrder.Length != height)
            //{
            //    rowOrder = new int[height];
            //    for (var count = 0; count < height; count++)
            //    {
            //        rowOrder[count] = height - count - 1;
            //    }

            //    // shuffle
            //    //for (var count = 0; count < viewHeight; count++)
            //    //{
            //    //    var target = Utility.Next(viewHeight);
            //    //    var temp = rowOrder[target];
            //    //    rowOrder[target] = rowOrder[count];
            //    //    rowOrder[count] = temp;
            //    //}
            //}

            if (columnOrder == null || columnOrder.Length != width)
            {
                columnOrder = new int[width];
                for (var count = 0; count < width; count++)
                {
                    columnOrder[count] = count;
                }

                // shuffle
                for (var count = 0; count < width; count++)
                {
                    var target = Utility.Next(width);
                    var temp = columnOrder[target];
                    columnOrder[target] = columnOrder[count];
                    columnOrder[count] = temp;
                }
            }

            //Parallel.ForEach(rowOrder, tileY =>
            Parallel.For(0, height, tileY =>
            {
                RowThink((lastRow + height - tileY) % Height);
            });

            lastRow += height;
            lastRow %= Height;

            //Utility.DebugMessage2 = String.Format("{0}, {1}, {2}", lastRow, width, height);
        }

        int lastRow = 0;

        void RowThink(int row)
        {
            //Utility.DebugMessage2 = row.ToString();

            var isLongThink = Utility.Roll64();
            SwapRandom(columnOrder);

            foreach (var tileX in columnOrder)
            {
                var tile = MaterialThink(tileX, row);
                if (isLongThink)
                    LongThink(tileX, row, tile);
            }
        }

        void SwapRandom(int[] array)
        {
            lock (array)
            {
                var source = Utility.Next(array.Length);
                var target = Utility.Next(array.Length);
                var temp = array[target];
                array[target] = array[source];
                array[source] = temp;
            }
        }

        public IEnumerable<Placeable> FindPlaceables(string name)
        {
            return from p in Entities.OfType<Placeable>() where p.Name == name select p;
        }

        public IEnumerable<Placeable> FindPlaceables(ItemId typeId)
        {
            return from p in Entities.OfType<Placeable>() where p.TypeId == typeId select p;
        }

        public Placeable FindPlaceable(Point position)
        {
            foreach (var entity in Entities)
            {
                if (entity is Placeable && entity.ContainsPoint(position))
                    return entity as Placeable;
            }

            return null;
        }

        public Entity FindEntity(int id)
        {
            lock (EntitiesById)
            {
                if (EntitiesById.ContainsKey(id))
                    return EntitiesById[id];
                return null;
            }
        }

        public Entity FindEntity(Vector2 position)
        {
            return FindEntity(new Point((int)position.X, (int)position.Y));
        }

        public Entity FindEntity(Point position)
        {
            foreach (var entity in Entities)
            {
                if (entity.ContainsPoint(position))
                    return entity;
            }

            return null;
        }

        public Entity FindEntity(Rectangle target)
        {
            foreach (var entity in Entities)
            {
                if (entity.OffsetBoundingBox.Intersects(target))
                    return entity;
            }

            return null;
        }

        public Entity FindEntity(Vector2 position, ItemId type, int range = 75)
        {
            foreach (var entity in Entities)
            {
                var placeable = entity as Placeable;
                if (placeable != null && placeable.TypeId == type && placeable.WithinRange(position, range))
                    return entity;
            }

            return null;
        }

        public Placeable FindClosestPlaceable(Vector2 position, bool filterToActivatable = false)
        {
            var distanceSquared = Single.MaxValue;
            Placeable closest = null;
            foreach (var entity in Entities)
            {
                var placeable = entity as Placeable;
                if (placeable != null && (!filterToActivatable || placeable.Type.OnActivate != null))
                {
                    var distance = (entity.Position - position).LengthSquared();
                    if (distance < distanceSquared)
                    {
                        distanceSquared = distance;
                        closest = placeable;
                    }
                }
            }

            return closest;
        }

        public Enemy FindTargetEnemy(Vector2 position, string name)
        {
            var distanceSquared = Single.MaxValue;
            Enemy closest = null;
            foreach (var entity in Entities)
            {
                var enemy = entity as Enemy;
                if (enemy != null && enemy.Name == name)
                {
                    var distance = (enemy.Position - position).LengthSquared();
                    if (distance < distanceSquared)
                    {
                        closest = enemy;
                    }
                }
            }

            return closest;
        }

        public Enemy FindClosestEnemy(Vector2 position, out float distanceSquared)
        {
            distanceSquared = Single.MaxValue;
            Enemy closest = null;
            foreach (var entity in Entities)
            {
                var enemy = entity as Enemy;
                if (enemy != null)
                {
                    var distance = (enemy.Position - position).LengthSquared();
                    if (distance < distanceSquared)
                    {
                        distanceSquared = distance;
                        closest = enemy;
                    }
                }
            }

            return closest;
        }

        public Player FindClosestPlayer(Vector2 position, out float distanceSquared)
        {
            distanceSquared = Single.MaxValue;
            Player closest = null;
            foreach (var player in Players)
            {
                if (!player.IsDead)
                {
                    var distance = (player.Position - position).LengthSquared();
                    if (distance < distanceSquared)
                    {
                        distanceSquared = distance;
                        closest = player;
                    }
                }
            }

            return closest;
        }

        public Placeable FindPortalTo(Placeable sourcePortal, Map map, string name = null)
        {
            var portals = from placeable in Entities.OfType<Placeable>() where placeable != sourcePortal && (placeable.TypeId == ItemId.Portal || placeable.TypeId == ItemId.PortableWaypoint) && String.Equals(placeable.Value, map.Name, StringComparison.InvariantCultureIgnoreCase) select placeable;

            if (name != null)
            {
                var result = portals.Where(p => p.Name == name).FirstOrDefault();
                if (result != null)
                    return result;
            }

            return portals.FirstOrDefault();
        }

        public void AddEntity(Entity entity)
        {
            Now = DateTime.UtcNow; // if this isn't here then initial animations will time out.
            lock (addedEntities)
            {
                if (entity.Id == 0)
                {
                    // TODO: Check for id collision
                    entity.Id = NextEntityId++;
                }
                entity.Created = Now;
                entity.Map = this;
                addedEntities.Add(entity);
                lock (EntitiesById)
                {
                    EntitiesById.Add(entity.Id, entity);
                }
            }
            var player = entity as Player;
            if (player != null)
            {
                if (!World.IsConnectedToServer)
                {
                    InitialSpawnCheck(player.Position);
                    player.IsMapSpawnDataChanged = true;

                    player.MessageToClient("You have reached " + Name.AddSpaces() + ".", MessageType.System);

                    if (Tier > 0)
                        player.MessageToClient("This map is designed for tier " + Tier + " gear.", MessageType.System);

                    if (IsResetOnLeave)
                        player.MessageToClient("When you leave this map it will be reset.", MessageType.Error);
                    if (IsResetOnDie)
                        player.MessageToClient("When you die this map will be reset.", MessageType.Error);
                    if (IsTerrainLocked)
                        player.MessageToClient("Placing materials, Mining and Dematerializing are disabled on this map.", MessageType.Error);
                    if (LockTier > 0)
                        player.MessageToClient(String.Format("Materials of Tier {0} and above are indestructable on this map.", LockTier), MessageType.Error);
                }
            }
        }

        public void RemoveEntity(Entity entity)
        {
            lock (removedEntities)
            {
                removedEntities.Add(entity);

                if (entity.PlayingSoundEffect != null)
                {
                    entity.PlayingSoundEffect.Stop();
                    entity.PlayingSoundEffect = null;
                }
                lock (EntitiesById)
                {
                    EntitiesById.Remove(entity.Id);
                }
            }
        }

        public void RemoveEntity(int entityId)
        {
            var entity = FindEntity(entityId);
            if (entity != null)
                RemoveEntity(entity);
        }

        public Placeable AddPlaceable(Player source, ItemType type, Vector2 position)
        {
            var placeable = new Placeable() { Type = type, Position = position };
            AddEntity(placeable);
            if (type.OnPlace != null)
                type.OnPlace(source, placeable);
            return placeable;
        }

        int[] tierAltitudes;

        public int GetTierAltitude(int tier)
        {
            if (tier < 1 || tier > 8)
                return 0;

            if (tierAltitudes == null)
                GenerateTierAltitudes();

            return tierAltitudes[tier - 1];
        }

        public void GenerateTierAltitudes()
        {
            var sandstoneHeight = SeaLevel + 500;
            var sectionHeight = (Height - sandstoneHeight) / 7;
            var limestoneHeight = sandstoneHeight + sectionHeight;
            var quartziteHeight = limestoneHeight + sectionHeight;
            var graniteHeight = quartziteHeight + sectionHeight;
            var marbleHeight = graniteHeight + sectionHeight;
            var rhyoliteHeight = marbleHeight + sectionHeight;
            var basaltHeight = rhyoliteHeight + sectionHeight;

            tierAltitudes = new int[8];
            tierAltitudes[0] = 0;
            tierAltitudes[1] = sandstoneHeight * Map.BlockHeight;
            tierAltitudes[2] = limestoneHeight * Map.BlockHeight;
            tierAltitudes[3] = quartziteHeight * Map.BlockHeight;
            tierAltitudes[4] = graniteHeight * Map.BlockHeight;
            tierAltitudes[5] = marbleHeight * Map.BlockHeight;
            tierAltitudes[6] = rhyoliteHeight * Map.BlockHeight;
            tierAltitudes[7] = basaltHeight * Map.BlockHeight;
        }

        public int GetTier(Vector2 position)
        {
            int result = Tier;
            if (Tier == 0)
            {
                // set tier based on altitude.
                if (tierAltitudes == null)
                    GenerateTierAltitudes();

                for (int i = 0; i < tierAltitudes.Length; i++)
                {
                    if (position.Y < tierAltitudes[i])
                    {
                        result = i;
                        break;
                    }
                }
                if (result == 0)
                    result = 8;
            }

            return result;
        }

        public Enemy AddEnemy(EnemyType type, Vector2 position)
        {
            var tier = GetTier(position);
            var enemy = new Enemy() { Type = type, Position = position, Tier = tier, Map = this };
            AddEntity(enemy);
            enemy.PlaySound(Sound.EnemySpawn, 0, enemy.Type.Id);
            enemy.Initalize();
            return enemy;
        }

        public void AddSpawn(Spawn spawn)
        {
            Spawns.Add(spawn);

            foreach (var player in Players)
                player.IsMapSpawnDataChanged = true;
        }

        public void RemoveSpawn(Vector2 targetPosition)
        {
            foreach (var spawn in Spawns)
            {
                if (spawn.OffsetBoundingBox.Contains((int)targetPosition.X, (int)targetPosition.Y))
                {
                    Spawns.Remove(spawn);

                    foreach (var player in Players)
                        player.IsMapSpawnDataChanged = true;

                    return;
                }
            }
        }

        public void TriggerSwitches(string name, Player sourcePlayer = null, Placeable sourcePlaceable = null)
        {
            if (name == null)
            {
                sourcePlayer.MessageToClient("No name set on trigger source. Trigger must have a name that matches target placeables.", MessageType.Error);
                return;
            }

            foreach (var placeable in FindPlaceables(name))
            {
                if (placeable.Type.OnSwitch != null)
                    placeable.Type.OnSwitch(placeable, sourcePlaceable, sourcePlayer);
            }
        }

        Material MaterialThink(int x, int y)
        {
            var material = GetMaterial(x, y);
            switch (material)
            {
                case Material.Water:
                case Material.Oil:
                case Material.Slime:
                case Material.Tar:
                case Material.Acid:
                case Material.Lava:
                    if (FluidThink(x, y, material))
                        return Material.Boundry;
                    break;

                case Material.Fire:
                case Material.BlueFire:
                case Material.ReservedLiquid2:
                    var waterDirection = IsAdjacent(x, y, Material.Water);
                    if (waterDirection != Direction.None)
                    {
                        SetMaterial(x, y, Material.Steam);
                        SetAdjacent(x, y, waterDirection, Material.Steam);
                    }
                    else
                    {
                        if (Utility.Roll8())
                        {
                            SetMaterial(x, y, Material.Smoke);
                            break;

                            //var airDirection = IsAdjacent(x, y, Material.Air);
                            //if (airDirection == Direction.None)
                            //{
                            //    SetMaterial(x, y, Material.Smoke);
                            //    break;
                            //}
                            //else
                            //{
                            //    if (Utility.Roll8())
                            //        SetMaterial(x, y, Material.Smoke);
                            //        //SetAdjacent(x, y, airDirection, Material.Fire);
                            //}
                        }
                        //if (GetMaterial(x, y - 1) == Material.Air && Utility.Roll4())
                        //    SetMaterial(x, y - 1, Material.Smoke);
                        FluidThink(x, y, material);
                    }
                    break;

                case Material.Ice:
                    if ((GetMaterial(x, y - 1) != Material.Ice || GetMaterial(x, y + 1) != Material.Ice || GetMaterial(x - 1, y) != Material.Ice || GetMaterial(x + 1, y) != Material.Ice) && Utility.Roll16())
                        SetMaterial(x, y, Material.Water);
                    break;

                case Material.Gravel:
                case Material.Ash:
                case Material.Sand:
                case Material.Snow:
                    LooseThink(x, y, material);
                    break;

                case Material.Smoke:
                    if (Utility.Roll8() && IsAdjacent(x, y, Material.Air) != Direction.None)
                        SetMaterial(x, y, Material.Air);
                    GasThink(x, y, material);
                    break;

                case Material.Steam:
                case Material.PoisonGas:
                case Material.Vacuum:
                    GasThink(x, y, material);
                    break;

                case Material.NaturalGas:
                    if (IsAdjacent(x, y, Material.Fire) != Direction.None || IsAdjacent(x, y, Material.Lava) != Direction.None || IsAdjacent(x, y, Material.BlueFire) != Direction.None)
                        Explode(new Vector2(x * BlockWidth, y * BlockHeight), 50);
                    else
                        GasThink(x, y, material);
                    break;
            }

            return material;
        }

        void LongThink(int x, int y, Material material)
        {
            switch (material)
            {
                case Material.Water:
                    if (GetMaterial(x, y - 1) == Material.Air && Utility.Roll64())
                    {
                        //lock (this)
                        //{
                        SetMaterial(x, y, Material.Steam);
                        SetMaterial(x, y - 1, Material.Steam);
                        //}
                    }
                    //else
                    //{
                    //    var direction = IsAdjacentOrDiagonal(x, y, Material.Dirt);
                    //    if (direction != Direction.None)
                    //    {
                    //        SetMaterial(x, y, Material.Steam);
                    //        SetAdjacent(x, y, direction, Material.Mud);
                    //    }
                    //}
                    break;

                //case Material.Fire:
                //    {
                //        var direction = IsAdjacent(x, y - 1, Material.Oil);
                //        if (direction != Direction.None)
                //        {
                //            SetMaterial(x, y, Material.Fire);
                //            SetAdjacent(x, y, direction, Material.Fire);
                //        }
                //        else if (IsAdjacent(x, y, Material.Wood) != Direction.None)
                //        {
                //            SetMaterial(x, y, Material.Fire);
                //            SetAdjacent(x, y, IsAdjacent(x, y, Material.Wood), Material.Fire);
                //        }
                //        else if (Utility.Flip())
                //        {
                //            SetMaterial(x, y, Material.Smoke);
                //        }
                //    }
                //    break;

                case Material.Lava:
                    var direction = IsAdjacentOrDiagonal(x, y, Material.Water);
                    if (direction != Direction.None)
                    {
                        SetMaterial(x, y, Material.Obsidian);
                        SetAdjacent(x, y, direction, Material.Steam);
                    }
                    break;

                case Material.Dirt:
                    if (y < SeaLevel + 15 && Utility.Roll4())
                    {
                        var aboveMaterial = GetMaterial(x, y - 1);
                        if (!MaterialInfo.IsSolid(aboveMaterial) && !MaterialInfo.IsLiquid(aboveMaterial))
                            SetMaterial(x, y, Material.Grass);
                    }
                    break;

                //case Tile.Grass:
                //    if (Utility.Roll128() && GetTile(x, y - 1) == Tile.Air)
                //        SetTile(x, y - 1, Tile.Wood);
                //    break;

                //case Material.Wood:
                //    if (Utility.Roll32() && GetMaterial(x, y - 1) == Material.Air)
                //        SetMaterial(x, y - 1, Material.Wood);
                //    break;

                //case Material.Smoke:
                //    if (Utility.Roll4())
                //        SetMaterial(x, y, Material.Air);
                //    break;

                case Material.Steam:
                    if (Utility.Roll4() && GetMaterial(x, y - 1) == Material.Steam)
                    {
                        // condensation
                        SetMaterial(x, y - 1, Material.Air);
                        SetMaterial(x, y, Material.Water);
                    }
                    break;

                //case Material.Mud:
                //    if (Utility.Roll4())
                //    {
                //        if (GetMaterial(x, y - 1) == Material.Air)
                //        {
                //            SetMaterial(x, y, Material.Dirt);
                //            SetMaterial(x, y - 1, Material.Steam);
                //        }
                //    }
                //    else
                //    {
                //        if (Utility.Roll4() && GetMaterial(x, y - 1) == Material.Dirt)
                //        {
                //            SetMaterial(x, y, Material.Dirt);
                //            SetMaterial(x, y - 1, Material.Mud);
                //        }
                //    }
                //    break;

                case Material.Grass:
                    if (Utility.Roll4() && MaterialInfo.IsSolid(GetMaterial(x, y - 1)))
                        SetMaterial(x, y, Material.Dirt);
                    break;

            }
        }

        const int gasFlowCheckDistance = 6;

        bool GasThink(int x, int y, Material material)
        {

            // falling
            if (!Utility.Flip())
            {
                if (CheckSwap(x, y, x, y - 1, material))
                {
                    //CheckSwap(x, y + 1, x, y + 2, material);
                    return true;
                }
            }

            if (Utility.Flip())
            {
                if (!GasCheckLeft(x, y, material))
                    return GasCheckRight(x, y, material);
                return true;
            }
            else
            {
                if (!GasCheckRight(x, y, material))
                    return GasCheckRight(x, y, material);
                return true;
            }
        }

        public bool GasCheckLeft(int x, int y, Material material)
        {
            // find a place for me
            for (int count = 1; count <= gasFlowCheckDistance; count++)
            {
                var sideTile = GetMaterial(x - count, y);
                if (sideTile < material) // set this to <= for more accurate, but expensive liquids
                {
                    if (CheckSwap(x, y, x - count, y - 1, material))
                    {
                        if (!Utility.Roll8())
                            GasThink(x - count, y - 1, material);
                        return true;
                    }
                }
                else
                {
                    if (count > 1)
                        return CheckSwap(x, y, x - count + 1, y, material);

                    return false;
                }
            }
            return CheckSwap(x, y, x - gasFlowCheckDistance, y, material);
        }

        public bool GasCheckRight(int x, int y, Material material)
        {
            // find a place for me
            for (int count = 1; count <= gasFlowCheckDistance; count++)
            {
                var sideTile = GetMaterial(x + count, y);
                if (sideTile < material) // set this to <= for more accurate, but expensive liquids
                {
                    if (CheckSwap(x, y, x + count, y - 1, material))
                    {
                        if (!Utility.Roll8())
                            GasThink(x + count, y - 1, material);
                        return true;
                    }
                }
                else
                {
                    if (count > 1)
                        return CheckSwap(x, y, x + count - 1, y, material);

                    return false;
                }
            }
            return CheckSwap(x, y, x + gasFlowCheckDistance, y, material);
        }

        Direction IsAdjacent(int x, int y, Material material)
        {
            if (GetMaterial(x, y - 1) == material)
                return Direction.Up;
            if (GetMaterial(x, y + 1) == material)
                return Direction.Down;
            if (GetMaterial(x - 1, y) == material)
                return Direction.Left;
            if (GetMaterial(x + 1, y) == material)
                return Direction.Right;

            return Direction.None;
        }

        Direction IsAdjacentOrDiagonal(int x, int y, Material material)
        {
            if (GetMaterial(x, y - 1) == material)
                return Direction.Up;
            if (GetMaterial(x, y + 1) == material)
                return Direction.Down;
            if (GetMaterial(x - 1, y) == material)
                return Direction.Left;
            if (GetMaterial(x + 1, y) == material)
                return Direction.Right;

            if (GetMaterial(x - 1, y - 1) == material)
                return Direction.UpLeft;
            if (GetMaterial(x + 1, y - 1) == material)
                return Direction.UpRight;
            if (GetMaterial(x - 1, y + 1) == material)
                return Direction.DownLeft;
            if (GetMaterial(x + 1, y + 1) == material)
                return Direction.DownRight;

            return Direction.None;
        }

        void SetAdjacent(int x, int y, Direction direction, Material material)
        {
            switch (direction)
            {
                case Direction.Up:
                    SetMaterial(x, y - 1, material);
                    break;
                case Direction.Down:
                    SetMaterial(x, y + 1, material);
                    break;
                case Direction.Left:
                    SetMaterial(x - 1, y, material);
                    break;
                case Direction.Right:
                    SetMaterial(x + 1, y, material);
                    break;
                case Direction.UpLeft:
                    SetMaterial(x - 1, y - 1, material);
                    break;
                case Direction.UpRight:
                    SetMaterial(x + 1, y - 1, material);
                    break;
                case Direction.DownLeft:
                    SetMaterial(x - 1, y + 1, material);
                    break;
                case Direction.DownRight:
                    SetMaterial(x + 1, y + 1, material);
                    break;
            }
        }

        bool CheckSwap(int x, int y, int targetX, int targetY, Material material)
        {
            var target = GetMaterial(targetX, targetY);

            if (target < material)
            {
                SetMaterial(x, y, target);
                SetMaterial(targetX, targetY, material);
                return true;
            }

            return false;
        }

        void LooseThink(int x, int y, Material material)
        {
            if (Utility.Flip())
            {
                if (CheckSwap(x, y, x, y + 1, material))
                    return;
            }

            if (Utility.Flip())
            {
                if (CheckSwap(x, y, x - 1, y + 1, material))
                    return;
                if (CheckSwap(x, y, x + 1, y + 1, material))
                    return;
            }
            else
            {
                if (CheckSwap(x, y, x + 1, y + 1, material))
                    return;
                if (CheckSwap(x, y, x - 1, y + 1, material))
                    return;
            }
        }

        const int fluidFlowCheckDistance = 100;

        bool FluidThink(int x, int y, Material material)
        {
            // falling
            if (!Utility.Flip())
            {
                if (CheckSwap(x, y, x, y + 1, material))
                {
                    //CheckSwap(x, y + 1, x, y + 2, material);
                    return true;
                }
            }

            //var above = GetTile(x, y - 1);
            //if (above == tile && !Utility.Roll16())
            //{
            //    return false;
            //}

            if (Utility.Flip())
            {
                if (!FluidCheckLeft(x, y, material))
                    return FluidCheckRight(x, y, material);
                return true;
            }
            else
            {
                if (!FluidCheckRight(x, y, material))
                    return FluidCheckLeft(x, y, material);
                return true;
            }
        }

        public bool FluidCheckLeft(int x, int y, Material material)
        {
            // find a place for me
            for (int count = 1; count <= fluidFlowCheckDistance; count++)
            {
                var sideTile = GetMaterial(x - count, y);
                if (sideTile < material) // set this to <= for more accurate, but expensive liquids
                {
                    if (CheckSwap(x, y, x - count, y + 1, material))
                    {
                        if (!Utility.Roll8())
                            FluidThink(x - count, y + 1, material);
                        return true;
                    }
                }
                else
                {
                    if (count > 1)
                        return CheckSwap(x, y, x - count + 1, y, material);

                    return false;
                }
            }
            return CheckSwap(x, y, x - fluidFlowCheckDistance, y, material);
        }

        public bool FluidCheckRight(int x, int y, Material material)
        {
            // find a place for me
            for (int count = 1; count <= fluidFlowCheckDistance; count++)
            {
                var sideTile = GetMaterial(x + count, y);
                if (sideTile < material) // set this to <= for more accurate, but expensive liquids
                {
                    if (CheckSwap(x, y, x + count, y + 1, material))
                    {
                        if (!Utility.Roll8())
                            FluidThink(x + count, y + 1, material);
                        return true;
                    }
                }
                else
                {
                    if (count > 1)
                        return CheckSwap(x, y, x + count - 1, y, material);

                    return false;
                }
            }
            return CheckSwap(x, y, x + fluidFlowCheckDistance, y, material);
        }

        public bool IsLooseOrSolid(Rectangle target)
        {
            for (var y = target.Y; y < target.Bottom; y += BlockHeight)
            {
                for (var x = target.X; x < target.Right; x += BlockWidth)
                {
                    if (MaterialInfo.IsLooseOrSolid(GetMaterial(x / BlockWidth, y / BlockHeight)))
                        return true;
                }
            }

            return false;
        }

        public void Explode(Vector2 position, int intensity, int digStrength = 8, bool dropParticles = true)
        {
            if (LockTier > 0 && digStrength >= LockTier)
                digStrength = LockTier - 1;

            var brushSize = intensity > 50 ? Brush.Size9 : Brush.Size7;

            CreateVein
            (
                Material.Fire,
                position,
                intensity,
                brushSize,
                false,
                digStrength,
                renderPosition =>
                {
                    if (dropParticles && Utility.Roll4())
                        AddEntity(new ParticleEmitter() { Position = renderPosition, Type = ParticleEffect.Explosion });
                }
            );
        }

        public void PlaySound(Vector2 position, Sound sound, float volume = 0, int enemyTypeId = 0)
        {
            SoundEvents.Add(new SoundEvent() { Sound = sound, Volume = volume, EnemyTypeId = enemyTypeId, X = (int)position.X, Y = (int)position.Y });
        }

        public int[] GetRemovedEntityIds()
        {
            return (from e in removedEntities select e.Id).ToArray();
        }

        public Map Copy(Rectangle source)
        {
            var result = new Map() { Width = source.Width / BlockWidth, Height = source.Height / BlockHeight };
            result.Materials = new byte[result.Width * result.Height];
            result.WallMaterials = new byte[result.Width * result.Height];

            for (var y = 0; y < source.Height / BlockHeight; y++)
            {
                for (var x = 0; x < source.Width / BlockWidth; x++)
                {
                    result.SetMaterial(x, y, GetMaterial((int)(source.Left / BlockWidth) + x, (int)(source.Top / BlockHeight) + y));
                    result.SetWallMaterial(x, y, GetWallMaterial((int)(source.Left / BlockWidth) + x, (int)(source.Top / BlockHeight) + y));
                }
            }

            return result;
        }

        public void Paste(Map source, Point destination, bool writeAir = true)
        {
            if (source != null)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    for (var x = 0; x < source.Width; x++)
                    {
                        var material = source.GetMaterial(x, y);
                        if ((writeAir || material != Material.Air) && material > 0 && (int)material < MaterialInfo.MaterialTypes.Length)
                            SetMaterial(x + destination.X / BlockWidth, y + destination.Y / BlockWidth, material);
                    }
                }
            }
        }

        public void PasteWall(Map source, Point destination, bool writeAir = true)
        {
            if (source != null)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    for (var x = 0; x < source.Width; x++)
                    {
                        var material = source.GetWallMaterial(x, y);
                        if ((writeAir || material != Material.Air) && material > 0 && (int)material < MaterialInfo.MaterialTypes.Length)
                            SetWallMaterial(x + destination.X / BlockWidth, y + destination.Y / BlockWidth, material);
                    }
                }
            }
        }

        const int hillWidth = 400;
        public void CreateHills(Material material, Material negativeMaterial, int level, int height, float startOffset = 0, float midOffset = 0, float endOffset = 0, bool isWall = false)
        {
            var sectionCount = Width / hillWidth - 1;
            var middleSection = sectionCount / 2;
            for (int section = 0; section <= sectionCount; section++) {
                if (section == middleSection)
                    CreateHill(material, negativeMaterial, section * hillWidth, level, hillWidth, height, midOffset, midOffset, isWall);
                else if (section == middleSection - 1)
                    CreateHill(material, negativeMaterial, section * hillWidth, level, hillWidth, height, 0, midOffset, isWall);
                else if (section == middleSection + 1)
                    CreateHill(material, negativeMaterial, section * hillWidth, level, hillWidth, height, midOffset, 0, isWall);
                else
                    CreateHill(material, negativeMaterial, section * hillWidth, level, hillWidth, height, section == 0 ? startOffset : 0, section == sectionCount ? endOffset : 0, isWall);
            }
        }

        void CreateHill(Material material, Material negativeMaterial, int x, int y, int width, int height, float startOffset, float endOffset, bool isWall)
        {
            var heightOffsets = new float[width];
            if (startOffset != 0 || endOffset != 0) {
                for (int i = 0; i < heightOffsets.Length; i++) {
                    heightOffsets[i] = startOffset + ((endOffset - startOffset) / heightOffsets.Length) * i;
                }
            }

            var heights = new float[width];
            MidPointDisplace(heights, 0, width - 1);

            var count = 0;
            for (int i = 0; i < heights.Length; i++) {
                SetHeight(material, negativeMaterial, x + count, (int)(heights[i] * height + heightOffsets[i]), y, isWall);
                count++;
            }
        }

        void SetHeight(Material material, Material negativeMaterial, int x, int y, int baseHeight, bool isWall = false)
        {
            if (y > 0) {
                while (y > 0) {
                    if (isWall)
                        SetWallMaterial(x, baseHeight - y, material);
                    else
                        SetMaterial(x, baseHeight - y, material);
                    y--;
                }
            } else {
                while (y <= 0) {
                    if (isWall)
                        SetWallMaterial(x, baseHeight - y, negativeMaterial);
                    else
                        SetMaterial(x, baseHeight - y, negativeMaterial);
                    y++;
                }
            }
        }

        void MidPointDisplace(float[] heights, int start, int end, int iterations = 1)
        {
            var length = end - start;
            var halfLength = length / 2;
            if (halfLength < 2)
                return;

            var halfLengthAsFloat = (float)halfLength;
            var offset = (float)(Utility.NextDouble() - 0.5) / (float)iterations;

            for (int line = 0; line <= halfLength; line++) {
                var value = offset * ((float)line / halfLengthAsFloat);
                heights[start + line] += value;
                if (start + line != end - line)
                    heights[end - line] += value;
            }

            //if (iterations > 1)
            //{
            MidPointDisplace(heights, start, start + length / 2, iterations * 2);
            MidPointDisplace(heights, start + length / 2, end, iterations * 2);
            //}
        }

        bool Settle(int x, int startY, Material material)
        {
            var y = startY + 1;
            while (GetMaterial(x, y) < material)
                y++;
            y--;

            if (y != startY) {
                Swap(x, startY, x, y);
                return true;
            }
            return false;
        }

        void Swap(int x, int y, int x2, int y2)
        {
            var material1 = GetMaterial(x, y);
            var material2 = GetMaterial(x2, y2);
            SetMaterial(x, y, material2);
            SetMaterial(x2, y2, material1);
        }

        public void Fill(Material material, Rectangle area)
        {
            for (var y = area.Y; y < area.Bottom; y++) {
                for (var x = area.X; x < area.Right; x++) {
                    SetMaterial(x, y, material);
                }
            }
        }

        public void FillWall(Material material, Rectangle area)
        {
            for (var y = area.Y; y < area.Bottom; y++) {
                for (var x = area.X; x < area.Right; x++) {
                    WallMaterials[y * Width + x] = (byte)material;
                }
            }
        }

        public void SprinkleVeins(Material material, int startDepth, int endDepth, int odds, int veinLength, Brush brush = Brush.Size1, Action<Vector2> veinCreated = null, bool solidOnly = true)
        {
            var driftLength = 1;

            if (odds > 1024) {
                driftLength = 32;
                odds /= 1024;
            } else if (odds >= 100) {
                driftLength = 10;
                odds /= 100;
            }

            for (var y = startDepth; y < endDepth; y += driftLength) {
                for (var x = 0; x < Width; x += driftLength) {
                    if (Utility.Roll(odds)) {
                        var position = new Vector2((x + Utility.Next(driftLength)) * BlockWidth, (y + Utility.Next(driftLength)) * BlockHeight);
                        if (!solidOnly || MaterialInfo.IsLooseOrSolid(GetMaterialAtPixel(position))) {
                            CreateVein(material, position, veinLength, brush, solidOnly);
                            if (veinCreated != null)
                                veinCreated(position);
                        }
                    }
                }
            }
        }

        public void CreateVein(Material material, Vector2 pixelPosition, int length, Brush brush = Brush.Size1, bool solidOnly = true, int digStrength = 9, Action<Vector2> callback = null)
        {
            var driftLength = 1.5f * BlockWidth;

            if (brush != Brush.Size1)
                driftLength = 3.5f * BlockWidth;

            if (brush >= Brush.Size9)
                driftLength = 4f * BlockWidth;

            while (length > 0) {
                var targetMaterial = GetMaterialAtPixel(pixelPosition);
                if (!solidOnly || ((MaterialInfo.IsLooseOrSolid(targetMaterial) || targetMaterial == material) && targetMaterial != Material.Boundry)) {
                    RenderBrush(pixelPosition, brush, material, digStrength);
                    if (callback != null)
                        callback(pixelPosition);
                } else
                    return;
                pixelPosition += Vector2.Transform(new Vector2(driftLength, 0), Matrix.CreateRotationZ((float)(Utility.NextDouble() * Math.PI * 2)));
                length--;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} Tier {4} {1}x{2} {3} entities\n", Name, Width, Height, Entities.Count, Tier);
        }
    }
}