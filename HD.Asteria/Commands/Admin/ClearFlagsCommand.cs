using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ClearFlagsCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/ClearFlags: Clears all flags (ie World Chests marked as opened) on the current player."; }
        }

        public override string Execute(Player player, string args)
        {
            if (player.Flags != null)
                player.Flags.Clear();

            return "All flags cleared.";
        }
    }
}