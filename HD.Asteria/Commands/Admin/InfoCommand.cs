//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;

//namespace HD
//{
//    public class InfoCommand : AdminCommand
//    {
//        public override string Description
//        {
//            get { return "/Info: Gives info on all entities in current zone."; }
//        }

//        public override string Execute(Player player, string args)
//        {
//            var result = new StringBuilder();

//            foreach (var entity in player.Map.Entities)
//            {
//                result.AppendLine(entity.ToString());
//            }

//            return result.ToString();
//        }
//    }
//}