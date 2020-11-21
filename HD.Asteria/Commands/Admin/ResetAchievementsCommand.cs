using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Steamworks;

namespace HD
{
    public class ResetAchievementsCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/ResetAchievements Sure: Resets your steam stats and achievements. Used for testing. You probably never want to run this."; }
        }

        public override string Execute(Player player, string args)
        {
            if (args.ToUpperInvariant() != "SURE")
            {
                return "Must pass \"Sure\" as an argument.";
            }

            Utility.LogMessage("SteamUserStats.ResetAllStats(true)");
            SteamUserStats.ResetAllStats(true);

            return "Achievements Reset";
        }
    }
}