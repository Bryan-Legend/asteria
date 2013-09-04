using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapSpawnCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapSpawn: Sets the maps spawn point to your current location."; }
        }

        public override string Execute(Player player, string args)
        {
            player.Map.StartingPosition = player.Position;
            return "Spawn point set.";
        }
    }
}