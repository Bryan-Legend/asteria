using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class OwnCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Own: Take ownership of the closest placeable."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            placeable.Owner = player.Name;

            return placeable.ToString() + " owner set to " + player.Name;
        }
    }
}