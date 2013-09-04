using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SummonCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Summon: Summons all players on the server to your location."; }
        }

        public override string Execute(Player player, string args)
        {
            foreach (var map in World.Maps) {
                foreach (var p in map.Players.ToArray()) {
                    if (p != player)
                        p.SendTo(player.Map, player.Position);
                }
            }

            return "All players summoned.";
        }
    }
}