using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ClearCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Clear: Clears the inventory of the nearest chest."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            if (placeable.Inventory == null)
                return "Closest placeable does not have an inventory.";

            for (int i = 0; i < placeable.Inventory.Length; i++) {
                if (placeable.Inventory[i] != null)
                    placeable.RemoveItem(placeable.Inventory[i]);
            }

            return String.Format("{0} inventory cleared.", placeable.ToString());
        }
    }
}