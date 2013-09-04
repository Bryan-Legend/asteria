using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapReplaceCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapReplace MaterialFrom MaterialTo [Above|Below]: Replaces a material with another material. (Material names are case sensitive)"; }
        }

        public override string Execute(Player player, string args)
        {
            var parts = args.Split(' ');
            if (parts.Length < 2)
                return "You must specify two materials.";

            var fromMaterial = Material.Air;
            if (!Enum.TryParse(parts[0], out fromMaterial))
                return "Invalid from material";

            var toMaterial = Material.Air;
            if (!Enum.TryParse(parts[1], out toMaterial))
                return "Invalid to material";

            var start = 0;
            var end = player.Map.Materials.Length;

            var lastArg = parts[parts.Length - 1].ToLowerInvariant();
            switch (lastArg) {
                case "above":
                    end = ((int)player.Position.Y / Map.BlockHeight) * player.Map.Width;
                    break;
                case "below":
                    start = ((int)player.Position.Y / Map.BlockHeight) * player.Map.Width;
                    break;
            }

            for (int i = start; i < end; i++) {
                if (player.Map.Materials[i] == (byte)fromMaterial)
                    player.Map.Materials[i] = (byte)toMaterial;
                if (player.Map.WallMaterials[i] == (byte)fromMaterial)
                    player.Map.WallMaterials[i] = (byte)toMaterial;
            }

            return String.Format("Material replaced.");
        }
    }
}