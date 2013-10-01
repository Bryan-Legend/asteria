using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace HD
{
    public class MapsCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Maps: List maps."; }
        }

        public override string Execute(Player player, string args)
        {
            var result = new StringBuilder();
            result.AppendLine("World Maps");

            foreach (var map in from m in World.Maps orderby m.Tier select m) {
                result.Append(map.ToString());
            }

            return result.ToString();
        }
    }
}