using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HD
{
    public class Recipe
    {
        public int Id;
        public Component[] Components;
        public Item Creates;
        public int CreateAmount = 1;
        public ItemId Location;
        public Achievement OnCraftAchievement;

        bool HasComponents(Player player, int amount = 1)
        {
            foreach (var component in Components)
            {
                if (!player.HasItem(component.TypeId, component.Amount * amount))
                    return false;
            }

            return true;
        }

        public bool CanBuild(Player player, int amount = 1)
        {
            if (!HasComponents(player, amount))
                return false;

            if (Location != ItemId.None)
            {
                var location = ItemBase.Get(Location);
                if (player.Map.FindEntity(player.Position, Location, location.PlaceAndActivateRange) == null)
                    return false;
            }

            return true;
        }

        public Item Create(Player player, int amount = 1)
        {
            if (!CanBuild(player, amount))
                return null;

            Counters.Increment(Counter.Crafted);
            player.GiveAchievement(OnCraftAchievement);

            foreach (var component in Components)
            {
                player.RemoveItem(component.TypeId, component.Amount * amount);
            }

            if (Location != ItemId.None)
            {
                var requirement = ItemBase.Get(Location);
                if (requirement.OnUse != null)
                    requirement.OnUse(null);
            }
            else
                player.PlaySound(Sound.Craft);

            var result = Creates.Clone();
            result.Amount = CreateAmount * amount;
            return result;
        }

        public override string ToString()
        {
            return Creates.Type.Name;
        }
    }
}
