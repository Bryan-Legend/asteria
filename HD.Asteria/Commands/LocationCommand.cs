using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class LocationCommand : Command
    {
        public override string Description
        {
            get { return "/Location: Get your current location."; }
        }

        public override string Execute(Player player, string args)
        {
            if (player == null)
                return "No active character.";

            return String.Format("{0}, {1:N0}, {2:N0}", player.Map.Name, player.Position.X, player.Position.Y);
        }
    }
}