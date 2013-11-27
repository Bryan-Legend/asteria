using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    public class MapGenerateCommand : AdminCommand, IMapGenerator
    {
        public override string Description
        {
            get { return "/MapGenerate: Wipes and generates an overworld for the current map."; }
        }

        public override string Execute(Player player, string args)
        {
            Generate(player.Map);

            return "Map generated.";
        }

        const int VerticalScale = 200 / Map.BlockHeight;
        Map map;

        public void Generate(Map map)
        {
            this.map = map;

            map.Entities.Clear();
            map.Spawns.Clear();

            map.IsAutospawn = true;
            map.Music = "Overworld";
            map.SeaLevel = map.Height / 5;

            Utility.StatusMessage = "Let there be space.";
            map.Materials = new byte[map.Width * map.Height];

            Utility.StatusMessage = "Let there be air.";
            map.Fill(Material.Air, new Rectangle(0, 0, map.Width, map.SeaLevel));

            Utility.StatusMessage = "Let there be dirt.";
            var dirtLevel = map.SeaLevel;
            var clayHeight = dirtLevel + 200;
            var sandstoneHeight = map.GetTierAltitude(2) / Map.BlockHeight;
            var sectionHeight = (map.Height - sandstoneHeight) / 7;
            var limestoneHeight = map.GetTierAltitude(3) / Map.BlockHeight;
            var quartziteHeight = map.GetTierAltitude(4) / Map.BlockHeight;
            var graniteHeight = map.GetTierAltitude(5) / Map.BlockHeight;
            var marbleHeight = map.GetTierAltitude(6) / Map.BlockHeight;
            var rhyoliteHeight = map.GetTierAltitude(7) / Map.BlockHeight;
            var basaltHeight = map.GetTierAltitude(8) / Map.BlockHeight;

            map.Fill(Material.Dirt, new Rectangle(0, dirtLevel, map.Width, 200));

            Utility.StatusMessage = "Let there be clay.";
            map.Fill(Material.Clay, new Rectangle(0, clayHeight, map.Width, 300));
            Utility.StatusMessage = "Let there be sandstone.";
            map.Fill(Material.Sandstone, new Rectangle(0, sandstoneHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be limestone.";
            map.Fill(Material.Limestone, new Rectangle(0, limestoneHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be granite.";
            map.Fill(Material.Quartzite, new Rectangle(0, quartziteHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be quartzite.";
            map.Fill(Material.Granite, new Rectangle(0, graniteHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be marble.";
            map.Fill(Material.Marble, new Rectangle(0, marbleHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be rhyolite.";
            map.Fill(Material.Rhyolite, new Rectangle(0, rhyoliteHeight, map.Width, sectionHeight));
            Utility.StatusMessage = "Let there be basalt.";
            map.Fill(Material.Basalt, new Rectangle(0, basaltHeight, map.Width, sectionHeight));

            Utility.StatusMessage = "Let there be rough terrain.";
            map.CreateHills(Material.Clay, Material.Dirt, clayHeight, 3 * VerticalScale);
            map.CreateHills(Material.Sandstone, Material.Clay, sandstoneHeight, 3 * VerticalScale);
            map.CreateHills(Material.Limestone, Material.Sandstone, limestoneHeight, 6 * VerticalScale);
            map.CreateHills(Material.Quartzite, Material.Limestone, quartziteHeight, 6 * VerticalScale);
            map.CreateHills(Material.Granite, Material.Quartzite, graniteHeight, 6 * VerticalScale);
            map.CreateHills(Material.Marble, Material.Granite, marbleHeight, 6 * VerticalScale);
            map.CreateHills(Material.Rhyolite, Material.Marble, rhyoliteHeight, 3 * VerticalScale);

            Utility.StatusMessage = "Let there be hills and lakes.";
            map.CreateHills(Material.Dirt, Material.Water, dirtLevel, 200, -200, 200, -200);

            map.SprinkleVeins(Material.Clay, dirtLevel - 500, clayHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Sandstone, clayHeight, sandstoneHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Limestone, sandstoneHeight, limestoneHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Quartzite, limestoneHeight, quartziteHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Granite, quartziteHeight, graniteHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Marble, graniteHeight, marbleHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Rhyolite, marbleHeight, rhyoliteHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Basalt, rhyoliteHeight, basaltHeight, 10000, 50, Brush.Size7);
            map.SprinkleVeins(Material.Lava, basaltHeight, map.Height, 10000, 50, Brush.Size7);

            Utility.StatusMessage = "Let there be lava.";
            var topLavaHeight = basaltHeight - 5 * VerticalScale;
            map.LavaLevel = topLavaHeight;
            map.Fill(Material.Lava, new Rectangle(0, topLavaHeight, map.Width, 5 * VerticalScale));
            var bottomLavaHeight = map.Height - 5 * VerticalScale;
            map.Fill(Material.Lava, new Rectangle(0, bottomLavaHeight, map.Width, 5 * VerticalScale));
            map.CreateHills(Material.Lava, Material.Rhyolite, topLavaHeight, 4 * VerticalScale);
            map.CreateHills(Material.Basalt, Material.Lava, basaltHeight, 4 * VerticalScale);
            map.CreateHills(Material.Lava, Material.Basalt, bottomLavaHeight, 6 * VerticalScale);

            map.SprinkleVeins(Material.Water, clayHeight, rhyoliteHeight, 250000, 100, Brush.Size9);
            map.SprinkleVeins(Material.Gravel, map.SeaLevel + 100, rhyoliteHeight, 250000, 50, Brush.Size9);
            map.SprinkleVeins(Material.Lava, marbleHeight, map.Height, 150000, 100, Brush.Size9);

            map.SprinkleVeins(Material.PoisonGas, marbleHeight, map.Height, 150000, 300, Brush.Size9);
            map.SprinkleVeins(Material.Oil, marbleHeight, map.Height, 150000, 150, Brush.Size9);

            Utility.StatusMessage = "Let there be floating islands.";
            map.SprinkleVeins(Material.Dirt, 100, sectionHeight, 125000, 350, Brush.Size10,
                (position) => {
                    if (!Utility.Roll8())
                        AddRandomSpawns(position, true);

                    if (Utility.Roll4()) {
                        var chestPosition = map.FindGround(new Vector2(position.X, 0)) + new Vector2(0, -50);
                        var chest = map.AddPlaceable(null, ItemBase.Get(ItemId.WorldHypercube), chestPosition);

                        var tier = 3;

                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                    }
                },
                false
            );

            Utility.StatusMessage = "Let there be caves.";
            map.SprinkleVeins(Material.Air, map.SeaLevel - 300, map.Height, 50000, 450, Brush.Size9,
                (position) => {
                    if (Utility.Flip())
                        map.AddPlaceable(null, ItemBase.Get(ItemId.LightOrb), position);

                    if (!Utility.Roll8())
                        AddRandomSpawns(position);

                    if (Utility.Roll4()) {
                        var chest = map.AddPlaceable(null, ItemBase.Get(ItemId.WorldHypercube), position);

                        var tier = map.GetTier(position);

                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                        chest.AddItem(GenerateRandomItem(tier));
                    }
                }
            );

            map.StartingPosition = new Vector2(map.PixelWidth / 2, (sectionHeight * Map.BlockHeight) + 1200); // add 1200 here so that spawn won't catch an island right in the center as it descends down.

            map.StartingPosition = map.FindGround(map.StartingPosition);
            map.StartingPosition = new Vector2(map.StartingPosition.X, map.StartingPosition.Y - 200f);
            AddPortal(map.StartingPosition, Material.Bone).Remove();

            var portal = AddPortal(new Vector2(map.StartingPosition.X, map.SeaLevelInPixels), Material.Bone);
            portal.Name = "Dungeon Tier 2";
            portal.Value = "KrawnixLair";
            map.AddEntity(new Placeable() { TypeId = ItemId.Workbench, Position = portal.Position + new Vector2(-80, 50) });

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(3) - Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 3";
            portal.Value = "Botanica";

            var skyPortalIslandPosition = new Vector2(Utility.Next(1000, map.PixelWidth - 1000), Utility.Next(500, sectionHeight * Map.BlockHeight - 100));
            map.CreateVein(Material.Dirt, skyPortalIslandPosition, 350, Brush.Size10, false);
            skyPortalIslandPosition = map.FindGround(skyPortalIslandPosition);
            portal = AddPortal(skyPortalIslandPosition + new Vector2(0, -50), Material.Bone);
            portal.Name = "Dungeon Tier 4";
            portal.Value = "SkyRealm";

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(5) - Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 5";
            portal.Value = "Oberon";

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(6) - Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 6";
            portal.Value = "CaveDive";

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(7) - Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 7";
            portal.Value = "Burrower";

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(8) - Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 8";
            portal.Value = "Vesuvius";

            portal = AddPortal(new Vector2(Utility.Next(1000, map.PixelWidth - 1000), map.GetTierAltitude(8) + Utility.Next(300, sectionHeight * Map.BlockHeight - 300)), Material.Bone);
            portal.Name = "Dungeon Tier 9";
            portal.Value = "FinalBoss";

            Utility.StatusMessage = "Let there be grass, snow and thin strips of terrain to hold up the oceans.";
            for (var y = 0; y < map.Height; y++) {
                var levelMaterial = Material.Dirt;
                if (y > clayHeight)
                    levelMaterial = Material.Clay;
                if (y > sandstoneHeight)
                    levelMaterial = Material.Sandstone;
                if (y > limestoneHeight)
                    levelMaterial = Material.Limestone;
                if (y > quartziteHeight)
                    levelMaterial = Material.Quartzite;
                if (y > graniteHeight)
                    levelMaterial = Material.Granite;
                if (y > marbleHeight)
                    levelMaterial = Material.Marble;
                if (y > rhyoliteHeight)
                    levelMaterial = Material.Rhyolite;
                if (y > map.LavaLevel + 25)
                    levelMaterial = Material.Basalt;

                for (var x = 0; x < map.Width; x++) {
                    var material = map.GetMaterial(x, y);
                    if (MaterialInfo.IsLiquid(material)) {
                        if (MaterialInfo.IsGas(map.GetMaterial(x, y + 1)))
                            map.SetMaterial(x, y + 1, levelMaterial);
                        if (MaterialInfo.IsGas(map.GetMaterial(x - 1, y)))
                            map.SetMaterial(x - 1, y, levelMaterial);
                        if (MaterialInfo.IsGas(map.GetMaterial(x + 1, y)))
                            map.SetMaterial(x + 1, y, levelMaterial);
                    } else {
                        // this is my horrible attempt at erosion.
                        //var adjacent = IsAdjacent(x, y, Material.Air);
                        //if (adjacent != Direction.Up && adjacent != Direction.Left && adjacent != Direction.None)
                        //{
                        //    SetMaterial(x, y, Material.Air);
                        //    continue;
                        //}

                        switch (material) {
                            case Material.Dirt:
                                if (map.GetMaterial(x, y - 1) == Material.Air) {
                                    if (y < map.SeaLevel + 15) {
                                        map.SetMaterial(x, y, Material.Grass);
                                        if (y < map.SeaLevel - 50) {
                                            map.SetMaterial(x, y - 1, Material.Snow);
                                            if (Utility.Flip())
                                                map.SetMaterial(x, y - 2, Material.Snow);
                                        }
                                    }
                                }
                                //else
                                //{
                                //    if (GetMaterial(x, y - 1) == Material.Water)
                                //        SetMaterial(x, y, Material.Mud);
                                //}
                                break;
                        }
                    }
                }
            }

            Utility.StatusMessage = "Let there be sandy beaches.";
            const int beachWidth = 600;
            const int beachHeight = 10;
            for (int i = 0; i < beachWidth; i++) {
                var ground = map.FindGround(new Vector2(i * Map.BlockWidth, map.SeaLevelInPixels - sectionHeight / 2 * Map.BlockHeight));
                for (int y = 0; y < beachHeight; y++) {
                    map.SetMaterialAtPixel(ground + new Vector2(0, -Map.BlockHeight * y), Material.Sand);
                }
            }
            for (int i = map.Width - 420; i < map.Width; i++) {
                var ground = map.FindGround(new Vector2(i * Map.BlockWidth, map.SeaLevelInPixels - sectionHeight / 2 * Map.BlockHeight));
                for (int y = 0; y < beachHeight; y++) {
                    map.SetMaterialAtPixel(ground + new Vector2(0, -Map.BlockHeight * y), Material.Sand);
                }
            }

            map.WallMaterials = (byte[])map.Materials.Clone();
            for (int i = 0; i < map.WallMaterials.Length; i++) {
                if (map.WallMaterials[i] == (byte)Material.Water)
                    map.WallMaterials[i] = (byte)Material.Dirt;
            }

            Utility.StatusMessage = "Let there be coal.";
            map.SprinkleVeins(Material.Coal, map.SeaLevel - 300, map.Height, 50000, 50, Brush.Size4);
            Utility.StatusMessage = "Let there be iron.";
            map.SprinkleVeins(Material.IronOre, map.SeaLevel - 300, map.SeaLevel + sectionHeight * 5, 10000, 100);
            Utility.StatusMessage = "Let there be aluminum.";
            map.SprinkleVeins(Material.AluminumOre, sandstoneHeight, sandstoneHeight + sectionHeight * 5, 15000, 100);
            Utility.StatusMessage = "Let there be copper.";
            map.SprinkleVeins(Material.CopperOre, sandstoneHeight, sandstoneHeight + sectionHeight * 2, 15000, 100);
            Utility.StatusMessage = "Let there be silver.";
            map.SprinkleVeins(Material.SilverOre, limestoneHeight, limestoneHeight + sectionHeight * 2, 15000, 100);
            Utility.StatusMessage = "Let there be gold.";
            map.SprinkleVeins(Material.GoldOre, quartziteHeight, graniteHeight + sectionHeight * 2, 15000, 100);
            Utility.StatusMessage = "Let there be dilithium.";
            map.SprinkleVeins(Material.Dilithium, graniteHeight, quartziteHeight + sectionHeight * 2, 15000, 40);
            Utility.StatusMessage = "Let there be radium.";
            map.SprinkleVeins(Material.RadiumOre, marbleHeight, marbleHeight + sectionHeight * 2, 15000, 40);
            Utility.StatusMessage = "Let there be uranium.";
            map.SprinkleVeins(Material.UraniumOre, basaltHeight, map.Height, 10000, 20);

            Utility.StatusMessage = "Let there be sodium.";
            map.SprinkleVeins(Material.Sodium, map.SeaLevel - 300, map.SeaLevel + sectionHeight * 4, 20000, 30);

            Utility.StatusMessage = "Let there be sulpher.";
            map.SprinkleVeins(Material.Sulfur, clayHeight, clayHeight + sectionHeight * 3, 25000, 20);
            Utility.StatusMessage = "Let there be topaz.";
            map.SprinkleVeins(Material.Topaz, sandstoneHeight, sandstoneHeight + sectionHeight, 25000, 20);
            Utility.StatusMessage = "Let there be amethyst.";
            map.SprinkleVeins(Material.Amethyst, limestoneHeight, limestoneHeight + sectionHeight, 25000, 20);
            Utility.StatusMessage = "Let there be emerald.";
            map.SprinkleVeins(Material.Emerald, quartziteHeight, quartziteHeight + sectionHeight, 25000, 20);
            Utility.StatusMessage = "Let there be sapphire.";
            map.SprinkleVeins(Material.Sapphire, graniteHeight, graniteHeight + sectionHeight, 25000, 20);
            Utility.StatusMessage = "Let there be rubies.";
            map.SprinkleVeins(Material.Ruby, marbleHeight, marbleHeight + sectionHeight, 25000, 20);
            Utility.StatusMessage = "Let there be diamonds.";
            map.SprinkleVeins(Material.Diamond, rhyoliteHeight, rhyoliteHeight + sectionHeight * 2, 25000, 20);

            foreach (var player in map.Players) {
                map.Entities.Add(player);
                player.Respawn();
            }

            // so that entities actually get saved
            map.FlushEntities();

            Utility.StatusMessage = "";
        }


        void AddRandomSpawns(Vector2 position, bool flyingOnly = false)
        {
            var tier = map.GetTier(position);

            if (flyingOnly)
                tier = Math.Min(3, tier);

            var possibleEnemyTypes = new List<EnemyType>();
            foreach (var enemyType in EnemyBase.Types) {
                if (enemyType.MinSpawnTier <= tier && enemyType.MaxSpawnTier >= tier && enemyType.IsAutoSpawn && !enemyType.IsSwimming && !enemyType.IsBoss) {
                    if (flyingOnly && !enemyType.IsFlying)
                        continue;
                    possibleEnemyTypes.Add(enemyType);
                }
            }

            if (possibleEnemyTypes.Count > 0) {
                var spawnEnemyType = possibleEnemyTypes[Utility.Next(possibleEnemyTypes.Count)];

                AddRandomSpawn(position, spawnEnemyType);
                AddRandomSpawn(position, spawnEnemyType);
                AddRandomSpawn(position, spawnEnemyType);
                AddRandomSpawn(position, spawnEnemyType);
                AddRandomSpawn(position, spawnEnemyType);
            }
        }

        void AddRandomSpawn(Vector2 position, EnemyType spawnEnemyType)
        {
            var spawnDistance = Utility.Next(400);
            var spawnAngle = Utility.RandomAngle();
            var spawnPoint = position.Offset((float)spawnDistance, (float)spawnAngle);
            var material = map.GetMaterialAtPixel(spawnPoint);

            if (material == Material.Air) {
                if (!spawnEnemyType.IsFlying) {
                    spawnPoint = map.FindGround(spawnPoint);
                    spawnPoint.Y -= spawnEnemyType.SpriteHeight * 2;
                }

                if (spawnEnemyType.SpawnAtCeiling) {
                    spawnPoint = map.FindCeiling(spawnPoint);
                    spawnPoint.Y += spawnEnemyType.SpriteHeight / 2;
                }

                map.AddSpawn(new Spawn() { Position = spawnPoint, Type = spawnEnemyType, });
            }
        }

        Placeable AddPortal(Vector2 position, Material floorMaterial)
        {
            const int blockSize = 40;
            map.Fill(Material.Air, new Rectangle((int)position.X / Map.BlockWidth - blockSize / 2, (int)position.Y / Map.BlockHeight - blockSize / 2, blockSize, blockSize));

            var portalType = ItemBase.Get(ItemId.Portal);
            var portalFloorPosition = position + new Vector2(portalType.BoundingBox.Left, portalType.BoundingBox.Bottom + Map.BlockHeight);
            for (int count = 0; count <= (portalType.BoundingBox.Width / Map.BlockWidth) + 1; count++) {
                map.SetMaterialAtPixel(portalFloorPosition, floorMaterial);
                portalFloorPosition.X += Map.BlockWidth;
            }

            return map.AddPlaceable(null, portalType, position);
        }

        Item[] commonItems = {
                                 new Item() { TypeId = ItemId.LightOrb, Amount = 10 },
                                 new Item() { TypeId = ItemId.LightOrb, Amount = 10 },
                                 new Item() { TypeId = ItemId.LightOrb, Amount = 10 },
                                 new Item() { TypeId = ItemId.RedLightOrb, Amount = 10 },
                                 new Item() { TypeId = ItemId.GreenLightOrb, Amount = 10 },
                                 new Item() { TypeId = ItemId.BlueLightOrb, Amount = 10 },
                                 
                                 new Item() { TypeId = ItemId.Energy, Amount = 5 },
                                 new Item() { TypeId = ItemId.Entropy, Amount = 1 },

                                 new Item() { TypeId = ItemId.EnergyCell, Amount = 10 },
                                 new Item() { TypeId = ItemId.MinorEnergyCell, Amount = 10 },
                                 new Item() { TypeId = ItemId.LesserEnergyCell, Amount = 10 },
                                 new Item() { TypeId = ItemId.GreaterEnergyCell, Amount = 10 },
                                 new Item() { TypeId = ItemId.MajorEnergyCell, Amount = 10 },
                                 new Item() { TypeId = ItemId.SuperEnergyCell, Amount = 10 },

                                 new Item() { TypeId = ItemId.RiftCreator, Amount = 5 },
                                 new Item() { TypeId = ItemId.MiningDrone, Amount = 5 },
                                 new Item() { TypeId = ItemId.LightDrone, Amount = 1 },

                                 new Item() { TypeId = ItemId.RifleBarrel, Amount = 1 },
                                 new Item() { TypeId = ItemId.PhotovoltaicCell, Amount = 1 },
                                 new Item() { TypeId = ItemId.EchoCrystal, Amount = 1 },

                                 new Item() { TypeId = ItemId.Door, Amount = 1 },
                                 new Item() { TypeId = ItemId.Hypercube, Amount = 1 },
                                 new Item() { TypeId = ItemId.PersonalHypercube, Amount = 1 },
                                 new Item() { TypeId = ItemId.Dynamite, Amount = 5 },
                             };

        Item[] rareItems = {
                                new Item() { TypeId = ItemId.Detector, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker2, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker3, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker4, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker5, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker6, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker7, Amount = 3 },
                                new Item() { TypeId = ItemId.PortalSeeker8, Amount = 3 },

                                new Item() { TypeId = ItemId.FullEnergyCell, Amount = 10 },
                                new Item() { TypeId = ItemId.HalfEnergyCell, Amount = 10 },

                                new Item() { TypeId = ItemId.EnergyShield1, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyShield3, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyShield5, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyBooster1, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyBooster5, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyBooster7, Amount = 1 },

                                new Item() { TypeId = ItemId.EnergyRegenerator1, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyRegenerator3, Amount = 1 },
                                new Item() { TypeId = ItemId.EnergyRegenerator5, Amount = 1 },

                                new Item() { TypeId = ItemId.AirTank, Amount = 1 },
                                new Item() { TypeId = ItemId.LeapTreads, Amount = 1 },
                                new Item() { TypeId = ItemId.HeatSink, Amount = 1 },
                                new Item() { TypeId = ItemId.HeadLamp, Amount = 1 },
                                new Item() { TypeId = ItemId.SpeedBoots, Amount = 1 },

                                new Item() { TypeId = ItemId.Magnet, Amount = 1 },
                                new Item() { TypeId = ItemId.Battery, Amount = 1 },

                                new Item() { TypeId = ItemId.PortableWaypoint, Amount = 1},

                                new Item() { TypeId = ItemId.NuclearBomb, Amount = 1 },
                           };

        Item GenerateRandomItem(int tier)
        {
            switch (Utility.Next(8)) {
                case 0:
                case 1:
                    return null;

                case 2:
                case 3: // material
                    var materialList = from m in MaterialInfo.MaterialTypes where m.Hardness == tier && m.IsVisible == true && MaterialInfo.IsLooseOrSolid(m.Id) select m;
                    var material = materialList.Skip(Utility.Next(materialList.Count())).FirstOrDefault();

                    switch (material.Rarity) {
                        case Rarity.Uncommon:
                            return new Item() { Type = ItemBase.TypesByMaterial[material.Id], Amount = Utility.Next(20) + 1 };
                        case Rarity.Epic:
                            return new Item() { Type = ItemBase.TypesByMaterial[material.Id], Amount = Utility.Next(10) + 1 };
                        default:
                            return new Item() { Type = ItemBase.TypesByMaterial[material.Id], Amount = Utility.Next(20) + 1 };
                    }

                case 4: // rare
                    var rareList = from c in rareItems where c.Type.Tier == 0 || c.Type.Tier == tier select c;
                    var rare = rareList.Skip(Utility.Next(rareList.Count())).FirstOrDefault();
                    if (rare != null)
                        return new Item() { Type = rare.Type, Amount = Utility.Next(rare.Amount) + 1 };
                    break;

                default: // common
                    var commonList = from c in commonItems where c.Type.Tier == 0 || c.Type.Tier == tier select c;
                    var common = commonList.Skip(Utility.Next(commonList.Count())).FirstOrDefault();
                    if (common != null)
                        return new Item() { Type = common.Type, Amount = Utility.Next(common.Amount) + 1 };
                    break;
            }

            return null;
        }
    }
}