using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SwitchCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Switch: Triggers the closest placeable as if it had been hit by a switch."; }
        }

        public override string Execute(Player player, string args)
        {
            var placeable = player.Map.FindClosestPlaceable(player.Position);
            if (placeable == null)
                return "Unable to find placeable.";

            placeable.TriggerSwitch(player);

            return placeable.ToString() + " swtich triggered.";
        }
    }
}