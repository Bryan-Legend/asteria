using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class GiveCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/Give ItemName [Amount]: Adds an item to your inventory."; }
        }

        public override string Execute(Player player, string args)
        {
            var splitArgs = SplitArgs(args);
            if (splitArgs.Length < 1)
                return "Must pass item as an argument.";

            var amount = 1;
            if (splitArgs.Length > 1) {
                var amountString = splitArgs[splitArgs.Length - 1];
                if (Int32.TryParse(amountString, out amount)) {
                    args = args.Substring(0, (args.Length - amountString.Length) - 1).Trim();
                }
            }
            if (amount < 1)
                amount = 1;

            var itemType = ItemBase.Get(args);
            if (itemType == null)
                return String.Format("Unable to find item {0}.", args);

            player.GiveItem(itemType.Id, amount);

            return String.Format("Added {0}x {1}", amount, itemType.Name);
        }
    }
}