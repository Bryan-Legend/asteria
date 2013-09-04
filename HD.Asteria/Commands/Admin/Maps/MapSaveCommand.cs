using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapSaveCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapSave [FileName]: Saves the current map to a new map file."; }
        }

        public override string Execute(Player player, string args)
        {
            if (String.IsNullOrEmpty(args))
                args = player.Map.Name;

            return "Map saved to " + World.SaveMap(player.Map, args);
        }
    }
}