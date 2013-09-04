using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SaveCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Save: Save the game world state."; }
        }

        public override string Execute(Player player, string args)
        {
            if (!player.IsAdmin && World.IsServer)
                return "Admin rights required to use save on multiplayer server.";

            var filename = World.Save();
            return "Game world saved to " + filename;
        }

        public override bool IsAvailable(Player player)
        {
            return player.IsAdmin || !World.IsServer;
        }
    }
}