using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapGravityCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapGravity amount: Sets the gravity of the current map.  Default value is " + Map.DefaultGravity; }
        }

        public override string Execute(Player player, string args)
        {
            float amount;
            if (Single.TryParse(args, out amount)) {
                var zone = player.Map;
                zone.Gravity = amount;
                return String.Format("Gravity set to {0}.", amount);
            }

            return "Invalid amount.";
        }
    }
}