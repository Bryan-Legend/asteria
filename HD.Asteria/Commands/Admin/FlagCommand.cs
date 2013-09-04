using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class FlagCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Flag: Toggles the Flag setting of the closest placeable."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            placeable.Flag = !placeable.Flag;

            return placeable.ToString() + " flag set to " + placeable.Flag;
        }
    }
}