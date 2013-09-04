using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ToggleCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Toggle AutoSpawn|Lock|ResetOnDie|ResetOnLeave: Toggles flags on the current map."; }
        }

        public override string Execute(Player player, string args)
        {
            switch (args.ToUpper()) {
                case "AUTOSPAWN":
                    player.Map.IsAutospawn = !player.Map.IsAutospawn;
                    return "Map Autospawn set to " + player.Map.IsAutospawn;
                case "LOCK":
                    player.Map.IsTerrainLocked = !player.Map.IsTerrainLocked;
                    return "Map Terrain Lock set to " + player.Map.IsTerrainLocked;
                case "RESETONLEAVE":
                    player.Map.IsResetOnLeave = !player.Map.IsResetOnLeave;
                    return "Map ResetOnLeave set to " + player.Map.IsResetOnLeave;
                case "RESETONDIE":
                    player.Map.IsResetOnDie = !player.Map.IsResetOnDie;
                    return "Map ResetOnDie set to " + player.Map.IsResetOnDie;
                default:
                    return "Unknown flag " + args;
            }
        }
    }
}