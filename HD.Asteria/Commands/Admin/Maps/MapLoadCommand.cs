using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.IO;

namespace HD
{
    public class MapLoadCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapLoad MapNameWithoutExtension: Loads a map from a .map file in the save game directory."; }
        }

        public override string Execute(Player player, string args)
        {
            if (String.IsNullOrEmpty(args))
                return "Must pass a map name without the extension";

            var filename = Path.Combine(Utility.SavePath, args + ".map");
            if (!File.Exists(filename))
                return filename + " does not exist.";

            var mapName = World.GetMapName(filename);
            if (World.MapExists(mapName))
                return "Map " + mapName + " already exists in the world.";

            World.Load(filename);

            return args + " loaded.";
        }
    }
}