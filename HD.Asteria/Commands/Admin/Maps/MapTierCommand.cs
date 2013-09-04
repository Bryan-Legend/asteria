using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapTierCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapTier amount: Sets the Tier of the current map. Controls the strength of the enemies."; }
        }

        public override string Execute(Player player, string args)
        {
            int amount;
            if (Int32.TryParse(args, out amount)) {
                var zone = player.Map;
                zone.Tier = amount;
                return String.Format("Tier set to {0}.", amount);
            }

            return "Invalid amount.";
        }
    }
}