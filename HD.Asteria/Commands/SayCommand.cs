using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SayCommand : Command
    {
        public override string Description
        {
            get { return "/Say <message>: Say something out loud."; }
        }

        public override string Execute(Player player, string args)
        {
            if (String.IsNullOrEmpty(args))
                return null;

            var message = String.Format("{0} said: {1}", player.Name, args);
            World.Broadcast(message, MessageType.Chat);

            return null;
        }
    }
}