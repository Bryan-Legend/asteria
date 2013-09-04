using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapAutospawnCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapAutospawn: Toggles the auto spawning of monsters on the current map."; }
        }

        public override string Execute(Player player, string args)
        {
            player.Map.IsAutospawn = !player.Map.IsAutospawn;
            return "Terrain auto spawn set to " + player.Map.IsAutospawn;
        }
    }
}