using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public abstract class MultiplayerCommand : Command
    {
        public override bool IsAvailable(Player player)
        {
            return World.IsServer;
        }
    }
}