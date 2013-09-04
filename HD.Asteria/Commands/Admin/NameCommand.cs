using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class NameCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Name Name: Sets the name text of the closest placeable."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            placeable.Name = args;

            return placeable.ToString() + " name set to " + args;
        }
    }
}