using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapLockCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapLock: Toggles the locking of the terrain.  When terrain is locked, players can not place or destroy the terrain."; }
        }

        public override string Execute(Player player, string args)
        {
            player.Map.IsTerrainLocked = !player.Map.IsTerrainLocked;
            return "Terrain lock set to " + player.Map.IsTerrainLocked;
        }
    }
}