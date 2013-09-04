using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapExtraLivesCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapExtraLives [amount]: Sets the number of extra lives before a player is kicked back to the overworld. 0 = No Limit"; }
        }

        public override string Execute(Player player, string args)
        {
            int amount;
            if (Int32.TryParse(args, out amount)) {
                var zone = player.Map;
                zone.ExtraLives = amount;
                return String.Format("Map extra lives set to {0}.", amount);
            }

            return "Invalid amount.";
        }
    }
}