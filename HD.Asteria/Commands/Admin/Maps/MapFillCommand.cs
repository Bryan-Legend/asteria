using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapFillCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapFill Material: Fills the map with a material (Case Sensitive)"; }
        }

        public override string Execute(Player player, string args)
        {
            var defaultMaterial = Material.Air;
            if (!Enum.TryParse(args, out defaultMaterial))
                return "Invalid material";

            player.Map.Materials = Enumerable.Repeat((byte)defaultMaterial, player.Map.Width * player.Map.Height).ToArray();

            return String.Format("Map filled");
        }
    }
}