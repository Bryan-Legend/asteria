using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using System.Linq;

namespace HD
{
    public class MapCreateCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/MapCreate Name Width Height [Default Material]: Creates a new map. (Each map allocates at least Width * Height * 2 bytes of memory) (Case Sensitive)"; }
        }

        public override string Execute(Player player, string args)
        {
            var splitArgs = SplitArgs(args);
            if (splitArgs.Length < 3)
                return "Must pass 3 arguments.";

            var name = splitArgs[0];
            if (name == null || name.Length < 3)
                return "Name must be at least 3 letters long.";

            if (World.GetMap(name) != null)
                return name + " already exists.";

            var width = 0;
            if (!Int32.TryParse(splitArgs[1], out width) || width > 10000 || width < 320)
                return "Invalid width.  Must be within 320-10000.";

            var height = 0;
            if (!Int32.TryParse(splitArgs[2], out height) || height > 10000 || height < 200)
                return "Invalid height.  Must be within 200-10000.";

            var defaultMaterial = Material.Air;
            if (splitArgs.Length >= 4) {
                if (!Enum.TryParse(splitArgs[3], out defaultMaterial))
                    return "Invalid default material";
            }

            var result = World.CreateMap(name, width, height);
            result.Materials = Enumerable.Repeat((byte)defaultMaterial, result.Width * result.Height).ToArray();
            result.WallMaterials = Enumerable.Repeat((byte)defaultMaterial, result.Width * result.Height).ToArray();
            result.StartingPosition = new Vector2(result.Width * Map.BlockWidth, result.Height * Map.BlockHeight) / 2;
            result.Now = player.Map.Now;
            player.SendTo(result);

            return String.Format("Map {0} created.", result.Name);
        }
    }
}