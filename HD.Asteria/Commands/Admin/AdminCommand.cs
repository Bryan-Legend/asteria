using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public abstract class AdminCommand : Command
    {
        public override bool IsAvailable(Player player)
        {
            return player.IsAdmin;
        }
    }
}