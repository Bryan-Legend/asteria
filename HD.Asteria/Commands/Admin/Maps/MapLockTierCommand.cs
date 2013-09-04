using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapLockTierCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapLockTier amount: Locks the terrain for materials of a certain tier or higher. This will disable enemy and explosion damage for the materials as well. Zero is disabled."; }
        }

        public override string Execute(Player player, string args)
        {
            int amount;
            if (Int32.TryParse(args, out amount)) {
                var zone = player.Map;
                zone.LockTier = amount;
                return String.Format("Map Lock Tier set to {0}.", amount);
            }

            return "Invalid amount.";
        }
    }
}