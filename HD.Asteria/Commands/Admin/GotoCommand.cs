using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class GotoCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Goto MapName: Goto a map."; }
        }

        public override string Execute(Player player, string args)
        {
            if (player == null)
                return "No character";

            if (args == "") {
                var result = new StringBuilder();

                result.AppendLine("Available Maps");

                foreach (var mapList in World.Maps) {
                    result.AppendLine(mapList.Name + " Tier: " + mapList.Tier);
                }
                result.Remove(result.Length - 1, 1);
                return result.ToString();
            }

            var map = World.GetMap(args);
            if (map == null)
                return "Unknown map.";
            else {
                player.SendTo(map);
                return "Moved to map " + map.Name + ".";
            }
        }
    }
}