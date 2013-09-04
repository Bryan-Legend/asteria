using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapLavaLevelCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapLavaLevel [amount]: Sets the lava level of the current map. For background rendering."; }
        }

        public override string Execute(Player player, string args)
        {
            int amount;
            if (!Int32.TryParse(args, out amount))
                amount = (int)player.Position.Y / Map.BlockHeight;

            player.Map.LavaLevel = amount;
            return String.Format("Lava Level set to {0}.", amount);
        }
    }
}