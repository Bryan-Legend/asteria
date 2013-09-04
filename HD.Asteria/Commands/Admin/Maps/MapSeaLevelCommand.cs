using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapSeaLevelCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapSeaLevel [amount]: Sets the sea level of the current map. This is used for the light calculating and background rendering."; }
        }

        public override string Execute(Player player, string args)
        {
            int amount;
            if (!Int32.TryParse(args, out amount))
                amount = (int)player.Position.Y / Map.BlockHeight;

            player.Map.SeaLevel = amount;
            player.Map.GenerateTierAltitudes();
            return String.Format("Sea Level set to {0}.", amount);
        }
    }
}