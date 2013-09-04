using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class KitCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Kit 0-9: Sets your gear to a specific tier."; }
        }

        public override string Execute(Player player, string args)
        {
            if (player == null)
                return "No character";

            int tier = 0;
            if (Int32.TryParse(args, out tier)) {
                // clear all slots
                foreach (var item in player.Inventory.ToArray()) {
                    if (item.SlotType != SlotType.Inventory)
                        player.SetSlot(item, SlotType.Inventory);
                }

                //ActionBar slot 0-9
                IssueAndEquip(player, GetEquipment(ItemCategory.Blaster, tier), 0);
                IssueAndEquip(player, GetEquipment(ItemCategory.Disruptor, tier), 1);
                IssueAndEquip(player, GetEquipment(ItemCategory.LaserRifle, tier), 2);
                IssueAndEquip(player, GetEquipment(ItemCategory.HomingMissile, tier), 3);
                IssueAndEquip(player, GetEquipment(ItemCategory.Grenade, tier), 4);

                if (tier >= 5)
                    IssueAndEquip(player, ItemBase.Get(ItemId.BoseEinsteinCondenser), 5);
                if (tier >= 6)
                    IssueAndEquip(player, ItemBase.Get(ItemId.FlameThrower), 6);

                IssueAndEquip(player, ItemBase.Get(ItemId.Dematerializer), 8);

                var seekerType = (from i in ItemBase.Types where i.Name.EndsWith("Portal Seeker") && i.Tier <= tier orderby i.Tier descending, i.Rarity descending select i).FirstOrDefault();
                IssueAndEquip(player, seekerType, 7);

                IssueAndEquip(player, GetEquipment(ItemCategory.MiningTool, tier), 9);

                //Equipment slot 0-7
                IssueAndEquip(player, GetEquipment(ItemCategory.Helmet, tier), 0);
                IssueAndEquip(player, GetEquipment(ItemCategory.Suit, tier), 1);
                IssueAndEquip(player, GetEquipment(ItemCategory.PowerCore, tier), 2);

                //Boots
                if (tier >= 2)
                    IssueAndEquip(player, ItemBase.Get(ItemId.SpeedBoots), 3);

                //Accessories
                if (tier < 4 && tier >= 2) {
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyBooster1), 4);
                }

                if (tier < 6 && tier >= 4) {
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyBooster3), 4);
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyShield1), 5);
                    //IssueAndEquip(player, ItemBase.Get(ItemId.EnergyRegenerator1), 6);
                }

                if (tier < 8 && tier >= 6) {
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyBooster5), 4);
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyShield3), 5);
                    //IssueAndEquip(player, ItemBase.Get(ItemId.EnergyRegenerator3), 6);
                }

                if (tier >= 8) {
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyBooster7), 4);
                    IssueAndEquip(player, ItemBase.Get(ItemId.EnergyShield5), 5);
                    //IssueAndEquip(player, ItemBase.Get(ItemId.EnergyRegenerator5), 6);
                }
                //Gravity Belts
                if (tier == 5)
                    IssueAndEquip(player, ItemBase.Get(ItemId.GravityBelt5), 6);
                if (tier == 6)
                    IssueAndEquip(player, ItemBase.Get(ItemId.GravityBelt6), 6);
                if (tier >= 7)
                    IssueAndEquip(player, ItemBase.Get(ItemId.GravityBelt7), 6);

                //Cheat Item
                IssueAndEquip(player, ItemBase.Get(ItemId.TheKeysOfTheKingdom), 7);
            }

            return "Player tier set to " + tier;
        }

        public void IssueAndEquip(Player player, ItemType type, int slot)
        {
            if (type != null) {
                if (!player.HasItem(type.Id)) {
                    player.GiveItem(type.Id);
                }

                var item = player.GetItem(type.Id);
                if (item.Type.Category == ItemCategory.Helmet || item.Type.Category == ItemCategory.Suit || item.Type.Category == ItemCategory.PowerCore || item.Type.Category == ItemCategory.Accessory || item.Type.Category == ItemCategory.Boots)
                    player.SetSlot(item, SlotType.Equipment, slot);
                else
                    player.SetSlot(item, SlotType.ActionBar, slot);
            }
        }

        public ItemType GetEquipment(ItemCategory category, int tier)
        {
            return (from i in ItemBase.Types
                    where i.Category == category
                        && i.Tier <= tier
                    orderby i.Tier descending,
                        i.Rarity descending
                    select i).FirstOrDefault();
        }
    }
}