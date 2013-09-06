using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace HD.Asteria
{
    public static class Items
    {
        public static void RegisterItems()
        {
            AddMiningTools();
            AddWeapons();
            AddUseables();
            AddEquipment();
            AddPlaceables();
            AddComponents();
        }

        static void AddUseables()
        {
            // useables
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Energy,
                Category = ItemCategory.Useable,
                Light = Color.FromNonPremultiplied(53, 246, 243, 100),
                Description = "Heals you for 5% of your energy.",
                IsConsumed = true,
                IsUsedImmediately = true,
                CoolDown = 0,
                OnUse = (item) => { item.Player.Heal(item.Player.MaxHealth / 20); }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Entropy,
                Category = ItemCategory.Useable,
                Light = Color.FromNonPremultiplied(87, 58, 101, 100),
                Description = "Drains you of 5% of your energy.",
                IsConsumed = true,
                IsUsedImmediately = true,
                CoolDown = 0,
                OnUse = (item) => { item.Player.Damage(item.Player.MaxHealth / 20); }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.ExtraLife,
                Name = "1UP",
                Category = ItemCategory.Useable,
                Light = Color.FromNonPremultiplied(53, 246, 243, 100),
                Description = "Gives you an extra life.",
                IsConsumed = true,
                IsUsedImmediately = true,
                CoolDown = 0,
                OnUse = (item) => {
                    if (item.Player.Map.ExtraLives > 0) {
                        item.Player.Lives++;
                        item.Player.AddCombatText("-= 1UP =-", CombatTextType.Heal);
                    }
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MinorEnergyCell,
                Category = ItemCategory.Useable,
                Description = "Heals you for 40 energy.",
                NameSource = "Energy Cell",
                IsConsumed = true,
                Tier = 1,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Ectoplasm, 10), } },
                OnUse = (item) => { item.Player.Heal(40); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.LesserEnergyCell,
                Category = ItemCategory.Useable,
                Description = "Heals you for 70 energy.",
                NameSource = "Aluminun Energy Cell",
                IsConsumed = true,
                Tier = 2,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(70); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.LesserEnergyCell }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MinorEnergyCell, 5), } });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.EnergyCell,
                Category = ItemCategory.Useable,
                NameSource = "{TierType} Energy Cell",
                IsConsumed = true,
                Tier = 3,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.CopperOre, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(105); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EnergyCell }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.LesserEnergyCell, 5), } });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.GreaterEnergyCell,
                Category = ItemCategory.Useable,
                Description = "Heals you for 190 energy.",
                NameSource = "{TierType} Energy Cell",
                IsConsumed = true,
                Tier = 4,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.SilverOre, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(190); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.GreaterEnergyCell }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EnergyCell, 5), } });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MajorEnergyCell,
                Category = ItemCategory.Useable,
                Tier = 5,
                ListPriority = 5,
                Description = "Heals you for 400 energy.",
                NameSource = "{TierType} Energy Cell",
                IsConsumed = true,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.GoldOre, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(400); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.MajorEnergyCell }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.GreaterEnergyCell, 5), } });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.SuperEnergyCell,
                Category = ItemCategory.Useable,
                NameSource = "{TierType} Energy Cell",
                Description = "Heals you for 800 energy.",
                IsConsumed = true,
                Tier = 6,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.RadiumOre, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(800); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.SuperEnergyCell }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MajorEnergyCell, 5), } });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.HalfEnergyCell,
                Category = ItemCategory.Useable,
                NameSource = "{TierType} Energy Cell",
                Description = "Heals you for 50% of your max energy.",
                IsConsumed = true,
                Tier = 7,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.UraniumOre, 3), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(item.Player.MaxHealth / 2); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.FullEnergyCell,
                Category = ItemCategory.Useable,
                Description = "Heals you to full energy.",
                NameSource = "{TierType} Energy Cell",
                IsConsumed = true,
                Tier = 8,
                ListPriority = 5,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.Uranium, 1), new Component(ItemId.Aluminum, 1), }, CreateAmount = 5 },
                OnUse = (item) => { item.Player.Heal(item.Player.MaxHealth); },
                CanUse = (player, alreadyUsing) => {
                    if (player.Health != player.MaxHealth)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your health is full.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.LightDrone,
                Category = ItemCategory.Useable,
                Description = "Creates a mechanical firefly that provides light.",
                IsConsumed = true,
                Tier = 1,
                ListPriority = 8,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.Iron, 1) } },
                OnUse = (item) => {
                    var fireFlyType = EnemyBase.Get("Light Drone");
                    item.Player.Map.AddEnemy(fireFlyType, item.Player.Position);
                }
            });
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningDrone,
                Category = ItemCategory.Useable,
                Description = "Creates a mechanical firefly that provides light.",
                IsConsumed = true,
                Tier = 1,
                ListPriority = 8,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 2), new Component(ItemId.Gold, 5), } },
                OnUse = (item) => {
                    var fireFlyType = EnemyBase.Get("Mining Drone");
                    var enemy = item.Player.Map.AddEnemy(fireFlyType, item.Player.Position);
                    enemy.Tier = Math.Max(1, item.Player.GetMiningTier());
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your mining drone will not be effective on this map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.RiftCreator,
                Category = ItemCategory.Useable,
                Description = "Makes a temporary rift that teleports you to your spawn location.",
                IsConsumed = true,
                Tier = 2,
                ListPriority = 9,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.EchoCrystal, 1) } },
                OnUse = (item) => {
                    item.Player.Map.AddPlaceable(null, ItemBase.Get(ItemId.Rift), item.Player.Position);
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.OverworldRiftCreator,
                Category = ItemCategory.Useable,
                Description = "Makes a temporary rift that teleports you to the Overworld.",
                IsConsumed = true,
                Tier = 2,
                ListPriority = 9,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.MercuryCell, 1) } },
                OnUse = (item) => {
                    item.Player.Map.AddPlaceable(null, ItemBase.Get(ItemId.OverworldRift), item.Player.Position);
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.PortalSeeker2,
                Tier = 2,
                ListPriority = 7,
                Category = ItemCategory.Useable,
                NameSource = "{TierType} Portal Seeker",
                Description = "Makes a drone that will fly towards the nearest dungeon portal based on its tier.",
                IsConsumed = true,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Steel, 2) } },
                OnUse = (item) => {
                    var portal = item.Player.Map.FindPlaceables("Dungeon Tier " + item.Type.Tier).FirstOrDefault();
                    item.Player.Map.AddEnemy(EnemyBase.Get("Portal Seeker"), item.Player.Position).TargetPlaceable = portal;
                },
                CanUse = (player, alreadyUsing) => {
                    if (player.Map == World.Overworld)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Your seeker can only seek portals in the Overworld.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                }
            });

            var seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker3;
            seeker.Tier = 3;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Copper, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);

            seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker4;
            seeker.Tier = 4;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Silver, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);

            seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker5;
            seeker.Tier = 5;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Gold, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);

            seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker6;
            seeker.Tier = 6;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Darksteel, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);

            seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker7;
            seeker.Tier = 7;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Adamantium, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);

            seeker = ItemBase.Get(ItemId.PortalSeeker2).Clone();
            seeker.Id = ItemId.PortalSeeker8;
            seeker.Tier = 8;
            seeker.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 1), new Component(ItemId.Obsidian, 5), new Component(ItemId.Detector, 1), } };
            ItemBase.AddItem(seeker);
        }

        static void AddComponents()
        {
            foreach (var itemId in (ItemId[])Enum.GetValues(typeof(ItemId))) {
                if (!ItemBase.TypesById.ContainsKey(itemId) && !itemId.ToString().StartsWith("Reserved")) {
                    ItemBase.AddItem(new ItemType() {
                        Id = itemId,
                        Category = ItemCategory.Component,
                        ListPriority = 99,
                    });
                }
            }

            ItemBase.TypesById[ItemId.Iron].Tier = 1;
            ItemBase.TypesById[ItemId.Sodium].Tier = 1;
            ItemBase.TypesById[ItemId.Coal].Tier = 1;
            ItemBase.TypesById[ItemId.Sulfur].Tier = 1;

            ItemBase.TypesById[ItemId.Ectoplasm].Tier = 1;
            ItemBase.TypesById[ItemId.Ectoplasm].ListPriority = 61;
            ItemBase.TypesById[ItemId.EctoplasmCore].Tier = 2;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EctoplasmCore }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Ectoplasm, 25), } });

            ItemBase.TypesById[ItemId.RedCrystal].Tier = 1;
            ItemBase.TypesById[ItemId.RedCrystal].Description = "A red crystal found on earth based monsters.";
            ItemBase.TypesById[ItemId.RedCrystal].ListPriority = 61;
            ItemBase.TypesById[ItemId.RedCrystal].Light = Color.FromNonPremultiplied(191, 37, 37, 100);

            ItemBase.TypesById[ItemId.BlueCrystal].Tier = 1;
            ItemBase.TypesById[ItemId.BlueCrystal].Description = "A blue crystal found on earth based monsters.";
            ItemBase.TypesById[ItemId.BlueCrystal].ListPriority = 61;
            ItemBase.TypesById[ItemId.BlueCrystal].Light = Color.FromNonPremultiplied(99, 114, 205, 100);

            ItemBase.TypesById[ItemId.GreenCrystal].Tier = 1;
            ItemBase.TypesById[ItemId.GreenCrystal].Description = "A green crystal found on earth based monsters.";
            ItemBase.TypesById[ItemId.GreenCrystal].ListPriority = 61;
            ItemBase.TypesById[ItemId.GreenCrystal].Light = Color.FromNonPremultiplied(225, 0, 225, 100);

            ItemBase.TypesById[ItemId.EchoCrystal].Tier = 1;
            ItemBase.TypesById[ItemId.EchoCrystal].Description = "A combination of red, blue and green crystals.";
            ItemBase.TypesById[ItemId.EchoCrystal].ListPriority = 61;
            ItemBase.TypesById[ItemId.EchoCrystal].Light = Color.FromNonPremultiplied(255, 0, 255, 100);
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EchoCrystal }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.RedCrystal, 1), new Component(ItemId.BlueCrystal, 1), new Component(ItemId.GreenCrystal, 1), }, CreateAmount = 2 });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EchoCrystal }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.RedCrystal, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EchoCrystal }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.BlueCrystal, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EchoCrystal }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.GreenCrystal, 2), } });

            ItemBase.TypesById[ItemId.GunnDiode].Tier = 1;
            ItemBase.TypesById[ItemId.GunnDiode].ListPriority = 61;
            ItemBase.TypesById[ItemId.GunnDiode].Description = "Small aliens are known to be made out of these.";
            ItemBase.TypesById[ItemId.FieldEffectTransistor].Tier = 1;
            ItemBase.TypesById[ItemId.FieldEffectTransistor].ListPriority = 61;
            ItemBase.TypesById[ItemId.FieldEffectTransistor].Description = "Flying aliens are known to be made out of these.";
            ItemBase.TypesById[ItemId.Thyratron].Tier = 1;
            ItemBase.TypesById[ItemId.Thyratron].ListPriority = 61;
            ItemBase.TypesById[ItemId.Thyratron].Description = "Small aliens are known to be made of these.";
            ItemBase.TypesById[ItemId.MercuryCell].Tier = 1;
            ItemBase.TypesById[ItemId.MercuryCell].ListPriority = 61;
            ItemBase.TypesById[ItemId.MercuryCell].Description = "Used to power mechanical tools and alien machines.";


            ItemBase.TypesById[ItemId.PhotovoltaicCell].Tier = 1;
            ItemBase.TypesById[ItemId.PhotovoltaicCell].ListPriority = 61;
            ItemBase.TypesById[ItemId.PhotovoltaicCell].Description = "A rare component found sometimes on alien machines and monsters.";
            ItemBase.TypesById[ItemId.Thermistor].Tier = 1;
            ItemBase.TypesById[ItemId.Thermistor].ListPriority = 61;
            ItemBase.TypesById[ItemId.Thermistor].Description = "Found on military units and insects.";

            ItemBase.TypesById[ItemId.Accelerometer].Tier = 1;
            ItemBase.TypesById[ItemId.Accelerometer].ListPriority = 61;
            ItemBase.TypesById[ItemId.Accelerometer].Description = "Made from a wire and a thermistor.";
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Accelerometer }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Wiring, 1), new Component(ItemId.Thermistor, 1), } });

            ItemBase.TypesById[ItemId.Wiring].Tier = 1;
            ItemBase.TypesById[ItemId.Wiring].ListPriority = 61;
            ItemBase.TypesById[ItemId.Wiring].Description = "Small aliens are known to be made out of these.";

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Thyratron, 1), new Component(ItemId.FieldEffectTransistor, 1), } });
            ItemBase.TypesById[ItemId.Detector].ListPriority = 61;
            ItemBase.TypesById[ItemId.Detector].Tier = 2;
            ItemBase.TypesById[ItemId.Detector].Description = "A component used to detect movement and objects in the world. Made from a thyratron and a transitor.";
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker2, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker3, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker4, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker5, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker6, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker7, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Detector }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PortalSeeker8, 1), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.MercuryArcRectifier }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 1), new Component(ItemId.GunnDiode, 1), } });
            ItemBase.TypesById[ItemId.MercuryArcRectifier].ListPriority = 61;
            ItemBase.TypesById[ItemId.MercuryArcRectifier].Tier = 2;
            ItemBase.TypesById[ItemId.MercuryArcRectifier].Description = "Used to make work stations. Made from a diode and a mercurycell.";

            ItemBase.TypesById[ItemId.IronSuperconductor].Tier = 1;
            ItemBase.TypesById[ItemId.IronSuperconductor].ListPriority = 69;
            ItemBase.TypesById[ItemId.IronSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.IronSuperconductor].AltCategory = ItemCategory.KeyComponent;

            ItemBase.TypesById[ItemId.Glass].Tier = 2;
            ItemBase.TypesById[ItemId.Glass].Description = "Made from sand.";
            ItemBase.TypesById[ItemId.Steel].Tier = 2;
            ItemBase.TypesById[ItemId.Steel].Description = "Made from iron.";
            ItemBase.TypesById[ItemId.Aluminum].Tier = 2;
            ItemBase.TypesById[ItemId.Battery].Tier = 2;
            ItemBase.TypesById[ItemId.Battery].ListPriority = 61;
            ItemBase.TypesById[ItemId.Battery].Description = "A strong power source for high tier equipment. Made in a chemical reactor.";
            ItemBase.TypesById[ItemId.OcularLens].Tier = 2;
            ItemBase.TypesById[ItemId.OcularLens].ListPriority = 61;
            ItemBase.TypesById[ItemId.OcularLens].Description = "Part of a gun used by strong alien units.";
            ItemBase.TypesById[ItemId.SteelSuperconductor].Tier = 2;
            ItemBase.TypesById[ItemId.SteelSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.SteelSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.SteelSuperconductor].AltCategory = ItemCategory.KeyComponent;

            ItemBase.TypesById[ItemId.Copper].Tier = 3;
            ItemBase.TypesById[ItemId.CopperSuperconductor].Tier = 3;
            ItemBase.TypesById[ItemId.CopperSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.CopperSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.CopperSuperconductor].AltCategory = ItemCategory.KeyComponent;
            ItemBase.TypesById[ItemId.Silver].Tier = 4;
            ItemBase.TypesById[ItemId.RifleBarrel].Tier = 4;
            ItemBase.TypesById[ItemId.RifleBarrel].ListPriority = 61;
            ItemBase.TypesById[ItemId.RifleBarrel].Description = "Commonly used by alien turrets.";
            ItemBase.TypesById[ItemId.GrenadeBarrel].Tier = 6;
            ItemBase.TypesById[ItemId.GrenadeBarrel].ListPriority = 61;
            ItemBase.TypesById[ItemId.SilverSuperconductor].Tier = 4;
            ItemBase.TypesById[ItemId.SilverSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.SilverSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.SilverSuperconductor].AltCategory = ItemCategory.KeyComponent;
            ItemBase.TypesById[ItemId.Gold].Tier = 5;
            ItemBase.TypesById[ItemId.Ice].Tier = 5;
            ItemBase.TypesById[ItemId.GoldSuperconductor].Tier = 5;
            ItemBase.TypesById[ItemId.GoldSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.GoldSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";

            ItemBase.TypesById[ItemId.Radium].Tier = 6;
            ItemBase.TypesById[ItemId.Radium].Light = Color.FromNonPremultiplied(26, 110, 210, 100);
            ItemBase.TypesById[ItemId.Darksteel].Tier = 6;
            ItemBase.TypesById[ItemId.Dilithium].Tier = 6;
            ItemBase.TypesById[ItemId.LaserGlass].Tier = 6;
            ItemBase.TypesById[ItemId.LaserGlass].Description = "Strong glass";
            ItemBase.TypesById[ItemId.LaserGlass].ListPriority = 61;
            ItemBase.TypesById[ItemId.Magnet].Tier = 6;
            ItemBase.TypesById[ItemId.Magnet].Description = "A component used in high tier equipment. Made in a chemical reactor.";
            ItemBase.TypesById[ItemId.DarksteelSuperconductor].Tier = 6;
            ItemBase.TypesById[ItemId.DarksteelSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.DarksteelSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.DarksteelSuperconductor].AltCategory = ItemCategory.KeyComponent;

            ItemBase.TypesById[ItemId.Adamantium].Tier = 7;
            ItemBase.TypesById[ItemId.AdamantiumSuperconductor].Tier = 7;
            ItemBase.TypesById[ItemId.AdamantiumSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.AdamantiumSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.AdamantiumSuperconductor].AltCategory = ItemCategory.KeyComponent;

            ItemBase.TypesById[ItemId.Uranium].Tier = 8;
            ItemBase.TypesById[ItemId.Uranium].Light = Color.FromNonPremultiplied(42, 226, 67, 100);
            ItemBase.TypesById[ItemId.Obsidian].Tier = 7;
            ItemBase.TypesById[ItemId.Obsidian].Description = "Made from water and lava combinding together";
            ItemBase.TypesById[ItemId.TurboSuperconductor].Tier = 8;
            ItemBase.TypesById[ItemId.TurboSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.TurboSuperconductor].Description = "Critical part for a digging tool. Alien's prized invention.";
            ItemBase.TypesById[ItemId.TurboSuperconductor].AltCategory = ItemCategory.KeyComponent;

            ItemBase.TypesById[ItemId.Plutonium].Tier = 9;
            ItemBase.TypesById[ItemId.Plutonium].Description = "Made from Uranium.";
            ItemBase.TypesById[ItemId.Einsteinium].Tier = 9;
            ItemBase.TypesById[ItemId.UltraSuperconductor].Tier = 9;
            ItemBase.TypesById[ItemId.UltraSuperconductor].ListPriority = 63;
            ItemBase.TypesById[ItemId.UltraSuperconductor].Description = "Critical part for a digging tool. Alien's most prized invention.";
            ItemBase.TypesById[ItemId.UltraSuperconductor].AltCategory = ItemCategory.KeyComponent;

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Glass }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Sand, 5), new Component(ItemId.Sodium, 1), new Component(ItemId.Coal, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Iron }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.IronOre, 5), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Steel }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Iron, 1), new Component(ItemId.Coal, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Aluminum }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.AluminumOre, 5), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Copper }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.CopperOre, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Silver }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.SilverOre, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Gold }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.GoldOre, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Ice }, Location = ItemId.ChemicalReactor, Components = new Component[] { new Component(ItemId.Snow, 500), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Radium }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.RadiumOre, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Darksteel }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.Steel, 5), new Component(ItemId.Dilithium, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.LaserGlass }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.Dilithium, 1), new Component(ItemId.Glass, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Battery }, Location = ItemId.ChemicalReactor, Components = new Component[] { new Component(ItemId.MercuryCell, 1), new Component(ItemId.Darksteel, 1), new Component(ItemId.Sodium, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Magnet }, Location = ItemId.ChemicalReactor, Components = new Component[] { new Component(ItemId.Darksteel, 4), new Component(ItemId.Aluminum, 5), new Component(ItemId.Accelerometer, 1), } });
            ItemBase.TypesById[ItemId.Magnet].ListPriority = 62;

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Uranium }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.UraniumOre, 5), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Plutonium }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.Uranium, 5), } });

            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Adamantium }, Location = ItemId.BessemerConverter, Components = new Component[] { new Component(ItemId.Radium, 5), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Einsteinium }, Location = ItemId.NuclearReactor, Components = new Component[] { new Component(ItemId.Plutonium, 5), } });

            ItemBase.TypesById[ItemId.AmethystScope].Tier = 4;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.AmethystScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Silver, 2), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.AmethystScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.AmethystScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.EmeraldScope].Tier = 5;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.EmeraldScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Gold, 2), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.EmeraldScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.EmeraldScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.SapphireScope].Tier = 6;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.SapphireScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Darksteel, 2), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.SapphireScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.SapphireScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.RubyScope].Tier = 7;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.RubyScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Adamantium, 2), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.RubyScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.RubyScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.DiamondScope].Tier = 8;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.DiamondScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Obsidian, 2), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.DiamondScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.DiamondScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.DiamondTacticalScope].Tier = 9;
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.DiamondTacticalScope }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 5), new Component(ItemId.Einsteinium, 5), new Component(ItemId.OcularLens, 1), } });
            ItemBase.TypesById[ItemId.DiamondTacticalScope].ListPriority = 61;
            ItemBase.TypesById[ItemId.DiamondTacticalScope].Description = "An attactment for the laser rifle.";
            ItemBase.TypesById[ItemId.Refrigerant].ListPriority = 61;
            ItemBase.TypesById[ItemId.Refrigerant].Tier = 4;
            ItemBase.TypesById[ItemId.FuelTank].ListPriority = 61;
            ItemBase.TypesById[ItemId.FuelTank].Tier = 5;
            ItemBase.TypesById[ItemId.EctoplasmCore].ListPriority = 61;
            ItemBase.TypesById[ItemId.EctoplasmCore].Light = Color.FromNonPremultiplied(65, 254, 110, 100);

            //Bricks
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.BlackBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.DarkGrayBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.LightGrayBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.WhiteBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.BlueBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.LightBlueBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.CyanBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.GreenBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.LightGreenBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.YellowBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.BrownBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.OrangeBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.TanBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.RedBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.PurpleBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.PinkBrick }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 2), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.GrayMetal }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.DarkGrayMetal }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.BlackMetal }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.RedMetal }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.WhiteMetal }, Location = ItemId.Furnace, Components = new Component[] { new Component(ItemId.Basalt, 1), } });
            RecipieBase.AddRecipe(new Recipe() { Creates = new Item() { TypeId = ItemId.Platform }, Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Basalt, 1), } });

            ItemBase.TypesById[ItemId.BlackBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.DarkGrayBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.LightGrayBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.BlueBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.LightBlueBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.CyanBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.GreenBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.LightGreenBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.YellowBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.BrownBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.OrangeBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.TanBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.RedBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.PurpleBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.PinkBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.WhiteBrick].ListPriority = 100;
            ItemBase.TypesById[ItemId.GrayMetal].ListPriority = 100;
            ItemBase.TypesById[ItemId.WhiteMetal].ListPriority = 100;
            ItemBase.TypesById[ItemId.BlackMetal].ListPriority = 100;
            ItemBase.TypesById[ItemId.DarkGrayMetal].ListPriority = 100;
            ItemBase.TypesById[ItemId.RedMetal].ListPriority = 100;
        }

        static void AddPlaceables()
        {
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.LightOrb,
                Description = "Floats and casts white light.",
                IsFloating = true,
                SpriteAnimation = SpriteAnimation.Cycle18,
                SpriteAnimationDelay = 100,
                Category = ItemCategory.Placable,
                Light = Color.FromNonPremultiplied(208, 208, 208, 255),
                Tier = 1,
                ListPriority = 0,
                Recipe = new Recipe() { Components = new Component[] { new Component(ItemId.Iron, 1), new Component(ItemId.Coal, 1), }, CreateAmount = 5 },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.BlueLightOrb,
                Description = "Floats and casts a dim blue light.",
                IsFloating = true,
                SpriteAnimation = SpriteAnimation.Cycle18,
                SpriteAnimationDelay = 100,
                Category = ItemCategory.Placable,
                Tier = 1,
                ListPriority = 0,
                Light = Color.FromNonPremultiplied(160, 160, 255, 255),
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.LightOrb, 5), new Component(ItemId.BlueCrystal, 1), }, CreateAmount = 5 },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.GreenLightOrb,
                Description = "Floats and casts a dim green light.",
                IsFloating = true,
                SpriteAnimation = SpriteAnimation.Cycle18,
                SpriteAnimationDelay = 100,
                Category = ItemCategory.Placable,
                Tier = 1,
                ListPriority = 0,
                Light = Color.FromNonPremultiplied(160, 255, 160, 255),
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.LightOrb, 5), new Component(ItemId.GreenCrystal, 1), }, CreateAmount = 5 },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.RedLightOrb,
                Description = "Floats and casts a dim red light.",
                IsFloating = true,
                SpriteAnimation = SpriteAnimation.Cycle18,
                SpriteAnimationDelay = 100,
                Category = ItemCategory.Placable,
                Tier = 1,
                ListPriority = 0,
                Light = Color.FromNonPremultiplied(255, 160, 160, 255),
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.LightOrb, 5), new Component(ItemId.RedCrystal, 1), }, CreateAmount = 5 },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.HiddenLight,
                Description = "Gives light but its source is hidden.",
                DrawEnable = false,
                ShowToolTip = false,
                IsFloating = true,
                IsDematerializable = false,
                Category = ItemCategory.Placable,
                Light = Color.White,
                Tier = 9,
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MaterialSource,
                Description = "Constantly creates a material at its location. Use /set to control what is created.",
                ShowToolTip = false,
                DrawEnable = false,
                IsFloating = true,
                IsDematerializable = false,
                Category = ItemCategory.Placable,
                Tier = 9,
                OnPlace = (player, placeable) => { placeable.Value = "Air"; },
                OnLongThink = (placeable) => {
                    if (!placeable.Flag) {
                        var material = MaterialInfo.GetMaterialByName(placeable.Value);
                        if (material != Material.None) {
                            var x = (int)placeable.Position.X / Map.BlockWidth;
                            var y = (int)placeable.Position.Y / Map.BlockHeight;
                            placeable.Map.SetMaterialIfNotLooseOrSolid(x, y, material);
                            placeable.Map.SetMaterialIfNotLooseOrSolid(x - 1, y, material);
                            placeable.Map.SetMaterialIfNotLooseOrSolid(x, y - 1, material);
                            placeable.Map.SetMaterialIfNotLooseOrSolid(x - 1, y - 1, material);
                        }
                    }
                },
                OnSwitch = (placeable, sourcePlaceable, sourcePlayer) => {
                    placeable.Flag = !placeable.Flag;
                    if (placeable.Flag)
                        placeable.Map.PlaySound(placeable.Position, Sound.MaterialSourceOn);
                    else
                        placeable.Map.PlaySound(placeable.Position, Sound.MaterialSourceOff);
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.TriggeredMaterialSource,
                Description = "When switched, creates a material at its location. Use /set to control what is created.",
                ShowToolTip = false,
                DrawEnable = false,
                IsFloating = true,
                IsDematerializable = false,
                Tier = 9,
                Category = ItemCategory.Placable,
                OnPlace = (player, placeable) => { placeable.Value = "Air"; },
                OnSwitch = (placeable, sourcePlaceable, sourcePlayer) => {
                    var material = MaterialInfo.GetMaterialByName(placeable.Value);
                    if (material != Material.None) {
                        var x = (int)placeable.Position.X / Map.BlockWidth;
                        var y = (int)placeable.Position.Y / Map.BlockHeight;
                        placeable.Map.SetMaterial(x, y, material);
                        placeable.Map.SetMaterial(x - 1, y, material);
                        placeable.Map.SetMaterial(x, y - 1, material);
                        placeable.Map.SetMaterial(x - 1, y - 1, material);
                    }
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MaterialSink,
                Description = "Constantly removes all materials from its location.",
                ShowToolTip = false,
                DrawEnable = false,
                IsFloating = true,
                IsDematerializable = false,
                Tier = 9,
                Category = ItemCategory.Placable,
                OnLongThink = (placeable) => {
                    if (!placeable.Flag) {
                        var x = (int)placeable.Position.X / Map.BlockWidth;
                        var y = (int)placeable.Position.Y / Map.BlockHeight;
                        placeable.Map.SetMaterial(x, y, Material.Air);
                        placeable.Map.SetMaterial(x - 1, y, Material.Air);
                        placeable.Map.SetMaterial(x, y - 1, Material.Air);
                        placeable.Map.SetMaterial(x - 1, y - 1, Material.Air);
                    }
                },
                OnSwitch = (placeable, sourcePlaceable, sourcePlayer) => {
                    placeable.Flag = !placeable.Flag;
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Door,
                Category = ItemCategory.Placable,
                OnPlace = (player, placeable) => { placeable.IsSolid = true; },
                Description = "A door that can be opened and closed.",
                SpriteAnimation = SpriteAnimation.Cycle9,
                Light = Color.LightBlue,
                Tier = 1,
                ListPriority = 4,
                IsDematerializable = true,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Aluminum, 5), } },
                OnActivate = (player, placeable) => {
                    var isClosable = true;
                    placeable.LivingCollision((living) => { isClosable = false; return true; });
                    if (isClosable) {
                        placeable.IsSolid = !placeable.IsSolid;
                        if (placeable.IsSolid) {
                            placeable.PlaySound(Sound.DoorClosed);
                            placeable.SetAnimation(Animation.Closing, true, 450);
                            World.Broadcast(placeable.Name + " " + placeable.Animation, MessageType.System);
                        } else {
                            placeable.PlaySound(Sound.DoorOpen);
                            placeable.SetAnimation(Animation.Opening, true, 450);
                            World.Broadcast(placeable.Name + " " + placeable.Animation, MessageType.System);
                        }
                    }
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    // This is here to prevent the system draw from happening.
                },
                OnDrawTop = (placeable, spriteBatch, location) => {
                    var frame = placeable.IsSolid ? 0 : 8;
                    if (placeable.Animation == Animation.Opening)
                        frame = Math.Min(placeable.AnimationAge / 50, 8);
                    if (placeable.Animation == Animation.Closing)
                        frame = 8 - Math.Min(placeable.AnimationAge / 50, 8);

                    spriteBatch.Draw(placeable.Type.Texture, location, placeable.Type.GetFrame(frame), Color.White);
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            var lockedDoor = ItemBase.Get(ItemId.Door).Clone();
            lockedDoor.Id = ItemId.LockedDoor;
            lockedDoor.Light = Color.Red;
            lockedDoor.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Aluminum, 5), } };
            lockedDoor.Description = "A door that remains open or closed unless triggered by a switch";
            lockedDoor.OnActivate = null;
            lockedDoor.OnSwitch = (placeable, sourcePlaceable, sourcePlayer) => {
                placeable.IsSolid = !placeable.IsSolid;
                if (placeable.IsSolid) {
                    placeable.PlaySound(Sound.DoorClosed);
                    placeable.SetAnimation(Animation.Closing, true, 450);
                    lockedDoor.Light = Color.Red;
                    World.Broadcast(placeable.Name + " " + placeable.Animation, MessageType.System);
                } else {
                    placeable.PlaySound(Sound.DoorOpen);
                    placeable.SetAnimation(Animation.Opening, true, 450);
                    lockedDoor.Light = Color.LightBlue;
                    World.Broadcast(placeable.Name + " " + placeable.Animation, MessageType.System);
                }
            };
            ItemBase.AddItem(lockedDoor);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Hypercube,
                Description = "Holds up to 24 items.",
                Category = ItemCategory.Placable,
                SpriteAnimation = SpriteAnimation.Cycle10,
                Tier = 1,
                ListPriority = 1,
                IsDematerializable = true,
                OnPlace = (source, p) => { p.InitalizeInventory(); },
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.HypercubeOfHoldingOpen);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Iron, 20), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.WorldHypercube,
                Description = "Holds up to 24 items that every player can take a copy of.",
                Category = ItemCategory.Placable,
                SpriteAnimation = SpriteAnimation.Cycle10,
                IsDematerializable = false,
                Tier = 9,
                OnPlace = (source, p) => {
                    p.InitalizeInventory();
                },
                OnActivate = (player, placeable) => {
                    if (player.Name == placeable.Owner) {
                        player.PlaySound(Sound.HypercubeOfHoldingOpen);
                        player.ActivePlaceable = placeable;
                    } else {
                        if (!player.IsFlagged(placeable)) {
                            player.SetFlag(placeable);
                            foreach (var item in placeable.Inventory) {
                                if (item != null) {
                                    player.GiveItem(item.TypeId, item.Amount);
                                    player.MessageToClient("You gained " + item.ToString() + (item.Amount > 1 ? " x" + item.Amount.ToString() : "") + ".", MessageType.System);
                                }
                            }
                            player.PlaySound(Sound.HypercubeOfHoldingOpen);
                        } else {
                            player.PlaySound(Sound.HypercubeOfHoldingLocked);
                            player.MessageToClient("You've already taken the contents of this chest.", MessageType.Error);
                        }
                    }
                }
            });

            var rewardChest = ItemBase.Get(ItemId.WorldHypercube).Clone();
            rewardChest.Id = ItemId.RewardHypercube;
            rewardChest.Description = "Acts like a world chest but disappears after 5 minutes.";
            rewardChest.IsTemporary = true;
            rewardChest.Light = Color.FromNonPremultiplied(255, 252, 0, 200);
            rewardChest.OnPlace = (source, placeable) => {
                placeable.InitalizeInventory();
                placeable.Map.AddEntity(new ParticleEmitter() { Position = placeable.Position, Color = Color.Yellow, Type = ParticleEffect.Explode, Value = 150 });
            };
            rewardChest.OnLongThink = (placeable) => {
                if (placeable.Age > 1000 * 60 * 2.5)
                    placeable.Remove();
            };
            ItemBase.AddItem(rewardChest);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.PersonalHypercube,
                Description = "Holds up to 24 items and is only usable by it's owner.",
                Category = ItemCategory.Placable,
                SpriteAnimation = SpriteAnimation.Cycle10,
                IsDematerializable = true,
                Tier = 1,
                ListPriority = 1,
                OnPlace = (source, p) => { p.Owner = source.Name; p.InitalizeInventory(); },
                OnActivate = (player, placeable) => {
                    if (player.Name == placeable.Owner) {
                        player.PlaySound(Sound.HypercubeOfHoldingOpen);
                        player.ActivePlaceable = placeable;
                    } else {
                        player.PlaySound(Sound.HypercubeOfHoldingLocked);
                        player.MessageToClient("This chest can only be opened by " + (placeable.Owner ?? "Nobody"), MessageType.Error);
                    }
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Hypercube, 1), new Component(ItemId.Thyratron, 3), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Furnace,
                Category = ItemCategory.Placable,
                Description = "Used for smelting Tier 1 to Tier 6 ores into metals.",
                Light = Color.FromNonPremultiplied(255, 214, 14, 255),
                SpriteAnimation = SpriteAnimation.Cycle9,
                SpriteAnimationDelay = 150,
                Tier = 1,
                ListPriority = -10,
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.Furnace);
                },
                OnUse = (item) => {
                    if (item != null && item.Player != null)
                        item.Player.PlaySound(Sound.Furnace);
                },
                Recipe = new Recipe() { Components = new Component[] { new Component(ItemId.Clay, 100), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MillingMachine,
                Category = ItemCategory.Placable,
                Tier = 1,
                ListPriority = -9,
                Light = Color.FromNonPremultiplied(0, 211, 127, 255),
                SpriteAnimation = SpriteAnimation.Oscillate9,
                Description = "Used for crafting low tier Armor, Weapons and Accessories.",
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.WorkBench);
                },
                Recipe = new Recipe() { Components = new Component[] { new Component(ItemId.Iron, 10), new Component(ItemId.FieldEffectTransistor, 1), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Forge,
                Category = ItemCategory.Placable,
                Tier = 5,
                ListPriority = -2,
                Description = "Used for crafting medium tier Armor, Weapons and Accessories.",
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.Forge);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Gold, 20), new Component(ItemId.Aluminum, 20), new Component(ItemId.GunnDiode, 10), new Component(ItemId.Thyratron, 10), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.CyberForge,
                Category = ItemCategory.Placable,
                Tier = 9,
                ListPriority = -2,
                Description = "Used for crafting high tier Armor, Weapons and Accessories.",
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.Forge);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryCell, 10), new Component(ItemId.Einsteinium, 20), new Component(ItemId.GunnDiode, 10), new Component(ItemId.Thyratron, 10), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.ChemicalReactor,
                Category = ItemCategory.Placable,
                Tier = 5,
                ListPriority = -1,
                Description = "Used for making Chemical-Based components.",
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.ChemicalReactor);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryArcRectifier, 5), new Component(ItemId.Gold, 10), new Component(ItemId.Coal, 20), new Component(ItemId.Sulfur, 20), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.BessemerConverter,
                Description = "Used for smelting high tier ores.",
                Category = ItemCategory.Placable,
                Tier = 5,
                ListPriority = -1,
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.ChemicalReactor);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.MercuryArcRectifier, 5), new Component(ItemId.Gold, 25), new Component(ItemId.Wiring, 5), new Component(ItemId.Sulfur, 20), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });


            ItemBase.AddItem(new ItemType() {
                Id = ItemId.NuclearReactor,
                Description = "Used to create plutonium and Einsteinium.",
                Category = ItemCategory.Placable,
                Tier = 8,
                ListPriority = -1,
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.ChemicalReactor);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 5), new Component(ItemId.Adamantium, 25), new Component(ItemId.Uranium, 25), new Component(ItemId.MercuryArcRectifier, 10), new Component(ItemId.Wiring, 15), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            //blah
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Workbench,
                Description = "Used to make devices and components for weapons, armor and accessories.",
                Category = ItemCategory.Placable,
                Tier = 1,
                ListPriority = -2,
                SpriteAnimation = SpriteAnimation.Cycle9,
                //SpriteAnimationDelay = 50,
                Light = Color.LightBlue,
                LightOrigin = new Vector2(-48, -48),
                OnActivate = (player, placeable) => {
                    player.ActivePlaceable = placeable;
                    player.PlaySound(Sound.WorkBench);
                },
                Recipe = new Recipe() { Components = new Component[] { new Component(ItemId.Iron, 10), } },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Sign,
                Category = ItemCategory.Placable,
                Description = "Used to post a sign that has text that can be edited.",
                IsEditable = true,
                Tier = 1,
                ListPriority = 2,
                IsDematerializable = true,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Iron, 5) } },
                OnPlace = (source, placeable) => {
                    placeable.Owner = source.Name;
                    placeable.Value = "Use /set to set this text.";
                },
                OnActivate = (player, placeable) => {
                    player.PlaySound(Sound.Tick);
                    player.ActivePlaceable = placeable;
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
                CanEdit = (player, placeable) => {
                    return (placeable.Owner == null || player.Name == placeable.Owner);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Waypoint,
                Description = "Activate to set your spawn point.",
                Category = ItemCategory.Placable,
                Tier = 9,
                SpriteAnimation = SpriteAnimation.Cycle9,
                SpriteAnimationDelay = 200,
                AlternateTextureName = "Portal Yellow",
                Light = Color.FromNonPremultiplied(255, 248, 139, 200),
                IsDematerializable = false,
                OnActivate = (player, placeable) => {
                    player.MessageToClient("Spawn Point Set", MessageType.System);
                    player.PlaySound(Sound.WaypointActivated);
                    player.Health = player.MaxHealth;
                    placeable.Map.AddEntity(new ParticleEmitter() { Position = placeable.Position, Color = Color.LightBlue, Type = ParticleEffect.Explode, Value = 150 });

                    if (player.StartPosition != placeable.Position) {
                        player.StartPosition = placeable.Position;
                    }
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var angle = -(float)placeable.Age / 1000f;
                    spriteBatch.Draw(placeable.Type.AlternateTexture, location + new Vector2(placeable.Type.SpriteWidth / 2, placeable.Type.SpriteHeight / 2 + 3), null, Color.White, angle, new Vector2(placeable.Type.AlternateTexture.Width / 2, placeable.Type.AlternateTexture.Height / 2), 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
                    placeable.Type.Draw(spriteBatch, location, (int)placeable.Age);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.PortableWaypoint,
                Description = "Can be activited to become your spawn location. Only useable placeable on Overworld.",
                Category = ItemCategory.Placable,
                Light = Color.FromNonPremultiplied(165, 255, 0, 200),
                SpriteAnimation = SpriteAnimation.Cycle9,
                SpriteAnimationDelay = 200,
                AlternateTextureName = "Portal Green",
                Tier = 9,
                ListPriority = 9,
                IsDematerializable = true,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 5), new Component(ItemId.Aluminum, 5), new Component(ItemId.FieldEffectTransistor, 5), } },
                OnActivate = (player, placeable) => {
                    player.PlaySound(Sound.WaypointActivated);
                    player.Health = player.MaxHealth;

                    if (player.StartPosition != placeable.Position) {
                        player.StartPosition = placeable.Position;
                    }
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var angle = (float)-placeable.Age / 1000f;
                    spriteBatch.Draw(placeable.Type.AlternateTexture, location + new Vector2(placeable.Type.SpriteWidth / 2, placeable.Type.SpriteHeight / 2 + 3), null, Color.White, angle, new Vector2(placeable.Type.AlternateTexture.Width / 2, placeable.Type.AlternateTexture.Height / 2), 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
                    placeable.Type.Draw(spriteBatch, location, (int)placeable.Age);
                },
                CanUse = (player, alreadyUsing) => {
                    if (player.Map == World.Overworld)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("Portable Waypoints can only be used in the overworld.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Rift,
                Description = "A rift that teleports you to your spawn location.",
                Category = ItemCategory.Placable,
                Light = Color.FromNonPremultiplied(96, 180, 255, 255),
                SpriteAnimation = SpriteAnimation.None,
                SpriteAnimationDelay = 150,
                IsDematerializable = false,
                Tier = 9,
                OnLongThink = (placeable) => {
                    if (placeable.Age > 1000 * 20) {
                        placeable.Map.AddEntity(new ParticleEmitter() { Color = Color.FromNonPremultiplied(88, 78, 255, 255), Type = ParticleEffect.Explode, Position = placeable.Position, Value = 50 });
                        placeable.Remove();
                    }
                },
                OnActivate = (player, placeable) => {
                    player.Position = player.ActualStartPosition;
                    player.Velocity = Vector2.Zero;

                    player.PlaySound(Sound.Portal);
                    player.SetAnimation(Animation.Spawn);

                    placeable.Map.AddEntity(new ParticleEmitter() { Color = Color.FromNonPremultiplied(88, 78, 255, 255), Type = ParticleEffect.Explode, Position = placeable.Position, Value = 50 });
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var angle = (float)placeable.Age / 200f;
                    spriteBatch.Draw(placeable.Type.Texture, location + new Vector2(placeable.Type.SpriteWidth / 2, placeable.Type.SpriteHeight / 2), null, Color.White, angle, new Vector2(placeable.Type.Texture.Width / 2, placeable.Type.Texture.Height / 2), 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.OverworldRift,
                Description = "A rift that teleports you to the Overworld.",
                Category = ItemCategory.Placable,
                Light = Color.FromNonPremultiplied(184, 133, 133, 255),
                SpriteAnimation = SpriteAnimation.None,
                SpriteAnimationDelay = 150,
                IsDematerializable = false,
                Tier = 9,
                OnLongThink = (placeable) => {
                    if (placeable.Age > 1000 * 20) {
                        placeable.Map.AddEntity(new ParticleEmitter() { Color = Color.Red, Type = ParticleEffect.Explode, Position = placeable.Position, Value = 50 });
                        placeable.Remove();
                    }
                },
                OnActivate = (player, placeable) => {
                    var map = World.GetMap("Overworld");
                    var targetPortal = map.FindPortalTo(placeable, player.Map, placeable.Name);
                    Player newPlayer;
                    if (targetPortal != null)
                        newPlayer = player.SendTo(map, targetPortal.Position);
                    else
                        newPlayer = player.SendTo(map);

                    placeable.Map.AddEntity(new ParticleEmitter() { Color = Color.Red, Type = ParticleEffect.Explode, Position = placeable.Position, Value = 50 });

                    newPlayer.PlaySound(Sound.Portal);
                    newPlayer.SetAnimation(Animation.Spawn);

                    player.PlaySound(Sound.Portal);
                    player.SetAnimation(Animation.Spawn);
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var angle = (float)placeable.Age / 200f;
                    spriteBatch.Draw(placeable.Type.Texture, location + new Vector2(placeable.Type.SpriteWidth / 2, placeable.Type.SpriteHeight / 2), null, Color.White, angle, new Vector2(placeable.Type.Texture.Width / 2, placeable.Type.Texture.Height / 2), 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Portal,
                Description = "Takes you to another part of the planet.",
                Category = ItemCategory.Placable,
                Light = Color.FromNonPremultiplied(96, 180, 255, 255),
                SpriteAnimation = SpriteAnimation.Cycle9,
                SpriteAnimationDelay = 200,
                AlternateTextureName = "Portal Blue",
                IsDematerializable = false,
                Tier = 9,
                OnActivate = (player, placeable) => {
                    var map = World.GetMap(placeable.Value);
                    if (map == null)
                        player.MessageToClient("Unable to find map " + (placeable.Value ?? "Not Set"), MessageType.Error);
                    else {
                        var targetPortal = map.FindPortalTo(placeable, player.Map, placeable.Name);
                        Player newPlayer;
                        if (targetPortal != null)
                            newPlayer = player.SendTo(map, targetPortal.Position);
                        else
                            newPlayer = player.SendTo(map);
                        newPlayer.PlaySound(Sound.Portal);
                        newPlayer.SetAnimation(Animation.Spawn);
                    }
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var angle = -(float)placeable.Age / 1000f;
                    spriteBatch.Draw(placeable.Type.AlternateTexture, location + new Vector2(placeable.Type.SpriteWidth / 2, placeable.Type.SpriteHeight / 2 + 3), null, Color.White, angle, new Vector2(placeable.Type.AlternateTexture.Width / 2, placeable.Type.AlternateTexture.Height / 2), 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
                    placeable.Type.Draw(spriteBatch, location, (int)placeable.Age);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Dynamite,
                Category = ItemCategory.Placable,
                Description = "Creates a small explosion after 3 seconds.",
                IsDematerializable = false,
                Tier = 1,
                ListPriority = 6,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Iron, 1), new Component(ItemId.Sulfur, 1), }, CreateAmount = 5 },
                OnPlace = (source, p) => {
                    new Thread(
                        () => {
                            Thread.Sleep(3000);
                            p.PlaySound(Sound.Explosion3);
                            p.Map.Explode(p.Position, 20);
                            p.Map.Explode(p.Position, 20);
                            p.Map.Explode(p.Position, 20);
                            p.Map.RemoveEntity(p);
                        }
                    ).Start();
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.NuclearBomb,
                Category = ItemCategory.Placable,
                Description = "Creates a MASSIVE explosion after a short time.",
                Tier = 9,
                Light = Color.FromNonPremultiplied(124, 254, 0, 50),
                ListPriority = 6,
                IsDematerializable = false,
                Recipe = new Recipe() { Location = ItemId.NuclearReactor, Components = new Component[] { new Component(ItemId.MercuryArcRectifier, 1), new Component(ItemId.Uranium, 20), new Component(ItemId.Wiring, 10), new Component(ItemId.Thermistor, 5), } },
                OnPlace = (source, p) => {
                    new Thread(
                        () => {
                            Thread.Sleep(3000);
                            p.PlaySound(Sound.Explosion3);
                            p.Map.Explode(p.Position, 30000, 9);
                            p.Map.Explode(p.Position, 100000, 9, false);
                            p.Map.Explode(p.Position, 100000, 9, false);
                            p.Map.Explode(p.Position, 100000, 9, false);
                            p.Map.Explode(p.Position, 100000, 9, false);
                            p.Map.Explode(p.Position, 100000, 9, false);
                            p.Map.RemoveEntity(p);
                        }
                    ).Start();
                },
                CanUse = (player, alreadyUsing) => {
                    if (player.Map == World.Overworld)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("This bomb can only be used in the overworld.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                }

            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Switch,
                Category = ItemCategory.Placable,
                Description = "Used to turn on and off other placables.",
                Tier = 1,
                OnPlace = (player, placeable) => { placeable.IsActivated = false; },
                SpriteAnimation = SpriteAnimation.Cycle9,
                Light = Color.Red,
                ListPriority = 3,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.GunnDiode, 1), new Component(ItemId.Wiring, 1), new Component(ItemId.FieldEffectTransistor, 1), } },
                OnActivate = (player, placeable) => {
                    placeable.IsActivated = !placeable.IsActivated;
                    if (placeable.IsActivated) {
                        placeable.PlaySound(Sound.Switch);
                        placeable.TriggerSwitch(player);
                        placeable.SetAnimation(Animation.On, true, 450);
                    } else {
                        placeable.PlaySound(Sound.Switch);
                        placeable.TriggerSwitch(player);
                        placeable.SetAnimation(Animation.Off, true, 450);
                    }
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var frame = 0;
                    if (placeable.Animation == Animation.On) {
                        frame = Math.Min(placeable.AnimationAge / 50, 8);
                        placeable.Type.Light = Color.LightGreen;
                    }
                    if (placeable.Animation == Animation.Off) {
                        frame = 8 - Math.Min(placeable.AnimationAge / 50, 8);
                        placeable.Type.Light = Color.Red;
                    }

                    spriteBatch.Draw(placeable.Type.Texture, location, placeable.Type.GetFrame(frame), Color.White);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.TimedSwitch,
                Category = ItemCategory.Placable,
                Description = "Temporary turns off other placeables for 10 seconds.",
                Tier = 1,
                ListPriority = 3,
                SpriteAnimation = SpriteAnimation.Cycle9,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.GunnDiode, 1), new Component(ItemId.Wiring, 1), new Component(ItemId.FieldEffectTransistor, 1), } },
                OnActivate = (player, placeable) => {
                    if (!placeable.Flag) {
                        placeable.Flag = true;
                        placeable.PlaySound(Sound.Switch);
                        placeable.PlayingSound = Sound.TimedSwitchActive;
                        placeable.TriggerSwitch(player);
                        placeable.SetAnimation(Animation.On, true, 450);

                        new Thread(
                            () => {
                                Thread.Sleep(10000);
                                placeable.PlaySound(Sound.Switch);
                                placeable.TriggerSwitch(player);
                                placeable.PlayingSound = Sound.None;
                                placeable.Flag = false;
                                placeable.SetAnimation(Animation.Off, true, 450);
                            }
                            ).Start();
                    }
                },
                CanUse = (player, alreadyUsing) => {
                    if (!player.Map.IsTerrainLocked)
                        return true;
                    if (!alreadyUsing) {
                        player.MessageToClient("You can not place this on a locked map.", MessageType.Error);
                        player.PlaySound(Sound.Error);
                    }
                    return false;
                },
                OnDraw = (placeable, spriteBatch, location) => {
                    var frame = 0;
                    if (placeable.Animation == Animation.On) {
                        frame = Math.Min(placeable.AnimationAge / 50, 8);
                        placeable.Type.Light = Color.LightGreen;
                    }
                    if (placeable.Animation == Animation.Off) {
                        frame = 8 - Math.Min(placeable.AnimationAge / 50, 8);
                        placeable.Type.Light = Color.Red;
                    }

                    spriteBatch.Draw(placeable.Type.Texture, location, placeable.Type.GetFrame(frame), Color.White);
                },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.PointOfInterest,
                Description = "Used to define points of interest for some enemies AI.",
                DrawEnable = false,
                ShowToolTip = false,
                IsFloating = true,
                IsDematerializable = false,
                Tier = 9,
                Category = ItemCategory.Placable,
            });
        }

        static void AddEquipment()
        {
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.TheKeysOfTheKingdom,
                Description = "Unlocks god mode, creative mode and all admin rights.",
                Category = ItemCategory.Accessory,
                Rarity = Rarity.Heroic,
                Tier = 9,
                ListPriority = 98,
                OnEquip = (p, i) => { p.IsAdmin = true; }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Suit2,
                NameSource = "{TierType} Suit",
                Tier = 2,
                ListPriority = 20,
                Category = ItemCategory.Suit,
                Description = "+80 Energy",
                OnEquip = (p, i) => { p.MaxHealth += 80; },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Steel, 15), } },
            });

            var suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit3;
            suit.Tier = 3;
            suit.Description = "+200 Energy";
            suit.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Copper, 30), new Component(ItemId.Topaz, 10), } };
            suit.OnEquip = (p, i) => { p.MaxHealth += 200; };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit4;
            suit.Tier = 4;
            suit.Description = "+300 Energy";
            suit.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Silver, 30), new Component(ItemId.Amethyst, 10), } };
            suit.OnEquip = (p, i) => { p.MaxHealth += 200; };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit5;
            suit.Tier = 5;
            suit.Description = "+750 Energy";
            suit.OnEquip = (p, i) => { p.MaxHealth += 750; };
            suit.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 30), new Component(ItemId.Emerald, 10), } };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit6;
            suit.Tier = 6;
            suit.Description = "+1,600 Energy";
            suit.OnEquip = (p, i) => { p.MaxHealth += 1600; };
            suit.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Darksteel, 30), new Component(ItemId.Sapphire, 10), } };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit7;
            suit.Tier = 7;
            suit.Description = "+3,200 Energy";
            suit.OnEquip = (p, i) => { p.MaxHealth += 3200; };
            suit.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Adamantium, 30), new Component(ItemId.Ruby, 10), } };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit8;
            suit.Tier = 8;
            suit.Description = "+6,400 Energy";
            suit.OnEquip = (p, i) => { p.MaxHealth += 6400; };
            suit.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Obsidian, 30), new Component(ItemId.Diamond, 10), } };
            ItemBase.AddItem(suit);

            suit = ItemBase.Get(ItemId.Suit2).Clone();
            suit.Id = ItemId.Suit9;
            suit.Tier = 9;
            suit.Description = "+12,800 Energy";
            suit.OnEquip = (p, i) => { p.MaxHealth += 12800; };
            suit.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Einsteinium, 30), new Component(ItemId.Diamond, 10), } };
            ItemBase.AddItem(suit);


            ItemBase.AddItem(new ItemType() {
                Id = ItemId.PowerCore2,
                NameSource = "{TierType} Power Core",
                Tier = 2,
                ListPriority = 21,
                Category = ItemCategory.PowerCore,
                Description = "+20 Energy\n+1 Energy Regeneration",
                OnEquip = (p, i) => { p.MaxHealth += 20; p.Regeneration += 1; },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 2), new Component(ItemId.Steel, 5), } },
            });

            var powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore3;
            powerCore.Tier = 3;
            powerCore.Description = "+75 Energy\n+2 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 75; p.Regeneration += 2; };
            powerCore.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Copper, 10), new Component(ItemId.Topaz, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore4;
            powerCore.Tier = 4;
            powerCore.Description = "+250 Energy\n+4 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 250; p.Regeneration += 4; };
            powerCore.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 20), new Component(ItemId.Silver, 10), new Component(ItemId.Amethyst, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore5;
            powerCore.Tier = 5;
            powerCore.Description = "+500 Energy\n+8 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 500; p.Regeneration += 8; };
            powerCore.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 20), new Component(ItemId.Gold, 10), new Component(ItemId.Emerald, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore6;
            powerCore.Tier = 6;
            powerCore.Description = "+1,000 Energy\n+16 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 1000; p.Regeneration += 16; };
            powerCore.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Darksteel, 10), new Component(ItemId.Sapphire, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore7;
            powerCore.Tier = 7;
            powerCore.Description = "+2,000 Energy\n+32 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 2000; p.Regeneration += 32; };
            powerCore.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Adamantium, 10), new Component(ItemId.Ruby, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore8;
            powerCore.Tier = 8;
            powerCore.Description = "+4,000 Energy\n+64 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 4000; p.Regeneration += 64; };
            powerCore.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Obsidian, 10), new Component(ItemId.Diamond, 15), } };
            ItemBase.AddItem(powerCore);

            powerCore = ItemBase.Get(ItemId.PowerCore2).Clone();
            powerCore.Id = ItemId.PowerCore9;
            powerCore.Tier = 9;
            powerCore.Description = "+8,000 Energy\n+128 Energy Regeneration";
            powerCore.OnEquip = (p, i) => { p.MaxHealth += 8300; p.Regeneration += 128; };
            powerCore.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Einsteinium, 15), new Component(ItemId.Diamond, 15), } };
            ItemBase.AddItem(powerCore);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Helmet3,
                NameSource = "{TierType} Helmet",
                Tier = 3,
                ListPriority = 22,
                Category = ItemCategory.Helmet,
                Description = "+25 Energy\n+1 Defense",
                OnEquip = (p, i) => { p.MaxHealth += 25; p.Defense += 1; },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Copper, 5), new Component(ItemId.Topaz, 15), new Component(ItemId.Glass, 10), } },
            });

            var helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet4;
            helmet.Tier = 4;
            helmet.Description = "+150 Energy\n+2 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 150; p.Defense += 2; };
            helmet.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Silver, 5), new Component(ItemId.Amethyst, 15), new Component(ItemId.Glass, 15), } };
            ItemBase.AddItem(helmet);

            helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet5;
            helmet.Tier = 5;
            helmet.Description = "+250 Energy\n+4 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 250; p.Defense += 4; };
            helmet.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 5), new Component(ItemId.Emerald, 15), new Component(ItemId.Glass, 20), } };
            ItemBase.AddItem(helmet);

            helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet6;
            helmet.Tier = 6;
            helmet.Description = "+500 Energy\n+8 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 500; p.Defense += 8; };
            helmet.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Darksteel, 5), new Component(ItemId.Sapphire, 15), new Component(ItemId.LaserGlass, 1), } };
            ItemBase.AddItem(helmet);

            helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet7;
            helmet.Tier = 7;
            helmet.Description = "+1,100 Energy\n+16 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 1100; p.Defense += 16; };
            helmet.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Adamantium, 5), new Component(ItemId.Ruby, 15), new Component(ItemId.LaserGlass, 1), } };
            ItemBase.AddItem(helmet);

            helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet8;
            helmet.Tier = 8;
            helmet.Description = "+2,200 Energy\n+32 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 2200; p.Defense += 32; };
            helmet.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Obsidian, 5), new Component(ItemId.Diamond, 15), new Component(ItemId.LaserGlass, 1), } };
            ItemBase.AddItem(helmet);

            helmet = ItemBase.Get(ItemId.Helmet3).Clone();
            helmet.Id = ItemId.Helmet9;
            helmet.Tier = 9;
            helmet.Description = "+4,400 Energy\n+64 Defense";
            helmet.OnEquip = (p, i) => { p.MaxHealth += 4400; p.Defense += 64; };
            helmet.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Einsteinium, 10), new Component(ItemId.Diamond, 15), new Component(ItemId.LaserGlass, 1), } };
            ItemBase.AddItem(helmet);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.EnergyShield1,
                NameSource = "{TierType} Energy Shield",
                Tier = 4,
                ListPriority = 45,
                Category = ItemCategory.Accessory,
                Description = "+ 1 Defense",
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EchoCrystal, 5), new Component(ItemId.Silver, 15), } },
                OnEquip = (p, i) => { p.Defense++; }
            });

            //var energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            //energyShield.Id = ItemId.EnergyShield2;
            //energyShield.Tier = 5;
            //energyShield.Description = "+ 2 Defense";
            //energyShield.OnEquip = (p, i) => { p.Defense += 2; };
            //energyShield.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EchoCrystal, 10), new Component(ItemId.Gold, 15), } };
            //ItemBase.AddItem(energyShield);

            var energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            energyShield.Id = ItemId.EnergyShield3;
            energyShield.Tier = 6;
            energyShield.Description = "+ 4 Defense";
            energyShield.OnEquip = (p, i) => { p.Defense += 4; };
            energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 5), new Component(ItemId.Darksteel, 15), } };
            ItemBase.AddItem(energyShield);

            //energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            //energyShield.Id = ItemId.EnergyShield4;
            //energyShield.Tier = 7;
            //energyShield.Description = "+ 8 Defense";
            //energyShield.OnEquip = (p, i) => { p.Defense += 8; };
            //energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 10), new Component(ItemId.Adamantium, 15), } };
            //ItemBase.AddItem(energyShield);

            //energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            //energyShield.Id = ItemId.CorruptedShield;
            //energyShield.NameSource = "Corrupted Bulwark";
            //energyShield.Tier = 7;
            //energyShield.Description = " + 16 Armor\n  - 600 Energy";
            //energyShield.OnEquip = (p, i) => { p.Defense += 16; p.MaxHealth -= 600; };
            //energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 3), new Component(ItemId.Darksteel, 10), new Component(ItemId.Adamantium, 10), } };
            //ItemBase.AddItem(energyShield);

            //energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            //energyShield.Id = ItemId.ForgeShield;
            //energyShield.NameSource = "Forge Bulwark";
            //energyShield.Tier = 7;
            //energyShield.Description = " + 16 Armor\n  - 6 Regeneration";
            //energyShield.OnEquip = (p, i) => { p.Defense += 16; p.Regeneration -= 6; };
            //energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 3), new Component(ItemId.Adamantium, 25), } };
            //ItemBase.AddItem(energyShield);

            energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            energyShield.Id = ItemId.EnergyShield5;
            energyShield.Tier = 8;
            energyShield.Description = "+ 16 Defense";
            energyShield.OnEquip = (p, i) => { p.Defense += 16; };
            energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 5), new Component(ItemId.Obsidian, 15), } };
            ItemBase.AddItem(energyShield);

            energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            energyShield.Id = ItemId.CorruptedShield2;
            energyShield.NameSource = "Corrupted Energy Shield";
            energyShield.Tier = 8;
            energyShield.Description = " + 32 Armor\n  - 320 Energy\n - 16 Regeneration";
            energyShield.OnEquip = (p, i) => { p.Defense += 32; p.MaxHealth -= 320; p.Regeneration -= 16; };
            energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 3), new Component(ItemId.MercuryCell, 2), new Component(ItemId.Obsidian, 15), } };
            ItemBase.AddItem(energyShield);

            energyShield = ItemBase.Get(ItemId.EnergyShield1).Clone();
            energyShield.Id = ItemId.ForgeShield2;
            energyShield.NameSource = "Enforced Energy Shield";
            energyShield.Tier = 8;
            energyShield.Description = " + 32 Armor\n  - 8 Regeneration\n -640 Energy";
            energyShield.OnEquip = (p, i) => { p.Defense += 32; p.MaxHealth -= 630; p.Regeneration -= 8; };
            energyShield.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 2), new Component(ItemId.MercuryCell, 3), new Component(ItemId.Obsidian, 15), } };
            ItemBase.AddItem(energyShield);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.EnergyBooster1,
                NameSource = "{TierType} Energy Tank",
                Tier = 2,
                ListPriority = 46,
                Category = ItemCategory.Accessory,
                Description = "+ 10 Energy",
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Steel, 15), } },
                OnEquip = (p, i) => { p.MaxHealth += 10; }
            });

            //energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            //energyBooster.Id = ItemId.ReservedEnergyBooster2;
            //energyBooster.Tier = 3;
            //energyBooster.Description = "+ 20 Energy.";
            //energyBooster.OnEquip = (p, i) => { p.MaxHealth += 20; };
            //energyBooster.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Copper, 15), } };
            //ItemBase.AddItem(energyBooster);

            var energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            energyBooster.Id = ItemId.EnergyBooster3;
            energyBooster.Tier = 4;
            energyBooster.Description = "+ 40 Energy";
            energyBooster.OnEquip = (p, i) => { p.MaxHealth += 40; };
            energyBooster.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Silver, 15), } };
            ItemBase.AddItem(energyBooster);

            //energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            //energyBooster.Id = ItemId.EnergyBooster4;
            //energyBooster.Tier = 5;
            //energyBooster.Description = "+ 80 Energy";
            //energyBooster.OnEquip = (p, i) => { p.MaxHealth += 80; };
            //energyBooster.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Gold, 15), } };
            //ItemBase.AddItem(energyBooster);

            energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            energyBooster.Id = ItemId.EnergyBooster5;
            energyBooster.Tier = 6;
            energyBooster.Description = "+ 160 Energy";
            energyBooster.OnEquip = (p, i) => { p.MaxHealth += 160; };
            energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Darksteel, 15), } };
            ItemBase.AddItem(energyBooster);

            //energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            //energyBooster.Id = ItemId.EnergyBooster6;
            //energyBooster.Tier = 7;
            //energyBooster.Description = "+ 320 Energy";
            //energyBooster.OnEquip = (p, i) => { p.MaxHealth += 320; };
            //energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Adamantium, 15), } };
            //ItemBase.AddItem(energyBooster);

            //energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            //energyBooster.Id = ItemId.AegisEnergyTank1;
            //energyBooster.NameSource = "Aegis Energy Tank";
            //energyBooster.Tier = 7;
            //energyBooster.Description = "+ 500 Energy\n - 4 Regeneration";
            //energyBooster.OnEquip = (p, i) => { p.MaxHealth += 500; p.Regeneration -= 4; };
            //energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 20), new Component(ItemId.Adamantium, 10), } };
            //ItemBase.AddItem(energyBooster);

            //energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            //energyBooster.Id = ItemId.VigorEnergyTank1;
            //energyBooster.NameSource = "Vigor Energy Tank";
            //energyBooster.Tier = 7;
            //energyBooster.Description = "+ 500 Energy\n - 3 Defense";
            //energyBooster.OnEquip = (p, i) => { p.MaxHealth += 500; p.Defense -= 3; };
            //energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 25), new Component(ItemId.Adamantium, 7), } };
            //ItemBase.AddItem(energyBooster);

            energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            energyBooster.Id = ItemId.EnergyBooster7;
            energyBooster.Tier = 8;
            energyBooster.Description = "+ 640 Energy";
            energyBooster.OnEquip = (p, i) => { p.MaxHealth += 640; };
            energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 10), new Component(ItemId.Obsidian, 15), } };
            ItemBase.AddItem(energyBooster);

            energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            energyBooster.Id = ItemId.AegisEnergyTank2;
            energyBooster.NameSource = "Aegis Energy Tank";
            energyBooster.Tier = 8;
            energyBooster.Description = "+ 1280 Energy\n - 16 Regeneration\n - 8 Defense";
            energyBooster.OnEquip = (p, i) => { p.MaxHealth += 1280; p.Regeneration -= 16; p.Defense -= 8; };
            energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 20), new Component(ItemId.Obsidian, 10), } };
            ItemBase.AddItem(energyBooster);

            energyBooster = ItemBase.Get(ItemId.EnergyBooster1).Clone();
            energyBooster.Id = ItemId.VigorEnergyTank2;
            energyBooster.NameSource = "Vigor Energy Tank";
            energyBooster.Tier = 8;
            energyBooster.Description = "+ 1280 Energy\n - 16 Defense\n - 8 Regeneration";
            energyBooster.OnEquip = (p, i) => { p.MaxHealth += 1280; p.Defense -= 16; p.Regeneration -= 8; };
            energyBooster.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EctoplasmCore, 25), new Component(ItemId.Obsidian, 7), } };
            ItemBase.AddItem(energyBooster);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.EnergyRegenerator1,
                NameSource = "{TierType} Energy Refill",
                Tier = 4,
                ListPriority = 47,
                Category = ItemCategory.Accessory,
                Description = "+ 1 Regeneration",
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 2), new Component(ItemId.Silver, 15), } },
                OnEquip = (p, i) => { p.Regeneration++; }
            });

            //var energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            //energyReg.Id = ItemId.EnergyRegenerator2;
            //energyReg.Tier = 5;
            //energyReg.Description = "+ 2 Regeneration";
            //energyReg.OnEquip = (p, i) => { p.Regeneration += 2; };
            //energyReg.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 5), new Component(ItemId.Gold, 15), } };
            //ItemBase.AddItem(energyReg);

            var energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            energyReg.Id = ItemId.EnergyRegenerator3;
            energyReg.Tier = 6;
            energyReg.Description = "+ 4 Regeneration";
            energyReg.OnEquip = (p, i) => { p.Regeneration += 4; };
            energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 2), new Component(ItemId.Darksteel, 15), } };
            ItemBase.AddItem(energyReg);

            //energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            //energyReg.Id = ItemId.EnergyRegenerator4;
            //energyReg.Tier = 7;
            //energyReg.Description = "+8 Regeneration";
            //energyReg.OnEquip = (p, i) => { p.Regeneration += 8; };
            //energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 5), new Component(ItemId.Adamantium, 15), } };
            //ItemBase.AddItem(energyReg);

            //energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            //energyReg.Id = ItemId.FrenzyRegeneration1;
            //energyReg.NameSource = "Frenzied Energy Refill";
            //energyReg.Tier = 7;
            //energyReg.Description = "+ 14 Regeneration\n-400 Energy";
            //energyReg.OnEquip = (p, i) => { p.Regeneration += 14; p.MaxHealth -= 400;  };
            //energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 25), new Component(ItemId.Adamantium, 5), } };
            //ItemBase.AddItem(energyReg);

            //energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            //energyReg.Id = ItemId.EchoRegeneration1;
            //energyReg.NameSource = "Echo Energy Refill";
            //energyReg.Tier = 7;
            //energyReg.Description = "+ 14 Regeneration\n- 6 Defense";
            //energyReg.OnEquip = (p, i) => { p.Regeneration += 14; p.Defense -= 6; };
            //energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 15), new Component(ItemId.Adamantium, 5), } }; //Needs something besides obsidian
            //ItemBase.AddItem(energyReg);

            energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            energyReg.Id = ItemId.EnergyRegenerator5;
            energyReg.Tier = 8;
            energyReg.Description = "+16 Regeneration";
            energyReg.OnEquip = (p, i) => { p.Regeneration += 16; };
            energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 2), new Component(ItemId.Obsidian, 15), } };
            ItemBase.AddItem(energyReg);

            energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            energyReg.Id = ItemId.FrenzyRegeneration2;
            energyReg.NameSource = "Advanced Energy Refill";
            energyReg.Tier = 8;
            energyReg.Description = "+ 32 Regeneration\n -640 Energy\n -8 Defense";
            energyReg.OnEquip = (p, i) => { p.Regeneration += 32; p.MaxHealth -= 800; p.Defense -= 8; };
            energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.PhotovoltaicCell, 10), new Component(ItemId.Obsidian, 5), } };
            ItemBase.AddItem(energyReg);

            energyReg = ItemBase.Get(ItemId.EnergyRegenerator1).Clone();
            energyReg.Id = ItemId.EchoRegeneration2;
            energyReg.NameSource = "Echo Energy Refill";
            energyReg.Tier = 8;
            energyReg.Description = "+ 32 Regeneration\n- 16 Defense\n -320 Energy";
            energyReg.OnEquip = (p, i) => { p.Regeneration += 32; p.Defense -= 16; p.MaxHealth -= 320; };
            energyReg.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.EchoCrystal, 15), new Component(ItemId.Adamantium, 5), } };
            ItemBase.AddItem(energyReg);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.GravityBelt5,
                NameSource = "{TierType} Jump Amplifier",
                Tier = 5,
                ListPriority = 48,
                Category = ItemCategory.Accessory,
                Description = "Reduces the effect of gravity by 15%",
                OnEquip = (p, i) => { p.FallReduction = 0.15f; },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 15), new Component(ItemId.Accelerometer, 5), } },
            });

            var belt = ItemBase.Get(ItemId.GravityBelt5).Clone();
            belt.Id = ItemId.GravityBelt6;
            belt.Tier = 6;
            belt.Description = "Reduces the effect of gravity by 30%";
            belt.OnEquip = (p, i) => { p.FallReduction = 0.3f; };
            belt.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 1), new Component(ItemId.Darksteel, 15), new Component(ItemId.Accelerometer, 5), } };
            ItemBase.AddItem(belt);

            belt = ItemBase.Get(ItemId.GravityBelt5).Clone();
            belt.Id = ItemId.GravityBelt7;
            belt.Tier = 7;
            belt.Description = "Reduces the effect of gravity by 45%";
            belt.OnEquip = (p, i) => { p.FallReduction = 0.45f; };
            belt.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 2), new Component(ItemId.Adamantium, 15), new Component(ItemId.Accelerometer, 5), } };
            ItemBase.AddItem(belt);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.LeapTreads,
                Tier = 4,
                ListPriority = 47,
                Category = ItemCategory.Boots,
                Description = "Increase your jump by 80",
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Silver, 20), new Component(ItemId.Accelerometer, 5), new Component(ItemId.FieldEffectTransistor, 5), } },
                OnEquip = (p, i) => {
                    p.MaxJumpLength += 80;
                    p.JumpSpeed += 200;
                }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.AirTank,
                Tier = 5,
                ListPriority = 49,
                Category = ItemCategory.Accessory,
                Description = "Increase your air supply to last 5 seconds longer.",
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 20), new Component(ItemId.Thyratron, 5), } },
                OnEquip = (p, i) => { p.MaxAirSupply += 5; }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.HeatSink,
                NameSource = "Convector",
                Tier = 7,
                ListPriority = 49,
                Category = ItemCategory.Accessory,
                Description = "Reduces fire damage by 75%.",
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Adamantium, 20), new Component(ItemId.Aluminum, 5), new Component(ItemId.Thermistor, 5), } },
                OnEquip = (p, i) => { p.FireResistance = 0.75; }
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.HeadLamp,
                Tier = 3,
                ListPriority = 49,
                Category = ItemCategory.Accessory,
                Description = "An attactment that gives off light.",
                OnEquip = (p, i) => { p.CastLight = Color.FromNonPremultiplied(128, 128, 128, 128); },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.MercuryCell, 1), new Component(ItemId.Silver, 15), new Component(ItemId.Aluminum, 1), new Component(ItemId.Glass, 200), new Component(ItemId.FieldEffectTransistor, 5), new Component(ItemId.LightOrb, 1000) } },
            });
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Carbonite,
                Name = "Carbonite",
                Tier = 9,
                ListPriority = 49,
                Category = ItemCategory.Accessory,
                Description = "Your suit blows up upon death.",
                Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Einsteinium, 10), new Component(ItemId.Uranium, 10), new Component(ItemId.MercuryArcRectifier, 10), } },
                OnEquip = (p, i) => { p.ExplodesOnDeath = true; }
            });
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.SpeedBoots,
                Description = "Increases your running speed by 40.",
                Category = ItemCategory.Boots,
                Tier = 2,
                ListPriority = 47,
                OnEquip = (p, i) => { p.MaxRunSpeed += 40; },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Steel, 25), new Component(ItemId.Accelerometer, 5), } },
            });
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.ShockAbsorbers,
                Description = "Makes you immune to fall damage.",
                Category = ItemCategory.Boots,
                ListPriority = 47,
                Tier = 9,
                Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Einsteinium, 25), new Component(ItemId.Accelerometer, 10), new Component(ItemId.GunnDiode, 10), } },
                OnEquip = (p, i) => { p.IsImmuneToFallDamage = true; }
            });
        }

        static void AddWeapons()
        {
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Blaster1,
                NameSource = "{TierType} Blaster",
                Tier = 1,
                ListPriority = 11,
                Category = ItemCategory.Blaster,
                Description = "High firing rate and low power.\n{5} Damage\n{CoolDown:N0}ms Cool Down",
                CoolDown = 0,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Iron, 5), } },
                OnUse = (item) => {
                    if (item.UseLength > 3800) {
                        item.Player.UsingItem = null;
                        //if (item.Player.Map.Name != "Start") // Don't explode in the start zone so that players can't skip crafting their mining tool.
                        item.Player.Map.Explode(item.Player.GetShootingOrigin(), 5, 0);
                    }
                },
                OnUsingDraw = (item, spriteBatch) => {
                    Particles.AddExplode(item.Player.GetShootingOrigin(), Utility.GetTierColor(item.Type.Tier), Math.Min(5, (int)item.UseLength / 200));
                },
                OnStartUsing = (item) => {
                    item.Player.PlayingSound = Sound.BlasterCharge;
                },
                OnStopUsing = (item) => {
                    if (item.UseLength > 1000)
                        Projectile.Shoot(ProjectileId.BlasterCharged, item.Player, item.GetTier());
                    else
                        Projectile.Shoot(ProjectileId.Blaster, item.Player, item.GetTier());
                    item.Player.PlayingSound = Sound.None;
                }
            });

            var weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster2;
            weapon.Tier = 2;
            weapon.Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Steel, 10), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster3;
            weapon.Tier = 3;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Copper, 10), new Component(ItemId.Topaz, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster4;
            weapon.Tier = 4;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Silver, 10), new Component(ItemId.Amethyst, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster5;
            weapon.Tier = 5;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 10), new Component(ItemId.Emerald, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster6;
            weapon.Tier = 6;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Darksteel, 10), new Component(ItemId.Sapphire, 5), new Component(ItemId.GunnDiode, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster7;
            weapon.Tier = 7;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Adamantium, 10), new Component(ItemId.Ruby, 5), new Component(ItemId.GunnDiode, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster8;
            weapon.Tier = 8;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Obsidian, 10), new Component(ItemId.Diamond, 5), new Component(ItemId.GunnDiode, 5), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Blaster1).Clone();
            weapon.Id = ItemId.Blaster9;
            weapon.Tier = 9;
            weapon.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Einsteinium, 10), new Component(ItemId.Diamond, 5), new Component(ItemId.GunnDiode, 5), } };
            ItemBase.AddItem(weapon);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Disruptor3,
                NameSource = "{TierType} Disruptor",
                Tier = 3,
                ListPriority = 12,
                Category = ItemCategory.Disruptor,
                Description = "Fires a short range spread of bullets.\n{4} Damage Each\n{CoolDown:N0}ms Cool Down",
                CoolDown = 250,
                PlaceAndActivateRange = 10000,
                IsManualFire = true,
                Light = Color.White,
                OnUse = (item) => {
                    Projectile.Shoot(ProjectileId.Disruptor, item.Player, item.GetTier(), (float)-Math.PI / 18);
                    Projectile.Shoot(ProjectileId.Disruptor, item.Player, item.GetTier());
                    Projectile.Shoot(ProjectileId.Disruptor, item.Player, item.GetTier(), (float)Math.PI / 18);
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Copper, 15), new Component(ItemId.Topaz, 15), new Component(ItemId.Thermistor, 3), } },
            });

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor4;
            weapon.Tier = 4;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Silver, 15), new Component(ItemId.Amethyst, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor5;
            weapon.Tier = 5;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Gold, 15), new Component(ItemId.Emerald, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor6;
            weapon.Tier = 6;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Darksteel, 15), new Component(ItemId.Sapphire, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor7;
            weapon.Tier = 7;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Adamantium, 15), new Component(ItemId.Ruby, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor8;
            weapon.Tier = 8;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 2), new Component(ItemId.Obsidian, 15), new Component(ItemId.Diamond, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Disruptor3).Clone();
            weapon.Id = ItemId.Disruptor9;
            weapon.Tier = 9;
            weapon.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Magnet, 2), new Component(ItemId.Einsteinium, 15), new Component(ItemId.Diamond, 15), new Component(ItemId.Thermistor, 3), } };
            ItemBase.AddItem(weapon);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.LaserRifle4,
                NameSource = "{TierType} Laser Rifle",
                Tier = 4,
                ListPriority = 13,
                Category = ItemCategory.LaserRifle,
                Description = "Fires a long range high power shot.\n{19} Damage\n{CoolDown:N0}ms Cool Down",
                CoolDown = 1000,
                PlaceAndActivateRange = 10000,
                IsManualFire = true,
                Light = Color.White,
                OnUse = (item) => {
                    Projectile.Shoot(ProjectileId.LaserRifle, item.Player, item.GetTier());
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 20), new Component(ItemId.Silver, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.AmethystScope, 1), } },
            });

            weapon = ItemBase.Get(ItemId.LaserRifle4).Clone();
            weapon.Id = ItemId.LaserRifle5;
            weapon.Tier = 5;
            weapon.Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 40), new Component(ItemId.Gold, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.EmeraldScope, 1), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.LaserRifle4).Clone();
            weapon.Id = ItemId.LaserRifle6;
            weapon.Tier = 6;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Darksteel, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.SapphireScope, 1), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.LaserRifle4).Clone();
            weapon.Id = ItemId.LaserRifle7;
            weapon.Tier = 7;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 1), new Component(ItemId.Adamantium, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.RubyScope, 1), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.LaserRifle4).Clone();
            weapon.Id = ItemId.LaserRifle8;
            weapon.Tier = 8;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Magnet, 2), new Component(ItemId.Obsidian, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.DiamondScope, 1), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.LaserRifle4).Clone();
            weapon.Id = ItemId.LaserRifle9;
            weapon.Tier = 9;
            weapon.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Magnet, 3), new Component(ItemId.Einsteinium, 15), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.DiamondTacticalScope, 1), } };
            ItemBase.AddItem(weapon);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Homing5,
                NameSource = "{TierType} Homing Missile",
                Tier = 5,
                ListPriority = 14,
                Category = ItemCategory.HomingMissile,
                Description = "Fires homing missles.\n{11} Damage\n{CoolDown:N0}ms Cool Down",
                CoolDown = 800,
                PlaceAndActivateRange = 10000,
                IsManualFire = true,
                Light = Color.White,
                OnUse = (item) => {
                    Projectile.Shoot(ProjectileId.HomingMissile, item.Player, item.GetTier());
                },
            });

            weapon = ItemBase.Get(ItemId.Homing5).Clone();
            weapon.Id = ItemId.Homing6;
            weapon.Tier = 6;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Darksteel, 20), new Component(ItemId.Sapphire, 20), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.Detector, 1) } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Homing5).Clone();
            weapon.Id = ItemId.Homing7;
            weapon.Tier = 7;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Adamantium, 20), new Component(ItemId.Ruby, 20), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.Detector, 1) } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Homing5).Clone();
            weapon.Id = ItemId.Homing8;
            weapon.Tier = 8;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Obsidian, 20), new Component(ItemId.Diamond, 20), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.Detector, 1) } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.Homing5).Clone();
            weapon.Id = ItemId.Homing9;
            weapon.Tier = 9;
            weapon.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Einsteinium, 20), new Component(ItemId.Diamond, 20), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.Detector, 1) } };
            ItemBase.AddItem(weapon);

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.FlameThrower,
                Category = ItemCategory.Tool,
                Description = "Projects a long controllable stream of fire.",
                CoolDown = 50,
                PlaceAndActivateRange = 10000,
                Light = Color.Red,
                Tier = 6,
                ListPriority = 19,
                OnUse = (item) => {
                    item.Player.PlayingSound = Sound.GunFlameThrower;
                    Projectile.Shoot(ProjectileId.FlameThrower, item.Player, item.GetTier());
                },
                OnStopUsing = (item) => {
                    item.Player.PlayingSound = Sound.None;
                },
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Darksteel, 20), new Component(ItemId.Thermistor, 10), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.FuelTank, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.BoseEinsteinCondenser,
                Description = "Creates fast melting ice out of the moisture in the air.",
                Category = ItemCategory.Tool,
                CoolDown = 190,
                PlaceAndActivateRange = 10000,
                ListPriority = 19,
                Light = Color.LightBlue,
                IsManualFire = true,
                Tier = 5,
                OnUse = (item) => {
                    Projectile.Shoot(ProjectileId.BoseEinsteinCondenser, item.Player, item.GetTier());
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Aluminum, 20), new Component(ItemId.RifleBarrel, 1), new Component(ItemId.Thermistor, 10), new Component(ItemId.Refrigerant, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.GrenadeLauncher6,
                NameSource = "{TierType} Grenade Launcher",
                Tier = 6,
                ListPriority = 15,
                Category = ItemCategory.Grenade,
                Description = "High power gun that shoots Grenades.\n{5} Damage\n{CoolDown:N0}ms Cool Down",
                CoolDown = 500,
                PlaceAndActivateRange = 10000,
                IsManualFire = true,
                Light = Color.White,
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.GrenadeBarrel), new Component(ItemId.Darksteel, 20), new Component(ItemId.Sapphire, 20), new Component(ItemId.GunnDiode, 3), } },
                OnUse = (item) => {
                    Projectile.Shoot(ProjectileId.Grenade_2, item.Player, item.GetTier());
                },
            });
            weapon = ItemBase.Get(ItemId.GrenadeLauncher6).Clone();
            weapon.Id = ItemId.GrenadeLauncher7;
            weapon.Tier = 7;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.GrenadeBarrel), new Component(ItemId.Adamantium, 20), new Component(ItemId.Ruby, 20), new Component(ItemId.GunnDiode, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.GrenadeLauncher6).Clone();
            weapon.Id = ItemId.GrenadeLauncher8;
            weapon.Tier = 8;
            weapon.Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.GrenadeBarrel), new Component(ItemId.MercuryCell, 1), new Component(ItemId.Obsidian, 20), new Component(ItemId.Diamond, 20), new Component(ItemId.GunnDiode, 3), } };
            ItemBase.AddItem(weapon);

            weapon = ItemBase.Get(ItemId.GrenadeLauncher6).Clone();
            weapon.Id = ItemId.GrenadeLauncher9;
            weapon.Tier = 9;
            weapon.Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.GrenadeBarrel), new Component(ItemId.MercuryCell, 1), new Component(ItemId.Einsteinium, 20), new Component(ItemId.Diamond, 20), new Component(ItemId.GunnDiode, 3), } };
            ItemBase.AddItem(weapon);
        }

        static void AddMiningTools()
        {
            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool1,
                NameSource = "{TierType} Mining Tool",
                Tier = 1,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 150,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigSmall, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Ectoplasm, 10), new Component(ItemId.Iron, 15), new Component(ItemId.IronSuperconductor, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool2,
                NameSource = "{TierType} Mining Tool",
                Tier = 2,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 150,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigMedium, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.EctoplasmCore, 1), new Component(ItemId.Steel, 15), new Component(ItemId.SteelSuperconductor, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool3,
                NameSource = "{TierType} Mining Tool",
                Tier = 3,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 150,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigMedium, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 2), new Component(ItemId.Copper, 15), new Component(ItemId.Topaz, 10), new Component(ItemId.CopperSuperconductor, 1) } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool4,
                NameSource = "{TierType} Mining Tool",
                Tier = 4,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 150,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigLarge, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 2), new Component(ItemId.Silver, 15), new Component(ItemId.Amethyst, 10), new Component(ItemId.SilverSuperconductor, 1) } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool5,
                NameSource = "{TierType} Mining Tool",
                Tier = 5,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 150,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigLarge, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.EctoplasmCore, 2), new Component(ItemId.Gold, 15), new Component(ItemId.Emerald, 10), new Component(ItemId.GoldSuperconductor, 1) } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool6,
                NameSource = "{TierType} Mining Tool",
                Tier = 6,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 100,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 4), new Component(ItemId.Darksteel, 15), new Component(ItemId.DarksteelSuperconductor, 1) } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool7,
                NameSource = "{TierType} Mining Tool",
                Tier = 7,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 100,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier);
                },
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 5), new Component(ItemId.Adamantium, 15), new Component(ItemId.AdamantiumSuperconductor, 1) } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool8,
                NameSource = "{TierType} Mining Tool",
                Tier = 8,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 100,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier, -(float)Math.PI / 32f);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier, (float)Math.PI / 32f);
                },
                Recipe = new Recipe() { Location = ItemId.Forge, Components = new Component[] { new Component(ItemId.Battery, 5), new Component(ItemId.Obsidian, 15), new Component(ItemId.Diamond, 10), new Component(ItemId.TurboSuperconductor, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.MiningTool9,
                NameSource = "{TierType} Mining Tool",
                Tier = 9,
                ListPriority = 10,
                Category = ItemCategory.MiningTool,
                Description = "Can break up to tier {Tier} materials.",
                CoolDown = 50,
                PlaceAndActivateRange = 10000,
                Light = Color.White,
                OnUse = (item) => {
                    item.Player.PlaySound(Sound.MiningToolFire);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier, -(float)Math.PI / 16f);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier);
                    Projectile.Shoot(ProjectileId.DigExtraLarge, item.Player, item.Type.Tier, +(float)Math.PI / 16f);
                },
                Recipe = new Recipe() { Location = ItemId.CyberForge, Components = new Component[] { new Component(ItemId.Battery, 5), new Component(ItemId.Einsteinium, 15), new Component(ItemId.Diamond, 20), new Component(ItemId.RifleBarrel, 3), new Component(ItemId.UltraSuperconductor, 1), } },
            });

            ItemBase.AddItem(new ItemType() {
                Id = ItemId.Dematerializer,
                Category = ItemCategory.Tool,
                Description = "Used on placables to return it to the hypercube..\nHold shift to break down walls.",
                CoolDown = 0,
                Tier = 1,
                ListPriority = 19,
                Recipe = new Recipe() { Location = ItemId.Workbench, Components = new Component[] { new Component(ItemId.Iron, 5), } },
                OnStartUsing = (item) => {
                    if (item.Player.Map.IsTerrainLocked) {
                        item.Player.MessageToClient("Unable to dematerialize on terrain locked map.", MessageType.Error);
                        item.Player.PlaySound(Sound.Error);
                        return;
                    }

                    var placement = item.Player.Map.FindPlaceable(item.Player.MousePosition);
                    if (placement != null) {
                        if (!placement.Type.IsDematerializable) {
                            item.Player.MessageToClient("Unable to dematerialize " + placement.Type.Name, MessageType.Error);
                            item.Player.PlaySound(Sound.Error);
                        } else if (placement.HasInventoryItems(placement)) {
                            item.Player.MessageToClient("Unable to dematerialize " + placement.Type.Name + " because it has items inside", MessageType.Error);
                            item.Player.PlaySound(Sound.Error);
                        } else {
                            item.Player.Map.RemoveEntity(placement);
                            item.Player.GiveItem(placement.TypeId);
                            item.Player.PlaySound(Sound.Dematerialize);
                        }
                    }
                },
                OnUse = (item) => {
                    if (item.Player.Map.IsTerrainLocked)
                        return;

                    if (item.Player.Keyboard.IsShift()) {
                        var position = item.Player.GetToolTargetPosition(1);
                        var x = (int)position.X / Map.BlockWidth;
                        var y = (int)position.Y / Map.BlockWidth;
                        var material = item.Player.Map.GetWallMaterial(x, y);
                        if (MaterialInfo.IsLooseOrSolid(material)) {
                            item.Player.Map.SetWallMaterial(x, y, Material.Air);
                            var materialInfo = MaterialInfo.MaterialTypes[(int)material];
                            item.Player.GiveItem(materialInfo.Item.TypeId);
                            item.Player.PlaySound(Sound.Dematerialize);
                        }
                    } else {
                        // special case for dematerializing platform
                        var position = item.Player.GetToolTargetPosition(1);
                        var material = item.Player.Map.GetMaterialAtPixel(position);
                        if (material == Material.Platform) {
                            item.Player.Map.SetMaterialAtPixel(position, Material.Air);
                            var materialInfo = MaterialInfo.MaterialTypes[(int)material];
                            item.Player.GiveItem(materialInfo.Item.TypeId);
                            item.Player.PlaySound(Sound.Dematerialize);
                        }
                    }
                },
            });
        }
    }
}