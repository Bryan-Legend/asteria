using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SetCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Set Value: Sets the value text of the closest placeable."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            placeable.Value = args;

            return placeable.ToString() + " value set to " + args;
        }
    }
}