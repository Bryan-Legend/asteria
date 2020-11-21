using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using ProtoBuf;

#if WINDOWS
using System.Xaml;
using Steamworks;
#endif

namespace HD
{
    [ProtoContract]
    public class Player : Living
    {
        public const int ActionBarSlots = 10;
        public const int InventorySlots = 900;
        public const int EquipmentSlots = 8;
        const int SpriteWidth = 64;
        const int SpriteHeight = 64;

        const int scrollSpeed = 1250;

        public static Texture2D[] Textures;
        public static Texture2D[] ArmTextures;
        public static Action<Player, Player> OnPlayerSwap;

        // reminder: set the default for these type of variables in CalculateStats()
        public float MaxRunSpeed;
        public float JumpSpeed;
        public float MaxJumpLength;
        public bool IsImmuneToFallDamage;
        public int Regeneration;
        public Color CastLight;
        public double MaxAirSupply;
        public float FallReduction;
        public bool ExplodesOnDeath;

        public bool IsRunning;
        public bool IsJumping;
        public float JumpLength;
        public CreativeMode CreativeMode;

        public string Name { get; set; }
        public string SaveMap { get; set; }
        public string SaveWorldName { get; set; }
        public string Password { get; set; }
        public List<Item> Inventory { get; set; }
        public List<Flag> Flags { get; set; }
        public byte SelectedActionSlot { get; set; }
        public bool IsAdmin { get; set; }
        public int Selected { get; set; }
        public Brush SelectedBrush { get; set; }
        public Vector2 StartPosition { get; set; }
        public double UsedAirSupply { get; set; }
        public bool IsGameCompleted { get; set; }
        public int Skin { get; set; }
        public int Lives { get; set; }

        public Placeable ActivePlaceable;

        public Point MousePosition;
        public bool MouseLeftButtonPressed;
        public bool MouseRightButtonPressed;
        public bool LastMouseLeftButtonPressed;
        public bool LastMouseRightButtonPressed;
        public GamePadState GamePad;
        public GamePadState LastGamePad;
        public KeyboardState Keyboard;
        public KeyboardState LastKeyboard;

        public Queue<FileTransfer> PendingFileTransfers = new Queue<FileTransfer>();
        public int ClientResolutionHeight;
        public int ClientResolutionWidth;
        public bool InventoryChanged;
        public bool IsMainPlayer;
        public bool IsMapChanged;
        public bool IsMapSpawnDataChanged = true;
        public int CraftAmount;
        public int CraftRecipe;
        public bool SortInventory;
        public Map clipboardMap;

        public bool IsGod;

        public List<Message> Messages = new List<Message>();
        public Dictionary<SlotType, Item[]> SlotIndex;

        public Rectangle ScreenRectangle;
        CopySelectionMode copySelectionMode;
        Vector2 CopySelectionStart;
        Vector2 CopySelectionEnd;
        Rectangle copySelectionRectangle { get { return new Rectangle((int)CopySelectionStart.X, (int)CopySelectionStart.Y, (int)CopySelectionEnd.X - (int)CopySelectionStart.X, (int)CopySelectionEnd.Y - (int)CopySelectionStart.Y); } }

        public Vector2 ActualStartPosition { get { return StartPosition != Vector2.Zero ? StartPosition : Map.StartingPosition; } }
        public Item Tool { get { return GetItem(SlotType.ActionBar, SelectedActionSlot); } }
        public override Color Light { get { return CastLight; } }
        public bool IsSwimming { get { return CurrentMaterialInfo != null && CurrentMaterialInfo.MaxVelocity != 0; } }

        public override Material CurrentMaterial
        {
            get
            {
                return base.CurrentMaterial;
            }
            set
            {
                if (CurrentMaterial != value)
                {
                    if (CurrentMaterialInfo != null && CurrentMaterialInfo.ExitSound != Sound.None && Map != null)
                    {
                        Map.AddEntity(new ParticleEmitter() { Position = Position, Type = ParticleEffect.Splash });
                        PlaySound(CurrentMaterialInfo.ExitSound);
                    }
                    base.CurrentMaterial = value;
                    CurrentMaterialInfo = MaterialInfo.Get(CurrentMaterial);
                    if (CurrentMaterialInfo != null && CurrentMaterialInfo.EnterSound != Sound.None && Map != null)
                    {
                        Map.AddEntity(new ParticleEmitter() { Position = Position, Type = ParticleEffect.Splash });
                        PlaySound(CurrentMaterialInfo.EnterSound);
                    }
                }
            }
        }

        public Player()
        {
            Name = "Player Name";
            SaveMap = "Start";

            ///Player Size
            BoundingBox = new Rectangle(-20, -32, 40, 64);

            Inventory = new List<Item>();
            UpdateSlots();

            Respawn();
        }

