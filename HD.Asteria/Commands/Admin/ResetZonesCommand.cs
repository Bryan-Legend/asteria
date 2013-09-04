using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class ResetZonesCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/ResetZones: Resets all zones."; }
        }

        public override string Execute(Player player, string args)
        {
            Server.ResetZones();
            return "Server zones reset.";
        }
    }
}