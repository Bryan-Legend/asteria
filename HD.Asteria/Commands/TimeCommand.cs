using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class TimeCommand : Command
    {
        public override string Description
        {
            get { return "/Time [Hours]: Get or set the current game time."; }
        }

        public override string Execute(Player player, string args)
        {
            float amount;
            if (Single.TryParse(args, out amount)) {
                player.Map.GameTime = DateTime.MinValue.AddHours(amount);
            }

            return "Current game time: " + player.Map.GameTime.ToLongTimeString();
        }
    }
}