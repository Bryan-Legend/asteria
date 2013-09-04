using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapGenerateCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapGenerate: Wipes and generates an overworld for the current map."; }
        }

        public override string Execute(Player player, string args)
        {
            player.Map.Generate();

            foreach (var p in player.Map.Players)
                p.Respawn();

            return "Map generated.";
        }
    }
}