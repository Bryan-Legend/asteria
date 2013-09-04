using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class DisownCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Disown: Remove ownership of the closest placeable."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            if (placeable.Owner != player.Name)
                return "You do not own " + placeable.ToString();

            placeable.Owner = null;

            return "Removed ownership of " + placeable.ToString();
        }
    }
}