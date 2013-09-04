using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HD
{
    public class SetClassCommand : AdminCommand
    {
        public override string Description
        {
            get { return "/SetClass ClassId: Sets your class."; }
        }
        
        public override string Execute(Player player, string args)
        {
            if (player == null)
                return "No character";

            int id = 0;
            Int32.TryParse(args, out id);
            var _class = World.Get<Class>(id);
            if (_class != null)
            {
                player.Class = _class;
                return "Character class set to " + _class.Name;
            }
            return "Invalid class";
        }
    }
}