        void CalculateStats()
        {
            IsAdmin = false;
            MaxHealth = 100;
            MaxRunSpeed = 400;
            Defense = 0;
            JumpSpeed = 850;
            MaxJumpLength = 250;
            IsImmuneToFallDamage = false;
            Regeneration = 0;
            CastLight = Color.Transparent;
            MaxAirSupply = 30;
            FallReduction = 0;
            ExplodesOnDeath = false;
            FireResistance = 0;
            IsGod = false;

            foreach (var item in SlotIndex[SlotType.Equipment])
            {
                if (item != null && item.Type != null && item.Type.OnEquip != null)
                    item.Type.OnEquip(this, item);
            }

            MaxHealth = Math.Max(100, MaxHealth);
            Defense = Math.Max(0, Defense);
            Regeneration = Math.Max(0, Regeneration);

            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public static string GetSaveFilename(string name)
        {
            return Path.Combine(Utility.SavePath, name + ".Player");
        }

        public void Save()
        {
            if (Map != null)
            {
                SaveMap = Map.Name;
                SaveWorldName = World.Name;
            }
#if WINDOWS
            XamlServices.Save(GetSaveFilename(Name), this);
#else
            using (var container = World.GetStorageContainer())
            {
                using (var stream = container.CreateFile(GetSaveFilename(Name)))
                {
                    ProtoBuf.Serializer.Serialize(stream, this);
                }
            }
#endif
        }

        public static Player Load(string filename)
        {
#if WINDOWS
            var loadedPlayer = XamlServices.Load(filename) as Player;
#else
            Player loadedPlayer;
            using (var container = World.GetStorageContainer())
            {
                using (var stream = container.OpenFile(filename, FileMode.Open))
                {
                    loadedPlayer = Serializer.Deserialize<Player>(stream);
                }
            }
#endif
            loadedPlayer.Id = 0; // clear so that it will get set to a new value
            loadedPlayer.UpdateSlots();
            loadedPlayer.IsMainPlayer = true;

            // clean any items that no longer have typess
            foreach (var item in loadedPlayer.Inventory.ToArray())
            {
                if (item == null || item.Type == null)
                    loadedPlayer.RemoveItem(item);
            }

            return loadedPlayer;
        }

        public void Respawn()
        {
            if (Map != null)
            {
                Position = Map.FindGround(ActualStartPosition) + new Vector2(0, BoundingBox.Top);
                Created = Map.Now;
                Map.InitialSpawnCheck(Position);
            }

            UsedAirSupply = 0;
            Velocity = Vector2.Zero;
            Health = MaxHealth;
            PlaySound(Sound.PlayerSpawn);
            SetAnimation(Animation.Spawn, true);
            Lives--;

            if (Map != null)
            {
                Map.ResetOnDieCheck(this);

                if (Map.ExtraLives > 0 && Lives < 0)
                {
                    var sourcePortal = World.Overworld.FindPortalTo(null, Map);
                    SendTo(World.Overworld, sourcePortal != null ? sourcePortal.Position : Vector2.Zero);
                }
                else
                    Map.AddEntity(new ParticleEmitter() { Position = Position, Color = Color.White, Type = ParticleEffect.Burst, Value = 150 });
            }
        }

        public Player SendTo(Map map, Vector2 location = new Vector2())
        {
            if (location == Vector2.Zero)
            {
                if (Map != map)
                    location = map.StartingPosition;
                else
                    location = ActualStartPosition;
            }

            map.InitialSpawnCheck(location);
            Position = location;

            if (Map != map)
            {
                Counters.Commit(); // if achievement counters are being used.

                Map.RemoveEntity(this);

                var newPlayer = (Player)MemberwiseClone();

                newPlayer.LastMouseRightButtonPressed = true; // to stop portal toggling
                newPlayer.LastKeyboard = Keyboard;
                newPlayer.LastGamePad = GamePad;

                newPlayer.Id = 0;
                newPlayer.IsMapChanged = true;
                newPlayer.Map = map;
                newPlayer.Lives = map.ExtraLives;

                if (Map.Name != map.Name)
                    newPlayer.StartPosition = Vector2.Zero;
                else
                    newPlayer.StartPosition = StartPosition; // preserve start position if this zone was just reset

                if (OnPlayerSwap != null)
                    OnPlayerSwap(this, newPlayer);
                map.AddEntity(newPlayer);

                return newPlayer;
            }

            return this;
        }

        public override int Damage(int amount)
        {
            if (IsGod)
                amount = 0;

            if (amount > 0)
            {
                amount = base.Damage(amount);

                if (amount > 0)
                {
                    AddCombatText(amount.ToString("N0"), CombatTextType.PlayerDamage, 0, amount);

                    if (!IsDead)
                    {
                        if (amount > MaxHealth / 20.0)  // if more than 5% damage done play sound
                            SetAnimation(Animation.Hit, true);

                        switch (Utility.Next(3))
                        {
                            case 0:
                                PlaySound(Sound.PlayerDamaged, (float)amount / (float)MaxHealth);
                                break;
                            case 1:
                                PlaySound(Sound.PlayerDamaged2, (float)amount / (float)MaxHealth);
                                break;
                            case 2:
                                PlaySound(Sound.PlayerDamaged3, (float)amount / (float)MaxHealth);
                                break;
                            default:
                                throw new Exception("Random damage sound generator broken.");
                        }
                    }
                }

                ActivePlaceable = null;
            }

            return amount;
        }

        public override void Die()
        {
            base.Die();

            UsingItem = null;
            PlayingSound = Sound.None;
            SetAnimation(Animation.Dead);

            Velocity.X = 0;

            if (ExplodesOnDeath)
                Map.Explode(Position, 200);

            PlaySound(Sound.PlayerDie);
        }

        public Item GiveItem(ItemId id, int amount = 1)
        {
            if (amount > 0)
            {
                var type = ItemBase.Get(id);
                if (type.IsUsedImmediately)
                {
                    var tempItem = new Item() { Type = type, Amount = amount, SlotType = SlotType.None };
                    for (int i = 0; i < amount; i++)
                    {
                        tempItem.Use(this);
                    }
                    return null;
                }

                InventoryChanged = true;

                var item = GetItem(id);
                if (item != null && (item.Type.Category == ItemCategory.Component || item.Type.Category == ItemCategory.Material || item.Type.Category == ItemCategory.Placable || item.Type.Category == ItemCategory.Useable))
                {
                    item.Amount += amount;
                }
                else
                {
                    item = new Item() { Type = type, Amount = amount };
                    Inventory.Add(item);
                    var freeActionBarSlot = FindFreeSlot(SlotType.ActionBar);

                    if (item.Type.Category == ItemCategory.Component
                        || item.Type.Category == ItemCategory.Material
                        || item.Type.Category == ItemCategory.Boots
                        || item.Type.Category == ItemCategory.Helmet
                        || item.Type.Category == ItemCategory.KeyComponent
                        || item.Type.Category == ItemCategory.PowerCore
                        || item.Type.Category == ItemCategory.Suit)
                    {
                        freeActionBarSlot = -1;
                    }

                    if (freeActionBarSlot == -1)
                        SetSlot(item, SlotType.Inventory);
                    else
                    {
                        //if(item.Type.NameSource == "{TierType} Mining Tool")
                        //SetSlot(item, SlotType.ActionBar, 9);
                        //else    
                        SetSlot(item, SlotType.ActionBar, freeActionBarSlot);
                    }
                }

                AddCombatText("+" + item.Type.Name + " x" + amount.ToString("N0"), CombatTextType.Pickup, item.TypeIdInt, amount, item.Type.Rarity);

                return item;
            }
            return null;
        }

        public int FindFreeSlot(SlotType type)
        {
            var slotArray = SlotIndex[type];
            var slot = 0;
            // find a slot
            while (slotArray[slot] != null)
            {
                slot++;
                if (slot >= slotArray.Length)
                    return -1;
            }
            return slot;
        }

        public void SetSlot(Item item, SlotType type, int slot = -1, bool calculateStats = true)
        {
            InventoryChanged = true;

            var previousSlotType = item.SlotType;
            var slotArray = SlotIndex[type];
            if (slotArray.Length <= slot)
                return;

            if (slot == -1)
            {
                slot = FindFreeSlot(type);
                if (slot == -1)
                    throw new Exception("Unable to find free slot with auto slot finder.");
            }

            if (IsPlayerSlot(item.SlotType) && item.SlotNumber >= 0)
                SlotIndex[item.SlotType][item.SlotNumber] = null;

            if (slotArray[slot] != null)
            {
                if (item.SlotType == SlotType.Dragging)
                {
                    slotArray[slot].SlotType = SlotType.Dragging;
                    slotArray[slot].SlotNumber = 0;
                    SlotIndex[SlotType.Dragging][0] = slotArray[slot];
                }
                else
                    throw new Exception(String.Format("Already an item in slot {0} {1}.", type, slot));
            }

            item.SlotType = type;
            item.SlotNumber = slot;
            slotArray[slot] = item;

            if (calculateStats && (previousSlotType == SlotType.Equipment || type == SlotType.Equipment))
                CalculateStats();
        }

        public void UpdateSlots()
        {
            SlotIndex = new Dictionary<SlotType, Item[]>();
            SlotIndex.Add(SlotType.Inventory, new Item[InventorySlots]);
            SlotIndex.Add(SlotType.ActionBar, new Item[ActionBarSlots]);
            SlotIndex.Add(SlotType.Equipment, new Item[EquipmentSlots]);
            SlotIndex.Add(SlotType.Dragging, new Item[1]);
            SlotIndex.Add(SlotType.Trash, new Item[1]);

            foreach (var item in Inventory)
            {
                if (item != null)
                    SetSlot(item, item.SlotType, item.SlotNumber, false);
            }

            CalculateStats();
        }

        public static bool IsPlayerSlot(SlotType type)
        {
            return type == SlotType.Inventory || type == SlotType.ActionBar || type == SlotType.Equipment || type == SlotType.Dragging || type == SlotType.Trash;
        }

        public void RemoveItem(ItemId id, int amount)
        {
            // todo: propery amount handling.
            foreach (var item in Inventory)
            {
                if (item.Type.Id == id)
                {
                    RemoveItem(item, amount);
                    break;
                }
            }
        }

        public void RemoveItem(Item item, int amount)
        {
            InventoryChanged = true;
            item.Amount -= amount;
            if (item.Amount <= 0)
                RemoveItem(item);
        }

        public void RemoveItem(Item item)
        {
            InventoryChanged = true;

            if (item == null)
            {
                Inventory.Remove(item);
                return;
            }

            if (IsPlayerSlot(item.SlotType))
                SlotIndex[item.SlotType][item.SlotNumber] = null;

            var originalSlotType = item.SlotType;

            item.SlotType = SlotType.None;
            item.SlotNumber = -1;
            Inventory.Remove(item);

            if (originalSlotType == SlotType.Equipment)
                CalculateStats();
        }

        public int GetItemAmount(ItemId itemId)
        {
            var result = 0;
            foreach (var item in Inventory)
            {
                if (item.Type.Id == itemId)
                {
                    result += item.Amount;
                }
            }
            return result;
        }

        public Item GetItem(ItemId itemId)
        {
            foreach (var item in Inventory)
            {
                if (item.Type.Id == itemId)
                {
                    return item;
                }
            }
            return null;
        }

        public bool IsSlotEmpty(SlotType slotType, int slotNumber)
        {
            if (slotType == SlotType.Chest)
                return ActivePlaceable.Inventory[slotNumber] == null;

            return SlotIndex[slotType][slotNumber] == null;
        }

        public Item GetItem(SlotType slotType, int slotNumber)
        {
            if (slotType == SlotType.Chest)
                return ActivePlaceable.Inventory[slotNumber];

            return SlotIndex[slotType][slotNumber];
        }

        public bool HasItem(Item item)
        {
            return HasItem(item.TypeId, item.Amount);
        }

        public bool HasItem(ItemId type, int amount = 1)
        {
            var targetAmount = amount;
            foreach (var inventoryItem in from i in Inventory where i.TypeId == type select i)
            {
                targetAmount -= inventoryItem.Amount;
                if (targetAmount <= 0)
                    return true;
            }
            return false;
        }

        TimeSpan timeSinceLastHealthRegen;

        public override void Think(GameTime gameTime)
        {
            IsRunning = false;

            var leftClickHandled = false;

            if (IsAdmin)
            {
                if (Keyboard.IsPressed(LastKeyboard, Keys.F1))
                {
                    if (CreativeMode != CreativeMode.Material)
                    {
                        CreativeMode = CreativeMode.Material;
                        if (MaterialInfo.MaterialTypes.Length < Selected || !MaterialInfo.Get((Material)Selected).IsVisible)
                            Selected = (int)Material.Dirt;
                    }
                    else
                    {
                        copySelectionMode = CopySelectionMode.None;
                        CreativeMode = CreativeMode.None;
                    }
                }
                if (Keyboard.IsPressed(LastKeyboard, Keys.F2))
                {
                    if (CreativeMode != CreativeMode.Placeable)
                        CreativeMode = CreativeMode.Placeable;
                    else
                        CreativeMode = CreativeMode.None;
                }
                if (Keyboard.IsPressed(LastKeyboard, Keys.F3))
                {
                    if (CreativeMode != CreativeMode.Enemy)
                        CreativeMode = CreativeMode.Enemy;
                    else
                        CreativeMode = CreativeMode.None;
                }
                if (Keyboard.IsPressed(LastKeyboard, Keys.F4))
                {
                    if (CreativeMode != CreativeMode.Item)
                        CreativeMode = CreativeMode.Item;
                    else
                        CreativeMode = CreativeMode.None;
                }

                if (Keyboard.IsPressed(LastKeyboard, Keys.OemTilde))
                    IsGod = !IsGod;

                if (CreativeMode == CreativeMode.Material)
                {
                    switch (copySelectionMode)
                    {
                        case CopySelectionMode.None:
                            if (Keyboard.IsPressed(LastKeyboard, Keys.C))
                            {
                                copySelectionMode = CopySelectionMode.SelectStart;
                                CopySelectionStart = Vector2.Zero;
                                CopySelectionEnd = Vector2.Zero;
                                MessageToClient("Click and drag to select the terrain to copy to the clipboard.", MessageType.System);
                            }
                            break;
                        case CopySelectionMode.SelectStart:
                            if (MouseLeftButtonPressed)
                            {
                                CopySelectionStart = GetToolTargetPosition();
                                CopySelectionEnd = GetToolTargetPosition();
                                copySelectionMode = CopySelectionMode.SelectActive;
                                leftClickHandled = true;
                            }
                            break;
                        case CopySelectionMode.SelectActive:
                            CopySelectionEnd = GetToolTargetPosition();
                            if (!MouseLeftButtonPressed)
                            {
                                copySelectionMode = CopySelectionMode.None;

                                if (CopySelectionEnd.X > Map.PixelWidth)
                                    CopySelectionEnd.X = Map.PixelWidth;
                                if (CopySelectionEnd.Y > Map.PixelHeight)
                                    CopySelectionEnd.Y = Map.PixelHeight;

                                if (CopySelectionEnd.X > CopySelectionStart.X && CopySelectionEnd.Y > CopySelectionStart.Y)
                                {
                                    clipboardMap = Map.Copy(copySelectionRectangle);
                                    clipboardUpdated = true;
                                    MessageToClient("Selection copied to the clipboard. Left mouse to paste, right mouse to paste ignoring air, shift to paste background, escape to cancel.", MessageType.System);
                                }
                            }
                            leftClickHandled = true;
                            break;
                    }
                }
                else
                    copySelectionMode = CopySelectionMode.None;

                if (Keyboard.IsPressed(LastKeyboard, Keys.Escape) || GamePad.IsPressed(LastGamePad, Buttons.B))
                {
                    if (clipboardMap != null)
                    {
                        clipboardMap = null;
                        clipboardUpdated = true;
                    }
                    else
                        CreativeMode = CreativeMode.None;
                }
            }

            var acceleration = Velocity;

            if (IsGod)
            {
                Velocity = Vector2.Zero;

                if (Keyboard.IsKeyDown(Keys.A) || Keyboard.IsKeyDown(Keys.Left))
                {
                    Position = new Vector2(Position.X - (scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), Position.Y);
                }
                else if (Keyboard.IsKeyDown(Keys.D) || Keyboard.IsKeyDown(Keys.Right))
                {
                    Position = new Vector2(Position.X + (scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), Position.Y);
                }

                if (Keyboard.IsKeyDown(Keys.W) || Keyboard.IsKeyDown(Keys.Up) || Keyboard.IsKeyDown(Keys.Space))
                {
                    Position = new Vector2(Position.X, Position.Y - (scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
                }
                else if (Keyboard.IsKeyDown(Keys.S) || Keyboard.IsKeyDown(Keys.Down))
                {
                    Position = new Vector2(Position.X, Position.Y + (scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
                }
            }
            else
            {
                if (!IsDead && Animation != Animation.Spawn)
                {
                    if (Keyboard.IsKeyDown(Keys.A) || Keyboard.IsKeyDown(Keys.Left) || GamePad.IsButtonDown(Buttons.LeftThumbstickLeft))
                    {
                        IsRunning = true;
                        IsFacingLeft = true;
                        if (Velocity.X > 0)
                            Velocity.X = 0;
                        // Accellerate over 200ms
                        Velocity.X -= (MaxRunSpeed / 200) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (Velocity.X < -MaxRunSpeed)
                            Velocity.X = -MaxRunSpeed;
                    }
                    else if (Keyboard.IsKeyDown(Keys.D) || Keyboard.IsKeyDown(Keys.Right) || GamePad.IsButtonDown(Buttons.LeftThumbstickRight))
                    {
                        IsRunning = true;
                        IsFacingLeft = false;
                        if (Velocity.X < 0)
                            Velocity.X = 0;
                        // Accellerate over 200ms
                        Velocity.X += (MaxRunSpeed / 200) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (Velocity.X > MaxRunSpeed)
                            Velocity.X = MaxRunSpeed;
                    }
                    else
                    {
                        // wind resistance to slow the player while in air
                        if (Velocity.X > 0)
                        {
                            Velocity.X -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 2f;
                            if (Velocity.X < 0)
                                Velocity.X = 0;
                        }
                        else if (Velocity.X < 0)
                        {
                            Velocity.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 2f;
                            if (Velocity.X > 0)
                                Velocity.X = 0;
                        }
                    }

                    if (Keyboard.IsKeyDown(Keys.Down) || Keyboard.IsKeyDown(Keys.S) || GamePad.IsButtonDown(Buttons.LeftThumbstickDown))
                        IsDroppingThruPlatform = true;
                    else
                        IsDroppingThruPlatform = false;

                    if (IsJumping)
                    {
                        if (Keyboard.IsKeyDown(Keys.Space) || GamePad.IsButtonDown(Buttons.A))
                        {
                            JumpLength += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                            //Utility.LogMessage("Time {0:n3}, Velocity {1:n3}, JumpLength {2:n3}, JumpFactor {3:n3}", gameTime.ElapsedGameTime.TotalSeconds, Velocity.Y, JumpLength, jumpFactor);

                            if (JumpLength >= MaxJumpLength)
                            {
                                IsJumping = false;
                                if (IsSwimming)
                                    JumpLength = 0;
                            }
                        }
                        else
                        {
                            IsJumping = false;
                            if (Velocity.Y < -300.0f)
                                Velocity.Y = -300.0f;
                        }
                    }

                    if (IsImmersed)
                    {
                        UsedAirSupply += gameTime.ElapsedGameTime.TotalSeconds;

                        if (UsedAirSupply > MaxAirSupply)
                        {
                            Die();
                        }
                    }
                    else
                        UsedAirSupply = 0;
                }

                if (CurrentMaterialInfo != null && CurrentMaterialInfo.MaxVelocity > 0)
                {
                    // TODO: make this more correct? Calc length etc...
                    if (Velocity.X > CurrentMaterialInfo.MaxVelocity)
                        Velocity.X = CurrentMaterialInfo.MaxVelocity;
                    if (Velocity.Y > CurrentMaterialInfo.MaxVelocity)
                        Velocity.Y = CurrentMaterialInfo.MaxVelocity;
                    if (Velocity.X < -CurrentMaterialInfo.MaxVelocity)
                        Velocity.X = -CurrentMaterialInfo.MaxVelocity;
                    //if (Velocity.Y < -CurrentMaterialInfo.MaxVelocity)
                    //    Velocity.Y = -CurrentMaterialInfo.MaxVelocity;
                }

                var fallAmount = ApplyGravity(gameTime);
                if (FallReduction > 0)
                    Velocity.Y -= fallAmount * FallReduction;

                acceleration = Velocity - acceleration;

                var wasOnGround = IsOnGround;
                var previousVerticalVelocity = Velocity.Y;
                MoveVertical(gameTime, acceleration);

                if (!IsImmuneToFallDamage && Velocity.Y == 0 && previousVerticalVelocity > 1750)
                {
                    var damagePrecentage = ((previousVerticalVelocity - 1200f) / 25f) / 100f;
                    Damage((int)(MaxHealth * damagePrecentage));
                }

                if (!wasOnGround && IsOnGround)
                {
                    PlaySound(Sound.PlayerLand, previousVerticalVelocity / 2000);
                    if (JumpLength > 200 && Animation != Animation.Dead && Animation != Animation.Hit && Animation != Animation.Spawn)
                        SetAnimation(Animation.Land);
                }

                if (!IsDead && Animation != Animation.Spawn)
                {
                    if ((Keyboard.IsPressed(LastKeyboard, Keys.Space) || GamePad.IsPressed(LastGamePad, Buttons.A)) && (IsOnGround || IsSwimming || IsNearGround()))
                    {
                        IsJumping = true;
                        Velocity.Y = -JumpSpeed;
                        JumpLength = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (IsSwimming)
                        {
                            SetAnimation(Animation.Jump, true);
                            PlaySound(Sound.PlayerSwim);
                        }
                        else
                            PlaySound(Sound.PlayerJump);
                    }
                    else
                    {
                        if (IsOnGround)
                        {
                            Velocity.Y = 0;
                            JumpLength = 0;

                            // make the player stop sliding via ground resistance
                            if (!IsRunning)
                            {
                                if (Velocity.X > 0)
                                {
                                    Velocity.X -= (MaxRunSpeed / 100) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                                    if (Velocity.X < 0)
                                        Velocity.X = 0;
                                }
                                else
                                {
                                    if (Velocity.X < 0)
                                    {
                                        Velocity.X += (MaxRunSpeed / 100) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                                        if (Velocity.X > 0)
                                            Velocity.X = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    Velocity.X = 0; // player is dead

                //Utility.LogMessage("IsOnGround: {0} JumpLength {1} wasOnGround {2}", IsOnGround, JumpLength, wasOnGround);

                MoveHorizontal(gameTime);
            }

            var isMovingLeft = IsFacingLeft;
            IsFacingLeft = MousePosition.X < Position.X;

            if (IsDead)
            {
                if ((Map.Now - Died).TotalSeconds > 3)
                {
                    if (!Map.CanRespawn())
                    {
                        Map.ResetOnDieCheck(this);
                        return;
                    }

                    Respawn();
                    return;
                }
            }
            else
            {
                timeSinceLastHealthRegen += gameTime.ElapsedGameTime;
                if (timeSinceLastHealthRegen.TotalSeconds > 1)
                {
                    Heal(Regeneration, false);
                    timeSinceLastHealthRegen -= TimeSpan.FromSeconds(1);
                }

                if (!leftClickHandled && MouseLeftButtonPressed)
                {
                    switch (CreativeMode)
                    {
                        case HD.CreativeMode.None:
                            var draggingItem = GetItem(SlotType.Dragging, 0);
                            if (draggingItem != null)
                            {
                                if (Keyboard.IsShift())
                                {
                                    var pickup = Map.AddPickup(Position, draggingItem.TypeId, draggingItem.Amount);
                                    pickup.Owner = Name;
                                    RemoveItem(draggingItem);
                                }
                                else
                                {
                                    draggingItem.Use(this);
                                    UsingItem = Tool; // hotwire this so that the dematerializer won't activate on the item that was just placed
                                }
                            }
                            else
                            {
                                if (!UseTool(Tool) && !LastMouseLeftButtonPressed)
                                    PlaySound(Sound.Error);
                            }
                            break;
                        case HD.CreativeMode.Material:
                            if (clipboardMap != null)
                            {
                                if (Keyboard.IsShift())
                                    Map.PasteWall(clipboardMap, GetToolTargetPosition().ToPoint());
                                else
                                    Map.Paste(clipboardMap, GetToolTargetPosition().ToPoint());
                            }
                            else
                            {
                                if (Keyboard.IsShift())
                                    Map.RenderWallBrush(GetToolTargetPosition(), SelectedBrush, (Material)Selected);
                                else
                                    Map.RenderBrush(GetToolTargetPosition(), SelectedBrush, (Material)Selected);
                            }
                            break;
                        case HD.CreativeMode.Enemy:
                            if (!LastMouseLeftButtonPressed)
                            {
                                if (EnemyBase.TypesById.ContainsKey(Selected))
                                {
                                    if (Keyboard.IsShift())
                                        Map.AddEnemy(EnemyBase.Get(Selected), GetToolTargetPosition());
                                    else
                                        Map.AddSpawn(new Spawn() { TypeId = Selected, Position = GetToolTargetPosition() });
                                }
                            }
                            break;
                        case HD.CreativeMode.Placeable:
                            if (Keyboard.IsShift())
                            {
                                var position = GetToolTargetPosition();
                                var placeable = Map.FindClosestPlaceable(position);
                                if (placeable != null)
                                    placeable.Position = position;
                            }
                            else
                            {
                                if (!LastMouseLeftButtonPressed)
                                {
                                    if (ItemBase.TypesById.ContainsKey((ItemId)Selected))
                                    {
                                        var placeable = ItemBase.Get((ItemId)Selected);
                                        if (placeable.Category == ItemCategory.Placable)
                                            Map.AddPlaceable(this, ItemBase.Get((ItemId)Selected), GetToolTargetPosition());
                                    }
                                }
                            }
                            break;
                        case HD.CreativeMode.Item:
                            if (!LastMouseLeftButtonPressed)
                                Map.AddPickup(GetToolTargetPosition(), (ItemId)Selected, 1);
                            break;
                    }
                }
                else if (!MouseRightButtonPressed && !MouseLeftButtonPressed)
                {
                    if (UsingItem != null)
                    {
                        UsingItem.StopUsing();
                        UsingItem = null;
                    }
                    if (lastItem != null)
                    {
                        lastItem.StopUsing();
                        lastItem = null;
                    }
                }

                if ((Keyboard.IsPressed(LastKeyboard, Keys.W) || Keyboard.IsPressed(LastKeyboard, Keys.Up) || GamePad.IsPressed(LastGamePad, Buttons.LeftThumbstickUp)) && ActivePlaceable == null)
                {
                    var placeable = Map.FindClosestPlaceable(MousePosition.ToVector2(), true);
                    if (placeable != null && WithinRange(placeable, placeable.Type.PlaceAndActivateRange))
                    {
                        placeable.Type.OnActivate(this, placeable);
                    }
                    else
                    {
                        // check near player if the mouse wasn't near an item
                        placeable = Map.FindClosestPlaceable(Position, true);
                        if (placeable != null && WithinRange(placeable, placeable.Type.PlaceAndActivateRange))
                        {
                            placeable.Type.OnActivate(this, placeable);
                        }
                    }
                }

                if (MouseRightButtonPressed)
                {
                    switch (CreativeMode)
                    {
                        case HD.CreativeMode.None:
                            if (!MouseLeftButtonPressed)
                            {
                                if (!UseTool(GetItem(SlotType.ActionBar, 9)) && !LastMouseRightButtonPressed)
                                    PlaySound(Sound.Error);
                            }
                            break;
                        case HD.CreativeMode.Material:
                            if (clipboardMap != null)
                            {
                                if (Keyboard.IsShift())
                                    Map.PasteWall(clipboardMap, GetToolTargetPosition().ToPoint(), false);
                                else
                                    Map.Paste(clipboardMap, GetToolTargetPosition().ToPoint(), false);
                            }
                            else
                            {
                                if (Keyboard.IsShift())
                                    Map.RenderWallBrush(GetToolTargetPosition(), SelectedBrush, Material.Air);
                                else
                                    Map.RenderBrush(GetToolTargetPosition(), SelectedBrush, Material.Air);
                            }
                            break;
                        case HD.CreativeMode.Enemy:

                            var targetPosition = GetToolTargetPosition();
                            Map.RemoveSpawn(targetPosition);

                            var enemy = Map.FindEntity(targetPosition);
                            if (enemy is Enemy)
                                enemy.Remove();
                            break;
                        case HD.CreativeMode.Placeable:
                            var entity = Map.FindEntity(GetToolTargetPosition());
                            if (entity is Placeable)
                                entity.Remove();
                            break;
                    }
                }
                else if (!MouseLeftButtonPressed && !MouseRightButtonPressed)
                {
                    if (UsingItem != null)
                    {
                        UsingItem.StopUsing();
                        UsingItem = null;
                    }
                    if (lastItem != null)
                    {
                        lastItem.StopUsing();
                        lastItem = null;
                    }
                }
            }

            if (ActivePlaceable != null && !WithinRange(ActivePlaceable.Position))
                ActivePlaceable = null;

            if (Animation != Animation.Hit && Animation != Animation.Dead && Animation != Animation.Land && Animation != Animation.Spawn)
            {
                if (IsOnGround || (JumpLength <= 0 && !IsSwimming))
                {
                    if (IsRunning)
                    {
                        if (isMovingLeft != IsFacingLeft)
                            SetAnimation(Animation.MoveBackwards);
                        else
                            SetAnimation(Animation.Move);
                    }
                    else
                        SetAnimation(Animation.None);
                }
                else
                    SetAnimation(Animation.Jump);
            }

            if (CraftRecipe != 0)
            {
                var draggingItem = GetItem(SlotType.Dragging, 0);
                if (draggingItem != null)
                {
                    PlaySound(Sound.Error);
                    MessageToClient("Unable to craft an item when you're already dragging an item.", MessageType.Error);
                    CraftRecipe = 0;
                }
                else
                {
                    var recipe = RecipeBase.Get(CraftRecipe);
                    CraftRecipe = 0;
                    if (recipe != null)
                    {
                        var newItem = recipe.Create(this, CraftAmount);
                        if (newItem != null)
                        {
                            GiveItem(newItem.TypeId, newItem.Amount);

                            //Inventory.Add(newItem);
                            //InventoryChanged = true;
                            //SetSlot(newItem, SlotType.Dragging);
                            PlaySound(Sound.SmallTick);
                        }
                    }
                }
            }

            if (SortInventory)
                DoSortInventory();

            base.Think(gameTime);

            LastKeyboard = Keyboard;
            LastMouseLeftButtonPressed = MouseLeftButtonPressed;
            LastMouseRightButtonPressed = MouseRightButtonPressed;
            LastGamePad = GamePad;
        }

        public void SelectPrevTool()
        {
            SelectedActionSlot--;
            if (SelectedActionSlot > 8)
                SelectedActionSlot = 8;
        }

        public void SelectNextTool()
        {
            SelectedActionSlot++;
            if (SelectedActionSlot > 8)
                SelectedActionSlot = 0;
        }

        public bool UseTool(Item tool)
        {
            if (tool != null)
            {
                var alreadyUsing = true;
                if (UsingItem != tool)
                {
                    tool.StartUsing(this);
                    lastItem = UsingItem;
                    UsingItem = tool;
                    alreadyUsing = false;
                }

                tool.Use(this, alreadyUsing);
                return true;
            }
            return false;
        }

        public int GetMiningTier()
        {
            return (from i in Inventory where i.Type.Category == ItemCategory.MiningTool orderby i.Type.Tier descending select i.Type.Tier).FirstOrDefault();
        }

        public Vector2 GetToolTargetPosition(int snapResolution = 8)
        {
            //Utility.DebugMessage2 = MousePosition.ToString();
            return (new Vector2(MousePosition.X, MousePosition.Y) - new Vector2(snapResolution / 2, snapResolution / 2)).Round(snapResolution);
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            var box = new Rectangle((int)Position.X, (int)Position.Y, SpriteWidth, SpriteHeight);
            box.Offset(-screenOffset.X - SpriteWidth / 2, -screenOffset.Y - SpriteHeight / 2);

            var frame = GetCurrentFrame();
            // draw arm
            if ((frame <= 34 || frame >= 48) && Animation != Animation.Spawn)
            {
                float armAngle;
                var shoulderOrigin = GetShoulderPosition(out armAngle, frame);
                spriteBatch.Draw(
                    ArmTextures[Skin],
                    new Vector2(shoulderOrigin.X - screenOffset.X, shoulderOrigin.Y - screenOffset.Y),
                    null,
                    Color.White,
                    armAngle,
                    new Vector2(5, 16),
                    1,
                    IsFacingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    0);
            }

            spriteBatch.Draw(Textures[Skin], box, GetFrame(frame), Color.White, 0, Vector2.Zero, IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

            if (Animation == Animation.Move || Animation == Animation.MoveBackwards)
            {
                if (PlayingSound == Sound.None)
                    PlayingSound = Sound.PlayerRun;

                if (IsOnGround)
                {
                    Particles.Add
                    (
                        new Particle()
                        {
                            Position = Position + new Vector2((IsFacingLeft != (Animation == Animation.Move) ? BoundingBox.Left : BoundingBox.Right) / 2, BoundingBox.Bottom),
                            Color = Color.Tan,
                            Velocity = Utility.RandomVector() * 0.1f,
                            MaxAge = 0.5,
                            Scale = 0.5f
                        }
                    );
                }
            }
            else
            {
                if (PlayingSound == Sound.PlayerRun)
                    PlayingSound = Sound.None;
            }

            if (Animation == Animation.Hit)
                Particles.AddExplode(Position, Color.LightBlue, 1);

            if (UsingItem != null && UsingItem.Type.OnUsingDraw != null && UsingItem.Player != null)
                UsingItem.Type.OnUsingDraw(UsingItem, spriteBatch);

            if (!IsMainPlayer)
            {
                var textSize = Utility.SmallFont.MeasureString(Name);
                var x = box.X + 32 - (textSize.X / 2);
                var y = box.Y - textSize.Y;
                spriteBatch.DrawString(Utility.SmallFont, Name, new Vector2(x, y), Color.White);
            }
        }

        public override void DrawTop(SpriteBatch spriteBatch, Point screenOffset)
        {
            if (IsMainPlayer)
            {
                if (CreativeMode == CreativeMode.Material)
                {
                    if (copySelectionMode != CopySelectionMode.None)
                    {
                        if (CopySelectionEnd.X > CopySelectionStart.X || CopySelectionEnd.Y > CopySelectionStart.Y)
                        {
                            var copySourceArea = copySelectionRectangle;
                            copySourceArea.X -= screenOffset.X;
                            copySourceArea.Y -= screenOffset.Y;
                            spriteBatch.DrawRectangle(copySourceArea, Color.FromNonPremultiplied(101, 133, 181, 128));
                        }
                    }
                    else
                    {
                        if (clipboardMap != null)
                        {
                            var pastePosition = GetToolTargetPosition() - screenOffset.ToVector2();
                            spriteBatch.DrawRectangle(new Rectangle((int)pastePosition.X, (int)pastePosition.Y, clipboardMap.PixelWidth, clipboardMap.PixelHeight), Color.FromNonPremultiplied(0, 0, 0, 64));
                            MapRenderer.Draw(clipboardMap, Keyboard.IsShift() ? spriteBatch : null, Keyboard.IsShift() ? null : spriteBatch, 0, 0, clipboardMap.PixelWidth - 1, clipboardMap.PixelHeight - 1, pastePosition);
                        }
                        else
                        {
                            var toolPosition = GetToolTargetPosition();
                            DrawBrush(spriteBatch, SelectedBrush, new Point((int)toolPosition.X - screenOffset.X - 8 * Map.BlockWidth, (int)toolPosition.Y - screenOffset.Y - 7 * Map.BlockHeight));
                        }
                    }
                }
                else
                {
                    var tool = Tool;
                    if (tool != null)
                    {
                        if (tool.Type.Category == ItemCategory.Material)
                        {
                            if (CanPlaceMaterial(tool))
                            {
                                var toolPosition = GetToolTargetPosition();
                                DrawBrush(spriteBatch, SelectedBrush, new Point((int)toolPosition.X - screenOffset.X - 8 * Map.BlockWidth, (int)toolPosition.Y - screenOffset.Y - 7 * Map.BlockHeight));
                            }
                        }
                        else if (tool.Type.Category == ItemCategory.Placable)
                        {
                            var toolPosition = GetToolTargetPosition();
                            if (tool.CanPlace(this, toolPosition))
                                tool.Type.Draw(spriteBatch, new Vector2((toolPosition.X - screenOffset.X) + tool.Type.BoundingBox.Left, (toolPosition.Y - screenOffset.Y) + tool.Type.BoundingBox.Top));
                        }
                    }
                }
            }

            base.DrawTop(spriteBatch, screenOffset);
        }

        void DrawBrush(SpriteBatch spriteBatch, Brush brush, Point position)
        {
            if (brush == Brush.Mega)
                spriteBatch.DrawRectangle(new Rectangle(position.X - (32 * Map.BlockWidth), position.Y - (32 * Map.BlockWidth), 64 * Map.BlockWidth, 64 * Map.BlockHeight), Color.FromNonPremultiplied(192, 192, 192, 192));
            else
                spriteBatch.Draw(Utility.BrushTexture, new Rectangle(position.X, position.Y, 16 * Map.BlockWidth, 16 * Map.BlockHeight), new Rectangle(16 * (int)brush, 0, 16, 16), Color.FromNonPremultiplied(255, 255, 255, 192));
        }

        readonly int[] shoulderOffsets = new int[] {
            -8, -6, -3, 0, 3, 0, -3, -6,
            0, 0, 0, 0, 0, 0, 0, 0, 
            8, 8, 9, 9, 0, 0, 0, 0, 
            8, 8, 9, 0, 0, 0, 0, 0, 
            -2, -5, -8, -8, -8, -8, -8, -8, 
            -8, -8, -8, -8, -8, -8, -8, -8,
            8, 7, 7, 7, 6, 0, 0, 0, 
        };

        public Vector2 GetShoulderPosition(out float armAngle, int frame = -1)
        {
            if (frame == -1)
                frame = GetCurrentFrame();
            var shoulderOrigin = Position + new Vector2(IsFacingLeft ? -shoulderOffsets[frame] : shoulderOffsets[frame], -19);
            armAngle = (float)(Math.Atan2(MousePosition.Y - shoulderOrigin.Y, MousePosition.X - shoulderOrigin.X));
            return shoulderOrigin;
        }

        public Vector2 GetShootingOrigin()
        {
            float armAngle;
            var shoulderOrigin = GetShoulderPosition(out armAngle);
            return GetShootingOrigin(shoulderOrigin, armAngle);
        }

        public Vector2 GetElbowPosition(Vector2 shoulderPosition, float armAngle)
        {
            return shoulderPosition + Vector2.Transform(new Vector2(10, IsFacingLeft ? -5 : 5), Matrix.CreateRotationZ(armAngle));
        }

        public Vector2 GetShootingOrigin(Vector2 shoulderPosition, float armAngle)
        {
            return shoulderPosition + Vector2.Transform(new Vector2(27, IsFacingLeft ? -5 : 5), Matrix.CreateRotationZ(armAngle));
        }

        public float GetShootingAngle(Vector2 shootingOrigin, float angleOffset = 0)
        {
            return (float)(Math.Atan2(MousePosition.Y - shootingOrigin.Y, MousePosition.X - shootingOrigin.X)) + angleOffset;
        }

        const int moveFrameDelay = 50;
        readonly int[] idleFrameLookup = new int[] { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 17, 18, 17, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 25, 26, 25, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 48, 49, 50, 51, 52, 51, 50, 49, 48 };
        readonly int[] spawnFrameLookup = new int[] { 45, 44, 43, 42, 13, 14, 15, 20, 21, 22, 21, 31 };
        private bool clipboardUpdated;
        public Item UsingItem;
        Item lastItem;

        public int GetCurrentFrame()
        {
            var animationAge = (int)(Map.Now - AnimationStart).TotalMilliseconds;
            if (animationAge < 0)
                animationAge = 0;
            switch (Animation)
            {
                case Animation.None:
                    if (animationAge < 50)
                        return 19;
                    var idleFrame = ((animationAge - 50) / 100) % idleFrameLookup.Length;
                    return idleFrameLookup[idleFrame];
                case Animation.Move:
                    if (animationAge < moveFrameDelay)
                        return 19;
                    return (2 + (animationAge / moveFrameDelay)) % 8;
                case Animation.MoveBackwards:
                    if (animationAge < moveFrameDelay)
                        return 19;
                    return 7 - ((2 + (animationAge / moveFrameDelay)) % 8);
                case Animation.Jump:
                    var jumpFrame = (animationAge / 100);
                    if (jumpFrame > 2)
                        jumpFrame = 2;
                    return 8 + jumpFrame;
                case Animation.Land:
                    var landFrame = (animationAge / 75);
                    if (landFrame > 1)
                    {
                        SetAnimation(Animation.None);
                        landFrame = 1;
                    }
                    return 11 + landFrame;
                case Animation.Hit:
                    var hitFrame = (animationAge / 30);
                    if (hitFrame > 4)
                    {
                        SetAnimation(Animation.None);
                        hitFrame = 4;
                    }
                    if (hitFrame >= 3)
                        hitFrame = 5 - hitFrame;
                    return 32 + hitFrame;
                case Animation.Dead:
                    var deadFrame = (animationAge / 100);
                    if (deadFrame > 15)
                        deadFrame = 15;

                    return 32 + deadFrame;
                case Animation.Spawn:
                    var spawnFrame = (animationAge / 100);

                    if (spawnFrame >= spawnFrameLookup.Length)
                    {
                        SetAnimation(Animation.None);
                        AnimationStart = Map.Now.AddMilliseconds(-100);
                        spawnFrame = spawnFrameLookup.Length - 1;
                    }
                    return spawnFrameLookup[spawnFrame];
                default:
                    return 16;
            }
        }

        static Rectangle GetFrame(int frame)
        {
            return new Rectangle((frame % 8) * SpriteWidth, (frame / 8) * SpriteHeight, SpriteWidth, SpriteHeight);
        }

        public bool CanPlaceMaterial(Item item)
        {
            if (Map.IsTerrainLocked)
                return false;

            var toolPosition = GetToolTargetPosition();

            if (!WithinRange(toolPosition, item.Type.PlaceAndActivateRange))
                return false;

            var material = item.Type.Material;

            if (Keyboard.IsShift() && !MaterialInfo.IsSolid(material))
                return false;

            //if (Map.FindEntity(new Rectangle((int)ToolPosition.X, (int)ToolPosition.Y, 16, 16)) != null)
            //    return false;

            return true;
        }

        public void PlaceMaterial(Item item)
        {
            if (CanPlaceMaterial(item))
            {
                var isWall = Keyboard.IsShift();
                var toolPosition = GetToolTargetPosition();

                var amount = 0;
                var x = (int)toolPosition.X / Map.BlockWidth - 8;
                var y = (int)toolPosition.Y / Map.BlockHeight - 7;

                foreach (Point pixel in new BrushWalker(SelectedBrush))
                {
                    var material = isWall ? Map.GetWallMaterial(x + pixel.X, y + pixel.Y) : Map.GetMaterial(x + pixel.X, y + pixel.Y);
                    if (!MaterialInfo.IsLooseOrSolid(material) && (isWall || Map.FindEntity(new Point((x + pixel.X) * Map.BlockWidth, (y + pixel.Y) * Map.BlockHeight)) == null))
                    {
                        if (isWall)
                            Map.SetWallMaterial(x + pixel.X, y + pixel.Y, item.Type.Material);
                        else
                            Map.SetMaterial(x + pixel.X, y + pixel.Y, item.Type.Material);
                        amount++;
                        Map.AddEntity(new ParticleEmitter() { Position = new Vector2((x + pixel.X) * Map.BlockWidth + 4, (y + pixel.Y) * Map.BlockHeight + 4), Type = ParticleEffect.Dust });
                        if (amount >= item.Amount)
                            break;
                    }
                }

                if (amount > 0)
                {
                    Counters.Increment(Counter.BlocksPlaced, amount);
                    Map.AddEntity(new ParticleEmitter() { Position = GetShootingOrigin(), Type = ParticleEffect.Dust });
                    PlaySound(Sound.PlaceMaterial);
                    RemoveItem(item, amount);
                }
            }
        }

        public void Equip(Item item, int mouseOverSlotNumber)
        {
            InventoryChanged = true;

            SetSlot(item, SlotType.Equipment, mouseOverSlotNumber);

            if (Utility.Flip())
                PlaySound(Sound.Equip);
            else
                PlaySound(Sound.Equip2);
        }

        public void DoDrag(SlotType slotType, int slotNumber, int amount = 0)
        {
            var draggingItem = GetItem(SlotType.Dragging, 0);
            if (draggingItem == null)
            {
                if (slotType == SlotType.BrushPalette)
                {
                    SelectedBrush = (Brush)slotNumber;
                }
                else if (slotType == SlotType.MaterialPalette)
                {
                    Selected = slotNumber;
                }
                else if (slotType == SlotType.Recipe)
                {
                }
                else
                {
                    var startDragItem = GetItem(slotType, slotNumber);
                    if (startDragItem != null)
                    {
                        if (slotType == SlotType.Chest)
                        {
                            ActivePlaceable.RemoveItem(startDragItem);
                            Inventory.Add(startDragItem);
                            InventoryChanged = true;
                        }

                        if (amount > 0 && amount < startDragItem.Amount)
                        {
                            startDragItem.Amount -= amount;
                            startDragItem = new Item() { Type = ItemBase.Get(startDragItem.TypeId), Amount = amount };
                            Inventory.Add(startDragItem);
                        }

                        SetSlot(startDragItem, SlotType.Dragging);
                        PlaySound(Sound.SmallTick);

                        if (Keyboard.IsControl() && Player.IsPlayerSlot(slotType))
                        {
                            var targetSlotType = SlotType.ActionBar;
                            var targetSlot = -1;

                            switch (slotType)
                            {
                                case SlotType.Inventory:
                                case SlotType.Trash:
                                    targetSlotType = SlotType.ActionBar;
                                    targetSlot = FindFreeSlot(targetSlotType);
                                    break;

                                case SlotType.ActionBar:
                                case SlotType.Equipment:
                                    targetSlotType = SlotType.Inventory;
                                    targetSlot = FindFreeSlot(targetSlotType);
                                    break;
                            }

                            if (slotType != SlotType.Equipment)
                            {
                                switch (startDragItem.Type.Category)
                                {
                                    case ItemCategory.Helmet:
                                    case ItemCategory.Boots:
                                    case ItemCategory.Accessory:
                                    case ItemCategory.Suit:
                                    case ItemCategory.PowerCore:
                                        var tempSlot = GetEmptyValidEquipmentSlot(startDragItem.Type.Category);
                                        if (tempSlot >= 0)
                                        {
                                            targetSlot = tempSlot;
                                            targetSlotType = SlotType.Equipment;
                                        }
                                        break;
                                }
                            }

                            if (targetSlot >= 0)
                                DoDrop(targetSlotType, targetSlot);
                        }
                    }
                }
            }
        }

        public void DoDrop(SlotType slotType, int slotNumber)
        {
            var draggingItem = GetItem(SlotType.Dragging, 0);
            if (draggingItem != null)
            {
                // drop
                switch (slotType)
                {
                    case SlotType.Inventory:
                    case SlotType.Trash:
                    case SlotType.ActionBar:
                        var existing = GetItem(slotType, slotNumber);
                        if (existing != null)
                        {
                            if (slotType == SlotType.Trash)
                                RemoveItem(existing);
                            else
                            {
                                if (existing.TypeId == draggingItem.TypeId)
                                {
                                    existing.Amount += draggingItem.Amount;
                                    RemoveItem(draggingItem);
                                    break;
                                }
                            }
                        }

                        SetSlot(draggingItem, slotType, slotNumber);
                        break;

                    case SlotType.Chest:
                        if (ActivePlaceable != null)
                        {
                            var existingItem = ActivePlaceable.Inventory[slotNumber];
                            if (existingItem != null)
                            {
                                ActivePlaceable.RemoveItem(existingItem);
                            }

                            RemoveItem(draggingItem);
                            ActivePlaceable.AddItem(draggingItem, slotNumber);
                            draggingItem = null;

                            if (existingItem != null)
                            {
                                Inventory.Add(existingItem);
                                SetSlot(existingItem, SlotType.Dragging);
                            }
                        }
                        break;

                    case SlotType.Equipment:
                        if (!IsValidEquipmentSlot(slotNumber, draggingItem.Type.Category))
                            return;

                        Equip(draggingItem, slotNumber);
                        break;
                }

                PlaySound(Sound.SmallTick);
            }
        }

        public int GetEmptyValidEquipmentSlot(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Helmet:
                    return IsSlotEmpty(SlotType.Equipment, 0) ? 0 : -1;
                case ItemCategory.Suit:
                    return IsSlotEmpty(SlotType.Equipment, 1) ? 1 : -1;
                case ItemCategory.PowerCore:
                    return IsSlotEmpty(SlotType.Equipment, 2) ? 2 : -1;
                case ItemCategory.Boots:
                    return IsSlotEmpty(SlotType.Equipment, 3) ? 3 : -1;
                case ItemCategory.Accessory:
                    if (IsSlotEmpty(SlotType.Equipment, 4))
                        return 4;
                    if (IsSlotEmpty(SlotType.Equipment, 5))
                        return 5;
                    if (IsSlotEmpty(SlotType.Equipment, 6))
                        return 6;
                    if (IsSlotEmpty(SlotType.Equipment, 7))
                        return 7;
                    return -1;
                default:
                    return -1;
            }
        }

        public bool IsValidEquipmentSlot(int slotNumber, ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Helmet:
                    return slotNumber == 0;
                case ItemCategory.Suit:
                    return slotNumber == 1;
                case ItemCategory.PowerCore:
                    return slotNumber == 2;
                case ItemCategory.Boots:
                    return slotNumber == 3;
                case ItemCategory.Accessory:
                    return slotNumber > 3 && slotNumber < 8;
                default:
                    return false;
            }
        }

        public void DoSortInventory()
        {
            SortInventory = false;

            foreach (var item in Inventory.ToArray())
            {
                if (item.SlotType == SlotType.Inventory)
                {
                    foreach (var duplicate in from i in Inventory.ToArray() where i.SlotType == SlotType.Inventory && i.TypeId == item.TypeId && i.SlotNumber != item.SlotNumber select i)
                    {
                        item.Amount += duplicate.Amount;
                        RemoveItem(duplicate);
                    }
                }
            }

            Inventory = Inventory.OrderBy(i => i.Type.Category).ThenBy(i => i.Type.ListPriority).ThenBy(i => i.Type.Tier).ThenBy(i => i.Type.Name).ToList();
            var slot = 0;
            foreach (var item in Inventory)
            {
                if (item.SlotType == SlotType.Inventory)
                {
                    item.SlotNumber = slot;
                    slot++;
                }
            }

            UpdateSlots();

            InventoryChanged = true;
        }

        public override string ToString()
        {
            return Name;
        }

        internal void StartSound(string soundName)
        {
        }

        internal void StopSound()
        {
        }

        public void MessageToClient(string text, MessageType messageType = MessageType.System)
        {
            lock (Messages)
            {
                Messages.Insert(0, new Message() { Text = text, Type = messageType, Created = (Map == null) ? DateTime.UtcNow : Map.Now });
                if (Messages.Count > 50)
                    Messages.RemoveAt(Messages.Count - 1);
            }
        }

        public bool IsFlagged(Entity entity)
        {
            if (Flags == null)
                Flags = new List<Flag>();
            return Flags.Contains(new Flag() { EntityId = entity.Id, MapId = Map.Id });
        }

        public void SetFlag(Entity entity)
        {
            if (!IsFlagged(entity))
                Flags.Add(new Flag() { EntityId = entity.Id, MapId = Map.Id });
        }

        public Point GetScreenOffset()
        {
            if (Map == null)
                return Point.Zero;

            var result = new Point((int)Position.X, (int)Position.Y);

            result.X -= ClientResolutionWidth / 2;
            if (result.X < 0)
                result.X = 0;
            if (result.X > Map.PixelWidth - ClientResolutionWidth)
                result.X = Map.PixelWidth - ClientResolutionWidth;

            result.Y -= ClientResolutionHeight / 2;
            if (result.Y < 0)
                result.Y = 0;

            if (result.Y > Map.PixelHeight - ClientResolutionHeight)
                result.Y = Map.PixelHeight - ClientResolutionHeight;

            UpdateScreenRectangle(result, 100);

            return result;
        }

        public void UpdateScreenRectangle(Point screenOffset, int margin)
        {
            ScreenRectangle = new Rectangle((int)screenOffset.X, (int)screenOffset.Y, ClientResolutionWidth, ClientResolutionHeight);
            ScreenRectangle.Inflate(margin, margin);
        }

        /// <remarks>GetScreenOffset needs to be called before this will work.</remarks>
        public bool IsOnScreen(Entity entity)
        {
            return IsOnScreen(entity.OffsetBoundingBox);
            //var difference = Position - entity.Position;
            //return Math.Abs(difference.X) < (ClientResolutionWidth + entity.BoundingBox.Width) / 2 && Math.Abs(difference.Y) < (ClientResolutionHeight + entity.BoundingBox.Height) / 2;
        }

        public bool IsOnScreen(Rectangle offsetBoundingBox)
        {
            return ScreenRectangle.Intersects(offsetBoundingBox);
        }

        //public bool IsOnScreen(Vector2 point, int margin)
        //{
        //    var difference = Position - point;
        //    return Math.Abs(difference.X) < ClientResolutionWidth / 2 + margin && Math.Abs(difference.Y) < ClientResolutionHeight / 2 + margin;
        //}

        //public bool IsOnScreen(Vector2 position)
        //{
        //    return screenRectangle.Contains((int)position.X, (int)position.Y);
        //}

        public bool IsNearGround()
        {
            for (int x = 0; x < OffsetBoundingBox.Width; x += Map.BlockWidth)
            {
                var material = Map.GetMaterialAtPixel(OffsetBoundingBox.Left + x, OffsetBoundingBox.Bottom + Map.BlockHeight + 1);
                if (MaterialInfo.IsLooseOrSolid(material))
                    return true;

                material = Map.GetMaterialAtPixel(OffsetBoundingBox.Left + x, OffsetBoundingBox.Bottom + Map.BlockHeight + Map.BlockHeight + 1);
                if (MaterialInfo.IsLooseOrSolid(material))
                    return true;

                material = Map.GetMaterialAtPixel(OffsetBoundingBox.Left + x, OffsetBoundingBox.Bottom + Map.BlockHeight + Map.BlockHeight + Map.BlockHeight + 1);
                if (MaterialInfo.IsLooseOrSolid(material))
                    return true;
            }
            return false;
        }

        public void ReturnToOverworld()
        {
            StartPosition = Vector2.Zero;
            SendTo(World.Overworld);

            if (GetMiningTier() == 0)
                GiveItem(ItemId.MiningTool1);

            if ((from i in Inventory where i.Type.Category == ItemCategory.Blaster orderby i.Type.Tier descending select i.Type.Tier).FirstOrDefault() == 0)
                GiveItem(ItemId.Blaster1);
        }

        public bool IsFightingBoss()
        {
            return (from e in Map.Entities.OfType<Enemy>() where e.Type.IsBoss && IsOnScreen(e) select e).Count() > 0;
        }
#if WINDOWS

        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as PlayerUpdate;
            if (result == null)
                result = new PlayerUpdate() { Name = Name };
            else
                result.Name = null; // don't keep sending name other than the first time.

            var dirty = false;

            result.Id = Id;

            if (SoundEvents.Count > 0)
            {
                result.SoundEvents = SoundEvents.ToArray();
                dirty = true;
            }
            else
                result.SoundEvents = null;

            if (result.X != (int)Position.X || result.Y != (int)Position.Y)
            {
                result.X = (int)Position.X;
                result.Y = (int)Position.Y;
                dirty = true;
            }

            if (result.Health != Health)
            {
                result.Health = Health;
                dirty = true;
            }

            if (result.MaxHealth != MaxHealth)
            {
                result.MaxHealth = MaxHealth;
                dirty = true;
            }

            if (result.CurrentAnimation != Animation)
            {
                result.CurrentAnimation = Animation;
                //Console.WriteLine("Sending animation " + CurrentAnimation);
                dirty = true;
            }

            if (result.IsFacingLeft != IsFacingLeft)
            {
                result.IsFacingLeft = IsFacingLeft;
                dirty = true;
            }

            if (result.ActivePlaceableId != (ActivePlaceable == null ? 0 : ActivePlaceable.Id))
            {
                result.ActivePlaceableId = ActivePlaceable == null ? 0 : ActivePlaceable.Id;
                dirty = true;
            }

            if (result.CreativeMode != CreativeMode)
            {
                result.CreativeMode = CreativeMode;
                dirty = true;
            }

            if (result.SelectedBrush != SelectedBrush)
            {
                result.SelectedBrush = SelectedBrush;
                dirty = true;
            }

            if (result.Selected != Selected)
            {
                result.Selected = Selected;
                dirty = true;
            }

            if (result.CopySelectionMode != copySelectionMode)
            {
                result.CopySelectionMode = copySelectionMode;
                dirty = true;
            }

            if (result.CopySelectionMode != CopySelectionMode.None)
            {
                if (result.CopySelectionStartX != (int)CopySelectionStart.X || result.CopySelectionStartY != (int)CopySelectionStart.Y)
                {
                    result.CopySelectionStartX = (int)CopySelectionStart.X;
                    result.CopySelectionStartY = (int)CopySelectionStart.Y;
                    dirty = true;
                }
                if (result.CopySelectionEndX != (int)CopySelectionEnd.X || result.CopySelectionEndY != (int)CopySelectionEnd.Y)
                {
                    result.CopySelectionEndX = (int)CopySelectionEnd.X;
                    result.CopySelectionEndY = (int)CopySelectionEnd.Y;
                    dirty = true;
                }
            }
            else
            {
                result.CopySelectionStartX = 0;
                result.CopySelectionStartY = 0;
                result.CopySelectionEndX = 0;
                result.CopySelectionEndY = 0;
            }

            // only send this to main player.
            if (clipboardUpdated && this == targetPlayer)
            {
                if (clipboardMap == null)
                    result.ClearClipboardMap = true;
                else
                    result.ClipboardMap = clipboardMap;
                clipboardUpdated = false;
                dirty = true;
            }
            else
            {
                result.ClearClipboardMap = false;
                result.ClipboardMap = null;
            }

            if (Messages.Count > 0 && this == targetPlayer)
            {
                result.Messages = Messages.ToArray();
                Messages.Clear();
                dirty = true;
            }
            else
                result.Messages = null;

            if (result.UsedAirSupply != UsedAirSupply)
            {
                result.UsedAirSupply = UsedAirSupply;
                dirty = true;
            }

            if (result.IsImmersed != IsImmersed)
            {
                result.IsImmersed = IsImmersed;
                dirty = true;
            }

            if (result.IsGameCompleted != IsGameCompleted)
            {
                result.IsGameCompleted = IsGameCompleted;
                dirty = true;
            }

            if (result.PlayingSound != PlayingSound)
            {
                result.PlayingSound = PlayingSound;
                dirty = true;
            }

            if (result.Skin != Skin)
            {
                result.Skin = Skin;
                dirty = true;
            }

            if (result.CastLight != CastLight.PackedValue)
            {
                result.CastLight = CastLight.PackedValue;
                dirty = true;
            }

            var usingItemId = UsingItem == null ? ItemId.None : UsingItem.TypeId;
            if (result.UsingItemId != usingItemId)
            {
                result.UsingItemId = usingItemId;
                dirty = true;
            }

            if (result.Lives != Lives)
            {
                result.Lives = Lives;
                dirty = true;
            }

            return dirty ? result : null;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (PlayerUpdate)entityUpdate;
            if (update.Name != null)
                Name = update.Name;
            Position = new Vector2(update.X, update.Y);
            Health = update.Health;
            MaxHealth = update.MaxHealth;
            Skin = update.Skin;
            if (Animation != update.CurrentAnimation)
            {
                Animation = update.CurrentAnimation;
                //Console.WriteLine("Received animation " + CurrentAnimation);
                AnimationStart = Map.Now;
            }
            IsFacingLeft = update.IsFacingLeft;
            CastLight = new Color() { PackedValue = update.CastLight };

            if (update.ActivePlaceableId == 0)
                ActivePlaceable = null;
            else
                ActivePlaceable = Map.FindEntity(update.ActivePlaceableId) as Placeable;
            CreativeMode = update.CreativeMode;
            SelectedBrush = update.SelectedBrush;
            Selected = update.Selected;

            copySelectionMode = update.CopySelectionMode;
            CopySelectionStart = new Vector2(update.CopySelectionStartX, update.CopySelectionStartY);
            CopySelectionEnd = new Vector2(update.CopySelectionEndX, update.CopySelectionEndY);

            UsedAirSupply = update.UsedAirSupply;
            IsImmersed = update.IsImmersed;
            IsGameCompleted = update.IsGameCompleted;
            Lives = update.Lives;

            if (update.ClipboardMap != null)
                clipboardMap = update.ClipboardMap;
            if (update.ClearClipboardMap)
                clipboardMap = null;

            PlayingSound = update.PlayingSound;

            if ((UsingItem == null && update.UsingItemId != ItemId.None) || (UsingItem != null && update.UsingItemId != UsingItem.TypeId))
            {
                UsingItem = GetItem(update.UsingItemId);
                if (UsingItem != null)
                {
                    UsingItem.Player = this;
                    UsingItem.StartUse = Map.Now;
                }
            }

            if (update.Messages != null)
            {
                foreach (var message in update.Messages)
                {
                    message.Created = Map.Now;
                    Messages.Insert(0, message);
                    if (Messages.Count > 50)
                        Messages.RemoveAt(Messages.Count - 1);
                }
            }
        }
#endif

        public void GiveAchievement(Achievement achievement)
        {
            if (achievement != Achievement.None && !World.IsServer)
            {
#if STEAM
                Utility.LogMessage("Giving achievement " + achievement);

                //try
                //{
                    if (SteamUserStats.SetAchievement(achievement.ToString()))
                    {
                        var message = "CONGRATULATIONS! You earned the " + Utility.AddSpaces(achievement.ToString()) + " achievement!";
                        Utility.LogMessage(message);
                        MessageToClient(message, MessageType.System);
                    }
                    SteamUserStats.StoreStats();
                //}
                //catch (InvalidOperationException e)
                //{
                //    Utility.LogMessage(e.ToString());
                //}
#endif
            }
        }
    }

#if WINDOWS
    [DataContract]
    public class PlayerUpdate : EntityUpdate
    {
        public override Type TargetType { get { return typeof(Player); } }

        [DataMember(Order = 1)]
        public int Health { get; set; }

        [DataMember(Order = 2)]
        public int MaxHealth { get; set; }

        [DataMember(Order = 3)]
        public Animation CurrentAnimation { get; set; }

        [DataMember(Order = 4)]
        public bool IsFacingLeft { get; set; }

        [DataMember(Order = 5)]
        public int ActivePlaceableId { get; set; }

        [DataMember(Order = 6)]
        public byte SelectedActionSlot { get; set; }

        [DataMember(Order = 7)]
        public CreativeMode CreativeMode { get; set; }

        [DataMember(Order = 8)]
        public int Selected { get; set; }

        [DataMember(Order = 9)]
        public Brush SelectedBrush { get; set; }

        [DataMember(Order = 10)]
        public string Name { get; set; }

        [DataMember(Order = 11)]
        public Message[] Messages { get; set; }

        [DataMember(Order = 12)]
        public double UsedAirSupply { get; set; }

        [DataMember(Order = 13)]
        public bool IsImmersed { get; set; }

        [DataMember(Order = 14)]
        public bool IsGameCompleted { get; set; }

        [DataMember(Order = 15)]
        public Sound PlayingSound { get; set; }

        [DataMember(Order = 16)]
        public int Skin { get; set; }

        [DataMember(Order = 17)]
        public ItemId UsingItemId { get; set; }

        [DataMember(Order = 18)]
        public uint CastLight { get; set; }

        [DataMember(Order = 19)]
        public int Lives { get; set; }

        [DataMember(Order = 100)]
        public CopySelectionMode CopySelectionMode { get; set; }

        [DataMember(Order = 101)]
        public int CopySelectionStartX { get; set; }
        [DataMember(Order = 102)]
        public int CopySelectionStartY { get; set; }

        [DataMember(Order = 103)]
        public int CopySelectionEndX { get; set; }
        [DataMember(Order = 104)]
        public int CopySelectionEndY { get; set; }

        [DataMember(Order = 105)]
        public Map ClipboardMap { get; set; }
        [DataMember(Order = 106)]
        public bool ClearClipboardMap { get; set; }
    }
#endif
}