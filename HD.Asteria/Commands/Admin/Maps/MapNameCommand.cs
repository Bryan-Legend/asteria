using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapNameCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapName Name: Sets the name of the map. (Case Sensitive)"; }
        }

        public override string Execute(Player player, string args)
        {
            if (args == null || args.Length < 3 || args.Contains(" "))
                return "Name must be at least 3 letters long and not contain spaces.";

            if (World.MapExists(args))
                return "Map already exists in the world.";

            World.RenameMap(player.Map, args);

            return "Map name set.";
        }
    }
}