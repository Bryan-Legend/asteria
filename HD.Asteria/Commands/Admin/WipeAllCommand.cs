//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;

//namespace Smote.Server
//{
//    public class WipeAllCommand : AdminCommand
//    {
//        public override string Description
//        {
//            get { return "/WipeAll: Resets all characters to start."; }
//        }
        
//        public override string Execute(Player player, string args)
//        {
//            using (var database = Server.CreateDBConnection())
//            {
//                foreach (var row in database.EvaluateTable("select * from Smote.Character"))
//                {
//                    source.MessageToClient("Wiping " + row["Name"], MessageType.System);
//                    CharacterStore.WipeCharacter(database, (int)row["Id"]);
//                }
//            }

//            return "All Characters Wiped";
//        }
//    }
//}