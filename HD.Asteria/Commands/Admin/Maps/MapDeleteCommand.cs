using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class MapDeleteCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapDelete Name: Deletes a map."; }
        }

        public override string Execute(Player player, string args)
        {
            var map = World.GetMap(args);
            if (map == null)
                return "Map not found.";

            if (map.Name == "Overworld")
                return "Unable to delete overworld.";

            World.DeleteMap(map);

            return map.Name + " removed.";
        }
    }
}