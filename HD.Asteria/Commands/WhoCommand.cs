using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class WhoCommand : Command
    {
        public override string Description
        {
            get { return "/Who: Lists connected players."; }
        }

        public override string Execute(Player player, string args)
        {
            var result = new StringBuilder();
            result.AppendLine("Connected players:");
            foreach (var map in World.Maps) {
                foreach (var p in map.Players) {
                    result.AppendFormat("{0} ({1}, {2:N0}, {3:N0})\n", p.Name, p.Map.Name, p.Position.X, p.Position.Y);
                }
            }
            return result.ToString();
        }
    }
}