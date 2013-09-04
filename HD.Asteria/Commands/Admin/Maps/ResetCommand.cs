using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ResetCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Reset: Resets current map."; }
        }

        public override string Execute(Player player, string args)
        {
            var result = World.ResetMap(player.Map);
            if (result != null)
                return result;

            return String.Format("Map {0} reset.", player.Map.Name);
        }
    }
}