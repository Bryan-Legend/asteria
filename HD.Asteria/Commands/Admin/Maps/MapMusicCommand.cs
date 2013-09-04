using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapMusicCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapMusic Name: Sets the music of the map. (Case Sensitive)"; }
        }

        public override string Execute(Player player, string args)
        {
            player.Map.Music = args;

            //if (args == null || args.Length < 3 || args.Contains(" "))
            //    return "Name must be at least 3 letters long and not contain spaces.";

            //if (World.MapExists(args))
            //    return "Map already exists in the world.";

            //World.RenameMap(player.Map, args);

            return "Map music set.";
        }
    }
}