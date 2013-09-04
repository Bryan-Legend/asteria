using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapAmbientCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapAmbient amount: Sets the ambient light level for the current map. (0-255)"; }
        }

        public override string Execute(Player player, string args)
        {
            byte amount;
            if (Byte.TryParse(args, out amount)) {
                player.Map.AmbientLight = amount;
                return String.Format("Ambient light set to {0}.", amount);
            }

            return "Invalid amount.";
        }
    }
}