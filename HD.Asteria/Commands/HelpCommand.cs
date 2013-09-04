using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HD
{
    public class HelpCommand : Command
    {
        public override string Description
        {
            get { return "/Help: List available commands."; }
        }

        public override string Execute(Player player, string args)
        {
            var result = new StringBuilder();

            result.AppendLine("Available Commands");

            var sortedCommands = from c in World.Commands orderby c.Name select c;
            foreach (var command in sortedCommands) {
                if (command.IsAvailable(player))
                    result.AppendLine(command.Description);
            }
            result.Remove(result.Length - 1, 1);

            return result.ToString();
        }
    }
}